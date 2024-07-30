using GTweak.Assets.UserControl;
using GTweak.Utilities;
using GTweak.Utilities.Helpers;
using GTweak.Utilities.Tweaks;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GTweak.View
{
    public partial class ConfidentialityView : UserControl
    {
        public ConfidentialityView()
        {
            InitializeComponent();

            App.LanguageChanged += (s, e) => { WorkWithText.TypeWriteAnimation((string)FindResource("defaultDescription"), TextDescription, TimeSpan.FromMilliseconds(0)); };
        }

        private void Tweak_MouseEnter(object sender, MouseEventArgs e)
        {
            ToggleButton toggleButton = (ToggleButton)sender;

            if (TextDescription.Text != (string)FindResource(toggleButton.Name+ "_description_confidentiality"))
            {
                string _descriptionTweak = (string)FindResource(toggleButton.Name + "_description_confidentiality");
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
            Parallel.Invoke(() => ConfidentialityTweaks.UseСonfidentiality(toggleButton.Name, toggleButton.State));
            СonfigSettings.configConfidentiality.Remove(toggleButton.Name);
            СonfigSettings.configConfidentiality.Add(toggleButton.Name, Convert.ToString(toggleButton.State));

            switch (toggleButton.Name)
            {
                case "TglButton8":
                case "TglButton15":
                    new ViewNotification().Show("restart");
                    break;
            }
            await Task.Delay(350);
            Parallel.Invoke(() => new ConfidentialityTweaks().ViewСonfidentiality(this));
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            WorkWithText.TypeWriteAnimation((string)FindResource("defaultDescription"), TextDescription, TimeSpan.FromMilliseconds(300));
            Parallel.Invoke(()=> new ConfidentialityTweaks().ViewСonfidentiality(this));
        }

        private void Page_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.System && (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt)))
                e.Handled = true;
        }
    }
}
