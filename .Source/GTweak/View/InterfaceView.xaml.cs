using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GTweak.Assets.UserControl;
using GTweak.Utilities.Maintenance;
using GTweak.Utilities.Managers;
using GTweak.Utilities.Tweaks;

namespace GTweak.View
{
    public partial class InterfaceView : UserControl
    {
        private readonly InterfaceTweaks _intfTweaks = new InterfaceTweaks();

        public InterfaceView()
        {
            InitializeComponent();

            if (!WindowsLicense.IsWindowsActivated)
            {
                NotificationManager.Show("info", "warn_activate_noty").Perform();
            }
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
            ToggleButton toggleButton = (ToggleButton)sender;

            _intfTweaks.ApplyTweaks(toggleButton.Name, toggleButton.State);

            if (ExplorerManager.IntfMapping.TryGetValue(toggleButton.Name, out bool needRestart))
            {
                ExplorerManager.Restart(new Process());
            }

            if (NotificationManager.IntfActions.TryGetValue(toggleButton.Name, out NotificationManager.NoticeAction action))
            {
                NotificationManager.Show().WithDelay(300).Perform(action);
            }

            Parallel.Invoke(async delegate { await Task.Delay(1000); _intfTweaks.AnalyzeAndUpdate(); });
        }
    }
}
