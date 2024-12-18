using GTweak.Utilities.Helpers;
using GTweak.Windows;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;

namespace GTweak.Utilities
{
    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct UsePath
    {
        internal static string Config = string.Empty;
        internal static string FileLocation => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"GTweak");
        internal static string SystemDisk => Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System));
        internal static string Hosts => Path.Combine(Environment.SystemDirectory, @"drivers\etc\hosts");
    }

    internal sealed class Settings
    {
        internal sealed class WinmmMethods
        {
            [DllImport("winmm.dll")]
            public static extern int waveOutGetVolume(IntPtr hwo, out uint dwVolume);

            [DllImport("winmm.dll")]
            public static extern int waveOutSetVolume(IntPtr hwo, uint dwVolume);
        }

        private const uint volumeFirstStart = 50;
        private static bool _isViewNotification = true;
        private static bool _isPlayingSound = true;
        private static bool _isTopMost = false;
        private static bool _isСheckingUpdate = true;
        private static string _language = App.GettingSystemLanguage;
        private static string _theme = "Dark";
        internal static bool _isHiddenIpAddress = false;

        internal static int PID = 0;
        internal static string currentRelease = (Assembly.GetEntryAssembly() ?? throw new InvalidOperationException()).GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion.Split(' ').Last().Trim();
        internal static readonly string currentName = AppDomain.CurrentDomain.FriendlyName;
        internal static readonly string currentLocation = Assembly.GetExecutingAssembly().Location;

        internal static bool IsViewNotification { get => _isViewNotification; set => _isViewNotification = value; }
        internal static bool IsPlayingSound { get => _isPlayingSound; set => _isPlayingSound = value; }
        internal static bool IsTopMost { get => _isTopMost; set => _isTopMost = value; }
        internal static bool IsСheckingUpdate { get => _isСheckingUpdate; set => _isСheckingUpdate = value; }
        internal static string Language { get => _language; set => _language = value; }
        internal static string Theme { get => _theme; set => _theme = value; }
        internal static bool IsHiddenIpAddress { get => _isHiddenIpAddress; set => _isHiddenIpAddress = value; }

        internal void СheckingParameters()
        {
            RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(@"Software\GTweak");
            if (registryKey == null || registryKey.GetValue("Notification", null) == null || registryKey.GetValue("Sound", null) == null || registryKey.GetValue("TopMost", null) == null ||
            registryKey.GetValue("Update", null) == null || registryKey.GetValue("Language", null) == null || registryKey.GetValue("HiddenIP", null) == null || registryKey.GetValue("Theme", null) == null)
            {
                registryKey?.SetValue("Notification", IsViewNotification, RegistryValueKind.String);
                registryKey?.SetValue("Sound", IsPlayingSound, RegistryValueKind.String);
                registryKey?.SetValue("TopMost", IsTopMost, RegistryValueKind.String);
                registryKey?.SetValue("Update", IsСheckingUpdate, RegistryValueKind.String);
                registryKey?.SetValue("HiddenIP", IsHiddenIpAddress, RegistryValueKind.String);
                registryKey?.SetValue("Theme", Theme, RegistryValueKind.String);
                registryKey?.SetValue("Language", App.GettingSystemLanguage, RegistryValueKind.String);
                WinmmMethods.waveOutSetVolume(IntPtr.Zero, ((uint)(double)((ushort.MaxValue / 100) * volumeFirstStart) & 0x0000ffff) | ((uint)(double)((ushort.MaxValue / 100) * volumeFirstStart) << 16));
            }
            else
            {
                IsViewNotification = bool.Parse(registryKey?.GetValue("Notification").ToString() ?? "True");
                IsPlayingSound = bool.Parse(registryKey?.GetValue("Sound").ToString() ?? "True");
                IsTopMost = bool.Parse(registryKey?.GetValue("TopMost").ToString() ?? "False");
                IsСheckingUpdate = bool.Parse(registryKey?.GetValue("Update").ToString() ?? "True");
                IsHiddenIpAddress = bool.Parse(registryKey?.GetValue("HiddenIP").ToString() ?? "False");
                Language = registryKey?.GetValue("Language").ToString() ?? App.GettingSystemLanguage;
                Theme = registryKey?.GetValue("Theme").ToString() ?? "Dark";

            }
            registryKey?.Close();
        }

        internal static void ChangingParameters<T>(T value, string selection) => Parallel.Invoke(delegate
        {
            RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(@"Software\GTweak");
            registryKey?.SetValue(selection, value.ToString(), RegistryValueKind.String);

            switch (selection)
            {
                case "Notification":
                    IsViewNotification = bool.Parse(registryKey?.GetValue("Notification").ToString() ?? "True");
                    break;
                case "Sound":
                    IsPlayingSound = bool.Parse(registryKey?.GetValue("Sound").ToString() ?? "True");
                    break;
                case "TopMost":
                    IsTopMost = bool.Parse(registryKey?.GetValue("TopMost").ToString() ?? "False");
                    break;
                case "Update":
                    IsСheckingUpdate = bool.Parse(registryKey?.GetValue("Update").ToString() ?? "True");
                    break;
                case "Language":
                    Language = registryKey?.GetValue("Language").ToString() ?? App.GettingSystemLanguage;
                    break;
                case "Theme":
                    Theme = registryKey?.GetValue("Theme").ToString() ?? "Dark";
                    break;
                case "HiddenIP":
                    IsHiddenIpAddress = bool.Parse(registryKey?.GetValue("HiddenIP").ToString() ?? "False");
                    break;
            }

            registryKey?.Close();
        });

        internal static void SaveFileConfig()
        {
            if (INIManager.UserTweaksConfidentiality.Count == 0 && INIManager.UserTweaksInterface.Count == 0 && INIManager.UserTweaksServices.Count == 0 && INIManager.UserTweaksSystem.Count == 0)
                new ViewNotification().Show("", "info", (string)Application.Current.Resources["export_warning_notification"]);
            else
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    FileName = "Config GTweak",
                    Filter = "(*.INI)|*.INI",
                    RestoreDirectory = true
                };

                bool? isResultNormal = saveFileDialog.ShowDialog();

                if (isResultNormal != true) return;

                try
                {
                    UsePath.Config = Path.GetDirectoryName(saveFileDialog.FileName) + @"\" + Path.GetFileNameWithoutExtension(saveFileDialog.FileName) + ".ini";

                    if (File.Exists(UsePath.Config))
                        File.Delete(UsePath.Config);

                    INIManager iniManager = new INIManager(UsePath.Config);
                    iniManager.Write("GTweak", "Author", "Greedeks");
                    iniManager.Write("GTweak", "Release", currentRelease);
                    iniManager.WriteAll("Confidentiality Tweaks", INIManager.UserTweaksConfidentiality);
                    iniManager.WriteAll("Interface Tweaks", INIManager.UserTweaksInterface);
                    iniManager.WriteAll("Services Tweaks", INIManager.UserTweaksServices);
                    iniManager.WriteAll("System Tweaks", INIManager.UserTweaksSystem);
                }
                catch (Exception ex) { Debug.WriteLine(ex.Message.ToString()); }
            }
        }

        internal static void OpenFileConfig()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "(*.INI)|*.INI"
            };

            bool? isResultNormal = openFileDialog.ShowDialog();

            if (isResultNormal != true) return;

            UsePath.Config = openFileDialog.FileName;
            INIManager iniManager = new INIManager(UsePath.Config);

            if (iniManager.GetKeysOrValue("GTweak", false).Contains("Greedeks") && iniManager.GetKeysOrValue("GTweak").Contains("Release"))
            {
                if (File.ReadLines(UsePath.Config).Any(line => line.Contains("TglButton")) || File.ReadLines(UsePath.Config).Any(line => line.Contains("Slider")))
                    new ImportWindow(openFileDialog.SafeFileName).ShowDialog();
                else
                    new ViewNotification().Show("", "info", (string)Application.Current.Resources["import_empty_notification"]);
            }
            else
                new ViewNotification().Show("", "info", (string)Application.Current.Resources["import_warning_notification"]);
        }

        internal static void SelfRemoval()
        {
            try
            {
                RegistryHelp.DeleteFolderTree(Registry.CurrentUser, @"Software\GTweak");
                RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Microsoft\Tracing\GTweak_RASAPI32");
                RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Microsoft\Tracing\GTweak_RASMANCS");

                Process.Start(new ProcessStartInfo()
                {
                    Arguments = $"/c taskkill /f /im {currentName} & choice /c y /n /d y /t 3 & del {new FileInfo(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath).Name} & " +
                    @$"rd /s /q {UsePath.FileLocation} & rd /s /q {Environment.SystemDirectory}\config\systemprofile\AppData\Local\GTweak",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    FileName = "cmd.exe"
                });
            }
            catch (Exception ex) { Debug.WriteLine(ex.Message.ToString()); }
        }

        internal static void SelfReboot()
        {
            Process.Start(new ProcessStartInfo()
            {
                Arguments = $"/c taskkill /f /im \"{currentName}\" & choice /c y /n /d y /t 1 & start \"\" \"{currentLocation}\"",
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                FileName = "cmd.exe"
            });
        }
    }
}
