using GTweak.Utilities;
using GTweak.Utilities.Tweaks;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
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
                new ViewNotification().Show("", (string)FindResource("title1_notification"), (string)FindResource("readyactivate_notification"));
            else
            {
                new ViewNotification().Show("", (string)FindResource("title0_notification"), (string)FindResource("activatewin_notification"));
                WindowsLicense.StartActivation();
            }
        }

        private async void BtnRestorePoint_ClickButton(object sender, EventArgs e)
        {
            if (RecoveryPoint.IsAlreadyPoint() == false)
            {
                new ViewNotification().Show("", (string)FindResource("title1_notification"), (string)FindResource("createpoint_notification"));

                await Task.Delay(100);

                BackgroundWorker backgroundWorker = new BackgroundWorker();
                backgroundWorker.DoWork += delegate
                {
                    try { RecoveryPoint.Create((string)FindResource("textpoint_more")); }
                    catch { new ViewNotification().Show("", (string)FindResource("title0_notification"), (string)FindResource("notsuccessfulpoint_notification")); }
                    finally { new ViewNotification().Show("", (string)FindResource("title1_notification"), (string)FindResource("successpoint_notification")); };
                };
                backgroundWorker.RunWorkerCompleted += delegate
                {
                    new ViewNotification().Show("", (string)FindResource("title1_notification"), (string)FindResource("successpoint_notification"));
                    backgroundWorker.Dispose();
                };
                backgroundWorker.RunWorkerAsync();
            }
            else
                new ViewNotification().Show("", (string)FindResource("title1_notification"), (string)FindResource("readypoint_notification"));
        }

        private void BtnRecoveyLaunch_ClickButton(object sender, EventArgs e)
        {
            try { RecoveryPoint.Run(); }
            catch { new ViewNotification().Show("", (string)FindResource("title0_notification"), (string)FindResource("notsuccessfulrecovery_notification")); }
        }

        private void BtnClear_ClickButton(object sender, EventArgs e) => ClearingMemory.StartMemoryCleanup();


        private void BtnDisableRecovery_ClickButton(object sender, EventArgs e)
        {
            if (!RecoveryPoint.IsSystemRestoreDisabled())
            {
                BackgroundWorker backgroundWorker = new BackgroundWorker();
                backgroundWorker.DoWork += delegate
                {
                    try { RecoveryPoint.DisablePoint(); } catch (Exception ex) { Debug.WriteLine(ex.Message.ToString()); }
                };
                backgroundWorker.RunWorkerCompleted += delegate
                {
                    new ViewNotification().Show("", (string)FindResource("title1_notification"), (string)FindResource("disable_recovery_notification"));
                    backgroundWorker.Dispose();
                };
                backgroundWorker.RunWorkerAsync();
            }
            else
                new ViewNotification().Show("", (string)FindResource("title1_notification"), (string)FindResource("warndisable_recovery_notification"));
        }
    }
}
