using GTweak.Utilities.Helpers;
using GTweak.Windows;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace GTweak.Utilities
{
    internal readonly struct Settings
    {
        internal static string PathConfig = string.Empty;
        internal static string PathSound => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)) + @"\GTweak\Sound.mp3";
        internal static string PathIcon => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)) + @"\GTweak\GTweak.ico";
        internal static string PathTempFiles => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)) + @"\GTweak";
        internal static string PathSystemDisk => Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System));

        private static bool _isViewNotification = true;
        private static bool _isSoundNotification = true;
        private static byte _volumeNotification = 50;
        private static bool _isTopMost = false;
        private static bool _isСheckingUpdate = true;
        private static string _language = App.GettingSystemLanguage;
        private static string _theme = "Dark";
        internal static bool _isHiddenIpAddress = false;

        internal static int PID = 0;

        internal static bool IsViewNotification { get => _isViewNotification; set => _isViewNotification = value; }
        internal static bool IsSoundNotification { get => _isSoundNotification; set => _isSoundNotification = value; }
        internal static byte VolumeNotification { get => _volumeNotification; set => _volumeNotification = value; }
        internal static bool IsTopMost { get => _isTopMost; set => _isTopMost = value; }
        internal static bool IsСheckingUpdate { get => _isСheckingUpdate; set => _isСheckingUpdate = value; }
        internal static string Language { get => _language; set => _language = value; }
        internal static string Theme { get => _theme; set => _theme = value; }
        internal static bool IsHiddenIpAddress { get => _isHiddenIpAddress; set => _isHiddenIpAddress = value; }

        internal readonly void RunAnalysis()
        {
            new ViewNotification().CheckingTempFile();
            СheckingParameters();
        }

        private readonly void СheckingParameters()
        {
            RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(@"Software\GTweak");
            if (registryKey == null || registryKey.GetValue("Notification", null) == null || registryKey.GetValue("Sound", null) == null || registryKey.GetValue("Volume", null) == null || registryKey.GetValue("TopMost", null) == null || 
            registryKey.GetValue("Update", null) == null || registryKey.GetValue("Language", null) == null || registryKey.GetValue("HiddenIP", null) == null || registryKey.GetValue("Theme", null) == null)
            {
                registryKey?.SetValue("Notification", IsViewNotification, RegistryValueKind.String);
                registryKey?.SetValue("Sound", IsSoundNotification, RegistryValueKind.String);
                registryKey?.SetValue("Volume", VolumeNotification, RegistryValueKind.String);
                registryKey?.SetValue("TopMost", IsTopMost, RegistryValueKind.String);
                registryKey?.SetValue("Update", IsСheckingUpdate, RegistryValueKind.String);
                registryKey?.SetValue("HiddenIP", IsHiddenIpAddress, RegistryValueKind.String);
                registryKey?.SetValue("Theme", Theme, RegistryValueKind.String);
                registryKey?.SetValue("Language", App.GettingSystemLanguage, RegistryValueKind.String);
            }
            else
            {
                IsViewNotification = bool.Parse(registryKey?.GetValue("Notification").ToString() ?? "True");
                IsSoundNotification = bool.Parse(registryKey?.GetValue("Sound").ToString() ?? "True");
                VolumeNotification = byte.Parse(registryKey?.GetValue("Volume").ToString() ?? "50");
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
                    IsSoundNotification = bool.Parse(registryKey?.GetValue("Sound").ToString() ?? "True");
                    break;
                case "Volume":
                    VolumeNotification = byte.Parse(registryKey?.GetValue("Volume").ToString() ?? "50");
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
                new ViewNotification().Show("", (string)Application.Current.Resources["title1_notification"], (string)Application.Current.Resources["export_warning_notification"]);
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
                    PathConfig = Path.GetDirectoryName(saveFileDialog.FileName) + @"\" + Path.GetFileNameWithoutExtension(saveFileDialog.FileName) + ".ini";

                    if (File.Exists(PathConfig))
                        File.Delete(PathConfig);

                    INIManager iniManager = new INIManager(PathConfig);
                    iniManager.Write("GTweak", "Author", "Greedeks");
                    iniManager.Write("GTweak", "Release", "v4");
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

            PathConfig = openFileDialog.FileName;
            INIManager iniManager = new INIManager(PathConfig);

            if (iniManager.Read("GTweak", "Author").Contains("Greedeks") && iniManager.Read("GTweak", "Release").Contains("v4"))
            {
                if (File.ReadLines(PathConfig).Any(line => line.Contains("TglButton")) || File.ReadLines(PathConfig).Any(line => line.Contains("Slider")))
                    new ImportWindow().ShowDialog();
                else
                    new ViewNotification().Show("", (string)Application.Current.Resources["title1_notification"], (string)Application.Current.Resources["import_empty_notification"]);
            }
            else
                new ViewNotification().Show("", (string)Application.Current.Resources["title1_notification"], (string)Application.Current.Resources["import_warning_notification"]);
        }

        internal static void SelfRemoval()
        {
            try
            {
                RegistryHelp.DeleteFolderTree(Registry.CurrentUser, @"Software\GTweak");
                RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Microsoft\Tracing\GTweak_RASAPI32");
                RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Microsoft\Tracing\GTweak_RASMANCS");

                Application.Current.Shutdown();
                Process.Start(new ProcessStartInfo()
                {
                    Arguments = "/c choice /c y /n /d y /t 3 & del \"" + (new FileInfo(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath)).Name + "\" & " +
                    "rd /s /q " + PathTempFiles + @"& rd /s /q " + Environment.SystemDirectory + @"\config\systemprofile\AppData\Local\GTweak",
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
                Arguments = "/c choice /c y /n /d y /t 1 & start \"\" \"" + Assembly.GetEntryAssembly().Location + "\"",
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                FileName = "cmd.exe"
            });
            Process.GetCurrentProcess().Kill();
        }
    }
}
