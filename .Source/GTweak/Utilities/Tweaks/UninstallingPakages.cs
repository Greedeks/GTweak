using GTweak.Utilities.Control;
using GTweak.Utilities.Helpers;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace GTweak.Utilities.Tweaks
{
    internal sealed class UninstallingPakages
    {
        internal static string InstalledPackages { get; private set; }

        internal static bool IsOneDriveInstalled => File.Exists(Environment.ExpandEnvironmentVariables(@"%userprofile%\AppData\Local\Microsoft\OneDrive\OneDrive.exe"));
        private static bool isLocalAccount = false;
        private static readonly string pathPackage = $@"{UsePath.SystemDisk}Program Files\WindowsApps";

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
            ["WindowsTerminal"] = false
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
            ["Widgets"] = "Windows.Client.WebExperience"
        };
        #endregion

        internal void ViewInstalledPackages()
        {
            List<string> packages = new List<string>();
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = "Get-AppxPackage | Select-Object -ExpandProperty Name",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using Process process = new Process { StartInfo = startInfo };

            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode == 0)
            {
                string[] packageNames = output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                packages.AddRange(packageNames);
                InstalledPackages = string.Join(Environment.NewLine, packages);
            }
        }

        internal static void DeletingPackage(string packageName)
        {
            if (packageName == "OneDrive")
                DeletedOneDrive();

            else
            {
                using (Process process = new Process())
                {
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
                    process.EnableRaisingEvents = true;
                    process.StartInfo.FileName = "powershell.exe";
                    process.StartInfo.Arguments = $"Get-AppxProvisionedPackage -online | where-object {{$_.PackageName -like '*{packageName}*'}} | Remove-AppxProvisionedPackage -alluser -online –Verbose";
                    process.Start();

                    foreach (string getPackage in PackageScripts[packageName])
                    {
                        TrustedInstaller.CreateProcessAsTrustedInstaller(SettingsRepository.PID, $@"cmd.exe /c for /d %i in (""{pathPackage}\*{getPackage}*"") do rd /s /q ""%i""");
                        process.StartInfo.Arguments = $"Get-AppxPackage -Name {getPackage} -AllUsers | Remove-AppxPackage";
                        process.Start();
                    }

                    if (PackageAliases.ContainsKey(packageName))
                    {
                        process.StartInfo.Arguments = $"Get-AppxProvisionedPackage -online | where-object {{$_.PackageName -like '*{PackageAliases[packageName]}*'}} | Remove-AppxProvisionedPackage -alluser -online –Verbose";
                        process.Start();
                    }

                    process.WaitForExit(1000);
                }

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
                }
            }
        }

        internal static void DeletedOneDrive()
        {
            using (Process process = new Process())
            {
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
                process.EnableRaisingEvents = true;
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = @"/c taskkill /f /im OneDrive.exe & %systemroot%\System32\OneDriveSetup.exe /uninstall & %systemroot%\SysWOW64\OneDriveSetup.exe /uninstall";
                process.Start();
                process.WaitForExit(1000);

                while (!process.HasExited && process.Responding)
                {
                    RegistryHelp.DeleteFolderTree(Registry.ClassesRoot, @"CLSID\{018D5C66-4533-4307-9B53-224DE2ED1FE6}");
                    RegistryHelp.DeleteFolderTree(Registry.ClassesRoot, @"Wow6432Node\CLSID\{018D5C66-4533-4307-9B53-224DE2ED1FE6}");
                }
            }

            string argumentsFolders = $@"/c rd /s /q %userprofile%\AppData\Local\Microsoft\OneDrive & rd /s /q %userprofile%\AppData\Local\OneDrive
            & rd /s /q ""%allusersprofile%\Microsoft OneDrive"" & rd /s /q {UsePath.SystemDisk}\OneDriveTemp";

            if (isLocalAccount)
                argumentsFolders += @" & rd /s /q %userprofile%\OneDrive";

            Process.Start(new ProcessStartInfo()
            {
                Arguments = argumentsFolders,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                FileName = "cmd.exe"
            });
        }

        internal static void ResetOneDrive()
        {
            using (Process process = new Process())
            {
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
                process.EnableRaisingEvents = true;
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = @"/c %systemroot%\System32\OneDriveSetup.exe & %systemroot%\SysWOW64\OneDriveSetup.exe";
                process.Start();
                process.WaitForExit(1000);
            }

            RegistryHelp.CreateFolder(Registry.ClassesRoot, @"CLSID\{018D5C66-4533-4307-9B53-224DE2ED1FE6}");
            RegistryHelp.CreateFolder(Registry.ClassesRoot, @"Wow6432Node\CLSID\{018D5C66-4533-4307-9B53-224DE2ED1FE6}");
        }

        internal void CheckingForLocalAccount()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = @"powershell.exe",
                Arguments = @"Get-LocalUser | Where-Object { $_.Enabled -match 'True'} | Select-Object -ExpandProperty PrincipalSource",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process process = new Process() { StartInfo = startInfo, EnableRaisingEvents = true };

            process.Start();

            isLocalAccount = !process.StandardOutput.ReadToEnd().Contains("MicrosoftAccount");

            process.Close();
            process.Dispose();
        }
    }
}
