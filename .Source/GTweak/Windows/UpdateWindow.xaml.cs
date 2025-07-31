using GTweak.Utilities.Animation;
using GTweak.Utilities.Configuration;
using GTweak.Utilities.Controls;
using GTweak.Utilities.Helpers;
using GTweak.Utilities.Managers;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Input;

namespace GTweak.Windows
{
    public partial class UpdateWindow
    {
        public UpdateWindow()
        {
            InitializeComponent();
            CurrentVerison.Text = SettingsEngine.currentRelease;
            DownloadVersion.Text = NewVerison.Text = SystemDiagnostics.DownloadVersion;
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e) => BeginAnimation(OpacityProperty, FactoryAnimation.CreateIn(0, 1, 0.3));

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Closing -= Window_Closing;
            e.Cancel = true;
            BeginAnimation(OpacityProperty, FactoryAnimation.CreateTo(0.1, () => { Close(); }));
        }

        private void BtnExit_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                Close();
        }

        private void BtnLog_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                Process.Start("https://github.com/Greedeks/GTweak/releases/latest");
        }

        private void BtnUpdate_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                QuestionUpdate.Visibility = Visibility.Hidden;
                DownloadUpdate.Visibility = Visibility.Visible;

                try
                {
                    string tempFileName = $"GTweak_{Guid.NewGuid().ToString("N").Substring(0, 8)}.exe";

                    using WebClient webClient = new WebClient();
                    webClient.DownloadProgressChanged += (s, e) =>
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
                    new NotificationManager().Show("warn", "warn_update_notification").None();
                }
            }
        }
    }
}
