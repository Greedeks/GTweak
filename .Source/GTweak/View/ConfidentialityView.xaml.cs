using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GTweak.Assets.UserControls;
using GTweak.Core.Base;
using GTweak.Utilities.Managers;
using GTweak.Utilities.Tweaks;

namespace GTweak.View
{
    public partial class ConfidentialityView : UserControl, IViewPageBase
    {
        private readonly ConfidentialityTweaks _confTweaks = new ConfidentialityTweaks();

        public ConfidentialityView()
        {
            InitializeComponent();
        }

        private void Tweak_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is ToggleButton tglButton && DescBlock.Text != tglButton.Description?.ToString())
            {
                DescBlock.Text = tglButton.Description?.ToString() ?? string.Empty;
                DescBlock.TargetState = tglButton.State;
            }
        }

        private void Tweak_MouseLeave(object sender, MouseEventArgs e)
        {
            DescBlock.Text = DescBlock.DefaultText;
            DescBlock.TargetState = null;
        }

        private void TglButton_ChangedState(object sender, RoutedEventArgs e)
        {
            ToggleButton tglButton = (ToggleButton)sender;

            DescBlock.TargetState = tglButton.State;

            _confTweaks.ApplyTweaks(tglButton.Name, tglButton.State);

            if (NotificationManager.ConfActions.TryGetValue(tglButton.Name, out var action))
            {
                NotificationManager.Show().WithDelay(300).Perform(action);
            }
        }
    }
}