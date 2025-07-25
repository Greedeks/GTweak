﻿using GTweak.Utilities.Animation;
using GTweak.Utilities.Controls;
using GTweak.Utilities.Helpers;
using GTweak.Utilities.Managers;
using GTweak.Utilities.Tweaks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace GTweak.View
{
    public partial class PakagesView : UserControl
    {
        private readonly TimerControlManager timer = default;
        private bool _isWebViewRemoval = false;

        public PakagesView()
        {
            InitializeComponent();

            timer = new TimerControlManager(TimeSpan.Zero, TimerControlManager.TimerMode.CountUp, time =>
            {
                if ((int)time.TotalSeconds % 5 == 0)
                {
                    BackgroundWorker backgroundWorker = new BackgroundWorker();
                    backgroundWorker.DoWork += delegate { new UninstallingPakages().LoadInstalledPackages(); };
                    backgroundWorker.RunWorkerCompleted += delegate { UpdateViewStatePakages(); };
                    backgroundWorker.RunWorkerAsync();
                }
            });
            timer.Start();
        }

        private void Apps_MouseEnter(object sender, MouseEventArgs e)
        {
            string descriptionApp = (string)FindResource(((Image)sender).Name + "_applications");

            if (CommentApp.Text != descriptionApp)
                CommentApp.Text = descriptionApp;
        }

        private void Apps_MouseLeave(object sender, MouseEventArgs e)
        {
            if (CommentApp.Text != (string)FindResource("defaultDescriptionApp"))
                CommentApp.Text = (string)FindResource("defaultDescriptionApp");
        }

        private async void ClickApp_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Image packageImage = (Image)sender;
            string packageName = packageImage.Name;

            try
            {
                switch (e.LeftButton)
                {
                    case MouseButtonState.Pressed when Equals(packageImage.Source, FindResource("A_DI_" + packageName)):
                        {
                            if (packageName.Equals("Edge"))
                            {
                                Overlay.Visibility = Visibility.Visible;

                                Overlay.BeginAnimation(OpacityProperty, FactoryAnimation.CreateIn(0, 1, 0.3));

                                TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

                                void DeleteHandler(object sender, RoutedEventArgs args)
                                {
                                    tcs.TrySetResult(true);
                                    BtnDelete.PreviewMouseLeftButtonDown -= DeleteHandler;
                                    BtnCancel.PreviewMouseLeftButtonDown -= CancelHandler;
                                }

                                void CancelHandler(object sender, RoutedEventArgs args)
                                {
                                    tcs.TrySetResult(false);
                                    BtnDelete.PreviewMouseLeftButtonDown -= DeleteHandler;
                                    BtnCancel.PreviewMouseLeftButtonDown -= CancelHandler;
                                }

                                BtnDelete.PreviewMouseLeftButtonDown += DeleteHandler;
                                BtnCancel.PreviewMouseLeftButtonDown += CancelHandler;

                                try { _isWebViewRemoval = await tcs.Task; }
                                catch (TaskCanceledException) { _isWebViewRemoval = false; }

                                Overlay.BeginAnimation(OpacityProperty, FactoryAnimation.CreateTo(0.25, () => { Overlay.Visibility = Visibility.Collapsed; }));
                            }

                            BackgroundQueue backgroundQueue = new BackgroundQueue();
                            await backgroundQueue.QueueTask(async () =>
                            {
                                Dispatcher.Invoke(() =>
                                {
                                    UninstallingPakages.HandleAvailabilityStatus(packageName, true);
                                    UpdateViewStatePakages();
                                });

                                await UninstallingPakages.RemoveAppxPackage(packageName, _isWebViewRemoval);

                                await Task.Delay(3000);

                                Dispatcher.Invoke(() =>
                                {
                                    UninstallingPakages.HandleAvailabilityStatus(packageName, false);
                                    UpdateViewStatePakages();

                                    if (ExplorerManager.PackageMapping.TryGetValue(packageName, out bool needRestart))
                                        ExplorerManager.Restart(new Process());
                                });

                            });
                            break;
                        }

                    case MouseButtonState.Pressed when Equals(packageImage.Source, FindResource("DA_DI_" + packageName)) && packageName == "OneDrive":
                        {
                            if (string.IsNullOrWhiteSpace(PathLocator.Executable.OneDrive))
                                new NotificationManager().Show("", "warn", "error_onedrive_notification");
                            else
                            {
                                new NotificationManager().Show("", "info", "success_onedrive_notification");

                                BackgroundQueue backgroundQueue = new BackgroundQueue();
                                await backgroundQueue.QueueTask(async () =>
                                {

                                    Dispatcher.Invoke(() =>
                                    {
                                        UninstallingPakages.HandleAvailabilityStatus(packageName, true);
                                        UpdateViewStatePakages();
                                    });

                                    await UninstallingPakages.RestoreOneDriveFolder();

                                    await Task.Delay(3000);

                                    Dispatcher.Invoke(() =>
                                    {
                                        UninstallingPakages.HandleAvailabilityStatus(packageName, false);
                                        UpdateViewStatePakages();
                                    });
                                });
                            }
                            break;
                        }
                }
            }
            catch (Exception ex) { ErrorLogging.LogDebug(ex); }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e) => UpdateViewStatePakages();

        private void Page_Unloaded(object sender, RoutedEventArgs e) => timer.Stop();

        private ImageSource AvailabilityInstalledPackage(string packageName, bool isOneDrive = false)
        {
            if (!UninstallingPakages.PackagesDetails.TryGetValue(packageName, out var details) || details.Scripts == null)
            {
                if (isOneDrive)
                    return !UninstallingPakages.HandleAvailabilityStatus(packageName) ? UninstallingPakages.IsOneDriveInstalled ? (DrawingImage)FindResource($"A_DI_{packageName}") : (DrawingImage)FindResource($"DA_DI_{packageName}") : (DrawingImage)FindResource("DI_Sandtime");
                return (DrawingImage)FindResource("DI_Sandtime");
            }

            bool isContains = details.Scripts.Any(pattern => UninstallingPakages.InstalledPackages.Any(pkg => Regex.IsMatch(pkg, $"^{Regex.Escape(pattern)}", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)));

            return !UninstallingPakages.HandleAvailabilityStatus(packageName) ? isContains ? (DrawingImage)FindResource($"A_DI_{packageName}") : (DrawingImage)FindResource($"DA_DI_{packageName}") : (DrawingImage)FindResource("DI_Sandtime");
        }

        private void UpdateViewStatePakages()
        {
            Dictionary<Image, string> packages = new Dictionary<Image, string>
            {
                { MicrosoftStore, "MicrosoftStore" },
                { Todos, "Todos" },
                { BingWeather, "BingWeather" },
                { OneDrive, "OneDrive" },
                { Mail, "Mail" },
                { Edge, "Edge" },
                { Cortana, "Cortana" },
                { Microsoft3D, "Microsoft3D" },
                { Music, "Music" },
                { GetHelp, "GetHelp" },
                { MicrosoftOfficeHub, "MicrosoftOfficeHub" },
                { MicrosoftSolitaireCollection, "MicrosoftSolitaireCollection" },
                { MixedReality, "MixedReality" },
                { Xbox, "Xbox" },
                { Paint3D, "Paint3D" },
                { OneNote, "OneNote" },
                { People, "People" },
                { MicrosoftStickyNotes, "MicrosoftStickyNotes" },
                { Widgets, "Widgets" },
                { ScreenSketch, "ScreenSketch" },
                { Phone, "Phone" },
                { Photos, "Photos" },
                { FeedbackHub, "FeedbackHub" },
                { SoundRecorder, "SoundRecorder" },
                { Alarms, "Alarms" },
                { SkypeApp, "SkypeApp" },
                { Maps, "Maps" },
                { Camera, "Camera" },
                { Video, "Video" },
                { BingNews, "BingNews" },
                { MicrosoftTeams, "MicrosoftTeams" },
                { PowerAutomateDesktop, "PowerAutomateDesktop" },
                { ClipChamp, "ClipChamp" },
                { GetStarted, "GetStarted" },
                { BingSports, "BingSports" },
                { BingFinance, "BingFinance" },
                { MicrosoftFamily, "MicrosoftFamily" },
                { BingSearch, "BingSearch" },
                { Outlook, "Outlook" },
                { QuickAssist, "QuickAssist" },
                { DevHome, "DevHome" },
                { WindowsTerminal, "WindowsTerminal" },
                { LinkedIn, "LinkedIn" },
                { WebMediaExtensions, "WebMediaExtensions" },
                { OneConnect, "OneConnect" }
            };

            foreach (var package in packages)
            {
                Image image = package.Key;
                string packageName = package.Value;
                image.Source = AvailabilityInstalledPackage(packageName, packageName == "OneDrive");
            }
        }
    }
}
