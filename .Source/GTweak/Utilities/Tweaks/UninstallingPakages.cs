using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using GTweak.Utilities.Controls;
using GTweak.Utilities.Helpers;
using GTweak.Utilities.Managers;
using Microsoft.Win32;

namespace GTweak.Utilities.Tweaks
{
    internal sealed class UninstallingPakages : TaskSchedulerManager
    {
        private static bool _isLocalAccount = false;

        internal static event Action DataChanged;
        internal static void OnPackagesChanged() => DataChanged?.Invoke();

        internal sealed class PackagesInfo
        {
            internal string Alias { get; }
            internal bool IsUnavailable { get; set; }
            internal IReadOnlyList<string> Scripts { get; }

            internal PackagesInfo(string alias = null, IReadOnlyList<string> scripts = null)
            {
                Alias = alias;
                Scripts = scripts;
            }
        }

        internal static bool IsOneDriveInstalled => File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "OneDrive", "OneDrive.exe")) ||
            File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Microsoft OneDrive", "OneDrive.exe")) || File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Microsoft OneDrive", "OneDrive.exe"));

        internal static readonly Dictionary<string, PackagesInfo> PackagesDetails = new Dictionary<string, PackagesInfo>(StringComparer.OrdinalIgnoreCase)
        {
            ["OneDrive"] = new PackagesInfo(),
            ["MicrosoftStore"] = new PackagesInfo(scripts: new[] { "Microsoft.WindowsStore" }),
            ["Todos"] = new PackagesInfo("TodoList", new[] { "Microsoft.Todos", "Microsoft.ToDo" }),
            ["BingWeather"] = new PackagesInfo("MSWeather", new[] { "Microsoft.BingWeather" }),
            ["Microsoft3D"] = new PackagesInfo("3DViewer", new[] { "Microsoft.Microsoft3DViewer" }),
            ["Music"] = new PackagesInfo("zunemusic", new[] { "Microsoft.ZuneMusic", "Microsoft.GrooveMusic" }),
            ["GetHelp"] = new PackagesInfo(scripts: new[] { "Microsoft.GetHelp" }),
            ["MicrosoftOfficeHub"] = new PackagesInfo("officehub", new[] { "Microsoft.MicrosoftOfficeHub" }),
            ["MicrosoftSolitaireCollection"] = new PackagesInfo("solitaire", new[] { "Microsoft.MicrosoftSolitaireCollection" }),
            ["MixedReality"] = new PackagesInfo("MixedRealityPortal", new[] { "Microsoft.MixedReality.Portal" }),
            ["Xbox"] = new PackagesInfo(scripts: new[] { "Microsoft.XboxApp", "Microsoft.GamingApp", "Microsoft.XboxGamingOverlay", "Microsoft.XboxGameOverlay", "Microsoft.XboxIdentityProvider", "Microsoft.Xbox.TCUI", "Microsoft.XboxSpeechToTextOverlay" }),
            ["Paint3D"] = new PackagesInfo(scripts: new[] { "Microsoft.MSPaint" }),
            ["OneNote"] = new PackagesInfo("MSOneNote", new[] { "Microsoft.Office.OneNote", "Microsoft.OneNote" }),
            ["People"] = new PackagesInfo(scripts: new[] { "Microsoft.People" }),
            ["MicrosoftStickyNotes"] = new PackagesInfo("MSStickyNotes", new[] { "Microsoft.MicrosoftStickyNotes" }),
            ["Widgets"] = new PackagesInfo("Windows.Client.WebExperience", new[] { "MicrosoftWindows.Client.WebExperience", "Microsoft.WidgetsPlatformRuntime", "Windows.Client.WebExperience" }),
            ["ScreenSketch"] = new PackagesInfo(scripts: new[] { "Microsoft.ScreenSketch" }),
            ["Phone"] = new PackagesInfo("PhoneLink", new[] { "Microsoft.YourPhone", "MicrosoftWindows.CrossDevice" }),
            ["Photos"] = new PackagesInfo("MSPhotos", new[] { "Microsoft.Windows.Photos" }),
            ["FeedbackHub"] = new PackagesInfo("feedback", new[] { "Microsoft.WindowsFeedbackHub" }),
            ["SoundRecorder"] = new PackagesInfo(scripts: new[] { "Microsoft.WindowsSoundRecorder" }),
            ["Alarms"] = new PackagesInfo(scripts: new[] { "Microsoft.WindowsAlarms" }),
            ["SkypeApp"] = new PackagesInfo("Skype", new[] { "Microsoft.SkypeApp" }),
            ["Maps"] = new PackagesInfo(scripts: new[] { "Microsoft.WindowsMaps" }),
            ["Camera"] = new PackagesInfo(scripts: new[] { "Microsoft.WindowsCamera" }),
            ["Video"] = new PackagesInfo("zunevideo", new[] { "Microsoft.ZuneVideo" }),
            ["BingNews"] = new PackagesInfo(scripts: new[] { "Microsoft.BingNews" }),
            ["Mail"] = new PackagesInfo("communicationsapps", new[] { "microsoft.windowscommunicationsapps" }),
            ["MicrosoftTeams"] = new PackagesInfo("Teams", new[] { "MicrosoftTeams", "MSTeams" }),
            ["PowerAutomateDesktop"] = new PackagesInfo(scripts: new[] { "Microsoft.PowerAutomateDesktop" }),
            ["Cortana"] = new PackagesInfo(scripts: new[] { "Microsoft.549981C3F5F10" }),
            ["ClipChamp"] = new PackagesInfo("Clipchamp Video Editor", new[] { "Clipchamp.Clipchamp" }),
            ["GetStarted"] = new PackagesInfo(scripts: new[] { "Microsoft.Getstarted" }),
            ["BingSports"] = new PackagesInfo(scripts: new[] { "Microsoft.BingSports" }),
            ["BingFinance"] = new PackagesInfo(scripts: new[] { "Microsoft.BingFinance" }),
            ["MicrosoftFamily"] = new PackagesInfo("FamilySafety", new[] { "MicrosoftCorporationII.MicrosoftFamily" }),
            ["BingSearch"] = new PackagesInfo(scripts: new[] { "Microsoft.BingSearch" }),
            ["Outlook"] = new PackagesInfo(scripts: new[] { "Microsoft.OutlookForWindows" }),
            ["QuickAssist"] = new PackagesInfo(scripts: new[] { "MicrosoftCorporationII.QuickAssist" }),
            ["DevHome"] = new PackagesInfo(scripts: new[] { "Microsoft.Windows.DevHome" }),
            ["WindowsTerminal"] = new PackagesInfo(scripts: new[] { "Microsoft.WindowsTerminal" }),
            ["LinkedIn"] = new PackagesInfo("LinkedInforWindows", new[] { "Microsoft.LinkedIn", "7EE7776C.LinkedInforWindows" }),
            ["WebMediaExtensions"] = new PackagesInfo(scripts: new[] { "Microsoft.WebMediaExtensions" }),
            ["OneConnect"] = new PackagesInfo("MobilePlans", new[] { "Microsoft.OneConnect" }),
            ["Edge"] = new PackagesInfo("MicrosoftEdge", new[] { "Microsoft.MicrosoftEdge.Stable", "Microsoft.MicrosoftEdgeDevToolsClient", "Microsoft.MicrosoftEdge.*", "Microsoft.Copilot" }),
            ["Notepad"] = new PackagesInfo("Notepad", new[] { "Microsoft.WindowsNotepad" }),
            ["Calculator"] = new PackagesInfo("Calculator", new[] { "Microsoft.WindowsCalculator" }),
            ["Copilot"] = new PackagesInfo("M365Copilot", new[] { "Microsoft.Copilot" }),
            ["WhiteBoard"] = new PackagesInfo(scripts: new[] { "Microsoft.Whiteboard" }),
            ["Wallet"] = new PackagesInfo("MSPay", new[] { "Microsoft.Wallet" }),
            ["Spotify"] = new PackagesInfo("Spotify", new[] { "SpotifyAB.SpotifyMusic" }),
            ["WhatsApp"] = new PackagesInfo("WhatsAppDesktop", new[] { "5319275A.WhatsAppDesktop" }),
            ["Facebook"] = new PackagesInfo("Facebook", new[] { "Facebook.Facebook" }),
            ["Hulu"] = new PackagesInfo("Hulu.Hulu", new[] { "HULULLC.HULUPLUS", "HuluLLC.HuluPlus" }),
            ["Disney"] = new PackagesInfo("Disney", new[] { "Disney.37853FC22B2CE" }),
            ["Instagram"] = new PackagesInfo("Instagram", new[] { "Facebook.InstagramBeta" }),
            ["Twitter"] = new PackagesInfo("Twitter", new[] { "9E2F88E3.Twitter" }),
            ["TikTok"] = new PackagesInfo("TikTok", new[] { "BytedancePte.Ltd.TikTok" }),
            ["MicrosoftSway"] = new PackagesInfo("Sway", new[] { "Microsoft.Office.Sway" }),
            ["Builder3D"] = new PackagesInfo("3D Builder", new[] { "Microsoft.3DBuilder" }),
            ["Netflix"] = new PackagesInfo("Netflix", new[] { "4DF9E0F8.Netflix" }),
            ["YandexMusic"] = new PackagesInfo(scripts: new[] { "A025C540.Yandex.Music" }),
            ["Messaging"] = new PackagesInfo(scripts: new[] { "Microsoft.Messaging" }),
            ["Picsart"] = new PackagesInfo(scripts: new[] { "PicsArt-PhotoStudio" }),
            ["PrimeVideo"] = new PackagesInfo("PrimeVideo", new[] { "AmazonVideo.PrimeVideo" }),
            ["TuneInRadio"] = new PackagesInfo("TuneInRadio", new[] { "TuneIn.TuneInRadio" }),
            ["iHeartRadio"] = new PackagesInfo("iHeart", new[] { "iHeartRadio" }),
            ["Shazam"] = new PackagesInfo("Shazam", new[] { "ShazamEntertainmentLtd.Shazam" }),
            ["Viber"] = new PackagesInfo(scripts: new[] { "Viber" }),
            ["Plex"] = new PackagesInfo("Plex", new[] { "CAF9E577.Plex" }),
            ["Pandora"] = new PackagesInfo("PandoraMediaInc", new[] { "PandoraMediaInc.29680B314EFC2" }),
            ["DolbyAccess"] = new PackagesInfo("DolbyAccess", new[] { "DolbyLaboratories.DolbyAccess" }),
        };

        internal static HashSet<string> InstalledPackagesCache = new HashSet<string>();

        internal void GetInstalledPackages()
        {
            try { InstalledPackagesCache = RegistryHelp.GetSubKeyNames<HashSet<string>>(Registry.CurrentUser, @"Software\Classes\Local Settings\Software\Microsoft\Windows\CurrentVersion\AppModel\Repository\Packages") ?? new HashSet<string>(); }
            catch (Exception ex)
            {
                ErrorLogging.LogDebug(ex);
                InstalledPackagesCache = new HashSet<string>();
            }
            finally { OnPackagesChanged(); }
        }

        internal static bool HandleAvailabilityStatus(string key, bool? isUnavailable = null)
        {
            if (PackagesDetails.TryGetValue(key, out var details))
            {
                if (isUnavailable.HasValue)
                {
                    details.IsUnavailable = isUnavailable.Value;
                }

                OnPackagesChanged();
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
            catch
            {
                _isLocalAccount = false;
            }
        }

        internal static async Task RestoreOneDriveFolder()
        {
            await CommandExecutor.InvokeRunCommand($@"/c {PathLocator.Executable.OneDrive}").ConfigureAwait(false);

            SetTaskState(true, oneDriveTask);

            RegistryHelp.CreateFolder(Registry.ClassesRoot, @"CLSID\{018D5C66-4533-4307-9B53-224DE2ED1FE6}");
            RegistryHelp.CreateFolder(Registry.ClassesRoot, @"Wow6432Node\CLSID\{018D5C66-4533-4307-9B53-224DE2ED1FE6}");
        }

        internal static async Task RemoveAppxPackage(string packageName, bool shouldRemoveWebView = false)
        {
            if (packageName == "OneDrive")
            {
                await CommandExecutor.InvokeRunCommand($@"/c taskkill /f /im OneDrive.exe & {PathLocator.Executable.OneDrive} /uninstall").ConfigureAwait(false);

                RegistryHelp.DeleteFolderTree(Registry.ClassesRoot, @"CLSID\{018D5C66-4533-4307-9B53-224DE2ED1FE6}");
                RegistryHelp.DeleteFolderTree(Registry.ClassesRoot, @"Wow6432Node\CLSID\{018D5C66-4533-4307-9B53-224DE2ED1FE6}");

                SetTaskState(false, oneDriveTask);

                CommandExecutor.RunCommand($@"/c rd /s /q %userprofile%\AppData\Local\Microsoft\OneDrive & rd /s /q %userprofile%\AppData\Local\OneDrive & 
                rd /s /q ""%allusersprofile%\Microsoft OneDrive"" & rd /s /q {PathLocator.Folders.SystemDrive}OneDriveTemp{(_isLocalAccount ? @" & rd /s /q %userprofile%\OneDrive" : "")}");

                return;
            }

            try
            {
                if (!PackagesDetails.TryGetValue(packageName, out PackagesInfo details))
                {
                    ErrorLogging.LogDebug(new InvalidOperationException($"PackageDetails does not contain key '{packageName}'"));
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
                    Get-AppxPackage -AllUsers | Where-Object {{ $_.Name -match $pattern }} | ForEach-Object {{ Remove-AppxPackage -AllUsers -Package $_.PackageFullName }}
                    Get-AppxProvisionedPackage -Online | Where-Object {{ $_.PackageName -match $pattern }} | ForEach-Object {{ Remove-AppxProvisionedPackage -Online -PackageName $_.PackageName -AllUsers}}";

                await CommandExecutor.InvokeRunCommand(psCommands, true).ConfigureAwait(false);

                CommandExecutor.RunCommandAsTrustedInstaller($@"/c for /d %i in ({string.Join(" ", packageNamesToRemove.Select(n => $@"""{Path.Combine(PathLocator.Folders.SystemDrive, "Program Files", "WindowsApps")}\*{n}*"""))}) do takeown /f ""%i"" /r /d y && icacls ""%i"" /inheritance:r /remove S-1-5-32-544 S-1-5-11 S-1-5-32-545 S-1-5-18 && icacls ""%i"" /grant {Environment.UserName}:F && rd /s /q ""%i""");
            }
            catch (Exception ex) { ErrorLogging.LogDebug(ex); }

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
                    if (RegistryHelp.KeyExists(Registry.ClassesRoot, @"*\shellex\ContextMenuHandlers\ModernSharing", true))
                    {
                        RegistryHelp.DeleteFolderTree(Registry.ClassesRoot, @"*\shellex\ContextMenuHandlers\SendTo");
                        RegistryHelp.DeleteFolderTree(Registry.ClassesRoot, @"*\shellex\ContextMenuHandlers\ModernShare");
                    }
                    else
                    {
                        RegistryHelp.DeleteFolderTree(Registry.ClassesRoot, @"AllFilesystemObjects\shellex\ContextMenuHandlers\SendTo");
                        RegistryHelp.DeleteFolderTree(Registry.ClassesRoot, @"AllFilesystemObjects\shellex\ContextMenuHandlers\ModernSharing");
                    }
                    CommandExecutor.RunCommandAsTrustedInstaller($@"/c reg delete ""HKEY_CLASSES_ROOT\CLSID\{{7AD84985-87B4-4a16-BE58-8B72A5B390F7}}"" /f & reg delete ""HKEY_CLASSES_ROOT\Wow6432Node\CLSID\{{7AD84985-87B4-4a16-BE58-8B72A5B390F7}}"" /f");
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
                                            {
                                                RegistryHelp.DeleteFolderTree(Registry.ClassesRoot, $@"SystemFileAssociations\{subkey}\shell\3D Print");
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex) { ErrorLogging.LogDebug(ex); }
                            }
                            baseKey.Close();
                        }
                    }
                    catch (Exception ex) { ErrorLogging.LogDebug(ex); }
                    break;
                case "Edge":
                    string[] processes = { "msedge", "edge", "edgeupdate", "edgeupdatem", "msedgewebview2", "microsoftedgeupdate", "msedgewebviewhost", "msedgeuserbroker", "usocoreworker", "widgets", "microsoftedgesh", "microsoftedgecp", "microsoftedge" };
                    CommandExecutor.RunCommandAsTrustedInstaller("/c taskkill /f " + string.Join(" ", processes.Select(p => $"/im {p}.exe")));

                    CommandExecutor.RunCommand("/c " + CommandExecutor.CleanCommand(string.Join(" & ", new[]
                    {
                        @"rmdir /s /q ""%LocalAppData%\Microsoft\Edge""",
                        @"rmdir /s /q ""%ProgramFiles%\Microsoft\Edge""",
                        @"del /f /q ""%AppData%\Microsoft\Internet Explorer\Quick Launch\*Edge*.lnk""",
                        @"del /f /q ""%AppData%\Microsoft\Internet Explorer\Quick Launch\User Pinned\TaskBar\*Edge*.lnk""",
                        @"del /f /q ""%Public%\Desktop\*Edge*.lnk""",
                        @"del /f /q ""%UserProfile%\Desktop\*Edge*.lnk""",
                        $@"del /f /q ""{PathLocator.Folders.SystemDrive}ProgramData\Microsoft\Windows\Start Menu\Programs\*Edge*.lnk""",
                        $@"for /r ""{PathLocator.Folders.SystemDrive}Users"" %f in (*Edge*) do @if exist ""%f"" del /f /q ""%f""",
                        $@"for /f ""delims="" %i in ('dir /b /s ""{PathLocator.Folders.SystemDrive}Windows\System32\Tasks\*Edge*""') do (if exist ""%i"" (if exist ""%i\"" (rmdir /s /q ""%i"") else (del /f /q ""%i"")))"
                    })));

                    RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", true);
                    RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Microsoft\Active Setup\Installed Components\{9459C573-B17A-45AE-9F64-1857B5D58CEE}", true);
                    RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\WOW6432Node\Microsoft\Edge", true);
                    RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Microsoft Edge", true);
                    RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Classes\MSEdgeHTM", true);
                    RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Clients\StartMenuInternet\Microsoft Edge", true);

                    if (shouldRemoveWebView)
                    {
                        RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Microsoft Edge Update", true);
                        RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\edgeupdate", true);
                        RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\edgeupdatem", true);
                        RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate", true);
                        RegistryHelp.DeleteFolderTree(Registry.ClassesRoot, @"AppID\MicrosoftEdgeUpdate.exe", true);
                        RegistryHelp.DeleteFolderTree(Registry.CurrentUser, @"Software\Microsoft\EdgeUpdate", true);
                        RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\WOW6432Node\Microsoft\EdgeWebView", true);
                        RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Microsoft\EdgeWebView", true);
                        RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Microsoft EdgeWebView", true);
                        RegistryHelp.DeleteFolderTree(Registry.CurrentUser, @"Software\Microsoft\EdgeWebView", true);
                        RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\MicrosoftEdgeElevationService", true);
                        RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Explorer\Browser Helper Objects", true);
                        RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Browser Helper Objects", true);

                        foreach (var path in new[] { @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", @"SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce" })
                        {
                            try
                            {
                                using RegistryKey key = Registry.CurrentUser.OpenSubKey(path, writable: true);
                                if (key != null)
                                {
                                    foreach (var valueName in key.GetValueNames())
                                    {
                                        if (valueName.Contains("MicrosoftEdgeAutoLaunch"))
                                        {
                                            RegistryHelp.DeleteValue(Registry.CurrentUser, path, valueName);
                                        }
                                    }
                                }
                            }
                            catch (Exception ex) { ErrorLogging.LogDebug(ex); }
                        }
                    }

                    RemoveTasks(edgeTasks);

                    static void RemoveDirectory(string path)
                    {
                        CommandExecutor.RunCommandAsTrustedInstaller($@"/c takeown /f ""{path}"" /r /d y && icacls ""{path}"" /inheritance:r && icacls ""{path}"" /remove *S-1-5-32-544 *S-1-5-11 *S-1-5-32-545 *S-1-5-18 && icacls ""{path}"" /grant {Environment.UserName}:F /t && rd /s /q ""{path}""");

                        for (int i = 0; Directory.Exists(path) && i < 10; i++)
                        {
                            try { Directory.Delete(path, true); Thread.Sleep(300); }
                            catch (Exception ex) { ErrorLogging.LogDebug(ex); }

                            CommandExecutor.RunCommand($"Remove-Item -LiteralPath '{path}' -Recurse -Force", true);
                        }
                    }

                    foreach (string folder in new[] { "Edge", "EdgeCore", "EdgeUpdate", "Temp", "EdgeWebView" })
                    {
                        if (!shouldRemoveWebView && (folder == "EdgeWebView" || folder == "EdgeCore" || folder == "EdgeUpdate"))
                        {
                            continue;
                        }

                        string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Microsoft", folder);
                        UnlockHandleHelper.UnlockDirectory(dir);
                        RemoveDirectory(dir);
                    }

                    try
                    {
                        using RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Appx\AppxAllUserStore\InboxApplications");
                        foreach (string subKey in key?.GetSubKeyNames() ?? Array.Empty<string>())
                        {
                            using RegistryKey subKeyEntry = key.OpenSubKey(subKey);
                            string path = subKeyEntry?.GetValue("Path") as string;
                            if (!string.IsNullOrEmpty(path) && path.Equals("Edge"))
                            {
                                if (!shouldRemoveWebView && path.Contains("WebView"))
                                {
                                    continue;
                                }

                                if (path.EndsWith(@"\AppxManifest.xml", StringComparison.OrdinalIgnoreCase))
                                {
                                    path = path.Replace(@"\AppxManifest.xml", "").Trim();
                                }

                                RemoveDirectory(path);

                                key.DeleteSubKey(subKey);

                                return;
                            }
                        }
                    }
                    catch (Exception ex) { ErrorLogging.LogDebug(ex); }
                    break;
                default:
                    break;
            }
        }
    }
}
