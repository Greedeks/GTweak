﻿using GTweak.Utilities.Controls;
using GTweak.Utilities.Helpers;
using GTweak.Utilities.Managers;
using GTweak.Utilities.Tweaks;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace GTweak.View
{
    public partial class PakagesView : UserControl
    {
        private TimerControlManager _timer = default;
        private readonly BackgroundQueue _backgroundQueue = new BackgroundQueue();
        private readonly UninstallingPakages _uninstalling = new UninstallingPakages();
        private bool _isWebViewRemoval = false;

        public PakagesView()
        {
            InitializeComponent();

            Unloaded += delegate { _timer.Stop(); };
            Loaded += delegate
            {
                _timer = new TimerControlManager(TimeSpan.FromSeconds(5), TimerControlManager.TimerMode.CountUp, async time =>
                {
                    BackgroundWorker backgroundWorker = new BackgroundWorker();
                    backgroundWorker.DoWork += delegate { _uninstalling.GetInstalledPackages(); };
                    backgroundWorker.RunWorkerCompleted += delegate { Dispatcher.Invoke(() => { UninstallingPakages.OnPackagesChanged(); }); };
                    backgroundWorker.RunWorkerAsync();
                });

                _timer.Start();
            };
        }

        private void Package_MouseEnter(object sender, MouseEventArgs e)
        {
            string description = ((ToggleButton)sender).ToolTip?.ToString() ?? string.Empty;

            if (DescBlock.Text != description)
                DescBlock.Text = description;

        }

        private void Package_MouseLeave(object sender, MouseEventArgs e)
        {
            if (DescBlock.Text != DescBlock.DefaultText)
                DescBlock.Text = DescBlock.DefaultText;
        }

        private async void ToggleButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ToggleButton toggleButton = (ToggleButton)sender;
            string packageName = toggleButton.Name;

            if (toggleButton.IsChecked == false && packageName == "OneDrive")
            {
                if (string.IsNullOrWhiteSpace(PathLocator.Executable.OneDrive))
                    NotificationManager.Show("warn", "error_onedrive_noty").Perform();
                else
                {
                    NotificationManager.Show("info", "success_onedrive_noty").Perform();

                    await _backgroundQueue.QueueTask(async () =>
                    {
                        Dispatcher.Invoke(() => { UninstallingPakages.HandleAvailabilityStatus(packageName, true); });

                        await UninstallingPakages.RestoreOneDriveFolder();

                        await Task.Delay(3000);

                        Dispatcher.Invoke(() => { UninstallingPakages.HandleAvailabilityStatus(packageName, false); });
                    });
                }
            }
            else if (toggleButton.IsChecked == false)
            {
                e.Handled = true;
                return;
            }
            else if (toggleButton.IsChecked == true)
            {
                if (packageName.Equals("Edge"))
                {
                    OverlayDialogManager overlayDialog = new OverlayDialogManager(Overlay, OpacityProperty, BtnDelete, BtnCancel);
                    _isWebViewRemoval = await overlayDialog.Show();
                }

                await _backgroundQueue.QueueTask(async () =>
                {
                    Dispatcher.Invoke(() => { UninstallingPakages.HandleAvailabilityStatus(packageName, true); }); ;

                    await UninstallingPakages.RemoveAppxPackage(packageName, _isWebViewRemoval);

                    await Task.Delay(3000);

                    Dispatcher.Invoke(() => { UninstallingPakages.HandleAvailabilityStatus(packageName, false); });

                    await Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (ExplorerManager.PackageMapping.TryGetValue(packageName, out bool needRestart))
                            ExplorerManager.Restart(new Process());
                    }), DispatcherPriority.ApplicationIdle);
                });
            }
        }
    }
}
