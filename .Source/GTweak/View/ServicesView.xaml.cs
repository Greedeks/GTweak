using GTweak.Assets.UserControl;
using GTweak.Utilities.Controls;
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
        }

        private void Tweak_MouseEnter(object sender, MouseEventArgs e)
        {
            string descriptionTweak = (string)FindResource(((ToggleButton)sender).Name + "_description_services");

            if (CommentTweak.Text != descriptionTweak)
                CommentTweak.Text = descriptionTweak;
        }

        private void Tweak_MouseLeave(object sender, MouseEventArgs e)
        {
            if (CommentTweak.Text != (string)FindResource("defaultDescription"))
                CommentTweak.Text = (string)FindResource("defaultDescription");
        }

        private void TglButton_ChangedState(object sender, EventArgs e)
        {
            ServicesTweaks.ApplyTweaks(((ToggleButton)sender).Name, ((ToggleButton)sender).State);

            new ViewNotification(300).Show("restart");

            Parallel.Invoke(async delegate { await Task.Delay(1000); new ServicesTweaks().AnalyzeAndUpdate(this); });
        }

        private void Page_Loaded(object sender, System.Windows.RoutedEventArgs e) => Parallel.Invoke(() => new ServicesTweaks().AnalyzeAndUpdate(this));
    }
}
