using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GTweak.Assets.UserControls;
using GTweak.Core.Interfaces;
using GTweak.Modules.Managers;
using GTweak.Modules.Tweaks;

namespace GTweak.View
{
    public partial class ServicesView : UserControl, IViewMarker
    {
        private readonly ServicesTweaks _svcTweaks = new ServicesTweaks();

        public ServicesView()
        {
            InitializeComponent();
        }

        private void Tweak_MouseEnter(object sender, MouseEventArgs e) => DescBlock.ContentSource = sender;
        private void Tweak_MouseLeave(object sender, MouseEventArgs e) => DescBlock.ContentSource = null;

        private void TglButton_ChangedState(object sender, RoutedEventArgs e)
        {
            ToggleButton tglButton = (ToggleButton)sender;

            DescBlock.ContentSource = tglButton;

            _svcTweaks.Apply(tglButton.Name, tglButton.State);

            NotificationManager.Default().WithDelay(300).Restart();
        }
    }
}
