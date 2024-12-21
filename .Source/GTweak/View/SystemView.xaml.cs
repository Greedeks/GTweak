using GTweak.Assets.UserControl;
using GTweak.Utilities.Control;
using GTweak.Utilities.Helpers;
using GTweak.Utilities.Tweaks;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GTweak.View
{
    public partial class SystemView : UserControl
    {
        public SystemView()
        {
            InitializeComponent();

            App.LanguageChanged += delegate { new TypewriterAnimation((string)FindResource("defaultDescription"), TextDescription, TimeSpan.FromMilliseconds(0)); };
        }

        private void Tweak_MouseEnter(object sender, MouseEventArgs e)
        {
            ToggleButton toggleButton = (ToggleButton)sender;
            string _descriptionTweak = (string)FindResource(toggleButton.Name + "_description_system");

            if (TextDescription.Text != _descriptionTweak)
                new TypewriterAnimation(_descriptionTweak, TextDescription, _descriptionTweak.Length < 200 ? TimeSpan.FromMilliseconds(400) : TimeSpan.FromMilliseconds(550));
        }

        private void TweakSlider_MouseEnter(object sender, MouseEventArgs e)
        {
            StackPanel stackPanel = (StackPanel)sender;
            string _descriptionTweak = (string)FindResource(stackPanel.Name + "_description_system");

            if (TextDescription.Text != (string)FindResource(stackPanel.Name + "_description_system"))
                new TypewriterAnimation(_descriptionTweak, TextDescription, _descriptionTweak.Length < 200 ? TimeSpan.FromMilliseconds(400) : TimeSpan.FromMilliseconds(550));
        }

        private void Tweak_MouseLeave(object sender, MouseEventArgs e)
        {
            if (TextDescription.Text != (string)FindResource("defaultDescription"))
                new TypewriterAnimation((string)FindResource("defaultDescription"), TextDescription, TimeSpan.FromMilliseconds(250));
        }

        private void Sliders_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            Slider slider = (Slider)sender;
            INIManager.UserTweaksSystem.Remove(slider.Name);
            INIManager.UserTweaksSystem.Add(slider.Name, Convert.ToString(slider.Value));
        }

        private void Sliders_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider slider = (Slider)sender;
            SystemTweaks.UseSystemSliders(slider.Name, (uint)slider.Value);
        }

        private void TglButton_ChangedState(object sender, EventArgs e)
        {
            ToggleButton toggleButton = (ToggleButton)sender;
            if (toggleButton.Name != "TglButton8")
            {
                Parallel.Invoke(() => SystemTweaks.UseSystem(toggleButton.Name, toggleButton.State));

                switch (toggleButton.Name)
                {
                    case "TglButton7":
                    case "TglButton9":
                    case "TglButton10":
                    case "TglButton12":
                    case "TglButton13":
                    case "TglButton14":
                    case "TglButton15":
                    case "TglButton20":
                        new ViewNotification(300).Show("restart");
                        break;
                    case "TglButton2":
                        new ViewNotification(300).Show("logout");
                        break;
                }

                Parallel.Invoke(async delegate { await Task.Delay(500); new SystemTweaks().ViewSystem(this); });
            }
            else
            {
                if (!SystemTweaks.isTweakWorkingAntivirus)
                {
                    SystemTweaks.isTweakWorkingAntivirus = true;
                    INIManager.UserTweaksSystem.Remove(toggleButton.Name);
                    INIManager.UserTweaksSystem.Add(toggleButton.Name, Convert.ToString(toggleButton.State));
                    SystemTweaks.UseSystem(toggleButton.Name, toggleButton.State);
                    new ViewNotification().Show("", "info", (string)FindResource("windefender_notification"));
                }
                else if (SystemTweaks.isTweakWorkingAntivirus)
                    new ViewNotification().Show("", "warn", (string)FindResource("warningwindef_notification"));
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            new TypewriterAnimation((string)FindResource("defaultDescription"), TextDescription, TimeSpan.FromMilliseconds(300));
            Parallel.Invoke(() => new SystemTweaks().ViewSystem(this));
        }
    }
}
