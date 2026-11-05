using System;
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
        internal static readonly string[] AvailableLangs = new ResourceDictionary { Source = new Uri("Languages/LanguageCatalog.xaml", UriKind.Relative) }.Keys.Cast<string>().Select(key => key.Replace("_", "-")).OrderBy(locale => locale, StringComparer.OrdinalIgnoreCase).ToArray();
        internal static readonly string[] AvailableThemes = { "Dark", "Light" };

        internal static (string Full, string Short) CurrentRelease => (Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? string.Empty) is var raw ? (raw, raw.Split(' ').Last().Trim()) : (string.Empty, string.Empty);
        internal static string CurrentName => AppDomain.CurrentDomain.FriendlyName;
        internal static string CurrentLocation => Assembly.GetExecutingAssembly().Location;

        internal static bool EnableNotifications { get => AppOptions.Get(true); set => AppOptions.Set(value); }
        internal static bool EnableProfileHeader { get => AppOptions.Get(true); set => AppOptions.Set(value); }
        internal static bool EnableUpdateCheck { get => AppOptions.Get(true); set => AppOptions.Set(value); }
        internal static bool EnableTopMost { get => AppOptions.Get(false); set => AppOptions.Set(value); }
        internal static bool EnableIpBlur { get => AppOptions.Get(true); set => AppOptions.Set(value); }
        internal static bool EnableSound { get => AppOptions.Get(true); set => AppOptions.Set(value); }
        internal static int VolumeLevel { get => AppOptions.Get(50); set => AppOptions.Set(value); }
        internal static string Language { get => AppOptions.Get(HardwareProvider.GetCurrentSystemLang().Code); set => AppOptions.Set(value); }
        internal static string Theme { get => AppOptions.Get(AvailableThemes[0]); set => AppOptions.Set(value); }
        internal static string AddonsPath { get => AppOptions.Get(PathTargets.Folders.Desktop); set => AppOptions.Set(value); }
        internal static string DownloadPath { get => AppOptions.Get(PathTargets.Folders.Desktop); set => AppOptions.Set(value); }

        internal static void ConfigureApp()
        {
            if (!AvailableThemes.Contains(Theme, StringComparer.OrdinalIgnoreCase))
            {
                Theme = AvailableThemes[0];
            }

            App.Language = Language;
            App.Theme = Theme;
            SyncVolume();
        }

        [DllImport("winmm.dll")]
        private static extern int waveOutSetVolume(IntPtr hwo, uint dwVolume);
        internal static void SyncVolume()
        {
            uint level = (uint)(VolumeLevel * ushort.MaxValue / 100);
            waveOutSetVolume(IntPtr.Zero, level | (level << 16));
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

                CommandExecutor.RunCommand("/c " + CommandExecutor.CleanCommand(string.Join(" & ", new[] { $@"taskkill /f /im ""{CurrentName}""", "timeout /t 3", $@"del ""{CurrentLocation}""", $@"rd /s /q ""{PathTargets.Folders.Workspace}""", $@"rd /s /q ""{Environment.SystemDirectory}\config\systemprofile\AppData\Local\GTweak""" })));
            }
            catch (Exception ex) { ErrorLogger.LogDebug(ex); }
        }

        internal static void SelfReboot() => CommandExecutor.RunCommand($@"/c taskkill /f /im ""{CurrentName}"" & timeout /t 1 & start """" ""{CurrentLocation}""");
    }
}