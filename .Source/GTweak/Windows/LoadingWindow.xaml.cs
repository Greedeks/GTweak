using GTweak.Utilities.Animation;
using GTweak.Utilities.Configuration;
using GTweak.Utilities.Controls;
using GTweak.Utilities.Helpers;
using GTweak.Utilities.Tweaks;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace GTweak.Windows
{
    public partial class LoadingWindow
    {
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
                new TypewriterAnimation((string)FindResource("text6_load"), TextLoad, TimeSpan.FromMilliseconds(300));
                await Task.Delay(310);
                Close();
                new MainWindow().Show();
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
                () => ExecuteWithLogging(new SystemDiagnostics().GetHardwareData, nameof(SystemDiagnostics.GetHardwareData)),
                () => ExecuteWithLogging(new SystemDiagnostics().ValidateVersionUpdates, nameof(SystemDiagnostics.ValidateVersionUpdates)),
                () => ExecuteWithLogging(new UninstallingPakages().LoadInstalledPackages, nameof(UninstallingPakages.LoadInstalledPackages)),
                () => ExecuteWithLogging(new UninstallingPakages().CheckingForLocalAccount, nameof(UninstallingPakages.CheckingForLocalAccount)),
                () => ExecuteWithLogging(new SystemTweaks().ViewNetshState, nameof(SystemTweaks.ViewNetshState)),
                () => ExecuteWithLogging(new SystemTweaks().ViewBluetoothStatus, nameof(SystemTweaks.ViewBluetoothStatus)),
                () => ExecuteWithLogging(new SystemTweaks().ViewConfigTick, nameof(SystemTweaks.ViewConfigTick))
            );

            ExecuteWithLogging(() => new MonitoringService().GetTotalProcessorUsage().GetAwaiter().GetResult(), nameof(MonitoringService.GetTotalProcessorUsage));
        }

        private void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            int index = Array.IndexOf(new[] { 5, 30, 55, 75, 95 }, e.ProgressPercentage);
            if (index >= 0)
                new TypewriterAnimation((string)FindResource($"text{++index}_load"), TextLoad, TimeSpan.FromMilliseconds(200));
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Closing -= Window_Closing;
            e.Cancel = true;
            BeginAnimation(OpacityProperty, FadeAnimation.FadeTo(0.1, () => { Close(); }));
        }

        private void Window_Loaded(object sender, System.Windows.RoutedEventArgs e) => BeginAnimation(OpacityProperty, FadeAnimation.FadeIn(1, 0.1));
    }
}
