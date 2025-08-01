using GTweak.Assets.UserControl;
using GTweak.Utilities.Managers;
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
        private readonly ConfidentialityTweaks _confTweaks = new ConfidentialityTweaks();

        public ConfidentialityView()
        {
            InitializeComponent();
        }

        private void Tweak_MouseEnter(object sender, MouseEventArgs e)
        {
            string descriptionTweak = (string)FindResource(((ToggleButton)sender).Name + "_description_confidentiality");

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
            ToggleButton toggleButton = (ToggleButton)sender;

            _confTweaks.ApplyTweaks(toggleButton.Name, toggleButton.State);

            if (NotificationManager.ConfActions.TryGetValue(toggleButton.Name, out NotificationManager.NoticeAction action))
                new NotificationManager(300).Show().Perform(action);

            Parallel.Invoke(async delegate { await Task.Delay(1000); _confTweaks.AnalyzeAndUpdate(this); });
        }

        private void Page_Loaded(object sender, RoutedEventArgs e) => Parallel.Invoke(() => _confTweaks.AnalyzeAndUpdate(this));

    }
}
