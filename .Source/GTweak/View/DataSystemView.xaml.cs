using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using GTweak.Core.Base;
using GTweak.Core.ViewModel;
using GTweak.Utilities.Animation;
using GTweak.Utilities.Configuration;
using GTweak.Utilities.Helpers;
using GTweak.Utilities.Managers;

namespace GTweak.View
{
    public partial class DataSystemView : UserControl, IViewPageBase
    {
        private readonly HardwareProvider _hardwareProvider = new HardwareProvider();
        private readonly BackgroundQueue backgroundQueue = new BackgroundQueue();
        private TimerControlManager _timer = default;

        public DataSystemView()
        {
            InitializeComponent();

            Loaded += delegate
            {
                App.LanguageChanged += OnLanguageChanged;
                StartMonitoringData();
                _timer.Start();
            };
            Unloaded += delegate
            {
                App.LanguageChanged -= OnLanguageChanged;
                _timer.Stop();
            };
        }

        private void OnLanguageChanged(object sender, EventArgs e)
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
                    UpdateDataContext();
                }
            }));
        }

        private void StartMonitoringData()
        {
            CPULoad.EndAngle = HardwareData.Processor.Usage * 3.6;
            RAMLoad.EndAngle = HardwareData.Memory.Usage * 3.6;

            _timer = new TimerControlManager(TimeSpan.Zero, TimerControlManager.TimerMode.CountUp, async time =>
            {
                if ((int)time.TotalSeconds % 2 == 0)
                {
                    await backgroundQueue.QueueTask(async () =>
                    {
                        await Task.WhenAll(_hardwareProvider.GetTotalProcessorUsage(), _hardwareProvider.GetPhysicalAvailableMemory(),
                            _hardwareProvider.GetProcessCount(), _hardwareProvider.GetServicesCount());
                    });

                    _ = Dispatcher.BeginInvoke(new Action(() =>
                    {
                        AnimateArcProgress(CPULoad, HardwareData.Processor.Usage);
                        AnimateArcProgress(RAMLoad, HardwareData.Memory.Usage);
                        UpdateDataContext();
                    }));
                }
                if ((int)time.TotalSeconds % 5 == 0)
                {
                    await backgroundQueue.QueueTask(async delegate { await _hardwareProvider.GetUserIpAddress(); });
                }
            });
        }

        private void UpdateDataContext()
        {
            if (DataContext as DataSystemViewModel is var vm && vm != null)
            {
                vm.Update();
            }
        }

        private void AnimateArcProgress(Wpf.Ui.Controls.Arc arc, double percent) => arc.BeginAnimation(Wpf.Ui.Controls.Arc.EndAngleProperty, FactoryAnimation.CreateIn(arc.EndAngle, percent * 3.6, 0.2, useCubicEase: true));

        private void HandleCopyingData_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkContentElement { IsEnabled: false } || sender is FrameworkElement { IsEnabled: false })
            {
                return;
            }

            if (e?.LeftButton == MouseButtonState.Pressed)
            {
                Clipboard.Clear();
                switch (sender)
                {
                    case TextBlock textBlock:
                        {
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
                    case Run runtext:
                        {
                            Clipboard.SetData(DataFormats.UnicodeText, runtext.Text.Replace('\n', ' '));
                            break;
                        }
                    default:
                        break;
                }

                if (!PopupCopy.IsOpen)
                {
                    PopupCopy.IsOpen = true;
                    CopyTextToastBody.BeginAnimation(OpacityProperty, FactoryAnimation.CreateIn(0, 0.9, 0.27, () => { PopupCopy.IsOpen = false; }, true));
                    PopupCopy.BeginAnimation(Popup.VerticalOffsetProperty, FactoryAnimation.CreateIn(-20, -50, 0.35, useCubicEase: true));
                }
            }
        }
    }
}
