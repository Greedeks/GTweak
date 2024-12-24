using GTweak.Assets.UserControl;
using GTweak.Utilities.Control;
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

            App.LanguageChanged += delegate { new TypewriterAnimation((string)FindResource("defaultDescription"), TextDescription, TimeSpan.FromMilliseconds(0)); };
        }

        private void Tweak_MouseEnter(object sender, MouseEventArgs e)
        {
            ToggleButton toggleButton = (ToggleButton)sender;
            string _descriptionTweak = (string)FindResource(toggleButton.Name + "_description_services");

            if (TextDescription.Text != _descriptionTweak)
                new TypewriterAnimation(_descriptionTweak, TextDescription, _descriptionTweak.Length < 200 ? TimeSpan.FromMilliseconds(400) : TimeSpan.FromMilliseconds(550));
        }

        private void Tweak_MouseLeave(object sender, MouseEventArgs e)
        {
            if (TextDescription.Text != (string)FindResource("defaultDescription"))
                new TypewriterAnimation((string)FindResource("defaultDescription"), TextDescription, TimeSpan.FromMilliseconds(250));
        }

        private void TglButton_ChangedState(object sender, EventArgs e)
        {
            ToggleButton toggleButton = (ToggleButton)sender;
            ServicesTweaks.UseServices(toggleButton.Name, toggleButton.State);

            new ViewNotification(300).Show("restart");

            Parallel.Invoke(async delegate { await Task.Delay(1000); new ServicesTweaks().ViewServices(this); });
        }

        private void Page_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            new TypewriterAnimation((string)FindResource("defaultDescription"), TextDescription, TimeSpan.FromMilliseconds(300));
            Parallel.Invoke(() => new ServicesTweaks().ViewServices(this));
        }
    }
}
