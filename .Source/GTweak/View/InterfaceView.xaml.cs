using System;
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
    public partial class InterfaceView : UserControl, IViewMarker
    {
        private readonly InterfaceTweaks _intfTweaks = new InterfaceTweaks();
        private ExplorerManager.ShellType _shellType = ExplorerManager.ShellType.None;

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
            NotificationManager.Default().WithDelay(300).Logout();
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            _intfTweaks.Apply(checkBox.Uid, checkBox.IsChecked == false);
            _shellType = checkBox.Uid.GetPostAction(typeof(InterfaceCheckbox)).Shell;
        }

        private void ExpandableBox_Closed(object sender, EventArgs e)
        {
            ExplorerManager.Handle(_shellType);
            _shellType = ExplorerManager.ShellType.None;
        }

        private void TglButton_ChangedState(object sender, RoutedEventArgs e)
        {
            ToggleButton tglButton = (ToggleButton)sender;
            DescBlock.ContentSource = tglButton;
            _intfTweaks.Apply(tglButton.Name, tglButton.State);

            PostActionAttribute postAction = tglButton.Name.GetPostAction(typeof(InterfaceToggle));

            if (postAction.HasShell())
            {
                ExplorerManager.Handle(postAction.Shell);
            }

            if (postAction.HasAlert())
            {
                NotificationManager.Default().WithDelay(300).Perform(postAction.Alert);
            }
        }
    }
}
