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
        public string DownloadVersion { get; set; } = SystemDiagnostics.DownloadVersion;

        public UpdateWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
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
                    SizeByte.Text = $"{Math.Round(e.BytesReceived / 1024.0)} KB / {Math.Round(e.TotalBytesToReceive / 1024.0)} KB";
                };
                webClient.DownloadFileCompleted += delegate
                {
                    CommandExecutor.RunCommand($"/c taskkill /f /im \"{SettingsEngine.currentName}\" && timeout /t 2 && del \"{SettingsEngine.currentLocation}\" && ren {tempFileName} \"{SettingsEngine.currentName}\" && \"{SettingsEngine.currentLocation}\"");
                };
                webClient.DownloadFileAsync(new Uri("https://github.com/Greedeks/GTweak/releases/latest/download/GTweak.exe"), tempFileName);
            }
            catch
            {
                Close();
                NotificationManager.Show("warn", "warn_update_noty").Perform();
            }
        }
    }

}
