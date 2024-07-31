using GTweak.Assets.UserControl;
using GTweak.Utilities;
using GTweak.Utilities.Helpers;
using GTweak.Utilities.Tweaks;
using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace GTweak.View
{
    public partial class ServicesView : UserControl
    {
        public ServicesView()
        {
            InitializeComponent();

            App.LanguageChanged += (s, e) => { WorkWithText.TypeWriteAnimation((string)FindResource("defaultDescription"), TextDescription, TimeSpan.FromMilliseconds(0)); };
        }

        private void Tweak_MouseEnter(object sender, MouseEventArgs e)
        {
            ToggleButton toggleButton = (ToggleButton)sender;

            if (TextDescription.Text != (string)FindResource(toggleButton.Name + "_description_services"))
            {
                string _descriptionTweak = (string)FindResource(toggleButton.Name + "_description_services");
                TimeSpan time = _descriptionTweak.Length < 200 ? TimeSpan.FromMilliseconds(400) : TimeSpan.FromMilliseconds(550);
                WorkWithText.TypeWriteAnimation(_descriptionTweak, TextDescription, time);
            }
        }

        private void Tweak_MouseLeave(object sender, MouseEventArgs e)
        {
            if (TextDescription.Text != (string)FindResource("defaultDescription"))
                WorkWithText.TypeWriteAnimation((string)FindResource("defaultDescription"), TextDescription, TimeSpan.FromMilliseconds(250));
        }

        private async void TglButton_ChangedState(object sender, EventArgs e)
        {
            ToggleButton toggleButton = (ToggleButton)sender;
            Parallel.Invoke(() => ServicesTweaks.UseServices(toggleButton.Name, toggleButton.State));
            СonfigSettings.configServices.Remove(toggleButton.Name);
            СonfigSettings.configServices.Add(toggleButton.Name, Convert.ToString(toggleButton.State));
            await Task.Delay(100);
            new ViewNotification().Show("restart");
            await Task.Delay(350);
            Parallel.Invoke(() => new ServicesTweaks().ViewServices(this));
        }

        private void Page_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            WorkWithText.TypeWriteAnimation((string)FindResource("defaultDescription"), TextDescription, TimeSpan.FromMilliseconds(300));
            Parallel.Invoke(() => new ServicesTweaks().ViewServices(this));
        }

        private void Page_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.System && (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt)))
                e.Handled = true;
        }
    }
}
