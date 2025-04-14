using GTweak.Assets.UserControl;
using GTweak.Utilities.Controls;
using GTweak.Utilities.Helpers.Storage;
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

                if (NotifActionsStorage.SysNotifActions.TryGetValue(toggleButton.Name, out string action))
                    new ViewNotification(300).Show(action);

                Parallel.Invoke(async delegate { await Task.Delay(1000); new SystemTweaks().AnalyzeAndUpdate(this); });
            }
            else
            {
                if (!SystemTweaks.isTweakWorkingAntivirus)
                {
                    SystemTweaks.isTweakWorkingAntivirus = true;
                    SystemTweaks.ApplyTweaks(toggleButton.Name, toggleButton.State);
                    new ViewNotification().Show("", "info", "windefender_notification");
                }
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e) => Parallel.Invoke(() => new SystemTweaks().AnalyzeAndUpdate(this));
    }
}
