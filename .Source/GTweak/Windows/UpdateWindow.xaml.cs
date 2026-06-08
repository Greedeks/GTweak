using System;
using System.Net;
using System.Windows;
using System.Windows.Input;
using GTweak.Utilities.Configuration;
using GTweak.Utilities.Controls;
using GTweak.Utilities.Helpers;
using GTweak.Utilities.Managers;
using Wpf.Ui.Controls;

namespace GTweak.Windows
{
    public partial class UpdateWindow : FluentWindow
    {
        public string DownloadVersion { get; set; } = NetworkProvider.DownloadVersion;

        public UpdateWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e?.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                string tempFileName = $"GTweak_{Guid.NewGuid().ToString("N").Substring(0, 8)}.exe";

                using WebClient webClient = new WebClient();
                webClient.DownloadProgressChanged += (_, e) =>
                {
                    ProgressDownload.Value = e.ProgressPercentage;
                    SizeByte.Text = $"{e.BytesReceived / 1048576.0:F1} MB / {e.TotalBytesToReceive / 1048576.0:F1} MB";
                };
                webClient.DownloadFileCompleted += delegate
                {
                    CommandExecutor.RunCommand($"/c taskkill /f /im \"{SettingsEngine.CurrentName}\" && timeout /t 2 && del \"{SettingsEngine.CurrentLocation}\" && ren {tempFileName} \"{SettingsEngine.CurrentName}\" && \"{SettingsEngine.CurrentLocation}\"");
                };
                webClient.DownloadFileAsync(new Uri(PathLocator.Links.LatestUpdate.Resolved), tempFileName);
            }
            catch
            {
                Close();
                NotificationManager.Show("warn", "warn_update_noty").Perform();
            }
        }
    }

}
