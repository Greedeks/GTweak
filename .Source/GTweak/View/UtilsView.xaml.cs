using System;
using System.IO;
using System.Windows.Controls;
using GTweak.Utilities.Configuration;
using GTweak.Utilities.Controls;
using GTweak.Utilities.Helpers;
using GTweak.Utilities.Maintenance;
using GTweak.Utilities.Managers;
using GTweak.Windows;
using Ookii.Dialogs.Wpf;

namespace GTweak.View
{
    public partial class UtilsView : UserControl
    {
        private readonly SystemRestoreService _systemRestore = new SystemRestoreService();
        private readonly NTFSCompressor _ntfsCompressor = new NTFSCompressor();

        private bool _isWinOldRemoval = false;
        private static bool _hasDeclined = false;

        public UtilsView()
        {
            InitializeComponent();
        }

        private async void BtnLicenseWindows_ClickButton(object sender, EventArgs e)
        {
            if (WindowsLicense.IsWindowsActivated)
            {
                NotificationManager.Show("info", "ready_activate_noty").Perform();
            }
            else
            {
                if (new SystemDiagnostics().IsNetworkAvailable())
                {
                    await WindowsLicense.StartActivation();
                }
                else
                {
                    NotificationManager.Show("warn", "network_activate_noty").Perform();
                }
            }
        }

        private async void BtnCreatePoint_ClickButton(object sender, EventArgs e)
        {
            OverlayWindow overlayWindow = new OverlayWindow();
            overlayWindow.Show();
            NotificationManager.Show("info", "createpoint_noty").Perform();
            BackgroundQueue backgroundQueue = new BackgroundQueue();
            await backgroundQueue.QueueTask(delegate { _systemRestore.CreateRestorePoint(); });
            overlayWindow.Close();
        }

        private void BtnRecovey_ClickButton(object sender, EventArgs e) => _systemRestore.StartRecovery();

        private async void BtnDisablePoint_ClickButton(object sender, EventArgs e)
        {
            if (_systemRestore.IsPointCreationAllowed)
            {
                BackgroundQueue backgroundQueue = new BackgroundQueue();
                await backgroundQueue.QueueTask(delegate
                {
                    try { _systemRestore.DisableRestorePoint(); } catch (Exception ex) { ErrorLogging.LogDebug(ex); }
                });
                await backgroundQueue.QueueTask(delegate { NotificationManager.Show("info", "disable_recovery_noty").WithDelay(300).Perform(); });
            }
            else
            {
                NotificationManager.Show("info", "warn_recovery_noty").Perform();
            }
        }

        private async void BtnEnablePoint_ClickButton(object sender, EventArgs e)
        {
            if (!_systemRestore.IsPointCreationAllowed)
            {
                BackgroundQueue backgroundQueue = new BackgroundQueue();
                await backgroundQueue.QueueTask(delegate
                {
                    try { _systemRestore.EnableRestorePoint(); } catch (Exception ex) { ErrorLogging.LogDebug(ex); }
                });
                await backgroundQueue.QueueTask(delegate { NotificationManager.Show("info", "enable_recovery_noty").WithDelay(300).Perform(); });
            }
            else
            {
                NotificationManager.Show("info", "warn_point_enabled_noty").Perform();
            }
        }

        private async void BtnCompression_ClickButton(object sender, EventArgs e)
        {
            VistaFolderBrowserDialog folderDialog = new VistaFolderBrowserDialog();

            if (folderDialog.ShowDialog() == false)
            {
                return;
            }

            string selectedPath = folderDialog.SelectedPath;
            if (await _ntfsCompressor.IsSupportNtfs(selectedPath))
            {
                if ((new DirectoryInfo(selectedPath).Attributes & FileAttributes.Compressed) != FileAttributes.Compressed)
                {
                    BackgroundQueue backgroundQueue = new BackgroundQueue();
                    await backgroundQueue.QueueTask(delegate
                    {
                        try { _ntfsCompressor.SetCompression(selectedPath, true); }
                        catch { NotificationManager.Show("warn", "error_compression_noty").Perform(); }
                    });
                    await backgroundQueue.QueueTask(delegate { NotificationManager.Show("info", "success_compression_noty").WithDelay(500).Perform(); });
                }
                else
                {
                    NotificationManager.Show("info", "ready_compression_noty").Perform();
                }
            }
            else
            {
                NotificationManager.Show("warn", "notsupport_ntfs_noty").Perform();
            }
        }

        private async void BtnDecompression_ClickButton(object sender, EventArgs e)
        {
            VistaFolderBrowserDialog folderDialog = new VistaFolderBrowserDialog();

            if (folderDialog.ShowDialog() == false)
            {
                return;
            }

            string selectedPath = folderDialog.SelectedPath;
            if (await _ntfsCompressor.IsSupportNtfs(selectedPath))
            {
                if ((new DirectoryInfo(selectedPath).Attributes & FileAttributes.Compressed) == FileAttributes.Compressed)
                {
                    BackgroundQueue backgroundQueue = new BackgroundQueue();
                    await backgroundQueue.QueueTask(delegate
                    {
                        try { _ntfsCompressor.SetCompression(selectedPath, false); }
                        catch { NotificationManager.Show("warn", "error_compression_noty").Perform(); }

                    });
                    await backgroundQueue.QueueTask(delegate { NotificationManager.Show("info", "success_decompression_noty").WithDelay(500).Perform(); });
                }
                else
                {
                    NotificationManager.Show("info", "ready_decompression_noty").Perform();
                }
            }
            else
            {
                NotificationManager.Show("warn", "notsupport_ntfs_noty").Perform();
            }
        }

        private async void BtnClear_ClickButton(object sender, EventArgs e)
        {
            ClearingMemory clearingMemory = new ClearingMemory();

            if (clearingMemory.IsWinOldExists && !_hasDeclined)
            {
                OverlayDialogManager overlayDialog = new OverlayDialogManager(Overlay, OpacityProperty, BtnAgree, BtnDecline, onSecondary: () => _hasDeclined = true);
                _isWinOldRemoval = await overlayDialog.Show();
            }

            BackgroundQueue backgroundQueue = new BackgroundQueue();
            await backgroundQueue.QueueTask(delegate { clearingMemory.StartMemoryCleanup(_isWinOldRemoval); });
            await backgroundQueue.QueueTask(delegate { NotificationManager.Show("info", "clear_ram_noty").WithDelay(500).Perform(); });
        }

        private async void BtnRegExport_ClickButton(object sender, EventArgs e)
        {
            VistaSaveFileDialog vistaSaveFileDialog = new VistaSaveFileDialog
            {
                Filter = "Registry Files (*.reg)|*.reg",
                FileName = "RegBackup",
                DefaultExt = "reg"
            };

            if (vistaSaveFileDialog.ShowDialog() != true)
            {
                return;
            }

            OverlayWindow overlayWindow = new OverlayWindow();
            overlayWindow.Show();
            RegistryExporter registryExporter = new RegistryExporter();
            BackgroundQueue backgroundQueue = new BackgroundQueue();
            await backgroundQueue.QueueTask(delegate { registryExporter.Export(vistaSaveFileDialog.FileName); });
            overlayWindow.Close();
        }
    }
}
