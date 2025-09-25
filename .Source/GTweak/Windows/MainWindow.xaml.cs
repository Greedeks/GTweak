using GTweak.Utilities.Animation;
using GTweak.Utilities.Configuration;
using GTweak.Utilities.Controls;
using GTweak.Windows;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace GTweak
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            App.TweaksImported += delegate { BtnMore.IsChecked = true; };
            App.ThemeChanged += delegate { Close(); new RebootWindow().ShowDialog(); };
        }

        #region Button Title/Animation Window
        private void ButtonHelp_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) => Process.Start(new ProcessStartInfo("https://github.com/Greedeks/GTweak/issues/new/choose") { UseShellExecute = true });

        private void AnimationSettingsMenu()
        {
            DoubleAnimation rotateAnimation = FactoryAnimation.CreateIn(0, 360, 0.5);

            Storyboard storyboard = new Storyboard();
            Storyboard.SetTarget(rotateAnimation, ImageSettings);
            Storyboard.SetTargetProperty(rotateAnimation, new PropertyPath("(UIElement.RenderTransform).(RotateTransform.Angle)"));

            storyboard.Children.Add(rotateAnimation);
            storyboard.Begin();
            SettingsMenu.BeginAnimation(WidthProperty, FactoryAnimation.CreateIn(SettingsMenu.Width != 400 ? 0 : 400, SettingsMenu.Width != 400 ? 400 : 0, 0.5));
        }

        private void SettingsMenu_QueryCursor(object sender, QueryCursorEventArgs e)
        {
            if (SettingsMenu.Width == 400)
                AnimationSettingsMenu();
        }

        private void ButtonSettings_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (SettingsMenu.Width == 0 || SettingsMenu.Width == 400)
                AnimationSettingsMenu();
        }

        private void ButtonMinimized_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) => WindowState = WindowState.Minimized;

        private void ButtonExit_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) => Close();

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Closing -= Window_Closing;
            e.Cancel = true;
            BeginAnimation(OpacityProperty, FactoryAnimation.CreateTo(0.1, () => { Close(); }));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            BeginAnimation(OpacityProperty, FactoryAnimation.CreateIn(0, 1, 0.3,
                async () =>
                {
                    if (SystemDiagnostics.IsNeedUpdate && SettingsEngine.IsUpdateCheckRequired)
                    {
                        await Task.Delay(500);
                        Dispatcher.Invoke(() => new UpdateWindow().ShowDialog());
                    }
                }));
            new TypewriterAnimation(UtilityTitle.Text, UtilityTitle, TimeSpan.FromSeconds(0.4));
        }
        #endregion

        #region Settings Menu
        private void BtnNotification_ChangedState(object sender, EventArgs e) => SettingsEngine.IsViewNotification = !BtnNotification.State;

        private void BtnUpdate_ChangedState(object sender, EventArgs e) => SettingsEngine.IsUpdateCheckRequired = !BtnUpdate.State;

        private void BtnTopMost_ChangedState(object sender, EventArgs e) => SettingsEngine.IsTopMost = Topmost = !BtnTopMost.State;

        private void BtnSoundNtn_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) => SettingsEngine.IsPlayingSound = (bool)!BtnSoundNtn.IsChecked;

        private void SliderVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SliderVolume.Value = SliderVolume.Value == 0 ? 1 : SliderVolume.Value;
            SettingsEngine.Volume = (int)SliderVolume.Value;
            SettingsEngine.waveOutSetVolume(IntPtr.Zero, ((uint)(double)(ushort.MaxValue / 100 * SliderVolume.Value) & 0x0000ffff) | ((uint)(double)(ushort.MaxValue / 100 * SliderVolume.Value) << 16));
        }

        private void BtnExport_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) => SettingsEngine.SaveFileConfig();

        private void BtnImport_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) => SettingsEngine.OpenFileConfig();

        private void BtnDelete_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) => SettingsEngine.SelfRemoval();

        private void BtnContacts_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Process.Start(new ProcessStartInfo(((Image)sender).Uid switch
            {
                "git" => "https://github.com/Greedeks",
                "tg" => "https://t.me/Greedeks",
                _ => "https://steamcommunity.com/id/greedeks/"
            })
            { UseShellExecute = true });
        }
        #endregion
    }
}

