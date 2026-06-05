using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GTweak.Assets.UserControls;
using GTweak.Core.Base;
using GTweak.Utilities.Managers;
using GTweak.Utilities.Tweaks;

namespace GTweak.View
{
    public partial class InterfaceView : UserControl, IViewPageBase
    {
        private readonly InterfaceTweaks _intfTweaks = new InterfaceTweaks();
        private ExplorerManager.ExplorerAction _explorerAction = ExplorerManager.ExplorerAction.None;

        public InterfaceView()
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

        private void ColorPicker_ColorPicked(object sender, EventArgs e)
        {
            ColorPicker colorPicker = (ColorPicker)sender;
            _intfTweaks.ApplyTweaks(colorPicker.Name, colorPicker.SelectedColorString);
            NotificationManager.Show().WithDelay(300).Logout();
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            _intfTweaks.ApplyTweaks(checkBox.Uid, checkBox.IsChecked == false);

            if (ExplorerManager.IntfActions.TryGetValue(checkBox.Uid, out ExplorerManager.ExplorerAction explorerAction))
            {
                _explorerAction = explorerAction;
            }
        }

        private void ExpandableBox_Closed(object sender, EventArgs e)
        {
            switch (_explorerAction)
            {
                case ExplorerManager.ExplorerAction.Restart:
                    ExplorerManager.Restart();
                    break;
                case ExplorerManager.ExplorerAction.Refresh:
                    ExplorerManager.RefreshDesktop();
                    break;
                default:
                    break;
            }

            _explorerAction = ExplorerManager.ExplorerAction.None;
        }

        private void TglButton_ChangedState(object sender, RoutedEventArgs e)
        {
            ToggleButton tglButton = (ToggleButton)sender;

            DescBlock.TargetState = tglButton.State;

            _intfTweaks.ApplyTweaks(tglButton.Name, tglButton.State);

            if (ExplorerManager.IntfActions.TryGetValue(tglButton.Name, out ExplorerManager.ExplorerAction explorerAction) && explorerAction == ExplorerManager.ExplorerAction.Restart)
            {
                ExplorerManager.Restart();
            }

            if (NotificationManager.IntfActions.TryGetValue(tglButton.Name, out NotificationManager.NoticeAction noticeAction))
            {
                NotificationManager.Show().WithDelay(300).Perform(noticeAction);
            }
        }
    }
}
