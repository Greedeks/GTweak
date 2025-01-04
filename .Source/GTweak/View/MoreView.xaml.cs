using GTweak.Utilities.Control;
using GTweak.Utilities.Helpers;
using GTweak.Utilities.Tweaks;
using System;
using System.Diagnostics;
using System.Windows.Controls;

namespace GTweak.View
{
    public partial class MoreView : UserControl
    {
        public MoreView()
        {
            InitializeComponent();
        }

        private void BtnLicenseWindows_ClickButton(object sender, EventArgs e)
        {
            if (WindowsLicense.IsWindowsActivated)
                new ViewNotification().Show("", "info", (string)FindResource("readyactivate_notification"));
            else
            {
                new ViewNotification().Show("", "warn", (string)FindResource("activatewin_notification"));
                WindowsLicense.StartActivation();
            }
        }

        private async void BtnRestorePoint_ClickButton(object sender, EventArgs e)
        {
            new ViewNotification().Show("", "info", (string)FindResource("createpoint_notification"));
            BackgroundQueue backgroundQueue = new BackgroundQueue();
            await backgroundQueue.QueueTask(delegate { SystemMaintenance.CreateRestorePoint((string)FindResource("textpoint_more")); });
        }

        private void BtnRecoveyLaunch_ClickButton(object sender, EventArgs e) => SystemMaintenance.StartRecovery(); 

        private void BtnClear_ClickButton(object sender, EventArgs e) => new ClearingMemory().StartMemoryCleanup();

        private void BtnDisableDefrag_ClickButton(object sender, EventArgs e) => new SystemMaintenance().DisableDefrag();

        private async void BtnDisableRecovery_ClickButton(object sender, EventArgs e)
        {
            BackgroundQueue backgroundQueue = new BackgroundQueue();
            await backgroundQueue.QueueTask(delegate
            {
                try { SystemMaintenance.DisableRestorePoint(); } catch (Exception ex) { Debug.WriteLine(ex.Message); }
            });
            await backgroundQueue.QueueTask(delegate { new ViewNotification(300).Show("", "info", (string)FindResource("disable_recovery_notification")); });
        }
    }
}
