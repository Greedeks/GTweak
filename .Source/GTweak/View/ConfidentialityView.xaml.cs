using GTweak.Assets.UserControl;
using GTweak.Utilities.Controls;
using GTweak.Utilities.Helpers.Storage;
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

            ConfidentialityTweaks.ApplyTweaks(toggleButton.Name, toggleButton.State);

            if (NotifActionsStorage.ConfNotifActions.TryGetValue(toggleButton.Name, out string action))
                new ViewNotification(300).Show(action);

            Parallel.Invoke(async delegate { await Task.Delay(1000); new ConfidentialityTweaks().AnalyzeAndUpdate(this); });
        }

        private void Page_Loaded(object sender, RoutedEventArgs e) => Parallel.Invoke(() => new ConfidentialityTweaks().AnalyzeAndUpdate(this));

    }
}
