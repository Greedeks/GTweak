using GTweak.Core.ViewModel;
using GTweak.Utilities.Animation;
using GTweak.Utilities.Configuration;
using GTweak.Utilities.Controls;
using GTweak.Utilities.Helpers;
using GTweak.Utilities.Managers;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Threading;

namespace GTweak.View
{
    public partial class DataSystemView : UserControl
    {
        private readonly MonitoringService _monitoringService = new MonitoringService();
        private readonly SystemDiagnostics _systemDiagnostics = new SystemDiagnostics();
        private readonly TimerControlManager timer = default;

        public DataSystemView()
        {
            InitializeComponent();

            _monitoringService.HandleDevicesEvents += OnHandleDevicesEvents;
            _monitoringService.StartDeviceMonitoring();

            RAMLoad.Value = _monitoringService.GetMemoryUsage;
            CPULoad.Value = MonitoringService.GetProcessorUsage;

            App.LanguageChanged += delegate
            {
                if (new Dictionary<SystemDiagnostics.ConnectionStatus, string>
                {
                    { SystemDiagnostics.ConnectionStatus.Lose, "connection_lose_systemInformation" },
                    { SystemDiagnostics.ConnectionStatus.Block, "connection_block_systemInformation" },
                    { SystemDiagnostics.ConnectionStatus.Limited, "connection_limited_systemInformation" }
                }.TryGetValue(SystemDiagnostics.CurrentConnection, out string resourceKey)) { SystemDiagnostics.HardwareData.UserIPAddress = (string)FindResource(resourceKey); }
                DataContext = new DataSystemVM();
            };

            timer = new TimerControlManager(TimeSpan.Zero, TimerControlManager.TimerMode.CountUp, async time =>
            {
                if ((int)time.TotalSeconds % 2 == 0)
                {
                    BackgroundQueue backgroundQueue = new BackgroundQueue();
                    await backgroundQueue.QueueTask(async delegate { await _monitoringService.GetTotalProcessorUsage(); });
                    AnimationProgressBars();
                }
                else if ((int)time.TotalSeconds % 5 == 0)
                {
                    BackgroundQueue backgroundQueue = new BackgroundQueue();
                    await backgroundQueue.QueueTask(delegate { _systemDiagnostics.GetUserIpAddress(); });
                    DataContext = new DataSystemVM();
                }

                if (BtnHiddenIP.IsChecked.Value & BtnHiddenIP.Visibility == Visibility.Hidden & !SystemDiagnostics.isIPAddressFormatValid)
                {
                    DoubleAnimation doubleAnim = new DoubleAnimation(0, (Duration)TimeSpan.FromSeconds(0.18));
                    doubleAnim.Completed += delegate { SettingsEngine.IsHiddenIpAddress = false; };
                    Timeline.SetDesiredFrameRate(doubleAnim, 240);
                    IpAddress.Effect.BeginAnimation(BlurEffect.RadiusProperty, doubleAnim);
                }
            });
            timer.Start();
        }

        #region Animations
        private void AnimationProgressBars()
        {
            Dispatcher.Invoke(() =>
            {
                DoubleAnimation doubleAnim = new DoubleAnimation()
                {
                    From = CPULoad.Value,
                    To = MonitoringService.GetProcessorUsage,
                    EasingFunction = new QuadraticEase(),
                    Duration = TimeSpan.FromSeconds(0.2)
                };
                Timeline.SetDesiredFrameRate(doubleAnim, 240);
                CPULoad.BeginAnimation(ProgressBar.ValueProperty, doubleAnim);

                doubleAnim = new DoubleAnimation()
                {
                    From = RAMLoad.Value,
                    To = _monitoringService.GetMemoryUsage,
                    EasingFunction = new QuadraticEase(),
                    Duration = TimeSpan.FromSeconds(0.2)
                };
                Timeline.SetDesiredFrameRate(doubleAnim, 240);
                RAMLoad.BeginAnimation(ProgressBar.ValueProperty, doubleAnim);
            });
        }
        private void AnimationPopup()
        {
            Dispatcher.Invoke(() =>
            {
                PopupCopy.IsOpen = true;
                CopyTextToastBody.BeginAnimation(ContextMenu.OpacityProperty, FadeAnimation.FadeIn(0.9, 0.27, () => { PopupCopy.IsOpen = false; }, true));

                DoubleAnimation offsetAnim = new DoubleAnimation()
                {
                    From = -20,
                    To = -50,
                    SpeedRatio = 8,
                    EasingFunction = new QuadraticEase(),
                    Duration = TimeSpan.FromSeconds(3)
                };
                Timeline.SetDesiredFrameRate(offsetAnim, 240);
                PopupCopy.BeginAnimation(Popup.VerticalOffsetProperty, offsetAnim);
            });
        }
        #endregion

        private void BtnHiddenIP_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SettingsEngine.IsHiddenIpAddress = !BtnHiddenIP.IsChecked.Value;

            DoubleAnimation doubleAnim = new DoubleAnimation()
            {
                From = BtnHiddenIP.IsChecked.Value ? 20 : 0,
                To = BtnHiddenIP.IsChecked.Value ? 0 : 20,
                EasingFunction = new QuadraticEase(),
                Duration = TimeSpan.FromSeconds(0.2)
            };
            Timeline.SetDesiredFrameRate(doubleAnim, 240);
            IpAddress.Effect.BeginAnimation(BlurEffect.RadiusProperty, doubleAnim);
        }

        private async void OnHandleDevicesEvents(MonitoringService.DeviceType deviceType)
        {
            BackgroundQueue backgroundQueue = new BackgroundQueue();
            await backgroundQueue.QueueTask(delegate { _systemDiagnostics.UpdatingDevicesData(deviceType); });
        }

        private void HandleCopyingData_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Clipboard.Clear();
                switch (sender.GetType().Name)
                {
                    case "TextBlock":
                        {
                            TextBlock textBlock = (TextBlock)sender;
                            double cumulativeHeight = 0;
                            string SelectedLine = string.Empty;

                            foreach (string line in textBlock.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                            {
                                FormattedText formattedText = new FormattedText(line, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface(textBlock.FontFamily,
                                    textBlock.FontStyle, textBlock.FontWeight, textBlock.FontStretch), textBlock.FontSize, textBlock.Foreground, VisualTreeHelper.GetDpi(textBlock).PixelsPerDip);
                                cumulativeHeight += formattedText.Height;

                                if (cumulativeHeight >= e.GetPosition(textBlock).Y)
                                {
                                    SelectedLine = line;
                                    break;
                                }
                            }
                            Clipboard.SetData(DataFormats.UnicodeText, SelectedLine);
                            break;
                        }
                    case "Run":
                        {
                            Run runtext = (Run)sender;
                            Clipboard.SetData(DataFormats.UnicodeText, runtext.Text.Replace('\n', ' '));
                            break;
                        }
                }

                if (!PopupCopy.IsOpen)
                    AnimationPopup();
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _monitoringService.StopDeviceMonitoring();
            timer.Stop();
        }
    }
}
