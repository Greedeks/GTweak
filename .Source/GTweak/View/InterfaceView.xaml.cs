using GTweak.Assets.UserControl;
using GTweak.Utilities.Animation;
using GTweak.Utilities.Configuration;
using GTweak.Utilities.Managers;
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
        private readonly InterfaceTweaks _intfTweaks = new InterfaceTweaks();

        public InterfaceView()
        {
            InitializeComponent();

            if (!WindowsLicense.IsWindowsActivated)
                new NotificationManager().Show("", "info", "warn_activate_notification");

            if (SystemDiagnostics.HardwareData.OSBuild.CompareTo("22621.2361") < 0)
                TglButton21.IsEnabled = false;
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

            if (ExplorerManager.IntfMapping.TryGetValue(toggleButton.Name, out bool needRestart))
                ExplorerManager.Restart(new Process());

            if (NotificationManager.IntfActions.TryGetValue(toggleButton.Name, out string action))
                new NotificationManager(300).Show(action);

            Parallel.Invoke(async delegate { await Task.Delay(1000); _intfTweaks.AnalyzeAndUpdate(this); });
        }

        private void Page_Loaded(object sender, RoutedEventArgs e) => Parallel.Invoke(() => _intfTweaks.AnalyzeAndUpdate(this));

    }
}
