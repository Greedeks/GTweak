using GTweak.Utilities;
using GTweak.Utilities.Helpers;
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
            backgroundWorker.DoWork += BackgroundWorker_DoWorkAsync;
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

        private void BackgroundWorker_DoWorkAsync(object sender, DoWorkEventArgs e)
        {
            for (byte i = 0; i <= 100; i++)
            {
                (sender as BackgroundWorker)?.ReportProgress(i);
                Thread.Sleep(17);
            }

            Parallel.Invoke(
                delegate { Settings.PID = TrustedInstaller.StartTrustedInstallerService(); },
                new Settings().СheckingParameters,
                new WindowsLicense().LicenseStatus,
                new SystemData.СomputerСonfiguration().GetConfiguration,
                new SearchUpdates().StartСhecking,
                new UninstallingPakages().ViewInstalledPackages,
                new UninstallingPakages().CheckingForLocalAccount,
                new SystemTweaks().ViewNetshState,
                new SystemTweaks().ViewBluetoothStatus);
        }

        private void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            switch (e.ProgressPercentage)
            {
                case 5:
                    new TypewriterAnimation((string)FindResource("text1_load"), TextLoad, TimeSpan.FromMilliseconds(200));
                    break;
                case 30:
                    new TypewriterAnimation((string)FindResource("text2_load"), TextLoad, TimeSpan.FromMilliseconds(200));
                    break;
                case 55:
                    new TypewriterAnimation((string)FindResource("text3_load"), TextLoad, TimeSpan.FromMilliseconds(200));
                    break;
                case 75:
                    new TypewriterAnimation((string)FindResource("text4_load"), TextLoad, TimeSpan.FromMilliseconds(200));
                    break;
                case 95:
                    new TypewriterAnimation((string)FindResource("text5_load"), TextLoad, TimeSpan.FromMilliseconds(200));
                    break;
            }
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
