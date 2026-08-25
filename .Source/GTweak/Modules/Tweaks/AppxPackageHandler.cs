using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GTweak.Modules.Common;
using GTweak.Modules.Helpers;
using GTweak.Modules.Managers;
using GTweak.Modules.Storage;
using Microsoft.Win32;

namespace GTweak.Modules.Tweaks
{
    internal sealed class AppxPackageHandler : TaskSchedulerManager
    {
        private static volatile bool _isLocalAccount = false;

        internal static event Action DataChanged;
        internal static void OnPackagesChanged() => DataChanged?.Invoke();

        internal static bool IsOneDriveInstalled => PathTargets.Executable.OneDriveInstances.Any(File.Exists) || (Directory.Exists(PathTargets.Folders.OneDrive) &&
            Directory.EnumerateDirectories(PathTargets.Folders.OneDrive).Any(dir => File.Exists(Path.Combine(dir, "OneDrive.exe"))));

        internal static bool IsEdgeInstalled => Directory.Exists(PathTargets.Folders.Edge);

        internal static HashSet<string> InstalledPackagesCache = new HashSet<string>();

        internal void GetInstalledPackages()
        {
            try
            {
                HashSet<string> packages = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var (hive, path) in new[]
                {
                    (Registry.CurrentUser,  @"Software\Classes\Local Settings\Software\Microsoft\Windows\CurrentVersion\AppModel\Repository\Packages"),
                    (Registry.CurrentUser,  @"Software\Classes\Local Settings\Software\Microsoft\Windows\CurrentVersion\AppModel\Repository\Families"),
                    (Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Appx\AppxAllUserStore\Packages"),
                    (Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Appx\AppxAllUserStore\InboxApplications"),
                    (Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Appx\AppxAllUserStore\Staged"),
                })
                {
                    HashSet<string> result = RegistryHelper.GetSubKeyNames<HashSet<string>>(hive, path);
                    if (result != null)
                    {
                        packages.UnionWith(result);
                    }
                }

                InstalledPackagesCache = packages;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogDebug(ex);
                InstalledPackagesCache = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }
            finally { OnPackagesChanged(); }
        }

        internal static bool HandleAvailabilityStatus(string key, bool? isUnavailable = null)
        {
            if (PackageStorage.PackagesDetails.TryGetValue(key, out var details))
            {
                if (isUnavailable.HasValue)
                {
                    details.IsUnavailable = isUnavailable.Value;
                    OnPackagesChanged();
                }

                return details.IsUnavailable;
            }

            return false;
        }

        internal static void CheckingForLocalAccount()
        {
            try
            {
                string output = CommandExecutor.GetCommandOutput("Get-LocalUser | Where-Object { $_.Enabled -match 'True'} | Select-Object -ExpandProperty PrincipalSource").GetAwaiter().GetResult();
                _isLocalAccount = output.IndexOf("MicrosoftAccount", StringComparison.OrdinalIgnoreCase) < 0;
            }
            catch { _isLocalAccount = false; }
        }

        internal static async Task RestoreOneDriveFolder()
        {
            await CommandExecutor.InvokeRunCommand($@"/c {PathTargets.Executable.OneDriveSetup}").ConfigureAwait(false);

            SetTaskState(true, oneDriveTask);

            RegistryHelper.CreateFolder(Registry.ClassesRoot, @"CLSID\{018D5C66-4533-4307-9B53-224DE2ED1FE6}");
            RegistryHelper.CreateFolder(Registry.ClassesRoot, @"Wow6432Node\CLSID\{018D5C66-4533-4307-9B53-224DE2ED1FE6}");
        }

        internal static async Task RemoveAppxPackage(string packageName, bool shouldRemoveWebView = false)
        {
            if (packageName == "OneDrive")
            {
                await CommandExecutor.InvokeRunCommand($@"/c taskkill /f /im OneDrive.exe & {PathTargets.Executable.OneDriveSetup} /uninstall").ConfigureAwait(false);

                RegistryHelper.DeleteFolderTree(Registry.ClassesRoot, @"CLSID\{018D5C66-4533-4307-9B53-224DE2ED1FE6}");
                RegistryHelper.DeleteFolderTree(Registry.ClassesRoot, @"Wow6432Node\CLSID\{018D5C66-4533-4307-9B53-224DE2ED1FE6}");

                SetTaskState(false, oneDriveTask);

                CommandExecutor.RunCommand($@"/c rd /s /q %userprofile%\AppData\Local\Microsoft\OneDrive & rd /s /q %userprofile%\AppData\Local\OneDrive & 
                rd /s /q ""%allusersprofile%\Microsoft OneDrive"" & rd /s /q {PathTargets.Folders.SystemDrive}OneDriveTemp{(_isLocalAccount ? @" & rd /s /q %userprofile%\OneDrive" : "")}");

                return;
            }

            try
            {
                if (!PackageStorage.PackagesDetails.TryGetValue(packageName, out PackagesInfo details))
                {
                    ErrorLogger.LogDebug(new InvalidOperationException($"PackageDetails does not contain key '{packageName}'"));
                    return;
                }

                string alias = details.Alias;
                IReadOnlyList<string> scripts = details.Scripts;

                List<string> packageNamesToRemove = new List<string> { packageName };

                if (!string.IsNullOrWhiteSpace(alias))
                {
                    packageNamesToRemove.Add(alias);
                }

                if (scripts != null && scripts.Count > 0)
                {
                    packageNamesToRemove.AddRange(scripts);
                }

                string psCommands = $@"$pattern = '{string.Join("|", packageNamesToRemove.Select(Regex.Escape))}'
                Get-AppxPackage -AllUsers -PackageTypeFilter Bundle, Resource, Main | Where-Object {{ $_.Name -match $pattern -or $_.PackageFullName -match $pattern }} | ForEach-Object {{ Remove-AppxPackage -AllUsers -Package $_.PackageFullName }}
                Get-AppxProvisionedPackage -Online | Where-Object {{ $_.DisplayName -match $pattern -or $_.PackageName -match $pattern }} | ForEach-Object {{ Remove-AppxProvisionedPackage -Online -PackageName $_.PackageName -AllUsers }}";


                await CommandExecutor.InvokeRunCommand(psCommands, true).ConfigureAwait(false);

                CommandExecutor.RunCommandAsTrustedInstaller($@"/c for /d %i in ({string.Join(" ", packageNamesToRemove.Select(n => $@"""{Path.Combine(PathTargets.Folders.SystemDrive, "Program Files", "WindowsApps")}\*{n}*"""))}) do takeown /f ""%i"" /r /d y && icacls ""%i"" /inheritance:r /remove S-1-5-32-544 S-1-5-11 S-1-5-32-545 S-1-5-18 && icacls ""%i"" /grant ""{Environment.UserName}"":F && rd /s /q ""%i""");

                string[] allUserStorePaths =
                {
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Appx\AppxAllUserStore\Staged",
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Appx\AppxAllUserStore\Packages",
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Appx\AppxAllUserStore\InboxApplications"
                };

                foreach (string storePath in allUserStorePaths)
                {
                    using RegistryKey baseKey = Registry.LocalMachine.OpenSubKey(storePath, true);
                    if (baseKey == null)
                    {
                        continue;
                    }

                    foreach (string subKeyName in baseKey.GetSubKeyNames())
                    {
                        if (packageNamesToRemove.Any(name => subKeyName.StartsWith(name, StringComparison.OrdinalIgnoreCase)))
                        {
                            try { baseKey.DeleteSubKeyTree(subKeyName, false); }
                            catch (Exception ex) { ErrorLogger.LogDebug(ex); }
                        }
                    }
                }
            }
            catch (Exception ex) { ErrorLogger.LogDebug(ex); }

            switch (packageName)
            {
                case "Widgets": PostRemoveWidgets(); break;
                case "DevHome": PostRemoveDevHome(); break;
                case "Outlook": PostRemoveOutlook(); break;
                case "Cortana": PostRemoveCortana(); break;
                case "Phone": PostRemovePhone(); break;
                case "Paint3D": PostRemovePaint3D(); break;
                case "Edge": PostRemoveEdge(shouldRemoveWebView); break;
                default:
                    break;
            }
        }

        private static void PostRemoveWidgets() => RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Dsh", "AllowNewsAndInterests", 0, RegistryValueKind.DWord);

        private static void PostRemoveDevHome() => RegistryHelper.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\Orchestrator\UScheduler_Oobe\DevHomeUpdate");

        private static void PostRemoveOutlook() => RegistryHelper.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\Orchestrator\UScheduler_Oobe\OutlookUpdate");

        private static void PostRemoveCortana()
        {
            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Speech_OneCore\Preferences", "ModelDownloadAllowed", 0, RegistryValueKind.DWord);
            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Search", "AllowCloudSearch", 0, RegistryValueKind.DWord);
            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Search", "AllowCortana", 0, RegistryValueKind.DWord);
            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Search", "AllowSearchToUseLocation", 0, RegistryValueKind.DWord);
            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Search", "ConnectedSearchUseWeb", 0, RegistryValueKind.DWord);
            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Search", "DisableWebSearch", 1, RegistryValueKind.DWord);
            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Search", "AllowNewsAndInterests", 0, RegistryValueKind.DWord);
            RegistryHelper.Write(Registry.CurrentUser, @"Software\Microsoft\InputPersonalization", "RestrictImplicitInkCollection", 1, RegistryValueKind.DWord);
            RegistryHelper.Write(Registry.CurrentUser, @"Software\Microsoft\InputPersonalization", "RestrictImplicitTextCollection", 1, RegistryValueKind.DWord);
            RegistryHelper.Write(Registry.CurrentUser, @"Software\Microsoft\InputPersonalization\TrainedDataStore", "HarvestContacts", 0, RegistryValueKind.DWord);
            RegistryHelper.Write(Registry.CurrentUser, @"Software\Microsoft\Personalization\Settings", "AcceptedPrivacyPolicy", 0, RegistryValueKind.DWord);
            RegistryHelper.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Windows Search", "CortanaConsent", 0, RegistryValueKind.DWord);
        }

        private static void PostRemovePhone()
        {
            RegistryHelper.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\Orchestrator\UScheduler_Oobe\CrossDeviceUpdate");
            if (RegistryHelper.KeyExists(Registry.ClassesRoot, @"*\shellex\ContextMenuHandlers\ModernSharing", true))
            {
                RegistryHelper.DeleteFolderTree(Registry.ClassesRoot, @"*\shellex\ContextMenuHandlers\SendTo");
                RegistryHelper.DeleteFolderTree(Registry.ClassesRoot, @"*\shellex\ContextMenuHandlers\ModernShare");
            }
            else
            {
                RegistryHelper.DeleteFolderTree(Registry.ClassesRoot, @"AllFilesystemObjects\shellex\ContextMenuHandlers\SendTo");
                RegistryHelper.DeleteFolderTree(Registry.ClassesRoot, @"AllFilesystemObjects\shellex\ContextMenuHandlers\ModernSharing");
            }
            CommandExecutor.RunCommandAsTrustedInstaller($@"/c reg delete ""HKEY_CLASSES_ROOT\CLSID\{{7AD84985-87B4-4a16-BE58-8B72A5B390F7}}"" /f & reg delete ""HKEY_CLASSES_ROOT\Wow6432Node\CLSID\{{7AD84985-87B4-4a16-BE58-8B72A5B390F7}}"" /f");
        }

        private static void PostRemovePaint3D()
        {
            try
            {
                using RegistryKey baseKey = Registry.ClassesRoot.OpenSubKey("SystemFileAssociations", true);
                if (baseKey != null)
                {
                    foreach (string subkey in baseKey.GetSubKeyNames())
                    {
                        try
                        {
                            using RegistryKey assocKey = baseKey.OpenSubKey(subkey, true);
                            if (assocKey != null)
                            {
                                using RegistryKey shellKey = assocKey.OpenSubKey("Shell", true);
                                if (shellKey != null)
                                {
                                    if (shellKey.GetSubKeyNames().Any(k => k.Equals("3D Print", StringComparison.OrdinalIgnoreCase)))
                                    {
                                        RegistryHelper.DeleteFolderTree(Registry.ClassesRoot, $@"SystemFileAssociations\{subkey}\shell\3D Print");
                                    }
                                }
                            }
                        }
                        catch (Exception ex) { ErrorLogger.LogDebug(ex); }
                    }
                    baseKey.Close();
                }
            }
            catch (Exception ex) { ErrorLogger.LogDebug(ex); }
        }

        private static void PostRemoveEdge(bool removeWebView)
        {
            string[] processes = { "msedge", "edge", "edgeupdate", "edgeupdatem", "msedgewebview2", "microsoftedgeupdate", "msedgewebviewhost", "msedgeuserbroker", "usocoreworker", "widgets", "microsoftedgesh", "microsoftedgecp", "microsoftedge" };
            CommandExecutor.RunCommandAsTrustedInstaller("/c taskkill /f " + string.Join(" ", processes.Select(p => $"/im {p}.exe")));

            string setup = PathTargets.Executable.EdgeSetup;
            string stub = PathTargets.Executable.EdgeTempStub;

            FileDirectoryHelper.CreateDirectory(Path.GetDirectoryName(stub));
            File.WriteAllBytes(stub, Array.Empty<byte>());

            if (!string.IsNullOrEmpty(setup))
            {
                ProcessStartInfo startInfo = new ProcessStartInfo()
                {
                    FileName = setup,
                    Arguments = "--uninstall --system-level --force-uninstall --delete-profile",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = true,
                    Verb = "runas",
                    CreateNoWindow = true
                };

                using Process process = new Process { StartInfo = startInfo };
                try
                {
                    process.Start();
                    process.WaitForExit();
                }
                catch (Exception ex) { ErrorLogger.LogDebug(ex); }
            }

            FileDirectoryHelper.DeleteDirectory(Path.GetDirectoryName(stub));

            if (removeWebView)
            {
                RemoveTasks(edgeTasks);

                RegistryHelper.DeleteFolderTree(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\edgeupdate", true);
                RegistryHelper.DeleteFolderTree(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\edgeupdatem", true);
                RegistryHelper.DeleteFolderTree(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\MicrosoftEdgeElevationService", true);
                RegistryHelper.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\WOW6432Node\Microsoft\Edge", true);
                RegistryHelper.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate", true);
                RegistryHelper.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\WOW6432Node\Microsoft\EdgeWebView", true);
                RegistryHelper.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Microsoft Edge Update", true);
                RegistryHelper.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Microsoft EdgeWebView", true);
                RegistryHelper.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Microsoft\EdgeWebView", true);
                RegistryHelper.DeleteFolderTree(Registry.CurrentUser, @"Software\Microsoft\EdgeUpdate", true);
                RegistryHelper.DeleteFolderTree(Registry.CurrentUser, @"Software\Microsoft\EdgeWebView", true);
                RegistryHelper.DeleteFolderTree(Registry.ClassesRoot, @"AppID\MicrosoftEdgeUpdate.exe", true);
                RegistryHelper.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Explorer\Browser Helper Objects", true);
                RegistryHelper.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Browser Helper Objects", true);
                RegistryHelper.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\Orchestrator\UScheduler_Oobe\EdgeUpdate", true);

                FileDirectoryHelper.ForceDeleteDirectory(PathTargets.Folders.EdgeComponents);
            }

            try
            {
                using RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Appx\AppxAllUserStore\InboxApplications");
                foreach (string subKey in key?.GetSubKeyNames() ?? Array.Empty<string>())
                {
                    using RegistryKey subKeyEntry = key.OpenSubKey(subKey);
                    string path = subKeyEntry?.GetValue("Path") as string;
                    if (!string.IsNullOrEmpty(path) && path.Contains("Edge"))
                    {
                        if (!removeWebView && path.Contains("WebView"))
                        {
                            continue;
                        }

                        if (path.EndsWith(@"\AppxManifest.xml", StringComparison.OrdinalIgnoreCase))
                        {
                            path = path.Replace(@"\AppxManifest.xml", "").Trim();
                        }

                        FileDirectoryHelper.ForceDeleteDirectory(path);

                        key.DeleteSubKey(subKey);

                        return;
                    }
                }
            }
            catch (Exception ex) { ErrorLogger.LogDebug(ex); }
        }
    }
}
