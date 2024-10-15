using GTweak.Core.ViewModel;
using GTweak.Utilities;
using GTweak.Utilities.Tweaks;
using System;
using System.ComponentModel;
using System.Threading;
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
    public partial class InformationSystemView : UserControl
    {
        private readonly DispatcherTimer timer = default;
        private TimeSpan time = TimeSpan.FromSeconds(0);

        public InformationSystemView()
        {
            InitializeComponent();

            App.LanguageChanged += delegate
            {
                if (!SystemData.СomputerСonfiguration.isConnectionLose)
                {
                    if (SystemData.СomputerСonfiguration.isInternetLimited)
                    {
                        SystemData.СomputerСonfiguration.СonfigurationData["IpAddress"] = (string)FindResource("limited_systemInformation");
                        DataContext = new InformationSystemVM();
                    }
                    else if (SystemData.СomputerСonfiguration.isConnectionBlock)
                    {
                        SystemData.СomputerСonfiguration.СonfigurationData["IpAddress"] = (string)FindResource("connection_block_systemInformation");
                        DataContext = new InformationSystemVM();
                    }
                }
                else
                {
                    SystemData.СomputerСonfiguration.СonfigurationData["IpAddress"] = (string)FindResource("connection_lose_systemInformation");
                    DataContext = new InformationSystemVM();
                }
            };

            timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
            {
                if (time.TotalSeconds % 2 == 0)
                {
                    BackgroundWorker backgroundWorker = new BackgroundWorker();
                    backgroundWorker.RunWorkerAsync();
                    backgroundWorker.DoWork += delegate
                    {
                        Parallel.Invoke(SystemData.СomputerСonfiguration.UpdatingDeviceData);
                        new Thread(() => new SystemData.MonitoringSystem().GetCpuUsage()).Start();
                        new Thread(() => new SystemData.MonitoringSystem().GetCpuUsage()).IsBackground = true;
                    };
                    backgroundWorker.RunWorkerCompleted += delegate
                    {
                        new Thread(() => new SystemData.MonitoringSystem().CountProcess.ToString()).Start();
                        new Thread(() => new SystemData.MonitoringSystem().CountProcess.ToString()).IsBackground = true;
                        Application.Current.Dispatcher.Invoke(ProgressBarAnim);
                        DataContext = new InformationSystemVM();

                        if (Settings.IsHiddenIpAddress & ImageHidden.Source == default)
                        {
                            DoubleAnimation doubleAnim = new DoubleAnimation(0, (Duration)TimeSpan.FromSeconds(0.18));
                            doubleAnim.Completed += (s, _) => { Parallel.Invoke(() => { Settings.ChangingParameters(false, "HiddenIP"); }); };
                            Timeline.SetDesiredFrameRate(doubleAnim, 400);
                            IpAddress.Effect.BeginAnimation(BlurEffect.RadiusProperty, doubleAnim);
                        }
                    };
                }
                else if (time.TotalSeconds % 5 == 0)
                {
                    BackgroundWorker backgroundWorker = new BackgroundWorker();
                    backgroundWorker.RunWorkerAsync();
                    backgroundWorker.DoWork += delegate { SystemData.СomputerСonfiguration.GetUserIP(); };
                    backgroundWorker.RunWorkerCompleted += delegate { DataContext = new InformationSystemVM(); };
                }

                time = time.Add(TimeSpan.FromSeconds(+1));
            }, Application.Current.Dispatcher);
            timer.Start();
        }

        private void AnimationPopup(bool _reverse)
        {
            PopupCopy.IsOpen = true;
            DoubleAnimation doubleAnim = new DoubleAnimation()
            {
                From = !_reverse ? 0 : 0.9,
                To = !_reverse ? 0.9 : 0,
                SpeedRatio = 10,
                EasingFunction = new QuadraticEase(),
                Duration = TimeSpan.FromSeconds(2)
            };
            doubleAnim.Completed += (s, _) => { if (_reverse) PopupCopy.IsOpen = false; };
            Timeline.SetDesiredFrameRate(doubleAnim, 400);
            CopyTextToastBody.BeginAnimation(ContextMenu.OpacityProperty, doubleAnim);
        }

        private void ImageHidden_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DoubleAnimation doubleAnim = new DoubleAnimation()
                {
                    From = Settings.IsHiddenIpAddress ? 20 : 0,
                    To = Settings.IsHiddenIpAddress ? 0 : 20,
                    EasingFunction = new QuadraticEase(),
                    Duration = TimeSpan.FromSeconds(0.2)
                };
                Timeline.SetDesiredFrameRate(doubleAnim, 400);
                IpAddress.Effect.BeginAnimation(BlurEffect.RadiusProperty, doubleAnim);

                Parallel.Invoke(() => { Settings.ChangingParameters(!Settings.IsHiddenIpAddress, "HiddenIP"); });
                DataContext = new InformationSystemVM();
            }
        }

        private async void TextBlock_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && e.ClickCount >= 2)
            {
                TextBlock textBlock = (TextBlock)sender;
                Clipboard.Clear();
                Clipboard.SetData(DataFormats.UnicodeText, textBlock.Text.Replace('\n', ' '));

                if (!PopupCopy.IsOpen)
                {
                    AnimationPopup(false);
                    await Task.Delay(400);
                    AnimationPopup(true);
                }
            }
        }

        private void ProgressBarAnim()
        {
            Parallel.Invoke(() =>
            {
                DoubleAnimation doubleAnim = new DoubleAnimation()
                {
                    From = CPULoad.Value,
                    To = SystemData.MonitoringSystem.CpuUsage,
                    EasingFunction = new QuadraticEase(),
                    Duration = TimeSpan.FromSeconds(0.2)
                };
                Timeline.SetDesiredFrameRate(doubleAnim, 400);
                CPULoad.BeginAnimation(ProgressBar.ValueProperty, doubleAnim);


                doubleAnim = new DoubleAnimation()
                {
                    From = RAMLoad.Value,
                    To = new SystemData.MonitoringSystem().RamUsage,
                    EasingFunction = new QuadraticEase(),
                    Duration = TimeSpan.FromSeconds(0.2)
                };
                Timeline.SetDesiredFrameRate(doubleAnim, 400);
                RAMLoad.BeginAnimation(ProgressBar.ValueProperty, doubleAnim);
            });
        }

        private async void Run_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && e.ClickCount >= 2)
            {
                Run runtext = (Run)sender;
                Clipboard.Clear();
                Clipboard.SetData(DataFormats.UnicodeText, runtext.Text.Replace('\n', ' '));

                if (!PopupCopy.IsOpen)
                {
                    AnimationPopup(false);
                    await Task.Delay(400);
                    AnimationPopup(true);
                }
            }
        }

        private void Page_MouseMove(object sender, MouseEventArgs e)
        {
            Point currentPos = this.PointToScreen(Mouse.GetPosition(this));
            PopupCopy.Placement = PlacementMode.Absolute;
            PopupCopy.HorizontalOffset = currentPos.X + 10;
            PopupCopy.VerticalOffset = currentPos.Y - 30;

        }

        private void Page_Unloaded(object sender, RoutedEventArgs e) => timer.Stop();

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() => { CPULoad.Value = SystemData.MonitoringSystem.CpuUsage; });
            Application.Current.Dispatcher.Invoke(() => { RAMLoad.Value = new SystemData.MonitoringSystem().RamUsage; });
        }
    }
}
