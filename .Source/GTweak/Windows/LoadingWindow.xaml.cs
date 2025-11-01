﻿using GTweak.Utilities.Animation;
using GTweak.Utilities.Configuration;
using GTweak.Utilities.Controls;
using GTweak.Utilities.Helpers;
using GTweak.Utilities.Maintenance;
using GTweak.Utilities.Tweaks;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Wpf.Ui.Controls;

namespace GTweak.Windows
{
    public partial class LoadingWindow : FluentWindow
    {
        private readonly SystemDiagnostics _systemDiagnostics = new SystemDiagnostics();
        private readonly SystemTweaks _systemTweaks = new SystemTweaks();
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

            Parallel.Invoke(
                () => ExecuteWithLogging(TrustedInstaller.StartTrustedInstallerService, nameof(TrustedInstaller.StartTrustedInstallerService)),
                () => ExecuteWithLogging(WindowsLicense.LicenseStatus, nameof(WindowsLicense.LicenseStatus)),
                () => ExecuteWithLogging(_systemDiagnostics.GetHardwareData, nameof(_systemDiagnostics.GetHardwareData)),
                () => ExecuteWithLogging(_systemDiagnostics.ValidateVersionUpdates, nameof(_systemDiagnostics.ValidateVersionUpdates)),
                () => ExecuteWithLogging(_uninstallingPakages.GetInstalledPackages, nameof(_uninstallingPakages.GetInstalledPackages)),
                () => ExecuteWithLogging(_uninstallingPakages.CheckingForLocalAccount, nameof(_uninstallingPakages.CheckingForLocalAccount)),
                () => ExecuteWithLogging(_systemTweaks.ViewNetshState, nameof(_systemTweaks.ViewNetshState)),
                () => ExecuteWithLogging(_systemTweaks.ViewBluetoothStatus, nameof(_systemTweaks.ViewBluetoothStatus)),
                () => ExecuteWithLogging(_systemTweaks.ViewConfigTick, nameof(_systemTweaks.ViewConfigTick))
            );

            ExecuteWithLogging(() => HardwareData.RunningProcessesCount = _systemDiagnostics.GetProcessCount().Result, nameof(_systemDiagnostics.GetProcessCount));
            ExecuteWithLogging(() => HardwareData.RunningServicesCount = _systemDiagnostics.GetServicesCount().Result, nameof(_systemDiagnostics.GetServicesCount));
            ExecuteWithLogging(() => _systemDiagnostics.GetTotalProcessorUsage().GetAwaiter().GetResult(), nameof(_systemDiagnostics.GetTotalProcessorUsage));
            ExecuteWithLogging(() => _systemDiagnostics.GetPhysicalAvailableMemory().GetAwaiter().GetResult(), nameof(_systemDiagnostics.GetPhysicalAvailableMemory));

        }

        private void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            int index = Array.IndexOf(new[] { 0, 10, 30, 55, 75, 95 }, e.ProgressPercentage);
            if (index > 0)
                TypewriterAnimation.Create((string)FindResource($"step{++index}_load"), StatusLoading, TimeSpan.FromMilliseconds(200));
        }
    }
}
