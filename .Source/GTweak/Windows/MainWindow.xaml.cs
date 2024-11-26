using GTweak.Utilities;
using GTweak.Utilities.Helpers;
using GTweak.Utilities.Tweaks;
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

            BtnNotification.StateNA = Settings.IsViewNotification;
            BtnUpdate.StateNA = Settings.IsСheckingUpdate;
            BtnTopMost.StateNA = Topmost = Settings.IsTopMost;
            BtnSoundNtn.IsChecked = Settings.IsPlayingSound;
            SliderVolume.Value = Settings.VolumeNotification;
            LanguageSelectionMenu.SelectedIndex = Settings.Language == "en" ? 0 : 1;
            ThemeSelectionMenu.SelectedIndex = Settings.Theme switch
            {
                "Dark" => 0,
                "Light" => 1,
                _ => 2,
            };

            App.ImportTweaksUpdate += delegate { BtnMore.IsChecked = true; };
            App.ThemeChanged += delegate { this.Close(); new RebootWindow().ShowDialog(); };
        }

        #region Button Title/Animation Window
        private void SettingsMenuAnimation()
        {
            Parallel.Invoke(() =>
            {
                Storyboard storyboard = new Storyboard();
                DoubleAnimation rotateAnimation = new DoubleAnimation()
                {
                    From = 0.0,
                    To = 360,
                    SpeedRatio = 3,
                    EasingFunction = new QuadraticEase(),
                    Duration = TimeSpan.FromSeconds(2)
                };
                DoubleAnimation animation = new DoubleAnimation()
                {
                    From = SettingsMenu.Width != 400 ? 0 : 400,
                    To = SettingsMenu.Width != 400 ? 400 : 0,
                    SpeedRatio = 6,
                    EasingFunction = new QuadraticEase(),
                    Duration = TimeSpan.FromSeconds(2)
                };
                Timeline.SetDesiredFrameRate(animation, 400);
                Timeline.SetDesiredFrameRate(rotateAnimation, 400);
                Storyboard.SetTarget(rotateAnimation, ImageSettings);
                Storyboard.SetTargetProperty(rotateAnimation, new PropertyPath("(UIElement.RenderTransform).(RotateTransform.Angle)"));
                storyboard.Children.Add(rotateAnimation);
                storyboard.Begin();
                SettingsMenu.BeginAnimation(WidthProperty, animation);
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

        private void ButtonMinimized_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) => this.WindowState = WindowState.Minimized;

        private void ButtonExit_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            switch (SystemTweaks.isTweakWorkingAntivirus)
            {
                case false:
                    this.Close();
                    break;
                case true:
                    new ViewNotification().Show("", (string)FindResource("title0_notification"), (string)FindResource("windefclose_notification"));
                    break;
            }
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Closing -= Window_Closing;
            e.Cancel = true;
            DoubleAnimation doubleAnim = new DoubleAnimation(0, (Duration)TimeSpan.FromSeconds(0.1));
            doubleAnim.Completed += delegate { this.Close(); };
            Timeline.SetDesiredFrameRate(doubleAnim, 400);
            BeginAnimation(OpacityProperty, doubleAnim);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DoubleAnimation doubleAnim = new DoubleAnimation()
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new QuadraticEase()
            };
            doubleAnim.Completed += async delegate
            {
                if (SystemData.UtilityСonfiguration.IsNeedUpdate && Settings.IsСheckingUpdate)
                {
                    await Task.Delay(500);
                    new UpdateWindow().ShowDialog();
                }
            };
            Timeline.SetDesiredFrameRate(doubleAnim, 400);
            BeginAnimation(OpacityProperty, doubleAnim);
            new TypewriterAnimation(UtilityTitle.Text, UtilityTitle, TimeSpan.FromSeconds(0.4));
        }
        #endregion

        #region Settings Menu
        private void BtnNotification_ChangedState(object sender, EventArgs e) => Parallel.Invoke(() => { Settings.ChangingParameters(!BtnNotification.State, "Notification"); });

        private void BtnUpdate_ChangedState(object sender, EventArgs e) => Parallel.Invoke(() => { Settings.ChangingParameters(!BtnUpdate.State, "Update"); });

        private void BtnTopMost_ChangedState(object sender, EventArgs e)
        {
            Parallel.Invoke(delegate
            {
                Settings.ChangingParameters(!BtnTopMost.State, "TopMost");
                Topmost = Settings.IsTopMost;
            });
        }

        private void SliderVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SliderVolume.Value = SliderVolume.Value == 0 ? 1 : SliderVolume.Value;
            Parallel.Invoke(() => { Settings.ChangingParameters(SliderVolume.Value, "Volume"); });
        }

        private void BtnSoundNtn_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Parallel.Invoke(() => { Settings.ChangingParameters(!BtnSoundNtn.IsChecked, "Sound"); });
        }

        private void LanguageSelectionMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (LanguageSelectionMenu.SelectedIndex)
            {
                case 0:
                    Settings.ChangingParameters("en", "Language");
                    App.Language = "en";
                    break;
                default:
                    Settings.ChangingParameters("ru", "Language");
                    App.Language = "ru";
                    break;
            }
        }

        private void ThemeSelectionMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (ThemeSelectionMenu.SelectedIndex)
            {
                case 0:
                    Settings.ChangingParameters("Dark", "Theme");
                    App.Theme = "Dark";
                    break;
                case 1:
                    Settings.ChangingParameters("Light", "Theme");
                    App.Theme = "Light";
                    break;
                default:
                    Settings.ChangingParameters("System", "Theme");
                    App.Theme = "System";
                    break;
            }
        }

        private void BtnExport_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) => Parallel.Invoke(Settings.SaveFileConfig);

        private void BtnImport_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) => Parallel.Invoke(Settings.OpenFileConfig);

        private void BtnDelete_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) => Settings.SelfRemoval();

        private void BtnContats_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Image btnConcat = (Image)sender;
            switch (btnConcat.Uid)
            {
                case "git":
                    Process.Start("https://github.com/Greedeks");
                    break;
                case "tg":
                    Process.Start("https://t.me/Greedeks");
                    break;
                case "steam":
                    Process.Start("https://steamcommunity.com/id/greedeks/");
                    break;
            }
        }
        #endregion
    }
}

