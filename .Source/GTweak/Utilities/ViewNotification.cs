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
                Thread _thread = new Thread(() => new Settings().RunAnalysis()) { IsBackground = true };
                _thread.Start();

                await Task.Delay(300);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    Parallel.Invoke(async delegate
                    {
                        NotificationWindow notificationWindow = new NotificationWindow();

                        if (notificationWindow.IsLoaded || isAlreadyLaunch)
                            return;

                        isAlreadyLaunch = true;

                        notificationWindow = new NotificationWindow
                        {
                            TitleNotice = tittle,
                            TextNotice = content,
                            ActionChoice = action,
                        };
                        await Task.Delay(100);
                        notificationWindow.Show();
                        notificationWindow.Closed += delegate { isAlreadyLaunch = false; };
                    });
                });
            }
        }

        internal void CheckingTempFile()
        {
            if (!File.Exists(Settings.PathIcon))
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

            if (!File.Exists(Settings.PathSound))
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
