using GTweak.Core.ViewModel;
using GTweak.Utilities.Configuration;
using GTweak.Utilities.Control;
using GTweak.Utilities.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Threading;

namespace GTweak.View
{
    public partial class DataSystemView : UserControl
    {
        private readonly DispatcherTimer timer = default;
        private TimeSpan time = TimeSpan.FromSeconds(0);

        public DataSystemView()
        {
            InitializeComponent();

            App.LanguageChanged += delegate
            {
                if (new Dictionary<SystemDiagnostics.ConnectionStatus, string>
                {
                    { SystemDiagnostics.ConnectionStatus.Lose, "connection_lose_systemInformation" },
                    { SystemDiagnostics.ConnectionStatus.Block, "connection_block_systemInformation" },
                    { SystemDiagnostics.ConnectionStatus.Limited, "limited_systemInformation" }
                }.TryGetValue(SystemDiagnostics.CurrentConnection, out string resourceKey))
                {
                    SystemDiagnostics.СonfigurationData["UserIpAddress"] = (string)FindResource(resourceKey);
                    DataContext = new DataSystemVM();
                }
            };

            timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, async delegate
            {
                if (time.TotalSeconds % 2 == 0)
                {
                    BackgroundQueue backgroundQueue = new BackgroundQueue();
                    await backgroundQueue.QueueTask(async delegate
                    {
                        Parallel.Invoke(SystemDiagnostics.GetUpdatingDevices);
                        await MonitoringSystem.GetTotalProcessorUsage();
                    });
                    Application.Current.Dispatcher.Invoke(AnimationProgressBars);
                    if (BtnHiddenIP.IsChecked.Value & BtnHiddenIP.Visibility == Visibility.Hidden)
                    {
                        DoubleAnimation doubleAnim = new DoubleAnimation(0, (Duration)TimeSpan.FromSeconds(0.18));
                        doubleAnim.Completed += delegate { SettingsRepository.ChangingParameters(false, "HiddenIP"); };
                        Timeline.SetDesiredFrameRate(doubleAnim, 400);
                        IpAddress.Effect.BeginAnimation(BlurEffect.RadiusProperty, doubleAnim);
                    }
                }
                else if (time.TotalSeconds % 5 == 0)
                {
                    BackgroundQueue backgroundQueue = new BackgroundQueue();
                    await backgroundQueue.QueueTask(delegate { SystemDiagnostics.GetUserIpAddress(); });
                    DataContext = new DataSystemVM();
                }

                time = time.Add(TimeSpan.FromSeconds(+1));
            }, Application.Current.Dispatcher);
            timer.Start();

            RAMLoad.Value = new MonitoringSystem().GetMemoryUsage();
            CPULoad.Value = MonitoringSystem.GetProcessorUsage;
        }

        #region Animations
        private void AnimationProgressBars()
        {
            Dispatcher.Invoke(() =>
            {
                DoubleAnimation doubleAnim = new DoubleAnimation()
                {
                    From = CPULoad.Value,
                    To = MonitoringSystem.GetProcessorUsage,
                    EasingFunction = new PowerEase(),
                    Duration = TimeSpan.FromSeconds(0.2)
                };
                Timeline.SetDesiredFrameRate(doubleAnim, 400);
                CPULoad.BeginAnimation(ProgressBar.ValueProperty, doubleAnim);


                doubleAnim = new DoubleAnimation()
                {
                    From = RAMLoad.Value,
                    To = new MonitoringSystem().GetMemoryUsage(),
                    EasingFunction = new PowerEase(),
                    Duration = TimeSpan.FromSeconds(0.2)
                };
                Timeline.SetDesiredFrameRate(doubleAnim, 400);
                RAMLoad.BeginAnimation(ProgressBar.ValueProperty, doubleAnim);
            });
        }

        private void AnimationPopup()
        {
            Dispatcher.Invoke(() =>
            {
                PopupCopy.IsOpen = true;

                DoubleAnimation opacityAnim = new DoubleAnimation()
                {
                    From = 0,
                    To = 0.9,
                    SpeedRatio = 9,
                    AutoReverse = true,
                    EasingFunction = new QuadraticEase(),
                    Duration = TimeSpan.FromSeconds(3)
                };
                opacityAnim.Completed += delegate { PopupCopy.IsOpen = false; };
                Timeline.SetDesiredFrameRate(opacityAnim, 400);
                CopyTextToastBody.BeginAnimation(ContextMenu.OpacityProperty, opacityAnim);

                DoubleAnimation offsetAnim = new DoubleAnimation()
                {
                    From = -20,
                    To = -50,
                    SpeedRatio = 8,
                    EasingFunction = new QuadraticEase(),
                    Duration = TimeSpan.FromSeconds(3)
                };
                Timeline.SetDesiredFrameRate(offsetAnim, 400);
                PopupCopy.BeginAnimation(Popup.VerticalOffsetProperty, offsetAnim);
            });
        }
        #endregion

        private void BtnHiddenIP_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SettingsRepository.ChangingParameters(!BtnHiddenIP.IsChecked.Value, "HiddenIP");

            DoubleAnimation doubleAnim = new DoubleAnimation()
            {
                From = BtnHiddenIP.IsChecked.Value ? 20 : 0,
                To = BtnHiddenIP.IsChecked.Value ? 0 : 20,
                EasingFunction = new QuadraticEase(),
                Duration = TimeSpan.FromSeconds(0.2)
            };
            Timeline.SetDesiredFrameRate(doubleAnim, 400);
            IpAddress.Effect.BeginAnimation(BlurEffect.RadiusProperty, doubleAnim);
        }

        private void TextComputerData_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && e.ClickCount >= 2)
            {
                Clipboard.Clear();
                if (sender.GetType().Name == "TextBlock")
                {
                    TextBlock textBlock = (TextBlock)sender;
                    Clipboard.SetData(DataFormats.UnicodeText, textBlock.Text.Replace('\n', ' '));
                }
                else if (sender.GetType().Name == "Run")
                {
                    Run runtext = (Run)sender;
                    Clipboard.SetData(DataFormats.UnicodeText, runtext.Text.Replace('\n', ' '));
                }

                if (!PopupCopy.IsOpen)
                    AnimationPopup();
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e) => timer.Stop();
    }
}
