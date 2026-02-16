using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
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
        private readonly SystemDataCollector _systemDataCollector = new SystemDataCollector();
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
            backgroundWorker.RunWorkerCompleted += async delegate
            {
                TypewriterAnimation.Create((string)FindResource("step7_load"), StatusLoading, TimeSpan.FromMilliseconds(300));
                await Task.Delay(400);
                new MainWindow().Show();
                Close();
            };
            backgroundWorker.RunWorkerAsync();
        }

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            for (byte i = 0; i <= 100; i++)
            {
                (sender as BackgroundWorker)?.ReportProgress(i);
                Thread.Sleep(17);
            }

            static void ExecuteWithLogging(Action action, string member)
            {
                try { action(); }
                catch (Exception ex) { ErrorLogging.LogWritingFile(ex, member); }
            }

            ExecuteWithLogging(TrustedInstaller.StartTrustedInstallerService, nameof(TrustedInstaller.StartTrustedInstallerService));
            ExecuteWithLogging(WinLicenseHandler.LicenseStatus, nameof(WinLicenseHandler.LicenseStatus));
            ExecuteWithLogging(() => _systemDataCollector.GetHardwareData(), nameof(_systemDataCollector.GetHardwareData));
            ExecuteWithLogging(_systemDataCollector.ValidateVersionUpdates, nameof(_systemDataCollector.ValidateVersionUpdates));
            ExecuteWithLogging(_uninstallingPakages.GetInstalledPackages, nameof(_uninstallingPakages.GetInstalledPackages));
            ExecuteWithLogging(UninstallingPakages.CheckingForLocalAccount, nameof(UninstallingPakages.CheckingForLocalAccount));
            ExecuteWithLogging(SystemTweaks.ViewNetshState, nameof(SystemTweaks.ViewNetshState));
            ExecuteWithLogging(SystemTweaks.ViewBluetoothStatus, nameof(SystemTweaks.ViewBluetoothStatus));
            ExecuteWithLogging(SystemTweaks.ViewConfigTick, nameof(SystemTweaks.ViewConfigTick));
            ExecuteWithLogging(() => HardwareData.RunningProcessesCount = _systemDataCollector.GetProcessCount().GetAwaiter().GetResult(), nameof(_systemDataCollector.GetProcessCount));
            ExecuteWithLogging(() => HardwareData.RunningServicesCount = _systemDataCollector.GetServicesCount().GetAwaiter().GetResult(), nameof(_systemDataCollector.GetServicesCount));
            ExecuteWithLogging(() => _systemDataCollector.GetTotalProcessorUsage().GetAwaiter().GetResult(), nameof(_systemDataCollector.GetTotalProcessorUsage));
            ExecuteWithLogging(() => _systemDataCollector.GetPhysicalAvailableMemory().GetAwaiter().GetResult(), nameof(_systemDataCollector.GetPhysicalAvailableMemory));
            ExecuteWithLogging(RunGuard.CheckingDefenderExclusions, nameof(RunGuard.CheckingDefenderExclusions));
        }

        private void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            int index = Array.IndexOf(new[] { 0, 10, 30, 55, 75, 95 }, e?.ProgressPercentage ?? 0);
            if (index > 0)
            {
                TypewriterAnimation.Create((string)FindResource($"step{++index}_load"), StatusLoading, TimeSpan.FromMilliseconds(200));
            }
        }
    }
}
