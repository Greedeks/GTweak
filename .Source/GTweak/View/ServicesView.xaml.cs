using GTweak.Assets.UserControl;
using GTweak.Utilities.Managers;
using GTweak.Utilities.Tweaks;
using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace GTweak.View
{
    public partial class ServicesView : UserControl
    {
        private readonly ServicesTweaks _svcTweaks = new ServicesTweaks();

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
            _svcTweaks.ApplyTweaks(((ToggleButton)sender).Name, ((ToggleButton)sender).State);

            new NotificationManager(300).Show().Restart();

            Parallel.Invoke(async delegate { await Task.Delay(((ToggleButton)sender).Name.Contains("15") ? 2000 : 1000); _svcTweaks.AnalyzeAndUpdate(); });
        }
    }
}
