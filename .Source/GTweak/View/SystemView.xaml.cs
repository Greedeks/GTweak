using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GTweak.Assets.UserControls;
using GTweak.Core.Interfaces;
using GTweak.Modules.Common;
using GTweak.Modules.Extensions;
using GTweak.Modules.Managers;
using GTweak.Modules.Tweaks;

namespace GTweak.View
{
    public partial class SystemView : UserControl, IViewMarker
    {
        private readonly SystemTweaks _sysTweaks = new SystemTweaks();

        public SystemView()
        {
            InitializeComponent();
        }

        private void Tweak_MouseEnter(object sender, MouseEventArgs e) => DescBlock.ContentSource = sender;
        private void Tweak_MouseLeave(object sender, MouseEventArgs e) => DescBlock.ContentSource = null;

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) => _sysTweaks.Apply(((Slider)sender).Name, (uint)((Slider)sender).Value);

        private void TglButton_ChangedState(object sender, RoutedEventArgs e)
        {
            ToggleButton tglButton = (ToggleButton)sender;
            DescBlock.ContentSource = tglButton;

            _sysTweaks.Apply(tglButton.Name, tglButton.State);

            PostActionAttribute postAction = tglButton.Name.GetPostAction(typeof(SystemToggle));

            if (postAction.HasAlert())
            {
                NotificationManager.Default().WithDelay(300).Perform(postAction.Alert);
            }
        }
    }
}
