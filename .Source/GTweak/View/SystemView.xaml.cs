using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GTweak.Assets.UserControls;
using GTweak.Core.Base;
using GTweak.Utilities.Managers;
using GTweak.Utilities.Tweaks;

namespace GTweak.View
{
    public partial class SystemView : UserControl, IViewPageBase
    {
        private readonly SystemTweaks _sysTweaks = new SystemTweaks();

        public SystemView()
        {
            InitializeComponent();
        }

        private void Tweak_MouseEnter(object sender, MouseEventArgs e)
        {
            string description = string.Empty;
            bool? state = null;

            switch (sender)
            {
                case StackPanel panel:
                    description = panel.ToolTip?.ToString() ?? string.Empty;
                    break;
                case ToggleButton tglButton:
                    description = tglButton.Description?.ToString() ?? string.Empty;
                    state = tglButton.State;
                    break;
            }

            if (DescBlock.Text != description)
            {
                DescBlock.Text = description;
                DescBlock.TargetState = state;
            }
        }

        private void Tweak_MouseLeave(object sender, MouseEventArgs e)
        {
            DescBlock.Text = DescBlock.DefaultText;
            DescBlock.TargetState = null;
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) => _sysTweaks.ApplyTweaks(((Slider)sender).Name, (uint)((Slider)sender).Value);

        private void TglButton_ChangedState(object sender, RoutedEventArgs e)
        {
            ToggleButton tglButton = (ToggleButton)sender;

            DescBlock.TargetState = tglButton.State;

            if (tglButton.Name != "TglButton3")
            {
                _sysTweaks.ApplyTweaks(tglButton.Name, tglButton.State);

                if (NotificationManager.SysActions.TryGetValue(tglButton.Name, out NotificationManager.NoticeAction action))
                {
                    NotificationManager.Show().WithDelay(300).Perform(action);
                }
            }
            else
            {
                _sysTweaks.ApplyTweaks(tglButton.Name, tglButton.State);
            }
        }
    }
}
