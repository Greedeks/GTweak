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
                switch (SystemData.СomputerСonfiguration.ConnectionStatus)
                {
                    case 1:
                        SystemData.СomputerСonfiguration.СonfigurationData["IpAddress"] = (string)FindResource("connection_lose_systemInformation");
                        break;
                    case 2:
                        SystemData.СomputerСonfiguration.СonfigurationData["IpAddress"] = (string)FindResource("connection_block_systemInformation");
                        break;
                    case 3:
                        SystemData.СomputerСonfiguration.СonfigurationData["IpAddress"] = (string)FindResource("limited_systemInformation");
                        break;
                }
                DataContext = new InformationSystemVM();
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
                        Thread _thread = new Thread(() => { new SystemData.MonitoringSystem().GetCpuUsage(); }) { IsBackground = true };
                        _thread.Start();
                    };
                    backgroundWorker.RunWorkerCompleted += delegate
                    {
                        Thread _thread = new Thread(() => { new SystemData.MonitoringSystem().CountProcess.ToString(); })
                        { IsBackground = true };
                        Application.Current.Dispatcher.Invoke(ProgressBarAnim);

                        if (BtnHiddenIP.IsChecked.Value & BtnHiddenIP.Visibility == Visibility.Hidden)
                        {
                            DoubleAnimation doubleAnim = new DoubleAnimation(0, (Duration)TimeSpan.FromSeconds(0.18));
                            doubleAnim.Completed += delegate { Parallel.Invoke(() => { Settings.ChangingParameters(false, "HiddenIP"); }); };
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

        private void AnimationPopup()
        {
            PopupCopy.IsOpen = true;
            DoubleAnimation doubleAnim = new DoubleAnimation()
            {
                From = 0,
                To = 0.9,
                SpeedRatio = 9,
                AutoReverse = true,
                EasingFunction = new QuadraticEase(),
                Duration = TimeSpan.FromSeconds(3),
            };
            doubleAnim.Completed += delegate { PopupCopy.IsOpen = false; };
            Timeline.SetDesiredFrameRate(doubleAnim, 400);
            CopyTextToastBody.BeginAnimation(ContextMenu.OpacityProperty, doubleAnim);

            doubleAnim = new DoubleAnimation()
            {
                From = -20,
                To = -50,
                SpeedRatio = 8,
                EasingFunction = new QuadraticEase(),
                Duration = TimeSpan.FromSeconds(3),
            };
            Timeline.SetDesiredFrameRate(doubleAnim, 400);
            PopupCopy.BeginAnimation(Popup.VerticalOffsetProperty, doubleAnim);
        }

        private void BtnHiddenIP_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Parallel.Invoke(() => { Settings.ChangingParameters(!BtnHiddenIP.IsChecked.Value, "HiddenIP"); });

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

        private void ProgressBarAnim()
        {
            Parallel.Invoke(() =>
            {
                DoubleAnimation doubleAnim = new DoubleAnimation()
                {
                    From = CPULoad.Value,
                    To = SystemData.MonitoringSystem.CpuUsage,
                    EasingFunction = new PowerEase(),
                    Duration = TimeSpan.FromSeconds(0.2)
                };
                Timeline.SetDesiredFrameRate(doubleAnim, 400);
                CPULoad.BeginAnimation(ProgressBar.ValueProperty, doubleAnim);


                doubleAnim = new DoubleAnimation()
                {
                    From = RAMLoad.Value,
                    To = new SystemData.MonitoringSystem().RamUsage,
                    EasingFunction = new PowerEase(),
                    Duration = TimeSpan.FromSeconds(0.2)
                };
                Timeline.SetDesiredFrameRate(doubleAnim, 400);
                RAMLoad.BeginAnimation(ProgressBar.ValueProperty, doubleAnim);
            });
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e) => timer.Stop();

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Thread _thread = new Thread(() => { new SystemData.MonitoringSystem().GetCpuUsage(); })
            { IsBackground = true };

            _thread = new Thread(() => { new SystemData.MonitoringSystem().CountProcess.ToString(); })
            { IsBackground = true };
            Application.Current.Dispatcher.Invoke(() => { CPULoad.Value = SystemData.MonitoringSystem.CpuUsage; });
            Application.Current.Dispatcher.Invoke(() => { RAMLoad.Value = new SystemData.MonitoringSystem().RamUsage; });
        }
    }
}
