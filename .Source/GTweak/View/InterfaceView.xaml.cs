using GTweak.Assets.UserControl;
using GTweak.Utilities;
using GTweak.Utilities.Helpers;
using GTweak.Utilities.Tweaks;
using System;
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

            App.LanguageChanged += delegate { new TypewriterAnimation((string)FindResource("defaultDescription"), TextDescription, TimeSpan.FromMilliseconds(0)); };

            if (WindowsLicense.statusLicense != 1)
                new ViewNotification().Show("", (string)Application.Current.Resources["title1_notification"], (string)Application.Current.Resources["viewlicense_notification"]);
        }

        private void Tweak_MouseEnter(object sender, MouseEventArgs e)
        {
            ToggleButton toggleButton = (ToggleButton)sender;

            if (TextDescription.Text != (string)FindResource(toggleButton.Name + "_description_interface"))
            {
                string _descriptionTweak = (string)FindResource(toggleButton.Name + "_description_interface");
                TimeSpan time = _descriptionTweak.Length < 200 ? TimeSpan.FromMilliseconds(400) : TimeSpan.FromMilliseconds(550);
                new TypewriterAnimation(_descriptionTweak, TextDescription, time);

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

            }
        }

        private void Tweak_MouseLeave(object sender, MouseEventArgs e)
        {
            if (TextViewColor.Visibility==Visibility.Visible)
                TextViewColor.Visibility=Visibility.Hidden;
            if (TextDescription.Text != (string)FindResource("defaultDescription"))
                new TypewriterAnimation((string)FindResource("defaultDescription"), TextDescription, TimeSpan.FromMilliseconds(250));
        }

        private async void TglButton_ChangedState(object sender, EventArgs e)
        {
            ToggleButton toggleButton = (ToggleButton)sender;
            Parallel.Invoke(() => InterfaceTweaks.UseInterface(toggleButton.Name, toggleButton.State));

            switch (toggleButton.Name)
            {
                case "TglButton1":
                case "TglButton2":
                case "TglButton3":
                case "TglButton4":
                case "TglButton5":
                case "TglButton10":
                case "TglButton11":
                case "TglButton12":
                case "TglButton26":
                case "TglButton27":
                    new ViewNotification().Show("logout");
                    break;
                case "TglButton22":
                    new ViewNotification().Show("restart");
                    break;
            }

            await Task.Delay(350);
            Parallel.Invoke(() => new InterfaceTweaks().ViewInterface(this));
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            new TypewriterAnimation((string)FindResource("defaultDescription"), TextDescription, TimeSpan.FromMilliseconds(300));
            Parallel.Invoke(() => new InterfaceTweaks().ViewInterface(this));
        }
    }
}
