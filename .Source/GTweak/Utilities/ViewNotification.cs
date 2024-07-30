using GTweak.Properties;
using GTweak.Windows;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace GTweak.Utilities
{
    internal sealed class ViewNotification
    {
        private static bool isAlreadyLaunch = false;

        internal async void Show(string action, string tittle = "", string content = "")
        {
            if (Settings.IsViewNotification)
            {
                new Thread(() => new Settings().RunAnalysis()).Start();
                new Thread(() => new Settings().RunAnalysis()).IsBackground = true;

                await Task.Delay(300);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    NotificationWindow notificationWindow = new NotificationWindow();

                    Parallel.Invoke(() =>
                    {
                        if (notificationWindow.IsLoaded || isAlreadyLaunch)
                            return;

                        isAlreadyLaunch = true;

                        notificationWindow = new NotificationWindow
                        {
                            TitleNotice = tittle,
                            TextNotice = content,
                            ActionChoice = action,
                        };

                        notificationWindow.Show();
                        notificationWindow.Closed += (s, e) => { isAlreadyLaunch = false; };

                    });
                });
            }
        }

        internal void CheckingTempFile()
        {
            if (File.Exists(Settings.PathIcon) == false)
            {
                byte[] _iconByte = default;
                using (MemoryStream fileOut = new MemoryStream(Resources.GTweak))
                using (GZipStream gz = new GZipStream(fileOut, CompressionMode.Decompress))
                using (MemoryStream ms = new MemoryStream())
                {
                    gz.CopyTo(ms);
                    _iconByte = ms.ToArray();
                }
                Directory.CreateDirectory(Settings.PathTempFiles);
                File.WriteAllBytes(Settings.PathIcon, _iconByte);
            }

            if (File.Exists(Settings.PathSound) == false)
            {
                byte[] _soundbyte = default;
                using (MemoryStream fileOut = new MemoryStream(Resources.Sound))
                using (GZipStream gz = new GZipStream(fileOut, CompressionMode.Decompress))
                using (MemoryStream ms = new MemoryStream())
                {
                    gz.CopyTo(ms);
                    _soundbyte = ms.ToArray();
                }

                Directory.CreateDirectory(Settings.PathTempFiles);
                File.WriteAllBytes(Settings.PathSound, _soundbyte);
            }

        }
    }
}
