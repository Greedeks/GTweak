using GTweak.Utilities.Configuration;
using GTweak.Utilities.Controls;
using GTweak.Utilities.Helpers;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace GTweak.Windows
{
    public partial class UpdateWindow : Window
    {
        public UpdateWindow()
        {
            InitializeComponent();
            CurrentVerison.Text = SettingsRepository.currentRelease;
            DownloadVersion.Text = NewVerison.Text = SystemDiagnostics.DownloadVersion;
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DoubleAnimation doubleAnim = new DoubleAnimation()
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new QuadraticEase()
            };
            Timeline.SetDesiredFrameRate(doubleAnim, 400);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Closing -= Window_Closing;
            e.Cancel = true;
            DoubleAnimation doubleAnim = new DoubleAnimation(0, (Duration)TimeSpan.FromSeconds(0.1));
            doubleAnim.Completed += delegate { this.Close(); };
            Timeline.SetDesiredFrameRate(doubleAnim, 400);
            BeginAnimation(OpacityProperty, doubleAnim);
        }

        private void BtnExit_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.Close();
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
                    using WebClient webClient = new WebClient();
                    webClient.DownloadProgressChanged += (s, e) =>
                    {
                        ProgressDownload.Value = e.ProgressPercentage;
                        SizeByte.Text = $"{Math.Round(e.BytesReceived / 1024.0)} KB / {Math.Round(e.TotalBytesToReceive / 1024.0)} KB";
                    };
                    webClient.DownloadFileCompleted += delegate
                    {
                        CommandExecutor.RunCommand($"/c taskkill /f /im \"{SettingsRepository.currentName}\" && timeout /t 1 && del \"{SettingsRepository.currentLocation}\" && ren Git-GTweak.exe \"{SettingsRepository.currentName}\" &&  \"{SettingsRepository.currentLocation}\"");
                    };
                    webClient.DownloadFileAsync(new Uri("https://github.com/Greedeks/GTweak/releases/latest/download/GTweak.exe"), "Git-GTweak.exe");
                }
                catch
                {
                    this.Close();
                    new ViewNotification().Show("", "warn", "warn_update_notification");
                }
            }
        }
    }
}
