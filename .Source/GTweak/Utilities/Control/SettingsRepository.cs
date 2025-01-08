using GTweak.Utilities.Helpers;
using GTweak.Windows;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;

namespace GTweak.Utilities.Control
{
    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct UsePath
    {
        internal static string Config = string.Empty;
        internal static string FileLocation => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"GTweak");
        internal static string SystemDisk => Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System));
        internal static string Hosts => Path.Combine(Environment.SystemDirectory, @"drivers\etc\hosts");
        internal static string RegLocation = @"HKEY_CURRENT_USER\Software\GTweak";
    }

    internal sealed class SettingsRepository
    {
        [DllImport("winmm.dll")]
        internal static extern int waveOutSetVolume(IntPtr hwo, uint dwVolume);

        internal static int PID = 0;
        internal static string currentRelease = (Assembly.GetEntryAssembly() ?? throw new InvalidOperationException()).GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion.Split(' ').Last().Trim();
        internal static readonly string currentName = AppDomain.CurrentDomain.FriendlyName;
        internal static readonly string currentLocation = Assembly.GetExecutingAssembly().Location;

        private static readonly Dictionary<string, Action> parameterUpdates = new Dictionary<string, Action>
        {
            { "Notification", () => IsViewNotification = RegistryHelp.GetValue(UsePath.RegLocation, "Notification", IsViewNotification) },
            { "Update", () => IsСheckingUpdate = RegistryHelp.GetValue(UsePath.RegLocation, "Update", IsСheckingUpdate) },
            { "TopMost", () => IsTopMost = RegistryHelp.GetValue(UsePath.RegLocation, "TopMost", IsTopMost) },
            { "Sound", () => IsPlayingSound = RegistryHelp.GetValue(UsePath.RegLocation, "Sound", IsPlayingSound) },
            { "Volume", () => Volume = RegistryHelp.GetValue(UsePath.RegLocation, "Volume", Volume) },
            { "Language", () => Language = RegistryHelp.GetValue(UsePath.RegLocation, "Language", Language) },
            { "Theme", () => Theme = RegistryHelp.GetValue(UsePath.RegLocation, "Theme", Theme) },
            { "HiddenIP", () => IsHiddenIpAddress = RegistryHelp.GetValue(UsePath.RegLocation, "HiddenIP", IsHiddenIpAddress) }
        };

        internal static readonly Dictionary<string, object> defaultRegValues = new Dictionary<string, object>
        {
            { "Notification", true },
            { "Update", true },
            { "TopMost", false },
            { "Sound", true },
            { "Volume", 50 },
            { "Language", App.GettingSystemLanguage },
            { "Theme", "Dark" },
            { "HiddenIP", false }
        };

        internal static bool IsViewNotification { get => (bool)defaultRegValues["Notification"]; set => defaultRegValues["Notification"] = value; }
        internal static bool IsСheckingUpdate { get => (bool)defaultRegValues["Update"]; set => defaultRegValues["Update"] = value; }
        internal static bool IsTopMost { get => (bool)defaultRegValues["TopMost"]; set => defaultRegValues["TopMost"] = value; }
        internal static bool IsPlayingSound { get => (bool)defaultRegValues["Sound"]; set => defaultRegValues["Sound"] = value; }
        internal static int Volume { get => (int)defaultRegValues["Volume"]; set => defaultRegValues["Volume"] = value; }
        internal static string Language { get => (string)defaultRegValues["Language"]; set => defaultRegValues["Language"] = value; }
        internal static string Theme { get => (string)defaultRegValues["Theme"]; set => defaultRegValues["Theme"] = value; }
        internal static bool IsHiddenIpAddress { get => (bool)defaultRegValues["HiddenIP"]; set => defaultRegValues["HiddenIP"] = value; }

        internal void СheckingParameters()
        {
            bool isRegistryEmpty = false;

            foreach (var key in defaultRegValues.Keys)
            {
                if (Registry.GetValue(UsePath.RegLocation, key, null) == null)
                {
                    isRegistryEmpty = true;
                    break;
                }
            }

            if (isRegistryEmpty)
            {
                foreach (var subkey in defaultRegValues)
                    RegistryHelp.Write(Registry.CurrentUser, @"Software\GTweak", subkey.Key, subkey.Value, RegistryValueKind.String);
            }
            else
            {
                foreach (Action executeUpdate in parameterUpdates.Values)
                    executeUpdate();
            }
        }

        internal static void ChangingParameters<T>(T value, string subkey)
        {
            RegistryHelp.Write(Registry.CurrentUser, @"Software\GTweak", subkey, value.ToString(), RegistryValueKind.String);
            parameterUpdates[subkey]();
        }

        internal static void SaveFileConfig()
        {
            if (INIManager.TempTweaksConf.Count == 0 && INIManager.TempTweaksIntf.Count == 0 && INIManager.TempTweaksSvc.Count == 0 && INIManager.TempTweaksSys.Count == 0)
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
                    iniManager.WriteAll(INIManager.SectionConf, INIManager.TempTweaksConf);
                    iniManager.WriteAll(INIManager.SectionIntf, INIManager.TempTweaksIntf);
                    iniManager.WriteAll(INIManager.SectionSvc, INIManager.TempTweaksSvc);
                    iniManager.WriteAll(INIManager.SectionSys, INIManager.TempTweaksSys);
                }
                catch (Exception ex) { Debug.WriteLine(ex.Message); }
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
            catch (Exception ex) { Debug.WriteLine(ex.Message); }
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
