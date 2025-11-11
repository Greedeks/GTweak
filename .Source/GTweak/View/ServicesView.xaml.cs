using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GTweak.Assets.UserControl;
using GTweak.Utilities.Managers;
using GTweak.Utilities.Tweaks;

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
            string description = ((ToggleButton)sender).Description?.ToString() ?? string.Empty;

            if (DescBlock.Text != description)
            {
                DescBlock.Text = description;
            }
        }

        private void Tweak_MouseLeave(object sender, MouseEventArgs e)
        {
            if (DescBlock.Text != DescBlock.DefaultText)
            {
                DescBlock.Text = DescBlock.DefaultText;
            }
        }

        private void TglButton_ChangedState(object sender, RoutedEventArgs e)
        {
            _svcTweaks.ApplyTweaks(((ToggleButton)sender).Name, ((ToggleButton)sender).State);

            NotificationManager.Show().WithDelay(300).Restart();
        }
    }
}
