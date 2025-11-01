using GTweak.Assets.UserControl;
using GTweak.Utilities.Managers;
using GTweak.Utilities.Tweaks;
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
            string description = ((ToggleButton)sender).Description?.ToString() ?? string.Empty;

            if (DescBlock.Text != description)
                DescBlock.Text = description;
        }

        private void Tweak_MouseLeave(object sender, MouseEventArgs e)
        {
            if (DescBlock.Text != DescBlock.DefaultText)
                DescBlock.Text = DescBlock.DefaultText;
        }

        private void TglButton_ChangedState(object sender, RoutedEventArgs e)
        {
            ToggleButton toggleButton = (ToggleButton)sender;

            _confTweaks.ApplyTweaks(toggleButton.Name, toggleButton.State);

            if (NotificationManager.ConfActions.TryGetValue(toggleButton.Name, out NotificationManager.NoticeAction action))
                NotificationManager.Show().WithDelay(300).Perform(action);

            Parallel.Invoke(async delegate { await Task.Delay(1000); _confTweaks.AnalyzeAndUpdate(); });
        }
    }
}
