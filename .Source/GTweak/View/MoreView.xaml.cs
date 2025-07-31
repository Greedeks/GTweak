using GTweak.Utilities.Configuration;
using GTweak.Utilities.Controls;
using GTweak.Utilities.Helpers;
using GTweak.Utilities.Managers;
using GTweak.Utilities.Tweaks;
using GTweak.Windows;
using Ookii.Dialogs.Wpf;
using System;
using System.IO;

namespace GTweak.View
{
    public partial class MoreView
    {
        private bool _isWinOldRemoval = false;
        private static bool _hasDeclined = false;

        public MoreView()
        {
            InitializeComponent();
        }

        private async void BtnLicenseWindows_ClickButton(object sender, EventArgs e)
        {
            if (WindowsLicense.IsWindowsActivated)
                new NotificationManager().Show("info", "ready_activate_notification").None();
            else
            {
                if (new SystemDiagnostics().IsNetworkAvailable())
                    await WindowsLicense.StartActivation();
                else
                    new NotificationManager().Show("warn", "network_activate_notification").None();
            }
        }

        private async void BtnRestorePoint_ClickButton(object sender, EventArgs e)
        {
            WaitingWindow waitingWindow = new WaitingWindow();
            waitingWindow.Show();
            new NotificationManager().Show("info", "createpoint_notification").None();
            BackgroundQueue backgroundQueue = new BackgroundQueue();
            await backgroundQueue.QueueTask(delegate { SystemMaintenance.CreateRestorePoint(); });
            waitingWindow.Close();
        }

        private void BtnRecoveyLaunch_ClickButton(object sender, EventArgs e) => SystemMaintenance.StartRecovery();

        private async void BtnClear_ClickButton(object sender, EventArgs e)
        {
            if (ClearingMemory.IsWinOldExists && !_hasDeclined)
            {
                OverlayDialogManager overlayDialog = new OverlayDialogManager(Overlay, OpacityProperty, BtnAgree, BtnDecline, onSecondary: () => _hasDeclined = true);
                _isWinOldRemoval = await overlayDialog.Show();
            }

            BackgroundQueue backgroundQueue = new BackgroundQueue();
            await backgroundQueue.QueueTask(delegate { ClearingMemory.StartMemoryCleanupAsync(_isWinOldRemoval); });
            await backgroundQueue.QueueTask(delegate { new NotificationManager(500).Show("info", "clear_ram_notification").None(); });
        }

        private void BtnDisableDefrag_ClickButton(object sender, EventArgs e) => SystemMaintenance.SetDefragState(false);

        private async void BtnDisableRecovery_ClickButton(object sender, EventArgs e)
        {
            BackgroundQueue backgroundQueue = new BackgroundQueue();
            await backgroundQueue.QueueTask(delegate
            {
                try { SystemMaintenance.DisableRestorePoint(); } catch (Exception ex) { ErrorLogging.LogDebug(ex); }
            });
            await backgroundQueue.QueueTask(delegate { new NotificationManager(300).Show("info", "disable_recovery_notification").None(); });
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
                        catch { new NotificationManager().Show("warn", "error_compression_notification").None(); }
                    });
                    await backgroundQueue.QueueTask(delegate { new NotificationManager(500).Show("info", "success_compression_notification").None(); });
                }
                else
                    new NotificationManager().Show("info", "ready_compression_notification").None();
            }
            else
                new NotificationManager().Show("warn", "notsupport_ntfs_notification").None();
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
                        catch { new NotificationManager().Show("warn", "error_compression_notification").None(); }

                    });
                    await backgroundQueue.QueueTask(delegate { new NotificationManager(500).Show("info", "success_decompression_notification").None(); });
                }
                else
                    new NotificationManager().Show("info", "ready_decompression_notification").None();
            }
            else
                new NotificationManager().Show("warn", "notsupport_ntfs_notification").None();
        }
    }
}
