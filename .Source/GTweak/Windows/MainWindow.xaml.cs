using GTweak.Utilities;
using GTweak.Utilities.Tweaks;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
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

            BtnNotification.State = Settings.IsViewNotification;
            BtnSoundNtn.State = Settings.IsSoundNotification;
            BtnTopMost.State = Topmost = Settings.IsTopMost;
            SliderVolume.Value = Settings.VolumeNotification;
            LanguageSelectionMenu.SelectedIndex = Settings.Language == "en" ? 0 : 1;

            App.ImportTweaksUpdate += (s, e) => { BtnMore.IsChecked = true; };
        }

        #region Button Title/Animation Window
        private void SettingsMenuAnimation()
        {
            Parallel.Invoke(() => {
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

        private void ButtonSettings_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed & SettingsMenu.Width == 0 || SettingsMenu.Width == 400)
                SettingsMenuAnimation();
        }

        private void SettingsMenu_QueryCursor(object sender, QueryCursorEventArgs e)
        {
            if (SettingsMenu.Width == 400)
                SettingsMenuAnimation();
        }

        private void ButtonExit_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;

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

        private void ButtonMinimized_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.WindowState = WindowState.Minimized;
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
            doubleAnim.Completed += (s, _) => { this.Close(); };
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
            Timeline.SetDesiredFrameRate(doubleAnim, 400);
            BeginAnimation(OpacityProperty, doubleAnim);
        }
        #endregion

        #region Settings Menu
        private void BtnNotification_ChangedState(object sender, EventArgs e) => Parallel.Invoke(() => { Settings.ChangingParameters(!BtnNotification.State, "Notification"); });

        private void BtnSoundNtn_ChangedState(object sender, EventArgs e) => Parallel.Invoke(() => { Settings.ChangingParameters(!BtnSoundNtn.State, "Sound"); });

        private void BtnTopMost_ChangedState(object sender, EventArgs e)
        {
            Parallel.Invoke(() =>
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

        private void LanguageSelectionMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LanguageSelectionMenu.SelectedIndex == 0)
            {
                Settings.ChangingParameters("en", "Language");
                App.Language = CultureInfo.GetCultureInfo("en-US");
            }
            else
            {
                Settings.ChangingParameters("ru", "Language");
                App.Language = CultureInfo.GetCultureInfo("ru-RU");
            }     
        }

        private void BtnExport_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                Parallel.Invoke(Settings.SaveFileConfig);
        }

        private void BtnImport_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                Parallel.Invoke(Settings.OpenFileConfig);
        }

        private void BtnDelete_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                Settings.SelfRemoval();
        }

        private void BtnContats_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Image btnConcat = (Image)sender;
                switch(btnConcat.Uid)
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
           
        }
        #endregion
    }
}

