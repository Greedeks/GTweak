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

            App.LanguageChanged += (s, e) => { WorkWithText.TypeWriteAnimation((string)FindResource("defaultDescription"), TextDescription, TimeSpan.FromMilliseconds(0)); };

            if (!SystemData.СomputerСonfiguration.clientWinVersion.Contains("11"))
            {
                TglButton15.IsEnabled = false; TglButton16.IsEnabled = false;
                TglButton17.IsEnabled = false; TglButton18.IsEnabled = false; TglButton19.IsEnabled = false;
            }

            if (WindowsLicense.statusLicense != 1)
            {
                new ViewNotification().Show("", (string)FindResource("title1_notification"), (string)FindResource("viewlicense_notification"));
                TglButton3.IsEnabled = false; TglButton4.IsEnabled = false; TglButton5.IsEnabled = false;
                TglButton6.IsEnabled = false; TglButton7.IsEnabled = false; TglButton8.IsEnabled = false;
            }
        }

        private void Tweak_MouseEnter(object sender, MouseEventArgs e)
        {
            ToggleButton toggleButton = (ToggleButton)sender;

            if (TextDescription.Text != (string)FindResource(toggleButton.Name + "_description_interface"))
            {
                string _descriptionTweak = (string)FindResource(toggleButton.Name + "_description_interface");
                TimeSpan time = _descriptionTweak.Length < 200 ? TimeSpan.FromMilliseconds(400) : TimeSpan.FromMilliseconds(550);
                WorkWithText.TypeWriteAnimation(_descriptionTweak, TextDescription, time);

                if (toggleButton.Name == "TglButton1")
                {
                    TextViewColor.Foreground = new SolidColorBrush(Color.FromArgb(255, 80, 80, 80));
                    TextViewColor.Visibility = Visibility.Visible;
                    WorkWithText.TypeWriteAnimation((string)FindResource("textcolor_interface"), TextViewColor, TimeSpan.FromMilliseconds(350));
                }
                else if (toggleButton.Name == "TglButton2")
                {
                    TextViewColor.Foreground = new SolidColorBrush(Color.FromArgb(255, 240, 255, 255));
                    TextViewColor.Visibility = Visibility.Visible;
                    WorkWithText.TypeWriteAnimation((string)FindResource("textcolor_interface"), TextViewColor, TimeSpan.FromMilliseconds(350));
                }

            }
        }

        private void Tweak_MouseLeave(object sender, MouseEventArgs e)
        {
            if(TextViewColor.Visibility==Visibility.Visible)
                TextViewColor.Visibility=Visibility.Hidden;
            if (TextDescription.Text != (string)FindResource("defaultDescription"))
                WorkWithText.TypeWriteAnimation((string)FindResource("defaultDescription"), TextDescription, TimeSpan.FromMilliseconds(250));
        }

        private async void TglButton_ChangedState(object sender, EventArgs e)
        {
            ToggleButton toggleButton = (ToggleButton)sender;
            Parallel.Invoke(() => InterfaceTweaks.UseInterface(toggleButton.Name, toggleButton.State));
            СonfigSettings.configInterface.Remove(toggleButton.Name);
            СonfigSettings.configInterface.Add(toggleButton.Name, Convert.ToString(toggleButton.State));

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
            WorkWithText.TypeWriteAnimation((string)FindResource("defaultDescription"), TextDescription, TimeSpan.FromMilliseconds(300));
            Parallel.Invoke(() => new InterfaceTweaks().ViewInterface(this));
        }

        private void Page_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.System && (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt)))
                e.Handled = true;
        }
    }
}
