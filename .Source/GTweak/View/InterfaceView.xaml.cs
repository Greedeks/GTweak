using GTweak.Assets.UserControl;
using GTweak.Utilities.Control;
using GTweak.Utilities.Helpers;
using GTweak.Utilities.Helpers.Animation;
using GTweak.Utilities.Helpers.Storage;
using GTweak.Utilities.Tweaks;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace GTweak.View
{
    public partial class InterfaceView : UserControl
    {
        public InterfaceView()
        {
            InitializeComponent();

            if (!WindowsLicense.IsWindowsActivated)
                new ViewNotification().Show("", "info", "viewlicense_notification");
        }

        private void Tweak_MouseEnter(object sender, MouseEventArgs e)
        {
            ToggleButton toggleButton = (ToggleButton)sender;
            string descriptionTweak = (string)FindResource(toggleButton.Name + "_description_interface");

            if (CommentTweak.Text != descriptionTweak)
            {
                CommentTweak.Text = descriptionTweak;

                if (toggleButton.Name == "TglButton1")
                {
                    TextViewColor.Foreground = new SolidColorBrush(Color.FromArgb(255, 80, 80, 80));
                    TextViewColor.Visibility = Visibility.Visible;
                    new TypewriterAnimation((string)FindResource("textcolor_interface"), TextViewColor, TimeSpan.FromMilliseconds(350));
                }
                else if (toggleButton.Name == "TglButton2")
                {
                    TextViewColor.Foreground = new SolidColorBrush(Color.FromArgb(255, 240, 255, 255));
                    TextViewColor.Visibility = Visibility.Visible;
                    new TypewriterAnimation((string)FindResource("textcolor_interface"), TextViewColor, TimeSpan.FromMilliseconds(350));
                }
                else if (toggleButton.Name == "TglButton4")
                    PreviewFlick.Visibility = Visibility.Visible;

            }
        }

        private void Tweak_MouseLeave(object sender, MouseEventArgs e)
        {
            if (TextViewColor.Visibility == Visibility.Visible)
                TextViewColor.Visibility = Visibility.Hidden;
            else if (PreviewFlick.Visibility == Visibility.Visible)
                PreviewFlick.Visibility = Visibility.Hidden;
            if (CommentTweak.Text != (string)FindResource("defaultDescription"))
                CommentTweak.Text = (string)FindResource("defaultDescription");
        }

        private void TglButton_ChangedState(object sender, EventArgs e)
        {
            ToggleButton toggleButton = (ToggleButton)sender;
            InterfaceTweaks.ApplyTweaks(toggleButton.Name, toggleButton.State);

            if (ExplorerManager.GetIntfStorage.TryGetValue(toggleButton.Name, out bool needRestart))
                ExplorerManager.Restart(new Process());

            if (NotifActionsStorage.GetIntfActions.TryGetValue(toggleButton.Name, out string action))
                new ViewNotification(300).Show(action);

            Parallel.Invoke(async delegate { await Task.Delay(1000); new InterfaceTweaks().AnalyzeAndUpdate(this); });
        }

        private void Page_Loaded(object sender, RoutedEventArgs e) => Parallel.Invoke(() => new InterfaceTweaks().AnalyzeAndUpdate(this));

    }
}
