using GTweak.Assets.UserControl;
using GTweak.Utilities.Control;
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
        }

        private void Tweak_MouseEnter(object sender, MouseEventArgs e)
        {
            string descriptionTweak = (string)FindResource(((ToggleButton)sender).Name + "_description_system");

            if (CommentTweak.Text != descriptionTweak)
                CommentTweak.Text = descriptionTweak;
        }

        private void TweakSlider_MouseEnter(object sender, MouseEventArgs e)
        {
            string descriptionTweak = (string)FindResource(((StackPanel)sender).Name + "_description_system");

            if (CommentTweak.Text != descriptionTweak)
                CommentTweak.Text = descriptionTweak;
        }

        private void Tweak_MouseLeave(object sender, MouseEventArgs e)
        {
            if (CommentTweak.Text != (string)FindResource("defaultDescription"))
                CommentTweak.Text = (string)FindResource("defaultDescription");
        }

        private void Sliders_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) => SystemTweaks.ApplyTweaksSlider(((Slider)sender).Name, (uint)((Slider)sender).Value);

        private void TglButton_ChangedState(object sender, EventArgs e)
        {
            ToggleButton toggleButton = (ToggleButton)sender;
            if (toggleButton.Name != "TglButton8")
            {
                SystemTweaks.ApplyTweaks(toggleButton.Name, toggleButton.State);

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

                Parallel.Invoke(async delegate { await Task.Delay(1000); new SystemTweaks().AnalyzeAndUpdate(this); });
            }
            else
            {
                if (!SystemTweaks.isTweakWorkingAntivirus)
                {
                    SystemTweaks.isTweakWorkingAntivirus = true;
                    SystemTweaks.ApplyTweaks(toggleButton.Name, toggleButton.State);
                    new ViewNotification().Show("", "info", (string)FindResource("windefender_notification"));
                }
                else if (SystemTweaks.isTweakWorkingAntivirus)
                    new ViewNotification().Show("", "warn", (string)FindResource("warningwindef_notification"));
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e) => Parallel.Invoke(() => new SystemTweaks().AnalyzeAndUpdate(this));
    }
}
