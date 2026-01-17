using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GTweak.Assets.UserControl;
using GTweak.Utilities.Managers;
using GTweak.Utilities.Tweaks;

namespace GTweak.View
{
    public partial class SystemView : UserControl
    {
        private readonly SystemTweaks _sysTweaks = new SystemTweaks();

        public SystemView()
        {
            InitializeComponent();
        }

        private void Tweak_MouseEnter(object sender, MouseEventArgs e)
        {
            string description = string.Empty;

            switch (sender)
            {
                case StackPanel panel:
                    description = panel.ToolTip?.ToString() ?? string.Empty;
                    break;

                case ToggleButton toggle:
                    description = toggle.Description?.ToString() ?? string.Empty;
                    break;
            }

            if (DescBlock.Text != description)
            {
                DescBlock.Text = description;
            }
        }

        private void Tweak_MouseLeave(object sender, MouseEventArgs e) => DescBlock.Text = DescBlock.DefaultText;

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) => _sysTweaks.ApplyTweaksSlider(((Slider)sender).Name, (uint)((Slider)sender).Value);

        private void TglButton_ChangedState(object sender, RoutedEventArgs e)
        {
            ToggleButton toggleButton = (ToggleButton)sender;
            if (toggleButton.Name != "TglButton3")
            {
                _sysTweaks.ApplyTweaks(toggleButton.Name, toggleButton.State);

                if (NotificationManager.SysActions.TryGetValue(toggleButton.Name, out NotificationManager.NoticeAction action))
                {
                    NotificationManager.Show().WithDelay(300).Perform(action);
                }
            }
            else
            {
                _sysTweaks.ApplyTweaks(toggleButton.Name, toggleButton.State);
            }
        }
    }
}
