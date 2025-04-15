using GTweak.Utilities.Configuration;
using GTweak.Utilities.Controls;
using GTweak.Utilities.Helpers;
using GTweak.Utilities.Tweaks;
using GTweak.Windows;
using Ookii.Dialogs.Wpf;
using System;
using System.IO;

namespace GTweak.View
{
    public partial class MoreView
    {
        public MoreView()
        {
            InitializeComponent();
        }

        private async void BtnLicenseWindows_ClickButton(object sender, EventArgs e)
        {
            if (WindowsLicense.IsWindowsActivated)
                new ViewNotification().Show("", "info", "readyactivate_notification");
            else
            {
                if (new SystemDiagnostics().IsNetworkAvailable())
                    await WindowsLicense.StartActivation();
                else
                    new ViewNotification().Show("", "warn", "networklicense_notification");
            }
        }

        private async void BtnRestorePoint_ClickButton(object sender, EventArgs e)
        {
            WaitingWindow waitingWindow = new WaitingWindow();
            waitingWindow.Show();
            new ViewNotification().Show("", "info", "createpoint_notification");
            BackgroundQueue backgroundQueue = new BackgroundQueue();
            await backgroundQueue.QueueTask(delegate { SystemMaintenance.CreateRestorePoint(); });
            waitingWindow.Close();
        }

        private void BtnRecoveyLaunch_ClickButton(object sender, EventArgs e) => SystemMaintenance.StartRecovery();

        private void BtnClear_ClickButton(object sender, EventArgs e) => new ClearingMemory().StartMemoryCleanup();

        private void BtnDisableDefrag_ClickButton(object sender, EventArgs e) => SystemMaintenance.SetDefragState(false);

        private async void BtnDisableRecovery_ClickButton(object sender, EventArgs e)
        {
            BackgroundQueue backgroundQueue = new BackgroundQueue();
            await backgroundQueue.QueueTask(delegate
            {
                try { SystemMaintenance.DisableRestorePoint(); } catch (Exception ex) { ErrorLogging.LogDebug(ex); }
            });
            await backgroundQueue.QueueTask(delegate { new ViewNotification(300).Show("", "info", "disable_recovery_notification"); });
        }

        private void BtnEnableDefrag_ClickButton(object sender, EventArgs e) => SystemMaintenance.SetDefragState(true);

        private async void BtnCompression_ClickButton(object sender, EventArgs e)
        {
            VistaFolderBrowserDialog folderDialog = new VistaFolderBrowserDialog();

            if (folderDialog.ShowDialog() == false)
                return;

            string selectedPath = folderDialog.SelectedPath;
            if (await NTFSCompressor.IsSupportNtfs(selectedPath))
            {
                if ((new DirectoryInfo(selectedPath).Attributes & FileAttributes.Compressed) != FileAttributes.Compressed)
                {
                    BackgroundQueue backgroundQueue = new BackgroundQueue();
                    await backgroundQueue.QueueTask(delegate
                    {
                        try { NTFSCompressor.SetCompression(selectedPath, true); }
                        catch { new ViewNotification().Show("", "warn", "error_compression_notification"); }
                    });
                    await backgroundQueue.QueueTask(delegate { new ViewNotification(500).Show("", "info", "succes_compression_notification"); });
                }
                else
                    new ViewNotification().Show("", "info", "ready_compression_notification");
            }
            else
                new ViewNotification().Show("", "warn", "notsupport_ntfs_notification");
        }

        private async void BtnDecompression_ClickButton(object sender, EventArgs e)
        {
            VistaFolderBrowserDialog folderDialog = new VistaFolderBrowserDialog();

            if (folderDialog.ShowDialog() == false)
                return;

            string selectedPath = folderDialog.SelectedPath;
            if (await NTFSCompressor.IsSupportNtfs(selectedPath))
            {
                if ((new DirectoryInfo(selectedPath).Attributes & FileAttributes.Compressed) == FileAttributes.Compressed)
                {
                    BackgroundQueue backgroundQueue = new BackgroundQueue();
                    await backgroundQueue.QueueTask(delegate
                    {
                        try { NTFSCompressor.SetCompression(selectedPath, false); }
                        catch { new ViewNotification().Show("", "warn", "error_compression_notification"); }

                    });
                    await backgroundQueue.QueueTask(delegate { new ViewNotification(500).Show("", "info", "succes_decompression_notification"); });
                }
                else
                    new ViewNotification().Show("", "info", "ready_decompression_notification");
            }
            else
                new ViewNotification().Show("", "warn", "notsupport_ntfs_notification");
        }
    }
}
