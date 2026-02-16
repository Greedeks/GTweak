using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using GTweak.Utilities.Controls;
using GTweak.Utilities.Helpers;
using GTweak.Utilities.Managers;
using GTweak.Utilities.Tweaks;

namespace GTweak.View
{
    public partial class PakagesView : UserControl
    {
        private TimerControlManager _timer = default;
        private readonly BackgroundQueue _backgroundQueue = new BackgroundQueue();
        private readonly BackgroundWorker backgroundWorker = new BackgroundWorker();
        private readonly UninstallingPakages _uninstalling = new UninstallingPakages();
        private bool _isWebViewRemoval = false;

        public PakagesView()
        {
            InitializeComponent();

            Unloaded += delegate { _timer.Stop(); };
            Loaded += delegate
            {
                backgroundWorker.DoWork += delegate { _uninstalling.GetInstalledPackages(); };
                backgroundWorker.RunWorkerCompleted += delegate { Dispatcher.Invoke(() => { UninstallingPakages.OnPackagesChanged(); }); };

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
        }

        private void Package_MouseEnter(object sender, MouseEventArgs e)
        {
            string description = ((ToggleButton)sender).ToolTip?.ToString() ?? string.Empty;

            if (DescBlock.Text != description)
            {
                DescBlock.Text = description;
            }
        }

        private void Package_MouseLeave(object sender, MouseEventArgs e) => DescBlock.Text = DescBlock.DefaultText;

        private async void ToggleButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ToggleButton toggleButton = (ToggleButton)sender;
            string packageName = toggleButton.Name;

            if (toggleButton.IsChecked == false && packageName == "OneDrive")
            {
                if (string.IsNullOrWhiteSpace(PathLocator.Executable.OneDrive))
                {
                    NotificationManager.Show("warn", "error_onedrive_noty").Perform();
                }
                else
                {
                    NotificationManager.Show("info", "success_onedrive_noty").Perform();

                    await _backgroundQueue.QueueTask(async () =>
                    {
                        await Dispatcher.InvokeAsync(() => { UninstallingPakages.HandleAvailabilityStatus(packageName, true); });

                        try { await UninstallingPakages.RestoreOneDriveFolder(); }
                        finally { await Dispatcher.InvokeAsync(() => { UninstallingPakages.HandleAvailabilityStatus(packageName, false); }); }
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
                    OverlayDialogManager overlayDialog = new OverlayDialogManager(Overlay, OpacityProperty, BtnDelete, BtnCancel);
                    _isWebViewRemoval = await overlayDialog.Show();
                }

                await _backgroundQueue.QueueTask(async () =>
                {
                    await Dispatcher.InvokeAsync(() => { UninstallingPakages.HandleAvailabilityStatus(packageName, true); });

                    try { await UninstallingPakages.RemoveAppxPackage(packageName, _isWebViewRemoval); }
                    finally { await Dispatcher.InvokeAsync(() => { UninstallingPakages.HandleAvailabilityStatus(packageName, false); }); }

                    await Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (ExplorerManager.PackageMapping.TryGetValue(packageName, out bool needRestart))
                        {
                            ExplorerManager.Restart(new Process());
                        }
                    }), DispatcherPriority.ApplicationIdle);
                });
            }
        }
    }
}
