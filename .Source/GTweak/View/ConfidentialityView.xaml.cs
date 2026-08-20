using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GTweak.Assets.UserControls;
using GTweak.Core.Interfaces;
using GTweak.Modules.Helpers;
using GTweak.Modules.Managers;
using GTweak.Modules.Tweaks;

namespace GTweak.View
{
    public partial class ConfidentialityView : UserControl, IViewMarker
    {
        private readonly ConfidentialityTweaks _confTweaks = new ConfidentialityTweaks();

        public ConfidentialityView()
        {
            InitializeComponent();
        }

        private void Tweak_MouseEnter(object sender, MouseEventArgs e) => DescBlock.ContentSource = sender;
        private void Tweak_MouseLeave(object sender, MouseEventArgs e) => DescBlock.ContentSource = null;

        private void TglButton_ChangedState(object sender, RoutedEventArgs e)
        {
            ToggleButton tglButton = (ToggleButton)sender;

            DescBlock.ContentSource = tglButton;

            _confTweaks.Apply(tglButton.Name, tglButton.State);

            if (NotificationManager.ConfActions.TryGetAction(tglButton.Name, out NotificationManager.NoticeAction action))
            {
                NotificationManager.Show().WithDelay(300).Perform(action);
            }
        }
    }
}