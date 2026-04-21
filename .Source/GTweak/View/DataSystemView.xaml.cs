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
            CPULoad.EndAngle = Math.Min(HardwareData.Processor.Usage * 3.6, 359.9);
            RAMLoad.EndAngle = Math.Min(HardwareData.Memory.Usage * 3.6, 359.9);

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

        private void AnimateArcProgress(Wpf.Ui.Controls.Arc arc, double percent) => arc.BeginAnimation(Wpf.Ui.Controls.Arc.EndAngleProperty, FactoryAnimation.CreateIn(arc.EndAngle, Math.Min(percent * 3.6, 359.9), 0.2, useCubicEase: true));

        private void HandleCopyingData_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            object source = e.OriginalSource;

            if (source is FrameworkContentElement { IsEnabled: false } || source is FrameworkElement { IsEnabled: false } || e?.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }

            string textToProcess = string.Empty;
            TextBlock parentTextBlock = null;
            double startY = 0;

            if (source is Run runtext && runtext.Parent is TextBlock tb)
            {
                textToProcess = runtext.Text;
                parentTextBlock = tb;
                startY = runtext.ContentStart.GetCharacterRect(LogicalDirection.Forward).Y;
            }
            else if (source is TextBlock textBlock)
            {
                textToProcess = textBlock.Text;
                parentTextBlock = textBlock;
                startY = 0;
            }

            if (parentTextBlock != null && !string.IsNullOrEmpty(textToProcess))
            {
                double clickY = e.GetPosition(parentTextBlock).Y;
                string selectedLine = GetClickedLine(textToProcess, parentTextBlock, startY, clickY);

                if (!string.IsNullOrEmpty(selectedLine))
                {
                    Clipboard.Clear();
                    Clipboard.SetData(DataFormats.UnicodeText, selectedLine);

                    e.Handled = true;

                    if (!PopupCopy.IsOpen)
                    {
                        PopupCopy.IsOpen = true;
                        CopyTextToastBody.BeginAnimation(OpacityProperty, FactoryAnimation.CreateIn(0, 0.9, 0.27, () => { PopupCopy.IsOpen = false; }, true));
                        PopupCopy.BeginAnimation(Popup.VerticalOffsetProperty, FactoryAnimation.CreateIn(-20, -50, 0.35, useCubicEase: true));
                    }
                }
            }
        }

        private string GetClickedLine(string text, TextBlock textBlock, double startY, double clickY)
        {
            double cumulativeHeight = startY;

            Typeface typeface = new Typeface(textBlock.FontFamily, textBlock.FontStyle, textBlock.FontWeight, textBlock.FontStretch);
            double pixelsPerDip = VisualTreeHelper.GetDpi(textBlock).PixelsPerDip;

            string[] lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {
                FormattedText formattedText = new FormattedText(line, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, textBlock.FontSize, textBlock.Foreground, pixelsPerDip);
                cumulativeHeight += formattedText.Height;

                if (cumulativeHeight >= clickY)
                {
                    return line;
                }
            }

            return string.Empty;
        }
    }
}
