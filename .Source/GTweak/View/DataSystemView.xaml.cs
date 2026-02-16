using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using GTweak.Core.ViewModel;
using GTweak.Utilities.Animation;
using GTweak.Utilities.Configuration;
using GTweak.Utilities.Controls;
using GTweak.Utilities.Helpers;
using GTweak.Utilities.Managers;

namespace GTweak.View
{
    public partial class DataSystemView : UserControl
    {
        private readonly SystemDataCollector _systemDataCollector = new SystemDataCollector();
        private readonly BackgroundQueue backgroundQueue = new BackgroundQueue();
        private TimerControlManager _timer = default;

        public DataSystemView()
        {
            InitializeComponent();

            App.LanguageChanged += delegate
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (new Dictionary<HardwareData.ConnectionStatus, string>
                    {
                        { HardwareData.ConnectionStatus.Lose, "connection_lose_sysinfo" },
                        { HardwareData.ConnectionStatus.Block, "connection_block_sysinfo" },
                        { HardwareData.ConnectionStatus.Limited, "connection_limited_sysinfo" }
                    }.TryGetValue(HardwareData.CurrentConnection, out string resourceKey))
                    {
                        HardwareData.UserIPAddress = (string)FindResource(resourceKey);
                        DataContext = new DataSystemViewModel();
                    }
                }));
            };

            Unloaded += delegate { _timer.Stop(); };
            Loaded += delegate
            {
                StartMonitoringData();
                _timer.Start();
            };
        }

        private void StartMonitoringData()
        {
            RAMLoad.Value = HardwareData.Memory.Usage;
            CPULoad.Value = HardwareData.Processor.Usage;

            _timer = new TimerControlManager(TimeSpan.Zero, TimerControlManager.TimerMode.CountUp, async time =>
            {
                if ((int)time.TotalSeconds % 2 == 0)
                {

                    await backgroundQueue.QueueTask(async delegate { await _systemDataCollector.GetTotalProcessorUsage(); });
                    await backgroundQueue.QueueTask(async delegate { await _systemDataCollector.GetPhysicalAvailableMemory(); });
                    AnimationProgressBars(HardwareData.Processor.Usage, HardwareData.Memory.Usage);
                    _ = Dispatcher.BeginInvoke(new Action(async () =>
                    {
                        HardwareData.RunningProcessesCount = await Task.Run(() => _systemDataCollector.GetProcessCount());
                        HardwareData.RunningServicesCount = await Task.Run(() => _systemDataCollector.GetServicesCount());
                    }));
                }
                else if ((int)time.TotalSeconds % 5 == 0)
                {
                    await backgroundQueue.QueueTask(delegate { _systemDataCollector.GetUserIpAddress(); });
                    _ = Dispatcher.BeginInvoke(new Action(() => { DataContext = new DataSystemViewModel(); }));
                }
                _ = Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (BtnVision.IsChecked.Value & BtnVision.Visibility == Visibility.Hidden & !SystemDataCollector.isIPAddressFormatValid)
                    {
                        IpAddress.Effect.BeginAnimation(BlurEffect.RadiusProperty, FactoryAnimation.CreateTo(0.18, () => { SettingsEngine.IsHiddenIpAddress = false; }));
                    }
                }));
            });
        }

        private void AnimationProgressBars(double cpuValue, double ramValue)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                CPULoad.BeginAnimation(RangeBase.ValueProperty, FactoryAnimation.CreateIn(CPULoad.Value, cpuValue, 0.2, useCubicEase: true));
                RAMLoad.BeginAnimation(RangeBase.ValueProperty, FactoryAnimation.CreateIn(RAMLoad.Value, ramValue, 0.2, useCubicEase: true));
            }));
        }

        private void AnimationPopup()
        {
            PopupCopy.IsOpen = true;
            CopyTextToastBody.BeginAnimation(OpacityProperty, FactoryAnimation.CreateIn(0, 0.9, 0.27, () => { PopupCopy.IsOpen = false; }, true));
            PopupCopy.BeginAnimation(Popup.VerticalOffsetProperty, FactoryAnimation.CreateIn(-20, -50, 0.35, useCubicEase: true));
        }

        private void BtnVision_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SettingsEngine.IsHiddenIpAddress = !BtnVision.IsChecked.Value;
            IpAddress.Effect.BeginAnimation(BlurEffect.RadiusProperty, BtnVision.IsChecked.Value ? FactoryAnimation.CreateTo(0.2) : FactoryAnimation.CreateIn(0, 20, 0.2));
        }

        private void BtnVision_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e?.Key == Key.Space || e?.Key == Key.Enter)
            {
                e.Handled = true;
            }
        }

        private void HandleCopyingData_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e?.LeftButton == MouseButtonState.Pressed)
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

                                if (cumulativeHeight >= e?.GetPosition(textBlock).Y)
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
                    default:
                        break;
                }

                if (!PopupCopy.IsOpen)
                {
                    AnimationPopup();
                }
            }
        }

    }
}
