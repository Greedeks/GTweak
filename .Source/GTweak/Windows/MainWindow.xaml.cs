using GTweak.Utilities.Animation;
using GTweak.Utilities.Configuration;
using GTweak.Utilities.Controls;
using GTweak.Windows;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
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

            BtnNotification.StateNA = SettingsEngine.IsViewNotification;
            BtnUpdate.StateNA = SettingsEngine.IsUpdateCheckRequired;
            BtnTopMost.StateNA = Topmost = SettingsEngine.IsTopMost;
            BtnSoundNtn.IsChecked = SettingsEngine.IsPlayingSound;
            SliderVolume.Value = SettingsEngine.Volume;
            LanguageSelectionMenu.SelectedIndex = GetSelectedIndex(SettingsEngine.Language, "en", SettingsEngine.AvailableLangs);
            ThemeSelectionMenu.SelectedIndex = GetSelectedIndex(SettingsEngine.Theme, "Dark", SettingsEngine.AvailableThemes);

            App.ImportTweaksUpdate += delegate { BtnMore.IsChecked = true; };
            App.ThemeChanged += delegate { Close(); new RebootWindow().ShowDialog(); };
        }

        private int GetSelectedIndex(string value, string defaultValue, params string[] listing)
        {
            int index = Array.IndexOf(listing, value);
            return index >= 0 ? index : Array.IndexOf(listing, defaultValue);
        }

        #region Button Title/Animation Window
        private void ButtonHelp_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) => Process.Start(new ProcessStartInfo("https://github.com/Greedeks/GTweak/issues/new/choose") { UseShellExecute = true });

        private void SettingsMenuAnimation()
        {
            Dispatcher.Invoke(() =>
            {
                Storyboard storyboard = new Storyboard();

                DoubleAnimation rotateAnimation = new DoubleAnimation()
                {
                    From = 0.0,
                    To = 360,
                    EasingFunction = new QuadraticEase(),
                    SpeedRatio = 2.0,
                    Duration = TimeSpan.FromSeconds(1)
                };

                DoubleAnimation widthAnimation = new DoubleAnimation()
                {
                    From = SettingsMenu.Width != 400 ? 0 : 400,
                    To = SettingsMenu.Width != 400 ? 400 : 0,
                    EasingFunction = new QuadraticEase(),
                    SpeedRatio = 2.0,
                    Duration = TimeSpan.FromSeconds(1)
                };

                Timeline.SetDesiredFrameRate(rotateAnimation, 240);
                Timeline.SetDesiredFrameRate(widthAnimation, 240);

                Storyboard.SetTarget(rotateAnimation, ImageSettings);
                Storyboard.SetTargetProperty(rotateAnimation, new PropertyPath("(UIElement.RenderTransform).(RotateTransform.Angle)"));

                storyboard.Children.Add(rotateAnimation);
                storyboard.Begin();
                SettingsMenu.BeginAnimation(WidthProperty, widthAnimation);
            });
        }

        private void SettingsMenu_QueryCursor(object sender, QueryCursorEventArgs e)
        {
            if (SettingsMenu.Width == 400)
                SettingsMenuAnimation();
        }

        private void ButtonSettings_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (SettingsMenu.Width == 0 || SettingsMenu.Width == 400)
                SettingsMenuAnimation();
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
            BeginAnimation(OpacityProperty, FadeAnimation.FadeTo(0.1, () => { Close(); }));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            BeginAnimation(OpacityProperty, FadeAnimation.FadeIn(1, 0.3,
                async () =>
                {
                    if (SystemDiagnostics.IsNeedUpdate && SettingsEngine.IsUpdateCheckRequired)
                    {
                        await Task.Delay(500);
                        new UpdateWindow().ShowDialog();
                    }
                }));
            new TypewriterAnimation(UtilityTitle.Text, UtilityTitle, TimeSpan.FromSeconds(0.4));
        }
        #endregion

        #region Settings Menu
        private void BtnNotification_ChangedState(object sender, EventArgs e) => SettingsEngine.IsViewNotification = !BtnNotification.State;

        private void BtnUpdate_ChangedState(object sender, EventArgs e) => SettingsEngine.IsUpdateCheckRequired = !BtnUpdate.State;

        private void BtnTopMost_ChangedState(object sender, EventArgs e)
        {
            SettingsEngine.IsTopMost = !BtnTopMost.State;
            Topmost = !BtnTopMost.State;
        }

        private void BtnSoundNtn_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) => SettingsEngine.IsPlayingSound = (bool)!BtnSoundNtn.IsChecked;

        private void SliderVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SliderVolume.Value = SliderVolume.Value == 0 ? 1 : SliderVolume.Value;
            SettingsEngine.Volume = (int)SliderVolume.Value;
            SettingsEngine.waveOutSetVolume(IntPtr.Zero, ((uint)(double)(ushort.MaxValue / 100 * SliderVolume.Value) & 0x0000ffff) | ((uint)(double)(ushort.MaxValue / 100 * SliderVolume.Value) << 16));
        }

        private void LanguageSelectionMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedLang = SettingsEngine.AvailableLangs.ElementAtOrDefault(LanguageSelectionMenu.SelectedIndex) ?? SettingsEngine.AvailableLangs.Last();
            SettingsEngine.Language = selectedLang;
            App.Language = selectedLang;
        }

        private void ThemeSelectionMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedTheme = SettingsEngine.AvailableThemes.ElementAtOrDefault(ThemeSelectionMenu.SelectedIndex) ?? SettingsEngine.AvailableThemes.Last();
            SettingsEngine.Theme = selectedTheme;
            App.Theme = selectedTheme;
        }

        private void BtnExport_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) => SettingsEngine.SaveFileConfig();

        private void BtnImport_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) => SettingsEngine.OpenFileConfig();

        private void BtnDelete_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) => SettingsEngine.SelfRemoval();

        private void BtnContats_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
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

