using GTweak.Utilities.Configuration;
using GTweak.Utilities.Helpers;
using GTweak.Utilities.Managers;
using GTweak.Windows;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace GTweak.Utilities.Controls
{
    internal sealed class SettingsEngine
    {
        [DllImport("winmm.dll")]
        internal static extern int waveOutSetVolume(IntPtr hwo, uint dwVolume);

        internal static readonly string[] AvailableLangs = { "en", "fr", "it", "ko", "pt-br", "ru", "uk" };
        internal static readonly string[] AvailableThemes = { "Dark", "Light", "Cobalt", "Dark amethyst", "Cold Blue", "System" };

        internal static string currentRelease = (Assembly.GetEntryAssembly() ?? throw new InvalidOperationException()).GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion.Split(' ').Last().Trim();
        internal static readonly string currentName = AppDomain.CurrentDomain.FriendlyName;
        internal static readonly string currentLocation = Assembly.GetExecutingAssembly().Location;

        private static readonly Dictionary<string, object> _defaultSettings = new Dictionary<string, object>
        {
            ["EnableNotifications"] = true,
            ["EnableAutoUpdate"] = true,
            ["EnableTopMost"] = false,
            ["EnableIpBlur"] = true,
            ["EnableSound"] = true,
            ["VolumeLevel"] = 50,
            ["Language"] = SystemDiagnostics.GetCurrentSystemLang().Сode,
            ["Theme"] = "Dark"
        };

        private static readonly Dictionary<string, object> _cachedSettings = new Dictionary<string, object>(_defaultSettings);

        internal static bool IsViewNotification { get => (bool)_cachedSettings["EnableNotifications"]; set => ChangingParameters("EnableNotifications", value); }
        internal static bool IsUpdateCheckRequired { get => (bool)_cachedSettings["EnableAutoUpdate"]; set => ChangingParameters("EnableAutoUpdate", value); }
        internal static bool IsTopMost { get => (bool)_cachedSettings["EnableTopMost"]; set => ChangingParameters("EnableTopMost", value); }
        internal static bool IsPlayingSound { get => (bool)_cachedSettings["EnableSound"]; set => ChangingParameters("EnableSound", value); }
        internal static int Volume { get => (int)_cachedSettings["VolumeLevel"]; set => ChangingParameters("VolumeLevel", value); }
        internal static string Language { get => (string)_cachedSettings["Language"]; set => ChangingParameters("Language", value); }
        internal static string Theme { get => (string)_cachedSettings["Theme"]; set => ChangingParameters("Theme", value); }
        internal static bool IsHiddenIpAddress { get => (bool)_cachedSettings["EnableIpBlur"]; set => ChangingParameters("EnableIpBlur", value); }

        private static void ChangingParameters(string key, object value)
        {
            var (regValue, kind) = value switch
            {
                bool valueBool => (valueBool ? 1 : 0, RegistryValueKind.DWord),
                int valueInt => (valueInt, RegistryValueKind.DWord),
                _ => ((object)value?.ToString(), RegistryValueKind.String)
            };

            _cachedSettings[key] = value;
            RegistryHelp.Write(Registry.CurrentUser, PathLocator.Registry.SubKey, key, regValue, kind);
        }

        internal static void СheckingParameters()
        {
            foreach (var kv in _defaultSettings)
            {
                if (RegistryHelp.ValueExists(PathLocator.Registry.BaseKey, kv.Key))
                    ChangingParameters(kv.Key, kv.Value);
                else
                {
                    _cachedSettings[kv.Key] = kv.Value is bool defaultBool ? RegistryHelp.GetValue(PathLocator.Registry.BaseKey, kv.Key, defaultBool ? 1 : 0) is int asBool ? asBool != 0 : defaultBool :
                         kv.Value is int defaultInt ? RegistryHelp.GetValue(PathLocator.Registry.BaseKey, kv.Key, defaultInt) :
                         kv.Value is string defaultString ? RegistryHelp.GetValue(PathLocator.Registry.BaseKey, kv.Key, defaultString) : kv.Value;
                }
            }

            App.Language = (string)_cachedSettings["Language"];
            App.Theme = (string)_cachedSettings["Theme"];
        }

        internal static void SaveFileConfig()
        {
            if (INIManager.IsAllTempDictionaryEmpty)
                new NotificationManager().Show("", "info", "export_warning_notification");
            else
            {
                VistaSaveFileDialog vistaSaveFileDialog = new VistaSaveFileDialog
                {
                    FileName = "Config GTweak",
                    Filter = "(*.INI)|*.INI",
                    RestoreDirectory = true
                };

                if (vistaSaveFileDialog.ShowDialog() != true)
                    return;

                try
                {
                    PathLocator.Files.Config = Path.Combine(Path.GetDirectoryName(vistaSaveFileDialog.FileName), Path.GetFileNameWithoutExtension(vistaSaveFileDialog.FileName) + ".ini");

                    if (File.Exists(PathLocator.Files.Config))
                        File.Delete(PathLocator.Files.Config);

                    INIManager iniManager = new INIManager(PathLocator.Files.Config);
                    iniManager.Write("GTweak", "Author", "Greedeks");
                    iniManager.Write("GTweak", "Release", currentRelease);
                    iniManager.WriteAll(INIManager.SectionConf, INIManager.TempTweaksConf);
                    iniManager.WriteAll(INIManager.SectionIntf, INIManager.TempTweaksIntf);
                    iniManager.WriteAll(INIManager.SectionSvc, INIManager.TempTweaksSvc);
                    iniManager.WriteAll(INIManager.SectionSys, INIManager.TempTweaksSys);
                }
                catch (Exception ex) { ErrorLogging.LogDebug(ex); }
            }
        }

        internal static void OpenFileConfig()
        {
            VistaOpenFileDialog vistaOpenFileDialog = new VistaOpenFileDialog
            {
                Filter = "(*.INI)|*.INI",
                RestoreDirectory = true,
            };

            if (vistaOpenFileDialog.ShowDialog() == false)
                return;

            PathLocator.Files.Config = vistaOpenFileDialog.FileName;
            INIManager iniManager = new INIManager(PathLocator.Files.Config);

            if (iniManager.GetKeysOrValue("GTweak", false).Contains("Greedeks") && iniManager.GetKeysOrValue("GTweak").Contains("Release"))
            {
                if (File.ReadLines(PathLocator.Files.Config).Any(line => line.Contains("TglButton")) || File.ReadLines(PathLocator.Files.Config).Any(line => line.Contains("Slider")))
                    new ImportWindow(Path.GetFileName(vistaOpenFileDialog.FileName)).ShowDialog();
                else
                    new NotificationManager().Show("", "info", "empty_import_notification");
            }
            else
                new NotificationManager().Show("", "info", "warn_import_notification");
        }

        internal static void SelfRemoval()
        {
            try
            {
                RegistryHelp.DeleteFolderTree(Registry.CurrentUser, PathLocator.Registry.SubKey);
                RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Microsoft\Tracing\GTweak_RASAPI32");
                RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Microsoft\Tracing\GTweak_RASMANCS");

                CommandExecutor.RunCommand($"/c taskkill /f /im \"{currentName}\" & choice /c y /n /d y /t 3 & del \"{currentLocation}\" & " +
                    @$"rd /s /q ""{PathLocator.Folders.Workspace}"" & rd /s /q ""{Environment.SystemDirectory}\config\systemprofile\AppData\Local\GTweak""");
            }
            catch (Exception ex) { ErrorLogging.LogDebug(ex); }
        }

        internal static void SelfReboot() => CommandExecutor.RunCommand($"/c taskkill /f /im \"{currentName}\" & choice /c y /n /d y /t 1 & start \"\" \"{currentLocation}\"");
    }
}
