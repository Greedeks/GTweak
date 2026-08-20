using System;
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
    public partial class InterfaceView : UserControl, IViewMarker
    {
        private readonly InterfaceTweaks _intfTweaks = new InterfaceTweaks();
        private ExplorerManager.ExplorerAction _explorerAction = ExplorerManager.ExplorerAction.None;

        public InterfaceView()
        {
            InitializeComponent();
        }

        private void Tweak_MouseEnter(object sender, MouseEventArgs e) => DescBlock.ContentSource = sender;
        private void Tweak_MouseLeave(object sender, MouseEventArgs e) => DescBlock.ContentSource = null;

        private void ColorPicker_ColorPicked(object sender, EventArgs e)
        {
            ColorPicker colorPicker = (ColorPicker)sender;
            _intfTweaks.Apply(colorPicker.Name, colorPicker.SelectedColorString);
            NotificationManager.Show().WithDelay(300).Logout();
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            _intfTweaks.Apply(checkBox.Uid, checkBox.IsChecked == false);

            if (ExplorerManager.IntfActions.TryGetAction(checkBox.Uid, out ExplorerManager.ExplorerAction explorerAction))
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

            DescBlock.ContentSource = tglButton;

            _intfTweaks.Apply(tglButton.Name, tglButton.State);

            if (ExplorerManager.IntfActions.TryGetAction(tglButton.Name, out ExplorerManager.ExplorerAction explorerAction) && explorerAction == ExplorerManager.ExplorerAction.Restart)
            {
                ExplorerManager.Restart();
            }

            if (NotificationManager.IntfActions.TryGetAction(tglButton.Name, out NotificationManager.NoticeAction noticeAction))
            {
                NotificationManager.Show().WithDelay(300).Perform(noticeAction);
            }
        }
    }
}
