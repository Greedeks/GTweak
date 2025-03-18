using GTweak.Utilities.Control;
using GTweak.Utilities.Helpers;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GTweak.Utilities.Tweaks
{
    internal sealed class UninstallingPakages : TaskSchedulerManager
    {
        internal static string InstalledPackages { get; private set; }

        internal static bool IsOneDriveInstalled => File.Exists(Environment.ExpandEnvironmentVariables(@"%userprofile%\AppData\Local\Microsoft\OneDrive\OneDrive.exe"));
        private static bool isLocalAccount = false;
        private static readonly string pathPackage = Path.Combine(StoragePaths.SystemDisk, "Program Files", "WindowsApps");

        private static readonly Dictionary<string, (string Alias, bool IsUnavailable, List<string> Scripts)> PackagesDetails = new Dictionary<string, (string Alias, bool IsUnavailable, List<string> Scripts)>()
        {
            ["MicrosoftStore"] = (null, false, new List<string> { "Microsoft.WindowsStore" }),
            ["Todos"] = ("TodoList", false, new List<string> { "Microsoft.Todos", "Microsoft.ToDo" }),
            ["BingWeather"] = ("MSWeather", false, new List<string> { "Microsoft.BingWeather" }),
            ["Microsoft3D"] = ("3DViewer", false, new List<string> { "Microsoft.Microsoft3DViewer" }),
            ["Music"] = ("zunemusic", false, new List<string> { "Microsoft.ZuneMusic", "Microsoft.GrooveMusic" }),
            ["GetHelp"] = (null, false, new List<string> { "Microsoft.GetHelp" }),
            ["MicrosoftOfficeHub"] = ("officehub", false, new List<string> { "Microsoft.MicrosoftOfficeHub" }),
            ["MicrosoftSolitaireCollection"] = ("solitaire", false, new List<string> { "Microsoft.MicrosoftSolitaireCollection" }),
            ["MixedReality"] = ("MixedRealityPortal", false, new List<string> { "Microsoft.MixedReality.Portal" }),
            ["Xbox"] = (null, false, new List<string> { "Microsoft.XboxApp", "Microsoft.GamingApp", "Microsoft.XboxGamingOverlay", "Microsoft.XboxGameOverlay", "Microsoft.XboxIdentityProvider", "Microsoft.Xbox.TCUI", "Microsoft.XboxSpeechToTextOverlay" }),
            ["Paint3D"] = (null, false, new List<string> { "Microsoft.MSPaint" }),
            ["OneNote"] = ("MSOneNote", false, new List<string> { "Microsoft.Office.OneNote", "Microsoft.OneNote" }),
            ["People"] = (null, false, new List<string> { "Microsoft.People" }),
            ["MicrosoftStickyNotes"] = ("MSStickyNotes", false, new List<string> { "Microsoft.MicrosoftStickyNotes" }),
            ["Widgets"] = ("Windows.Client.WebExperience", false, new List<string> { "MicrosoftWindows.Client.WebExperience", "Microsoft.WidgetsPlatformRuntime", "Windows.Client.WebExperience" }),
            ["ScreenSketch"] = (null, false, new List<string> { "Microsoft.ScreenSketch" }),
            ["Phone"] = ("PhoneLink", false, new List<string> { "Microsoft.YourPhone", "MicrosoftWindows.CrossDevice" }),
            ["Photos"] = ("MSPhotos", false, new List<string> { "Microsoft.Windows.Photos" }),
            ["FeedbackHub"] = ("feedback", false, new List<string> { "Microsoft.WindowsFeedbackHub" }),
            ["SoundRecorder"] = (null, false, new List<string> { "Microsoft.WindowsSoundRecorder" }),
            ["Alarms"] = (null, false, new List<string> { "Microsoft.WindowsAlarms" }),
            ["SkypeApp"] = ("Skype", false, new List<string> { "Microsoft.SkypeApp" }),
            ["Maps"] = (null, false, new List<string> { "Microsoft.WindowsMaps" }),
            ["Camera"] = (null, false, new List<string> { "Microsoft.WindowsCamera" }),
            ["Video"] = ("zunevideo", false, new List<string> { "Microsoft.ZuneVideo" }),
            ["BingNews"] = (null, false, new List<string> { "Microsoft.BingNews" }),
            ["Mail"] = ("communicationsapps", false, new List<string> { "microsoft.windowscommunicationsapps" }),
            ["MicrosoftTeams"] = ("Teams", false, new List<string> { "MicrosoftTeams", "MSTeams" }),
            ["PowerAutomateDesktop"] = (null, false, new List<string> { "Microsoft.PowerAutomateDesktop" }),
            ["Cortana"] = (null, false, new List<string> { "Microsoft.549981C3F5F10" }),
            ["ClipChamp"] = ("Clipchamp Video Editor", false, new List<string> { "Clipchamp.Clipchamp" }),
            ["GetStarted"] = (null, false, new List<string> { "Microsoft.Getstarted" }),
            ["BingSports"] = (null, false, new List<string> { "Microsoft.BingSports" }),
            ["BingFinance"] = (null, false, new List<string> { "Microsoft.BingFinance" }),
            ["MicrosoftFamily"] = ("FamilySafety", false, new List<string> { "MicrosoftCorporationII.MicrosoftFamily" }),
            ["BingSearch"] = (null, false, new List<string> { "Microsoft.BingSearch" }),
            ["Outlook"] = (null, false, new List<string> { "Microsoft.OutlookForWindows" }),
            ["QuickAssist"] = (null, false, new List<string> { "MicrosoftCorporationII.QuickAssist" }),
            ["DevHome"] = (null, false, new List<string> { "Microsoft.Windows.DevHome" }),
            ["WindowsTerminal"] = (null, false, new List<string> { "Microsoft.WindowsTerminal" }),
            ["LinkedIn"] = ("LinkedInforWindows", false, new List<string> { "Microsoft.LinkedIn" }),
            ["WebMediaExtensions"] = (null, false, new List<string> { "Microsoft.WebMediaExtensions" }),
            ["OneConnect"] = ("MobilePlans", false, new List<string> { "Microsoft.OneConnect" }),
            ["Edge"] = ("MicrosoftEdge", false, new List<string> { "Microsoft.MicrosoftEdge.Stable", "Microsoft.MicrosoftEdge.*" }),
            ["OneDrive"] = (null, false, null)
        };

        internal static bool HandleAvailabilityStatus(string key, bool? isUnavailable = null)
        {
            if (PackagesDetails.TryGetValue(key, out var details))
            {
                if (isUnavailable.HasValue)
                    PackagesDetails[key] = (details.Alias, isUnavailable.Value, details.Scripts);

                return details.IsUnavailable;
            }
            return false;
        }

        internal async void ViewInstalledPackages() => InstalledPackages = await CommandExecutor.GetCommandOutput("Get-AppxPackage | Select-Object -ExpandProperty Name");

        internal static Task DeletingPackage(string packageName)
        {
            if (packageName == "OneDrive")
                return DeletedOneDrive();
            else
            {
                return Task.Run(async () =>
                {
                    try
                    {
                        var (Alias, IsUnavailable, Scripts) = PackagesDetails[packageName];
                        string argument = "-NoLogo -NonInteractive -NoProfile -ExecutionPolicy Bypass -Command";

                        using Process process = new Process
                        {
                            StartInfo =
                            {
                                UseShellExecute = false,
                                CreateNoWindow = true,
                                WindowStyle = ProcessWindowStyle.Minimized,
                                FileName = "powershell.exe"
                            },
                            EnableRaisingEvents = true
                        };

                        process.StartInfo.Arguments = $"{argument} \"Get-AppxProvisionedPackage -online | where-object {{$_.PackageName -like '*{packageName}*'}} | Remove-AppxProvisionedPackage -alluser -online –Verbose\"";
                        process.Start();
                        process.WaitForExit();

                        foreach (string getScript in Scripts)
                        {
                            TrustedInstaller.CreateProcessAsTrustedInstaller(SettingsRepository.PID, $@"cmd.exe /c for /d %i in (""{pathPackage}\*{getScript}*"") do rd /s /q ""%i""");
                            process.StartInfo.Arguments = $"{argument} \"Get-AppxPackage -Name {getScript} -AllUsers | Remove-AppxPackage\"";
                            process.Start();
                            process.WaitForExit();
                        }

                        if (!string.IsNullOrEmpty(Alias))
                        {
                            process.StartInfo.Arguments = $"{argument} \"Get-AppxProvisionedPackage -online | where-object {{$_.PackageName -like '*{Alias}*'}} | Remove-AppxProvisionedPackage -alluser -online –Verbose\"";
                            process.WaitForExit();
                        }
                    }
                    catch (Exception ex) { Debug.WriteLine(ex); }

                    switch (packageName)
                    {
                        case "Widgets":
                            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Dsh", "AllowNewsAndInterests", 0, RegistryValueKind.DWord);
                            break;
                        case "Cortana":
                            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Speech_OneCore\Preferences", "ModelDownloadAllowed", 0, RegistryValueKind.DWord);
                            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Search", "AllowCloudSearch", 0, RegistryValueKind.DWord);
                            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Search", "AllowCortana", 0, RegistryValueKind.DWord);
                            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Search", "AllowSearchToUseLocation", 0, RegistryValueKind.DWord);
                            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Search", "ConnectedSearchUseWeb", 0, RegistryValueKind.DWord);
                            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Search", "DisableWebSearch", 1, RegistryValueKind.DWord);
                            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Search", "AllowNewsAndInterests", 0, RegistryValueKind.DWord);
                            RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\InputPersonalization", "RestrictImplicitInkCollection", 1, RegistryValueKind.DWord);
                            RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\InputPersonalization", "RestrictImplicitTextCollection", 1, RegistryValueKind.DWord);
                            RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\InputPersonalization\TrainedDataStore", "HarvestContacts", 0, RegistryValueKind.DWord);
                            RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Personalization\Settings", "AcceptedPrivacyPolicy", 0, RegistryValueKind.DWord);
                            RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Windows Search", "CortanaConsent", 0, RegistryValueKind.DWord);
                            break;
                        case "Phone":
                            if (RegistryHelp.KeyExists(Registry.ClassesRoot, @"*\shellex\ContextMenuHandlers\ModernSharing", false))
                            {
                                RegistryHelp.DeleteFolderTree(Registry.ClassesRoot, @"*\shellex\ContextMenuHandlers\SendTo");
                                RegistryHelp.DeleteFolderTree(Registry.ClassesRoot, @"*\shellex\ContextMenuHandlers\ModernShare");
                            }
                            else
                            {
                                RegistryHelp.DeleteFolderTree(Registry.ClassesRoot, @"AllFilesystemObjects\shellex\ContextMenuHandlers\SendTo");
                                RegistryHelp.DeleteFolderTree(Registry.ClassesRoot, @"AllFilesystemObjects\shellex\ContextMenuHandlers\ModernSharing");
                            }
                            TrustedInstaller.CreateProcessAsTrustedInstaller(SettingsRepository.PID, $@"cmd.exe /c reg delete ""HKEY_CLASSES_ROOT\CLSID\{{7AD84985-87B4-4a16-BE58-8B72A5B390F7}}"" /f & reg delete ""HKEY_CLASSES_ROOT\Wow6432Node\CLSID\{{7AD84985-87B4-4a16-BE58-8B72A5B390F7}}"" /f");
                            break;
                        case "Paint3D":
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
                                                        RegistryHelp.DeleteFolderTree(Registry.ClassesRoot, $@"SystemFileAssociations\{subkey}\shell\3D Print");
                                                }
                                            }
                                        }
                                        catch (Exception ex) { Debug.WriteLine(ex.Message); }
                                    }
                                    baseKey.Close();
                                }
                            }
                            catch (Exception ex) { Debug.WriteLine(ex.Message); }
                            break;
                        case "Edge":
                            string script = $@"
                            $region = (Get-ItemProperty -Path 'Registry::HKEY_USERS\.DEFAULT\Control Panel\International\Geo').Name
                            $policyFile = '{StoragePaths.SystemDisk}Windows\System32\IntegratedServicesRegionPolicySet.json'

                            if (Test-Path $policyFile) {{
                                $json = Get-Content -Path $policyFile -Raw | ConvertFrom-Json
                            
                                if (-not $json.Policies) {{
                                    exit 1
                                }}
                            
                                $policy = $json.Policies | Where-Object {{ $_.'$comment' -eq 'Edge is uninstallable.' }}
                            
                                if ($policy) {{
                                    if ($policy.defaultState -ne 'enabled') {{
                                        $policy.defaultState = 'enabled'
                                    }}
                            
                                    if ($policy.conditions.region.enabled -notcontains $region) {{
                                        $policy.conditions.region.enabled += $region
                                        $policy.conditions.region.enabled = $policy.conditions.region.enabled | Sort-Object -Unique
                            
                                        $json | ConvertTo-Json -Depth 10 | Set-Content -Path $policyFile -Encoding UTF8
                                    }}
                                }}
                            }} else {{
                                exit 1
                            }}";

                            TrustedInstaller.CreateProcessAsTrustedInstaller(SettingsRepository.PID, $"{Path.Combine(Environment.SystemDirectory, "WindowsPowerShell\\v1.0\\powershell.exe")} -NoLogo -NonInteractive -NoProfile -ExecutionPolicy Bypass -Command \"{script}\"");

                            foreach (var package in new[] { new { AppName = "Edge", Arguments = "--uninstall --msedge --channel=stable --system-level --verbose-logging" }, new { AppName = "EdgeWebView", Arguments = "--uninstall --msedgewebview --system-level --verbose-logging" } })
                            {
                                try
                                {
                                    CommandExecutor.RunCommand("taskkill /F /IM msedge.exe /IM edgeupdate.exe /IM msedgewebview2.exe /IM MicrosoftEdgeUpdate.exe /IM msedgewebviewhost.exe /IM msedgeuserbroker.exe /T");
                                    string setupPath = Path.Combine(Directory.GetDirectories(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Microsoft", package.AppName, "Application")).FirstOrDefault(), "Installer", "setup.exe");

                                    if (File.Exists(setupPath))
                                    {
                                        Process.Start(new ProcessStartInfo
                                        {
                                            FileName = setupPath,
                                            Arguments = package.Arguments,
                                            UseShellExecute = true,
                                            WindowStyle = ProcessWindowStyle.Hidden
                                        })?.WaitForExit();
                                    }
                                }
                                catch (Exception ex) { Debug.WriteLine(ex); }
                            }

                            DeletingTask(edgeTasks);
                            TrustedInstaller.CreateProcessAsTrustedInstaller(SettingsRepository.PID, @"cmd.exe /с rmdir /s /q %LocalAppData%\Microsoft\Edge");
                            TrustedInstaller.CreateProcessAsTrustedInstaller(SettingsRepository.PID, @"cmd.exe /c for /r ""%AppData%\Microsoft\Internet Explorer\Quick Launch"" %f in (*Edge*) do del ""%f""");
                            TrustedInstaller.CreateProcessAsTrustedInstaller(SettingsRepository.PID, @"cmd.exe /c del /q /f ""%AppData%\Microsoft\Internet Explorer\Quick Launch\User Pinned\TaskBar\*Edge*.lnk""");
                            TrustedInstaller.CreateProcessAsTrustedInstaller(SettingsRepository.PID, $@"cmd.exe /c for /r ""{StoragePaths.SystemDisk}ProgramData\Microsoft\Windows\Start Menu\Programs"" %f in (*Edge*) do del ""%f""");
                            TrustedInstaller.CreateProcessAsTrustedInstaller(SettingsRepository.PID, $@"cmd.exe /c for /r ""{StoragePaths.SystemDisk}Users"" %f in (*Edge*) do @if exist ""%f"" del /f /q ""%f""");
                            TrustedInstaller.CreateProcessAsTrustedInstaller(SettingsRepository.PID, $@"cmd.exe /c for /d %d in (""{StoragePaths.SystemDisk}Program Files (x86)\Microsoft\*Edge*"") do rmdir /s /q ""%d""");
                            TrustedInstaller.CreateProcessAsTrustedInstaller(SettingsRepository.PID, $@"cmd.exe /c for /f ""delims="" %i in ('dir /b /s ""{StoragePaths.SystemDisk}Windows\System32\Tasks\*Edge*""') do (if exist ""%i"" (if exist ""%i\"" (rmdir /s /q ""%i"") else (del /f /q ""%i"")))");

                            RegistryHelp.DeleteValue(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Run", "MicrosoftEdgeAutoLaunch_03AF54719E0271FA0A92D5F15CBA10EA");
                            RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", true);
                            RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Microsoft\Active Setup\Installed Components\{9459C573-B17A-45AE-9F64-1857B5D58CEE}", true);
                            RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\edgeupdate", true);
                            RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\edgeupdatem", true);
                            RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\MicrosoftEdgeElevationService", true);
                            RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate", true);
                            RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\WOW6432Node\Microsoft\Edge", true);
                            RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Microsoft Edge", true);
                            RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Microsoft Edge Update", true);
                            RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Microsoft EdgeWebView", true);
                            RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Classes\MSEdgeHTM", true);
                            RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Clients\StartMenuInternet\Microsoft Edge", true);
                            RegistryHelp.DeleteFolderTree(Registry.ClassesRoot, @"AppID\MicrosoftEdgeUpdate.exe", true);

                            foreach (string folder in new string[] { "Edge", "EdgeCore", "EdgeUpdate", "EdgeWebView", "Temp" })
                            {
                                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Microsoft", folder);
                                TakingOwnership.GrantAdministratorsAccess(path, TakingOwnership.SE_OBJECT_TYPE.SE_FILE_OBJECT);
                                await Task.Delay(1000);
                                TrustedInstaller.CreateProcessAsTrustedInstaller(SettingsRepository.PID, $@"cmd.exe /c rmdir /s /q ""{path}""");
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
                                        path = path.Replace(@"\AppxManifest.xml", "").Trim();
                                        TakingOwnership.GrantAdministratorsAccess(path, TakingOwnership.SE_OBJECT_TYPE.SE_FILE_OBJECT);
                                        TrustedInstaller.CreateProcessAsTrustedInstaller(SettingsRepository.PID, $@"cmd.exe /c rmdir /s /q ""{path}""");
                                        key.DeleteSubKey(subKey);
                                        return;
                                    }
                                }
                            }
                            catch (Exception ex) { Debug.WriteLine(ex); }
                            break;
                    }
                });
            }
        }

        internal static Task DeletedOneDrive()
        {
            return Task.Run(() =>
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = @"/c taskkill /f /im OneDrive.exe & %systemroot%\System32\OneDriveSetup.exe /uninstall & %systemroot%\SysWOW64\OneDriveSetup.exe /uninstall",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using Process process = new Process { StartInfo = startInfo };

                process.Start();
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    RegistryHelp.DeleteFolderTree(Registry.ClassesRoot, @"CLSID\{018D5C66-4533-4307-9B53-224DE2ED1FE6}");
                    RegistryHelp.DeleteFolderTree(Registry.ClassesRoot, @"Wow6432Node\CLSID\{018D5C66-4533-4307-9B53-224DE2ED1FE6}");

                    CommandExecutor.RunCommand($@"/c rd /s /q %userprofile%\AppData\Local\Microsoft\OneDrive & rd /s /q %userprofile%\AppData\Local\OneDrive & 
                    rd /s /q ""%allusersprofile%\Microsoft OneDrive"" & rd /s /q {StoragePaths.SystemDisk}OneDriveTemp{(isLocalAccount ? @" & rd /s /q %userprofile%\OneDrive" : "")}");
                }
            });
        }

        internal static Task ResetOneDrive()
        {
            return Task.Run(() =>
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = @"/c %systemroot%\System32\OneDriveSetup.exe & %systemroot%\SysWOW64\OneDriveSetup.exe",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using Process process = new Process { StartInfo = startInfo };

                process.Start();
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    RegistryHelp.CreateFolder(Registry.ClassesRoot, @"CLSID\{018D5C66-4533-4307-9B53-224DE2ED1FE6}");
                    RegistryHelp.CreateFolder(Registry.ClassesRoot, @"Wow6432Node\CLSID\{018D5C66-4533-4307-9B53-224DE2ED1FE6}");
                }
            });
        }

        internal async void CheckingForLocalAccount()
        {
            string output = await CommandExecutor.GetCommandOutput("Get-LocalUser | Where-Object { $_.Enabled -match 'True'} | Select-Object -ExpandProperty PrincipalSource");
            isLocalAccount = !output.Contains("MicrosoftAccount");
        }
    }
}
