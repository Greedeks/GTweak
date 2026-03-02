using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GTweak.Assets.UserControls;
using GTweak.Utilities.Maintenance;
using GTweak.Utilities.Managers;
using GTweak.Utilities.Tweaks;

namespace GTweak.View
{
    [ToolboxItem(false)]
    [DesignTimeVisible(false)]
    public partial class InterfaceView : UserControl
    {
        private readonly InterfaceTweaks _intfTweaks = new InterfaceTweaks();

        public InterfaceView()
        {
            InitializeComponent();

            if (!WinLicenseHandler.IsWindowsActivated)
            {
                NotificationManager.Show("info", "warn_activate_noty").Perform();
            }
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
                default:
                    break;
            }

            if (DescBlock.Text != description)
            {
                DescBlock.Text = description;
            }
        }
        private void Tweak_MouseLeave(object sender, MouseEventArgs e) => DescBlock.Text = DescBlock.DefaultText;

        private void ColorPicker_ColorPicked(object sender, EventArgs e)
        {
            ColorPicker colorPicker = (ColorPicker)sender;
            _intfTweaks.ApplyTweaksColor(colorPicker.Name, colorPicker.SelectedColorString);
            NotificationManager.Show().WithDelay(300).Logout();
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            _intfTweaks.ApplyTweaksCheckBox(checkBox.Uid, checkBox.IsChecked == false);
        }

        private void TglButton_ChangedState(object sender, RoutedEventArgs e)
        {
            ToggleButton toggleButton = (ToggleButton)sender;

            _intfTweaks.ApplyTweaks(toggleButton.Name, toggleButton.State);

            if (ExplorerManager.IntfMapping.TryGetValue(toggleButton.Name, out bool needRestart))
            {
                ExplorerManager.Restart();
            }

            if (NotificationManager.IntfActions.TryGetValue(toggleButton.Name, out NotificationManager.NoticeAction action))
            {
                NotificationManager.Show().WithDelay(300).Perform(action);
            }
        }
    }
}
