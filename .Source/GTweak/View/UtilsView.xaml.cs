using System;
using System.IO;
using System.Windows.Controls;
using GTweak.Core.Interfaces;
using GTweak.Modules.Common;
using GTweak.Modules.Configuration;
using GTweak.Modules.Maintenance;
using GTweak.Modules.Managers;
using GTweak.Windows;
using Ookii.Dialogs.Wpf;

namespace GTweak.View
{
    public partial class UtilsView : UserControl, IViewMarker
    {
        private readonly SystemRestoreService _systemRestore = new SystemRestoreService();
        private readonly NTFSCompressor _ntfsCompressor = new NTFSCompressor();

        private bool? _isWinOldRemoval = false;

        public UtilsView()
        {
            InitializeComponent();

            Unloaded += delegate { OverlayDialogManager.Close(); };
        }

        private async void BtnLicenseWindows_ClickButton(object sender, EventArgs e)
        {
            if (WinLicenseHandler.IsWindowsActivated)
            {
                NotificationManager.Show("info", "ready_activate_noty").Perform();
            }
            else
            {
                if (await new NetworkProvider().IsNetworkAvailable())
                {
                    await WinLicenseHandler.StartActivation();
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
            BackgroundQueueManager backgroundQueue = new BackgroundQueueManager();

            if (_systemRestore == null)
            {
                overlayWindow.Close();
                NotificationManager.Show("warn", "error_point_noty").WithDelay(300).Perform();
                return;
            }

            try { await backgroundQueue.QueueTask(delegate { _systemRestore.CreateRestorePoint(); }); }
            catch (Exception ex) { ErrorLogger.LogDebug(ex); }
            overlayWindow.Close();
        }

        private void BtnRecovey_ClickButton(object sender, EventArgs e) => _systemRestore.StartRecovery();

        private async void BtnDisablePoint_ClickButton(object sender, EventArgs e)
        {
            if (_systemRestore.IsPointCreationAllowed)
            {
                BackgroundQueueManager backgroundQueue = new BackgroundQueueManager();
                await backgroundQueue.QueueTask(delegate
                {
                    try { _systemRestore.DisableRestorePoint(); } catch (Exception ex) { ErrorLogger.LogDebug(ex); }
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
                BackgroundQueueManager backgroundQueue = new BackgroundQueueManager();
                await backgroundQueue.QueueTask(delegate
                {
                    try { _systemRestore.EnableRestorePoint(); } catch (Exception ex) { ErrorLogger.LogDebug(ex); }
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
                    BackgroundQueueManager backgroundQueue = new BackgroundQueueManager();
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
                    BackgroundQueueManager backgroundQueue = new BackgroundQueueManager();
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
            MemoryCleaner clearingMemory = new MemoryCleaner();

            if (clearingMemory.IsWinOldExists)
            {
                _isWinOldRemoval = await OverlayDialogManager.Show("title_over_more", "text_over_more", "question_over_more", "btn_agree", "btn_decline");

                if (_isWinOldRemoval == null)
                {
                    return;
                }
            }

            BackgroundQueueManager backgroundQueue = new BackgroundQueueManager();
            await backgroundQueue.QueueTask(delegate { clearingMemory.StartMemoryCleanup((bool)_isWinOldRemoval); });
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
            BackgroundQueueManager backgroundQueue = new BackgroundQueueManager();
            await backgroundQueue.QueueTask(delegate { registryExporter.Export(vistaSaveFileDialog.FileName); });
            overlayWindow.Close();
        }
    }
}
