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
using System.Windows.Media.Effects;
using System.Windows.Threading;

namespace GTweak.View
{
    public partial class DataSystemView : UserControl
    {
        private readonly MonitoringService _monitoringService = new MonitoringService();
        private readonly SystemDiagnostics _systemDiagnostics = new SystemDiagnostics();
        private readonly BackgroundQueue backgroundQueue = new BackgroundQueue();
        private readonly TimerControlManager _timer = default;

        public DataSystemView()
        {
            InitializeComponent();

            _monitoringService.HandleDevicesEvents += OnHandleDevicesEvents;

            RAMLoad.Value = _monitoringService.GetMemoryUsage;
            CPULoad.Value = MonitoringService.GetProcessorUsage;

            App.LanguageChanged += delegate
            {
                Dispatcher.Invoke(() =>
                {
                    if (new Dictionary<SystemDiagnostics.ConnectionStatus, string>
                    {
                        { SystemDiagnostics.ConnectionStatus.Lose, "connection_lose_systemInformation" },
                        { SystemDiagnostics.ConnectionStatus.Block, "connection_block_systemInformation" },
                        { SystemDiagnostics.ConnectionStatus.Limited, "connection_limited_systemInformation" }
                    }.TryGetValue(SystemDiagnostics.CurrentConnection, out string resourceKey)) { SystemDiagnostics.HardwareData.UserIPAddress = (string)FindResource(resourceKey); }
                    DataContext = new DataSystemVM();
                });
            };

            _timer = new TimerControlManager(TimeSpan.Zero, TimerControlManager.TimerMode.CountUp, async time =>
            {
                if ((int)time.TotalSeconds % 2 == 0)
                {
                    await backgroundQueue.QueueTask(async delegate { await _monitoringService.GetTotalProcessorUsage(); });
                    AnimationProgressBars();
                }
                else if ((int)time.TotalSeconds % 5 == 0)
                {
                    await backgroundQueue.QueueTask(delegate { _systemDiagnostics.GetUserIpAddress(); });
                    Dispatcher.Invoke(() => DataContext = new DataSystemVM());
                }

                Dispatcher.Invoke(() =>
                {
                    if (BtnHiddenIP.IsChecked.Value & BtnHiddenIP.Visibility == Visibility.Hidden & !SystemDiagnostics.isIPAddressFormatValid)
                        IpAddress.Effect.BeginAnimation(BlurEffect.RadiusProperty, FactoryAnimation.CreateTo(0.18, () => { SettingsEngine.IsHiddenIpAddress = false; }));
                });
            });
        }

        private async void OnHandleDevicesEvents(MonitoringService.DeviceType deviceType) => await backgroundQueue.QueueTask(delegate { _systemDiagnostics.UpdatingDevicesData(deviceType); });

        private void AnimationProgressBars()
        {
            Dispatcher.Invoke(() =>
            {
                CPULoad.BeginAnimation(ProgressBar.ValueProperty, FactoryAnimation.CreateIn(CPULoad.Value, MonitoringService.GetProcessorUsage, 0.2));
                RAMLoad.BeginAnimation(ProgressBar.ValueProperty, FactoryAnimation.CreateIn(RAMLoad.Value, _monitoringService.GetMemoryUsage, 0.2));
            });
        }
        private void AnimationPopup()
        {
            PopupCopy.IsOpen = true;
            CopyTextToastBody.BeginAnimation(UIElement.OpacityProperty, FactoryAnimation.CreateIn(0, 0.9, 0.27, () => { PopupCopy.IsOpen = false; }, true));
            PopupCopy.BeginAnimation(Popup.VerticalOffsetProperty, FactoryAnimation.CreateIn(-20, -50, 0.35));
        }

        private void BtnHiddenIP_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SettingsEngine.IsHiddenIpAddress = !BtnHiddenIP.IsChecked.Value;
            IpAddress.Effect.BeginAnimation(BlurEffect.RadiusProperty, BtnHiddenIP.IsChecked.Value ? FactoryAnimation.CreateTo(0.2) : FactoryAnimation.CreateIn(0, 20, 0.2));
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

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _monitoringService.StartDeviceMonitoring();
            _timer.Start();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _monitoringService.StopDeviceMonitoring();
            _timer.Stop();
        }
    }
}
