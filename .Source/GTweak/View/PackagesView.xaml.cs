using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using GTweak.Core.Interfaces;
using GTweak.Modules.Common;
using GTweak.Modules.Managers;
using GTweak.Modules.Tweaks;

namespace GTweak.View
{
    public partial class PackagesView : UserControl, IViewMarker
    {
        private TimerControlManager _timer = default;
        private readonly BackgroundQueueManager _backgroundQueue = new BackgroundQueueManager();
        private readonly BackgroundWorker backgroundWorker = new BackgroundWorker();
        private readonly AppxPackageHandler _packageHandler = new AppxPackageHandler();
        private bool? _isWebViewRemoval = false;

        public PackagesView()
        {
            InitializeComponent();

            Loaded += delegate
            {
                backgroundWorker.DoWork += delegate { _packageHandler.GetInstalledPackages(); };
                backgroundWorker.RunWorkerCompleted += delegate { Dispatcher.Invoke(() => { AppxPackageHandler.OnPackagesChanged(); }); };

                _timer = new TimerControlManager(TimeSpan.Zero, TimerControlManager.TimerMode.CountUp, time =>
                {
                    if ((int)time.TotalSeconds % 3 == 0)
                    {
                        if (!backgroundWorker.IsBusy)
                        {
                            backgroundWorker.RunWorkerAsync();
                        }
                    }
                });

                _timer.Start();
            };
            Unloaded += delegate
            {
                _timer.Stop();
                OverlayDialogManager.Close();
            };
        }

        private void Package_MouseEnter(object sender, MouseEventArgs e) => DescBlock.ContentSource = sender;
        private void Package_MouseLeave(object sender, MouseEventArgs e) => DescBlock.ContentSource = null;

        private async void ToggleButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ToggleButton toggleButton = (ToggleButton)sender;
            string packageName = toggleButton.Name;

            if (toggleButton.IsChecked == false && packageName == "OneDrive")
            {
                if (string.IsNullOrWhiteSpace(PathTargets.Executable.OneDriveSetup))
                {
                    NotificationManager.Show("warn", "error_onedrive_noty").Perform();
                }
                else
                {
                    NotificationManager.Show("info", "success_onedrive_noty").Perform();

                    await _backgroundQueue.QueueTask(async () =>
                    {
                        await Dispatcher.InvokeAsync(() => { AppxPackageHandler.HandleAvailabilityStatus(packageName, true); });

                        try { await AppxPackageHandler.RestoreOneDriveFolder(); }
                        finally { await Dispatcher.InvokeAsync(() => { AppxPackageHandler.HandleAvailabilityStatus(packageName, false); }); }
                    });
                }
            }
            else if (toggleButton.IsChecked == false)
            {
                e.Handled = true;
                return;
            }
            else if (toggleButton.IsChecked == true)
            {
                if (packageName.Equals("Edge"))
                {
                    _isWebViewRemoval = await OverlayDialogManager.Show("title_over_pkg", "text_over_pkg", "question_over_pkg", "btn_delete_all", "btn_keep_webview");

                    if (_isWebViewRemoval == null)
                    {
                        return;
                    }
                }

                await _backgroundQueue.QueueTask(async () =>
                {
                    await Dispatcher.InvokeAsync(() => { AppxPackageHandler.HandleAvailabilityStatus(packageName, true); });

                    try { await AppxPackageHandler.RemoveAppxPackage(packageName, (bool)_isWebViewRemoval); }
                    finally { await Dispatcher.InvokeAsync(() => { AppxPackageHandler.HandleAvailabilityStatus(packageName, false); }); }

                    await Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (ExplorerManager.PackageActions.TryGetValue(packageName, out ExplorerManager.ExplorerAction explorerAction))
                        {
                            ExplorerManager.Restart();
                        }
                    }), DispatcherPriority.ApplicationIdle);
                });
            }
        }
    }
}
