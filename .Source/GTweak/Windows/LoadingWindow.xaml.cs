using GTweak.Utilities.Configuration;
using GTweak.Utilities.Controls;
using GTweak.Utilities.Helpers;
using GTweak.Utilities.Helpers.Animation;
using GTweak.Utilities.Tweaks;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace GTweak.Windows
{
    public partial class LoadingWindow : Window
    {
        public LoadingWindow()
        {
            App.ViewingSettings();

            InitializeComponent();

            BlockRunTweaker.CheckingApplicationCopies();
            BlockRunTweaker.CheckingSystemRequirements();
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
                this.Close();
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


            static void ExecuteWithLogging(Action action, string methodName)
            {
                try { action(); }
                catch (Exception ex) { ErrorLogging.LogWritingFile(ex, methodName); }
            }


            Parallel.Invoke(
                () => ExecuteWithLogging(() => SettingsRepository.PID = TrustedInstaller.StartTrustedInstallerService(), nameof(SettingsRepository.PID)),
                () => ExecuteWithLogging(new SettingsRepository().СheckingParameters, nameof(SettingsRepository.СheckingParameters)),
                () => ExecuteWithLogging(new WindowsLicense().LicenseStatus, nameof(WindowsLicense.LicenseStatus)),
                () => ExecuteWithLogging(new SystemDiagnostics().GetHardwareData, nameof(SystemDiagnostics.GetHardwareData)),
                () => ExecuteWithLogging(new SystemDiagnostics().ValidateVersionUpdates, nameof(SystemDiagnostics.ValidateVersionUpdates)),
                () => ExecuteWithLogging(new UninstallingPakages().LoadInstalledPackages, nameof(UninstallingPakages.LoadInstalledPackages)),
                () => ExecuteWithLogging(new UninstallingPakages().CheckingForLocalAccount, nameof(UninstallingPakages.CheckingForLocalAccount)),
                () => ExecuteWithLogging(new SystemTweaks().ViewNetshState, nameof(SystemTweaks.ViewNetshState)),
                () => ExecuteWithLogging(new SystemTweaks().ViewBluetoothStatus, nameof(SystemTweaks.ViewBluetoothStatus))
            );


            MonitoringSystem monitoringSystem = new MonitoringSystem();
            ExecuteWithLogging(async () => await monitoringSystem.GetTotalProcessorUsage(), nameof(monitoringSystem.GetTotalProcessorUsage));
        }

        private void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            int index = Array.IndexOf(new int[] { 5, 30, 55, 75, 95 }, e.ProgressPercentage);
            if (index >= 0)
                new TypewriterAnimation((string)FindResource($"text{++index}_load"), TextLoad, TimeSpan.FromMilliseconds(200));
        }


        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Closing -= Window_Closing;
            e.Cancel = true;
            DoubleAnimation doubleAnim = new DoubleAnimation(0, TimeSpan.FromSeconds(0.1));
            doubleAnim.Completed += delegate { this.Close(); };
            this.BeginAnimation(OpacityProperty, doubleAnim);
        }
    }
}
