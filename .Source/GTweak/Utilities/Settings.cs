using GTweak.Utilities.Helpers;
using GTweak.Windows;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
        private static byte _volumeNotification = 100;
        private static string _language_ = string.Empty;
        private static bool _isTopMost = false;
        internal static int PID = 0;

        internal static bool IsViewNotification { get => _isViewNotification; set => _isViewNotification = value; }
        internal static bool IsSoundNotification { get => _isSoundNotification; set => _isSoundNotification = value; }
        internal static byte VolumeNotification { get => _volumeNotification; set => _volumeNotification = value; }
        internal static bool IsTopMost { get => _isTopMost; set => _isTopMost = value; }
        internal static string Language { get => _language_; set => _language_ = value; }

        internal readonly void RunAnalysis()
        {
            new ViewNotification().CheckingTempFile();
            СheckingParameters();
        }

        private readonly void СheckingParameters()
        {
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(@"Software\GTweak");
            if (registryKey == null || registryKey.GetValue("Notification", null) == null || registryKey.GetValue("Sound", null) == null || registryKey.GetValue("Volume", null) == null ||
            registryKey.GetValue("TopMost", null) == null || registryKey.GetValue("Language", null) == null)
            {
                registryKey = Registry.CurrentUser.CreateSubKey(@"Software\GTweak");
                registryKey?.SetValue("Notification", IsViewNotification, RegistryValueKind.String);
                registryKey?.SetValue("Sound", IsSoundNotification, RegistryValueKind.String);
                registryKey?.SetValue("Volume", VolumeNotification, RegistryValueKind.String);
                registryKey?.SetValue("TopMost", IsTopMost, RegistryValueKind.String);
                if (string.IsNullOrEmpty(Language))
                {
                    if (System.Text.RegularExpressions.Regex.Replace(CultureInfo.CurrentCulture.ToString(), @"-.+$", "", System.Text.RegularExpressions.RegexOptions.Multiline).ToString().Contains("ru"))
                        registryKey?.SetValue("Language", "ru", RegistryValueKind.String);
                    else
                        registryKey?.SetValue("Language", "en", RegistryValueKind.String);
                }
                else
                    registryKey?.SetValue("Language", Language, RegistryValueKind.String);
            }
            else
            {
                registryKey = Registry.CurrentUser.OpenSubKey(@"Software\GTweak");
                IsViewNotification = bool.Parse(registryKey?.GetValue("Notification").ToString() ?? "True");
                IsSoundNotification = bool.Parse(registryKey?.GetValue("Sound").ToString() ?? "True");
                VolumeNotification = byte.Parse(registryKey?.GetValue("Volume").ToString() ?? "100");
                IsTopMost = bool.Parse(registryKey?.GetValue("TopMost").ToString() ?? "False");
                Language = registryKey?.GetValue("Language").ToString();
            }
            registryKey.Close();
        }

        internal static void ChangingParameters<T>(T value, string selection)
        {
            Parallel.Invoke(() => {
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
                    VolumeNotification = byte.Parse(registryKey?.GetValue("Volume").ToString() ?? "100");
                        break;
                case "TopMost":
                    IsTopMost = bool.Parse(registryKey?.GetValue("TopMost").ToString() ?? "False");
                        break;
                case "Language":
                    Language = registryKey?.GetValue("Language").ToString();
                break;
            }
                registryKey.Close();
            });
        }

        internal static void SaveFileConfig()
        {
            if (СonfigSettings.configConfidentiality.Count == 0 && СonfigSettings.configInterface.Count == 0
                && СonfigSettings.configServices.Count == 0 && СonfigSettings.configSystem.Count == 0)
                new ViewNotification().Show("", (string)Application.Current.Resources["title1_notification"], (string)Application.Current.Resources["export_warning_notification"]);

            else
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    FileName = "Config GTweak",
                    Filter = "(*.INI)|*.INI",
                    RestoreDirectory = true
                };

                Nullable<bool> isResult = saveFileDialog.ShowDialog();

                if (isResult == true)
                {
                    string path = Path.GetDirectoryName(saveFileDialog.FileName);
                    string filename = Path.GetFileNameWithoutExtension(saveFileDialog.FileName);

                    try
                    {
                        PathConfig = path + @"\" + filename + ".ini";

                        if (File.Exists(PathConfig))
                            File.Delete(PathConfig);

                        СonfigSettings settingsFileINI = new СonfigSettings(PathConfig);
                        settingsFileINI.WriteConfig("GTweak", "Author", "Greedeks");
                        settingsFileINI.WriteConfig("GTweak", "Release", "v4");

                        foreach (KeyValuePair<string, string> addConfidentiality in СonfigSettings.configConfidentiality)
                            settingsFileINI.WriteConfig("Confidentiality Tweaks", addConfidentiality.Key, addConfidentiality.Value);

                        foreach (KeyValuePair<string, string> addInterface in СonfigSettings.configInterface)
                            settingsFileINI.WriteConfig("Interface Tweaks", addInterface.Key, addInterface.Value);

                        foreach (KeyValuePair<string, string> addServices in СonfigSettings.configServices)
                            settingsFileINI.WriteConfig("Services Tweaks", addServices.Key, addServices.Value);

                        foreach (KeyValuePair<string, string> addSystem in СonfigSettings.configSystem)
                            settingsFileINI.WriteConfig("System Tweaks", addSystem.Key, addSystem.Value);
                    }
                    catch (Exception ex) { Debug.WriteLine(ex.Message.ToString()); }
                }
            }
        }

        internal static void OpenFileConfig()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "(*.INI)|*.INI"
            };
            Nullable<bool> isResult = openFileDialog.ShowDialog();

            if (isResult == true)
            {
                PathConfig = openFileDialog.FileName;
                СonfigSettings settingsFileINI = new СonfigSettings(PathConfig);

                if (settingsFileINI.ReadConfig("GTweak", "Author").Contains("Greedeks") && settingsFileINI.ReadConfig("GTweak", "Release").Contains("v4"))
                {
                    if (File.ReadLines(PathConfig).Any(line => line.Contains("TglButton")) || File.ReadLines(PathConfig).Any(line => line.Contains("Slider")))
                        new ImportWindow().ShowDialog();
                    else
                        new ViewNotification().Show("", (string)Application.Current.Resources["title1_notification"], (string)Application.Current.Resources["import_empty_notification"]);
                }
                else
                    new ViewNotification().Show("", (string)Application.Current.Resources["title1_notification"], (string)Application.Current.Resources["import_warning_notification"]);
            }
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
                    Arguments = "/C choice /C Y /N /D Y /T 3 & Del \"" + (new FileInfo(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath)).Name + "\" & " +
                    "rd /s /q " + PathTempFiles + @"& rd /s /q "+Environment.SystemDirectory+ @"\config\systemprofile\AppData\Local\GTweak",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    FileName = "cmd.exe"
                });
            }
            catch (Exception ex) { Debug.WriteLine(ex.Message.ToString()); }
        }
    }
}
