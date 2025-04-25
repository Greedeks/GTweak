using GTweak.Utilities.Configuration;
using GTweak.Utilities.Controls;
using GTweak.Utilities.Helpers;
using GTweak.Utilities.Tweaks;
using GTweak.Windows;
using Ookii.Dialogs.Wpf;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

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

        private void OverlayAnimation(double from, double to, double duration, EventHandler onComplete = null)
        {
            DoubleAnimation doubleAnim = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = TimeSpan.FromSeconds(duration),
                EasingFunction = new QuadraticEase()
            };
            if (onComplete != null) { doubleAnim.Completed += onComplete; }
            Timeline.SetDesiredFrameRate(doubleAnim, 240);
            Overlay.BeginAnimation(OpacityProperty, doubleAnim);
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

        private async void BtnClear_ClickButton(object sender, EventArgs e)
        {
            if (ClearingMemory.IsWinOldExists && !_hasDeclined)
            {
                Overlay.Visibility = Visibility.Visible;

                OverlayAnimation(0, 1, 0.3);

                TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

                void AgreeHandler(object sender, RoutedEventArgs args)
                {
                    tcs.TrySetResult(true);
                    BtnAgree.PreviewMouseLeftButtonDown -= AgreeHandler;
                    BtnDecline.PreviewMouseLeftButtonDown -= DeclineHandler;
                }

                void DeclineHandler(object sender, RoutedEventArgs args)
                {
                    tcs.TrySetResult(false);
                    BtnAgree.PreviewMouseLeftButtonDown -= AgreeHandler;
                    BtnDecline.PreviewMouseLeftButtonDown -= DeclineHandler;
                    _hasDeclined = true;
                }

                BtnAgree.PreviewMouseLeftButtonDown += AgreeHandler;
                BtnDecline.PreviewMouseLeftButtonDown += DeclineHandler;

                try { _isWinOldRemoval = await tcs.Task; }
                catch (TaskCanceledException) { _isWinOldRemoval = false; }

                OverlayAnimation(1, 0, 0.25, (s, e) => Overlay.Visibility = Visibility.Collapsed);
            }

            BackgroundQueue backgroundQueue = new BackgroundQueue();
            await backgroundQueue.QueueTask(delegate { ClearingMemory.StartMemoryCleanupAsync(_isWinOldRemoval); });
            new ViewNotification(500).Show("", "info", "clear_ram_notification");
        }

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
