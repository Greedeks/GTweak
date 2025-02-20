using GTweak.Utilities.Control;
using GTweak.Utilities.Helpers;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace GTweak.Utilities.Tweaks
{
    internal sealed class UninstallingPakages : TaskSchedulerManager
    {
        internal static string InstalledPackages { get; private set; }

        internal static bool IsOneDriveInstalled => File.Exists(Environment.ExpandEnvironmentVariables(@"%userprofile%\AppData\Local\Microsoft\OneDrive\OneDrive.exe"));
        private static bool isLocalAccount = false;
        private static readonly string pathPackage = $@"{StoragePaths.SystemDisk}Program Files\WindowsApps";

        #region Lists packages
        internal static Dictionary<string, bool> IsAppUnavailable = new Dictionary<string, bool>
        {
            ["MicrosoftStore"] = false,
            ["Todos"] = false,
            ["BingWeather"] = false,
            ["Microsoft3D"] = false,
            ["Music"] = false,
            ["GetHelp"] = false,
            ["MicrosoftOfficeHub"] = false,
            ["MicrosoftSolitaireCollection"] = false,
            ["MixedReality"] = false,
            ["Xbox"] = false,
            ["Paint3D"] = false,
            ["OneNote"] = false,
            ["People"] = false,
            ["MicrosoftStickyNotes"] = false,
            ["Widgets"] = false,
            ["ScreenSketch"] = false,
            ["Phone"] = false,
            ["Photos"] = false,
            ["FeedbackHub"] = false,
            ["SoundRecorder"] = false,
            ["Alarms"] = false,
            ["SkypeApp"] = false,
            ["Maps"] = false,
            ["Camera"] = false,
            ["Video"] = false,
            ["BingNews"] = false,
            ["Mail"] = false,
            ["MicrosoftTeams"] = false,
            ["PowerAutomateDesktop"] = false,
            ["Cortana"] = false,
            ["ClipChamp"] = false,
            ["GetStarted"] = false,
            ["OneDrive"] = false,
            ["BingSports"] = false,
            ["BingFinance"] = false,
            ["MicrosoftFamily"] = false,
            ["BingSearch"] = false,
            ["Outlook"] = false,
            ["QuickAssist"] = false,
            ["DevHome"] = false,
            ["WindowsTerminal"] = false,
            ["LinkedIn"] = false,
            ["WebMediaExtensions"] = false,
            ["OneConnect"] = false,
            ["Edge"] = false
        };

        private static readonly SortedList<string, List<string>> PackageScripts = new SortedList<string, List<string>>
        {
            ["MicrosoftStore"] = new List<string>(1) { "Microsoft.WindowsStore" },
            ["Todos"] = new List<string>(1) { "Microsoft.Todos" },
            ["BingWeather"] = new List<string>(1) { "Microsoft.BingWeather" },
            ["Microsoft3D"] = new List<string>(1) { "Microsoft.Microsoft3DViewer" },
            ["Music"] = new List<string>(1) { "Microsoft.ZuneMusic" },
            ["GetHelp"] = new List<string>(1) { "Microsoft.GetHelp" },
            ["MicrosoftOfficeHub"] = new List<string>(1) { "Microsoft.MicrosoftOfficeHub" },
            ["MicrosoftSolitaireCollection"] = new List<string>(1) { "Microsoft.MicrosoftSolitaireCollection" },
            ["MixedReality"] = new List<string>(1) { "Microsoft.MixedReality.Portal" },
            ["Xbox"] = new List<string>(7) { "Microsoft.XboxApp", "Microsoft.GamingApp", "Microsoft.XboxGamingOverlay", "Microsoft.XboxGameOverlay", "Microsoft.XboxIdentityProvider", "Microsoft.Xbox.TCUI", "Microsoft.XboxSpeechToTextOverlay" },
            ["Paint3D"] = new List<string>(1) { "Microsoft.MSPaint" },
            ["OneNote"] = new List<string>(1) { "Microsoft.Office.OneNote" },
            ["People"] = new List<string>(1) { "Microsoft.People" },
            ["MicrosoftStickyNotes"] = new List<string>(1) { "Microsoft.MicrosoftStickyNotes" },
            ["Widgets"] = new List<string>(1) { "MicrosoftWindows.Client.WebExperience" },
            ["ScreenSketch"] = new List<string>(1) { "Microsoft.ScreenSketch" },
            ["Phone"] = new List<string>(1) { "Microsoft.YourPhone" },
            ["Photos"] = new List<string>(1) { "Microsoft.Windows.Photos" },
            ["FeedbackHub"] = new List<string>(1) { "Microsoft.WindowsFeedbackHub" },
            ["SoundRecorder"] = new List<string>(1) { "Microsoft.WindowsSoundRecorder" },
            ["Alarms"] = new List<string>(1) { "Microsoft.WindowsAlarms" },
            ["SkypeApp"] = new List<string>(1) { "Microsoft.SkypeApp" },
            ["Maps"] = new List<string>(1) { "Microsoft.WindowsMaps" },
            ["Camera"] = new List<string>(1) { "Microsoft.WindowsCamera" },
            ["Video"] = new List<string>(1) { "Microsoft.ZuneVideo" },
            ["BingNews"] = new List<string>(1) { "Microsoft.BingNews" },
            ["Mail"] = new List<string>(1) { "microsoft.windowscommunicationsapps" },
            ["MicrosoftTeams"] = new List<string>(2) { "MicrosoftTeams", "MSTeams" },
            ["PowerAutomateDesktop"] = new List<string>(1) { "Microsoft.PowerAutomateDesktop" },
            ["Cortana"] = new List<string>(1) { "Microsoft.549981C3F5F10" },
            ["ClipChamp"] = new List<string>(1) { "Clipchamp.Clipchamp" },
            ["GetStarted"] = new List<string>(1) { "Microsoft.Getstarted" },
            ["BingSports"] = new List<string>(1) { "Microsoft.BingSports" },
            ["BingFinance"] = new List<string>(1) { "Microsoft.BingFinance" },
            ["MicrosoftFamily"] = new List<string>(1) { "MicrosoftCorporationII.MicrosoftFamily" },
            ["BingSearch"] = new List<string>(1) { "Microsoft.BingSearch" },
            ["Outlook"] = new List<string>(1) { "Microsoft.OutlookForWindows" },
            ["QuickAssist"] = new List<string>(1) { "MicrosoftCorporationII.QuickAssist" },
            ["DevHome"] = new List<string>(1) { "Microsoft.Windows.DevHome" },
            ["WindowsTerminal"] = new List<string>(1) { "Microsoft.WindowsTerminal" },
            ["LinkedIn"] = new List<string>(1) { "Microsoft.LinkedIn" },
            ["WebMediaExtensions"] = new List<string>(1) { "Microsoft.WebMediaExtensions" },
            ["OneConnect"] = new List<string>(1) { "Microsoft.OneConnect" },
            ["Edge"] = new List<string>(2) { "Microsoft.MicrosoftEdge.Stable", "Microsoft.MicrosoftEdge.*" }
        };

        private static readonly SortedList<string, string> PackageAliases = new SortedList<string, string>
        {
            ["MicrosoftSolitaireCollection"] = "solitaire",
            ["MicrosoftOfficeHub"] = "officehub",
            ["FeedbackHub"] = "feedback",
            ["Mail"] = "communicationsapps",
            ["ClipChamp"] = "Clipchamp Video Editor",
            ["Music"] = "zunemusic",
            ["Video"] = "zunevideo",
            ["Widgets"] = "Windows.Client.WebExperience",
            ["LinkedIn"] = "LinkedInforWindows",
            ["Edge"] = "MicrosoftEdge"
        };
        #endregion

        internal async void ViewInstalledPackages() => InstalledPackages = await CommandExecutor.GetCommandOutput("Get-AppxPackage | Select-Object -ExpandProperty Name");

        internal static Task DeletingPackage(string packageName)
        {
            if (packageName == "OneDrive")
                return DeletedOneDrive();
            else
            {
                return Task.Run(() =>
                {
                    try
                    {
                        using Process process = new Process();
                        process.StartInfo.UseShellExecute = false;
                        process.StartInfo.CreateNoWindow = true;
                        process.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
                        process.EnableRaisingEvents = true;
                        process.StartInfo.FileName = "powershell.exe";
                        process.StartInfo.Arguments = $"Get-AppxProvisionedPackage -online | where-object {{$_.PackageName -like '*{packageName}*'}} | Remove-AppxProvisionedPackage -alluser -online –Verbose";
                        process.Start();
                        process.WaitForExit();

                        foreach (string getPackage in PackageScripts[packageName])
                        {
                            TrustedInstaller.CreateProcessAsTrustedInstaller(SettingsRepository.PID, $@"cmd.exe /c for /d %i in (""{pathPackage}\*{getPackage}*"") do rd /s /q ""%i""");
                            process.StartInfo.Arguments = $"Get-AppxPackage -Name {getPackage} -AllUsers | Remove-AppxPackage";
                            process.Start();
                            process.WaitForExit();
                        }

                        if (PackageAliases.ContainsKey(packageName))
                        {
                            process.StartInfo.Arguments = $"Get-AppxProvisionedPackage -online | where-object {{$_.PackageName -like '*{PackageAliases[packageName]}*'}} | Remove-AppxProvisionedPackage -alluser -online –Verbose";
                            process.Start();
                            process.WaitForExit();
                        }
                    }
                    catch (Exception ex) { Debug.WriteLine(ex); };

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
                        case "Edge":
                            DeletingTask(edgeTasks);
                            TrustedInstaller.CreateProcessAsTrustedInstaller(SettingsRepository.PID, $@"cmd.exe /c for /r ""%AppData%\Microsoft\Internet Explorer\Quick Launch"" %f in (*Edge*) do del ""%f""");
                            TrustedInstaller.CreateProcessAsTrustedInstaller(SettingsRepository.PID, $@"cmd.exe /c for /r ""{StoragePaths.SystemDisk}ProgramData\Microsoft\Windows\Start Menu\Programs"" %f in (*Edge*) do del ""%f""");
                            TrustedInstaller.CreateProcessAsTrustedInstaller(SettingsRepository.PID, $@"cmd.exe /c for /r ""%UserProfile%"" %f in (*edge*) do @if exist ""%f"" del /f /q ""%f""");
                            RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Microsoft\Active Setup\Installed Components\{9459C573-B17A-45AE-9F64-1857B5D58CEE}", true);
                            TrustedInstaller.CreateProcessAsTrustedInstaller(SettingsRepository.PID, $@"cmd.exe /c for /d %d in (""{StoragePaths.SystemDisk}Program Files (x86)\Microsoft\*Edge*"") do rmdir /s /q ""%d""");
                            TrustedInstaller.CreateProcessAsTrustedInstaller(SettingsRepository.PID, $@"cmd.exe /c for /f ""delims="" %i in ('dir /b /s ""{StoragePaths.SystemDisk}Windows\System32\Tasks\*Edge*""') do (if exist ""%i"" (if exist ""%i\"" (rmdir /s /q ""%i"") else (del /f /q ""%i"")))");
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
