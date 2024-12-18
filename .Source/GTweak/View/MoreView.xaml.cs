﻿using GTweak.Utilities;
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
                new ViewNotification().Show("", "info", (string)FindResource("readyactivate_notification"));
            else
            {
                new ViewNotification().Show("", "warn", (string)FindResource("activatewin_notification"));
                WindowsLicense.StartActivation();
            }
        }

        private async void BtnRestorePoint_ClickButton(object sender, EventArgs e)
        {
            if (await RecoveryPoint.IsAlreadyPointAsync() == false)
            {
                new ViewNotification().Show("", "info", (string)FindResource("createpoint_notification"));

                await Task.Delay(100);

                using BackgroundWorker backgroundWorker = new BackgroundWorker();
                backgroundWorker.DoWork += delegate { RecoveryPoint.Create((string)FindResource("textpoint_more")); };
                backgroundWorker.RunWorkerAsync();
            }
            else
                new ViewNotification().Show("", "info", (string)FindResource("readypoint_notification"));
        }

        private void BtnRecoveyLaunch_ClickButton(object sender, EventArgs e)
        {
            try { RecoveryPoint.Run(); }
            catch { new ViewNotification().Show("", "warn", (string)FindResource("notsuccessfulrecovery_notification")); }
        }

        private void BtnClear_ClickButton(object sender, EventArgs e) => new ClearingMemory().StartMemoryCleanup();

        private void BtnDisableRecovery_ClickButton(object sender, EventArgs e)
        {
            if (!RecoveryPoint.IsSystemRestoreDisabled())
            {
                using BackgroundWorker backgroundWorker = new BackgroundWorker();
                backgroundWorker.DoWork += delegate
                {
                    try { RecoveryPoint.DisablePoint(); } catch (Exception ex) { Debug.WriteLine(ex.Message.ToString()); }
                };
                backgroundWorker.RunWorkerCompleted += delegate
                {
                    new ViewNotification(300).Show("", "info", (string)FindResource("disable_recovery_notification"));
                };
                backgroundWorker.RunWorkerAsync();
            }
            else
                new ViewNotification().Show("", "info", (string)FindResource("warndisable_recovery_notification"));
        }
    }
}
