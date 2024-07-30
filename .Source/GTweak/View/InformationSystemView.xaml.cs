using GTweak.Core.ViewModel;
using GTweak.Utilities;
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

            App.LanguageChanged += (s, e) => 
            {
                if (SystemData.СomputerСonfiguration.isNoInternetConnection)
                {
                    SystemData.СomputerСonfiguration.СonfigurationData["IpAddress"] = (string)FindResource("connection_systemInformation");
                    DataContext = new InformationSystemVM();
                }
                else if (SystemData.СomputerСonfiguration.isInternetLimited)
                {
                    SystemData.СomputerСonfiguration.СonfigurationData["IpAddress"] = (string)FindResource("limited_systemInformation");
                    DataContext = new InformationSystemVM();
                }
            };

            timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
            {
                if (time.TotalSeconds % 2 == 0)
                {
                    BackgroundWorker backgroundWorker = new BackgroundWorker();
                    backgroundWorker.RunWorkerAsync();
                    backgroundWorker.DoWork += (s, e) => 
                    { 
                        Parallel.Invoke(SystemData.СomputerСonfiguration.UpdatingDeviceData);
                        new Thread(() => new SystemData.MonitoringSystem().GetCpuUsage()).Start();
                        new Thread(() => new SystemData.MonitoringSystem().GetCpuUsage()).IsBackground = true;
                    };
                    backgroundWorker.RunWorkerCompleted += (s, e) => 
                    {
                        DataContext = new InformationSystemVM();
                        new Thread(() => new SystemData.MonitoringSystem().CountProcess.ToString()).Start();
                        new Thread(() => new SystemData.MonitoringSystem().CountProcess.ToString()).IsBackground = true;
                        Application.Current.Dispatcher.Invoke(ProgressBarAnim); 
                    };
                }
                else if (time.TotalSeconds % 5 == 0)
                {
                    BackgroundWorker backgroundWorker = new BackgroundWorker();
                    backgroundWorker.RunWorkerAsync();
                    backgroundWorker.DoWork += (s, e) => { SystemData.СomputerСonfiguration.GetUserIP(); };
                    backgroundWorker.RunWorkerCompleted += (s, e) => DataContext = new InformationSystemVM();
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
            Parallel.Invoke(() =>  {
                DoubleAnimation doubleAnim = new DoubleAnimation()
                {
                    From = CPULoad.Value,
                    To = SystemData.MonitoringSystem.CpuUsage,
                    Duration = TimeSpan.FromSeconds(0.2)
                };
                Timeline.SetDesiredFrameRate(doubleAnim, 400);
                CPULoad.BeginAnimation(ProgressBar.ValueProperty, doubleAnim);


                doubleAnim = new DoubleAnimation()
                {
                    From = RAMLoad.Value,
                    To = new SystemData.MonitoringSystem().RamUsage,
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
            Point _currentPos = this.PointToScreen(Mouse.GetPosition(this));
            PopupCopy.Placement = PlacementMode.Absolute;
            PopupCopy.HorizontalOffset = _currentPos.X + 10;
            PopupCopy.VerticalOffset = _currentPos.Y - 30;

        }

        private void Page_Unloaded(object sender, RoutedEventArgs e) => timer.Stop();

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() => { CPULoad.Value = SystemData.MonitoringSystem.CpuUsage; });
            Application.Current.Dispatcher.Invoke(() => { RAMLoad.Value = new SystemData.MonitoringSystem().RamUsage; });
        }

        private void Page_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.System && (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt)))
                e.Handled = true;
        }
    }
}
