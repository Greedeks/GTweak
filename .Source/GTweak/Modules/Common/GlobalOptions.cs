using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using GTweak.Modules.Configuration;
using GTweak.Modules.Helpers;
using GTweak.Modules.Managers;
using GTweak.Windows;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;

namespace GTweak.Modules.Common
{
    internal sealed class GlobalOptions
    {
        [DllImport("winmm.dll")]
        internal static extern int waveOutSetVolume(IntPtr hwo, uint dwVolume);

        internal static readonly string[] AvailableLangs = new ResourceDictionary { Source = new Uri("Languages/LanguageCatalog.xaml", UriKind.Relative) }.Keys.Cast<string>().Select(key => key.Replace("_", "-")).OrderBy(locale => locale, StringComparer.OrdinalIgnoreCase).ToArray();
        internal static readonly string[] AvailableThemes = { "Dark", "Light" };

        internal static (string Full, string Short) CurrentRelease => (Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? string.Empty) is var raw ? (raw, raw.Split(' ').Last().Trim()) : (string.Empty, string.Empty);
        internal static string CurrentName => AppDomain.CurrentDomain.FriendlyName;
        internal static string CurrentLocation => Assembly.GetExecutingAssembly().Location;

        private static readonly Dictionary<string, object> _defaultSettings = new Dictionary<string, object>
        {
            ["EnableNotifications"] = true,
            ["EnableAutoUpdate"] = true,
            ["EnableTopMost"] = false,
            ["EnableIpBlur"] = true,
            ["EnableSound"] = true,
            ["VolumeLevel"] = 50,
            ["Language"] = HardwareProvider.GetCurrentSystemLang().Code,
            ["Theme"] = AvailableThemes.First(),
            ["AddonsPath"] = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
            ["DownloadPath"] = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)
        };

        private static readonly Dictionary<string, object> _cachedSettings = new Dictionary<string, object>(_defaultSettings);

        internal static bool IsViewNotification { get => (bool)_cachedSettings["EnableNotifications"]; set => ChangingParameters("EnableNotifications", value); }
        internal static bool IsUpdateCheckRequired { get => (bool)_cachedSettings["EnableAutoUpdate"]; set => ChangingParameters("EnableAutoUpdate", value); }
        internal static bool IsTopMost { get => (bool)_cachedSettings["EnableTopMost"]; set => ChangingParameters("EnableTopMost", value); }
        internal static bool IsHiddenIpAddress { get => (bool)_cachedSettings["EnableIpBlur"]; set => ChangingParameters("EnableIpBlur", value); }
        internal static bool IsPlayingSound { get => (bool)_cachedSettings["EnableSound"]; set => ChangingParameters("EnableSound", value); }
        internal static int Volume { get => (int)_cachedSettings["VolumeLevel"]; set => ChangingParameters("VolumeLevel", value); }
        internal static string Language { get => (string)_cachedSettings["Language"]; set => ChangingParameters("Language", value); }
        internal static string Theme { get => (string)_cachedSettings["Theme"]; set => ChangingParameters("Theme", value); }
        internal static string UserAddonsPath { get => (string)_cachedSettings["AddonsPath"]; set => ChangingParameters("AddonsPath", value); }
        internal static string DownloadPath { get => (string)_cachedSettings["DownloadPath"]; set => ChangingParameters("DownloadPath", value); }

        private static void ChangingParameters(string key, object value)
        {
            var (regValue, kind) = value switch
            {
                bool valueBool => (valueBool ? 1 : 0, RegistryValueKind.DWord),
                int valueInt => (valueInt, RegistryValueKind.DWord),
                _ => ((object)value?.ToString(), RegistryValueKind.String)
            };

            _cachedSettings[key] = value;
            RegistryHelper.Write(Registry.CurrentUser, PathTargets.Registry.SubKey, key, regValue, kind);
        }

        internal static void СheckingParameters()
        {
            foreach (var kv in _defaultSettings)
            {
                if (RegistryHelper.ValueExists(PathTargets.Registry.BaseKey, kv.Key) == false)
                {
                    ChangingParameters(kv.Key, kv.Value);
                }
                else
                {
                    _cachedSettings[kv.Key] = kv.Value switch
                    {
                        bool valueBool => RegistryHelper.GetValue(PathTargets.Registry.BaseKey, kv.Key, valueBool ? 1 : 0) is int i ? i != 0 : valueBool,
                        int valueInt => RegistryHelper.GetValue(PathTargets.Registry.BaseKey, kv.Key, valueInt),
                        _ => RegistryHelper.GetValue(PathTargets.Registry.BaseKey, kv.Key, kv.Value?.ToString())
                    };
                }
            }

            if (!AvailableThemes.Contains((string)_cachedSettings["Theme"], StringComparer.OrdinalIgnoreCase))
            {
                ChangingParameters("Theme", _defaultSettings["Theme"]);
            }

            App.Language = (string)_cachedSettings["Language"];
            App.Theme = (string)_cachedSettings["Theme"];
        }

        internal static void SaveFileConfig() => new ExportWindow().ShowDialog();

        internal static void OpenFileConfig()
        {
            VistaOpenFileDialog vistaOpenFileDialog = new VistaOpenFileDialog
            {
                Filter = "(*.JSON)|*.JSON",
                RestoreDirectory = true,
            };

            if (vistaOpenFileDialog.ShowDialog() == false)
            {
                return;
            }

            PathTargets.Files.Config = vistaOpenFileDialog.FileName;

            switch (JsonConfigManager.Validate(PathTargets.Files.Config))
            {
                case JsonConfigManager.ConfigStatus.Valid:
                    new ImportWindow(Path.GetFileName(PathTargets.Files.Config)).ShowDialog();
                    break;
                case JsonConfigManager.ConfigStatus.Empty:
                    NotificationManager.Info("empty_import_noty").Perform();
                    break;

                case JsonConfigManager.ConfigStatus.Invalid:
                    NotificationManager.Warn("warn_import_noty").Perform();
                    break;
            }
        }

        internal static void SelfRemoval()
        {
            try
            {
                RegistryHelper.DeleteFolderTree(Registry.CurrentUser, PathTargets.Registry.SubKey);
                RegistryHelper.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Microsoft\Tracing\GTweak_RASAPI32");
                RegistryHelper.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Microsoft\Tracing\GTweak_RASMANCS");

                CommandExecutor.RunCommand("/c " + CommandExecutor.CleanCommand(string.Join(" & ", new[] { $@"taskkill /f /im ""{CurrentName}""", "choice /c y /valueInt /d y /t 3", $@"del ""{CurrentLocation}""",
                    $@"rd /s /q ""{PathTargets.Folders.Workspace}""", $@"rd /s /q ""{Environment.SystemDirectory}\config\systemprofile\AppData\Local\GTweak""" })));
            }
            catch (Exception ex) { ErrorLogger.LogDebug(ex); }
        }

        internal static void SelfReboot() => CommandExecutor.RunCommand($"/c taskkill /f /im \"{CurrentName}\" & choice /c y /valueInt /d y /t 1 & start \"\" \"{CurrentLocation}\"");
    }
}