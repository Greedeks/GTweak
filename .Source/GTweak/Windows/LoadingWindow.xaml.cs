using GTweak.Utilities.Configuration;
using GTweak.Utilities.Control;
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

        private async void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            for (byte i = 0; i <= 100; i++)
            {
                (sender as BackgroundWorker)?.ReportProgress(i);
                Thread.Sleep(17);
            }

            Parallel.Invoke(
                delegate { SettingsRepository.PID = TrustedInstaller.StartTrustedInstallerService(); },
                new SettingsRepository().СheckingParameters,
                new WindowsLicense().LicenseStatus,
                new SystemDiagnostics().GetHardwareData,
                new SystemDiagnostics().ValidateVersionUpdates,
                new UninstallingPakages().LoadInstalledPackages,
                new UninstallingPakages().CheckingForLocalAccount,
                new SystemTweaks().ViewNetshState,
                new SystemTweaks().ViewBluetoothStatus);

            await new MonitoringSystem().GetTotalProcessorUsage();
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
