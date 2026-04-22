using System;
using System.ComponentModel;
using System.Threading;
using GTweak.Utilities.Animation;
using GTweak.Utilities.Configuration;
using GTweak.Utilities.Controls;
using GTweak.Utilities.Helpers;
using GTweak.Utilities.Maintenance;
using GTweak.Utilities.Tweaks;
using Wpf.Ui.Controls;

namespace GTweak.Windows
{
    public partial class LoadingWindow : FluentWindow
    {
        private readonly HardwareProvider _hardwareProvider = new HardwareProvider();
        private readonly UninstallingPakages _uninstallingPakages = new UninstallingPakages();

        public LoadingWindow()
        {
            InitializeComponent();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            using BackgroundWorker backgroundWorker = new BackgroundWorker();
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.DoWork += BackgroundWorker_DoWork;
            backgroundWorker.ProgressChanged += BackgroundWorker_ProgressChanged;
            backgroundWorker.RunWorkerCompleted += delegate
            {
                new MainWindow().Show();
                Close();
            };

            backgroundWorker.RunWorkerAsync();
        }

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            void ReportStep(byte stepId, int percent, int delay = 0)
            {
                worker?.ReportProgress(percent, stepId);

                if (delay > 0)
                {
                    Thread.Sleep(delay);
                }
            }

            static void ExecuteWithLogging(Action action, string member)
            {
                try { action(); }
                catch (Exception ex) { ErrorLogging.LogWritingFile(ex, false, member); }
            }

            ReportStep(1, 15);
            ExecuteWithLogging(WinLicenseHandler.LicenseStatus, nameof(WinLicenseHandler.LicenseStatus));
            ExecuteWithLogging(TrustedInstaller.StartTrustedInstallerService, nameof(TrustedInstaller.StartTrustedInstallerService));
            ExecuteWithLogging(RunGuard.CheckingDefenderExclusions, nameof(RunGuard.CheckingDefenderExclusions));

            ReportStep(2, 35);
            ExecuteWithLogging(SystemTweaks.ViewConfigTick, nameof(SystemTweaks.ViewConfigTick));
            ExecuteWithLogging(SystemTweaks.ViewBluetoothStatus, nameof(SystemTweaks.ViewBluetoothStatus));
            ExecuteWithLogging(SystemTweaks.ViewNetshState, nameof(SystemTweaks.ViewNetshState));

            ReportStep(3, 55);
            ExecuteWithLogging(_hardwareProvider.GetHardwareData, nameof(_hardwareProvider.GetHardwareData));
            ExecuteWithLogging(() => _hardwareProvider.GetProcessCount().GetAwaiter().GetResult(), nameof(_hardwareProvider.GetProcessCount));
            ExecuteWithLogging(() => _hardwareProvider.GetServicesCount().GetAwaiter().GetResult(), nameof(_hardwareProvider.GetServicesCount));
            ExecuteWithLogging(() => _hardwareProvider.GetTotalProcessorUsage().GetAwaiter().GetResult(), nameof(_hardwareProvider.GetTotalProcessorUsage));
            ExecuteWithLogging(() => _hardwareProvider.GetPhysicalAvailableMemory().GetAwaiter().GetResult(), nameof(_hardwareProvider.GetPhysicalAvailableMemory));

            ReportStep(4, 75, 300);
            ExecuteWithLogging(_uninstallingPakages.GetInstalledPackages, nameof(_uninstallingPakages.GetInstalledPackages));
            ExecuteWithLogging(UninstallingPakages.CheckingForLocalAccount, nameof(UninstallingPakages.CheckingForLocalAccount));

            ReportStep(5, 90);
            ExecuteWithLogging(() => _hardwareProvider.GetUserIpAddress().GetAwaiter().GetResult(), nameof(_hardwareProvider.GetUserIpAddress));
            ExecuteWithLogging(() => _hardwareProvider.ValidateVersionUpdates().GetAwaiter().GetResult(), nameof(_hardwareProvider.ValidateVersionUpdates));
            ReportStep(6, 100, 400);
        }

        private void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState is byte index)
            {
                TypewriterAnimation.Create((string)FindResource($"step{index}_load"), StatusLoading, TimeSpan.FromMilliseconds(200));
            }
        }
    }
}