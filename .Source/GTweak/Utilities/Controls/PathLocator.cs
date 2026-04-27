using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace GTweak.Utilities.Controls
{
    internal class PathLocator
    {
        internal static class Registry
        {
            internal const string SubKey = @"Software\GTweak";
            internal static readonly string BaseKey = @$"HKEY_CURRENT_USER\{SubKey}";
        }

        internal static class Links
        {
            internal const string GitHub = "https://github.com/Greedeks";

            internal const string Telegram = "https://t.me/Greedeks";

            internal const string Steam = "https://steamcommunity.com/id/greedeks/";

            internal static (string GitHub, string GitLabBase, string Resolved) LatestUpdate = ("https://github.com/Greedeks/GTweak/releases/latest/download/GTweak.exe", "https://gitlab.com/-/project/79375382/uploads/", GitHub);

            internal static readonly IReadOnlyList<string> ReleaseApi = Array.AsReadOnly(new[]
            {
                "https://api.github.com/repos/greedeks/gtweak/releases/latest",
                "https://gitlab.com/api/v4/projects/Greedeks%2Fgtweak-ota-server/releases"
            });

            internal static readonly IReadOnlyList<string> IpServices = Array.AsReadOnly(new[]
            {
                "https://free.freeipapi.com/api/json/",
                "https://api.db-ip.com/v2/free/self",
                "https://ipapi.co/json/",
                "https://reallyfreegeoip.org/json/",
                "https://get.geojs.io/v1/ip/geo.json",
                "https://api.ip.sb/geoip",
                "https://whois.pconline.com.cn/ipJson.jsp?json=true",
                "http://ip-api.com/json/"
            });

            internal static class Favicons
            {
                internal static string Google(string domain) => $"https://www.google.com/s2/favicons?domain={domain}&sz=48";
                internal static string DuckDuckGo(string domain) => $"https://icons.duckduckgo.com/ip3/{domain}.ico";
            }

            internal static class DownloadSources
            {
                internal static string GitHubLatest(string repoPath) => $"https://api.github.com/repos/{repoPath}/releases/latest";

                internal static string SourceForgeBest(string projectName) => $"https://sourceforge.net/projects/{projectName}/best_release.json";

                internal static string SourceForgeRss(string projectName) => $"https://sourceforge.net/projects/{projectName}/rss?path=/";

                internal static string SourceForgeFile(string projectName, string filePath) => $"https://downloads.sourceforge.net/project/{projectName}/{filePath}";

                internal static string SourceForgeRssRegex(string projectName) => $@"<link>https://sourceforge\.net/projects/{projectName}/files/([^<]+?)/download</link>";
            }
        }

        internal static class Folders
        {
            internal static readonly string Workspace = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "GTweak");

            internal static readonly string SystemDrive = Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System));

            internal static readonly string DefenderBackup = Path.Combine(Environment.SystemDirectory, "Config", "WDBackup_GTweak");

            internal static readonly string WindowsOld = Path.Combine(SystemDrive, "Windows.old");

            internal static readonly string Tasks = Path.Combine(Environment.SystemDirectory, "Tasks");

            internal static readonly string WallpaperCache = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Microsoft", "Windows", "Themes");

            internal static readonly string Edge = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Microsoft", "Edge");
        }

        internal static class Executable
        {
            private static readonly ConcurrentDictionary<string, string> exeCache = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            private static string FindExecutablePath(params string[] names)
            {
                if (names == null || names.Length == 0)
                {
                    return string.Empty;
                }

                for (int ni = 0; ni < names.Length; ni++)
                {
                    string name = names[ni];
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        continue;
                    }

                    if (exeCache.TryGetValue(name, out string cachedValue))
                    {
                        if (!string.IsNullOrEmpty(cachedValue))
                        {
                            return cachedValue;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    if (Path.IsPathRooted(name))
                    {
                        string absolute = File.Exists(name) ? name : string.Empty;
                        exeCache.TryAdd(name, absolute);
                        if (!string.IsNullOrEmpty(absolute))
                        {
                            return absolute;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    string pathextRaw = Environment.GetEnvironmentVariable("PATHEXT");
                    if (string.IsNullOrEmpty(pathextRaw))
                    {
                        pathextRaw = ".EXE";
                    }

                    string[] pathextEntries = pathextRaw.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int pei = 0; pei < pathextEntries.Length; pei++)
                    {
                        string e = pathextEntries[pei].Trim().Trim('"');
                        if (string.IsNullOrEmpty(e))
                        {
                            e = ".EXE";
                        }

                        if (e[0] != '.')
                        {
                            e = "." + e;
                        }

                        pathextEntries[pei] = e;
                    }

                    string[] namesToTry;
                    if (Path.HasExtension(name))
                    {
                        namesToTry = new[] { name };
                    }
                    else
                    {
                        List<string> tmp = new List<string>(pathextEntries.Length);
                        for (int pei = 0; pei < pathextEntries.Length; pei++)
                        {
                            tmp.Add(name + pathextEntries[pei]);
                        }

                        namesToTry = tmp.ToArray();
                    }

                    if (name.IndexOf(Path.DirectorySeparatorChar) >= 0 || name.IndexOf(Path.AltDirectorySeparatorChar) >= 0)
                    {
                        string current = Directory.GetCurrentDirectory();
                        for (int ti = 0; ti < namesToTry.Length; ti++)
                        {
                            string candidateRel = Path.Combine(current, namesToTry[ti]);
                            try
                            {
                                if (File.Exists(candidateRel))
                                {
                                    exeCache.TryAdd(name, candidateRel);
                                    return candidateRel;
                                }
                            }
                            catch (Exception ex) { ErrorLogging.LogDebug(ex); }
                        }
                    }

                    HashSet<string> searchDirs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                    string pathEnv = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
                    string[] pathEntries = pathEnv.Split(new[] { Path.PathSeparator }, StringSplitOptions.RemoveEmptyEntries);
                    for (int pe = 0; pe < pathEntries.Length; pe++)
                    {
                        string entry = pathEntries[pe].Trim().Trim('"');
                        if (!string.IsNullOrEmpty(entry))
                        {
                            searchDirs.Add(entry);
                        }
                    }

                    string systemDir = Environment.SystemDirectory;
                    if (!string.IsNullOrEmpty(systemDir))
                    {
                        searchDirs.Add(systemDir);
                    }

                    string windows = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
                    if (!string.IsNullOrEmpty(windows))
                    {
                        searchDirs.Add(Path.Combine(windows, "System32"));
                        searchDirs.Add(Path.Combine(windows, "Sysnative"));
                        searchDirs.Add(Path.Combine(windows, "SysWOW64"));
                    }

                    bool found = false;
                    string foundPath = string.Empty;
                    foreach (string dir in searchDirs)
                    {
                        try
                        {
                            if (!Directory.Exists(dir))
                            {
                                continue;
                            }

                            for (int ti = 0; ti < namesToTry.Length; ti++)
                            {
                                string candidate = Path.Combine(dir, namesToTry[ti]);
                                try
                                {
                                    if (File.Exists(candidate))
                                    {
                                        found = true;
                                        foundPath = candidate;
                                        break;
                                    }
                                }
                                catch (Exception ex) { ErrorLogging.LogDebug(ex); }
                            }

                            if (found)
                            {
                                break;
                            }
                        }
                        catch (Exception ex) { ErrorLogging.LogDebug(ex); }
                    }

                    if (found)
                    {
                        exeCache.TryAdd(name, foundPath);
                        return foundPath;
                    }

                    string[] programRoots = new[] { Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) };

                    for (int r = 0; r < programRoots.Length; r++)
                    {
                        string root = programRoots[r];
                        if (string.IsNullOrEmpty(root) || !Directory.Exists(root))
                        {
                            continue;
                        }

                        try
                        {
                            string[] candidateDirs = Directory.GetDirectories(root, "PowerShell*", SearchOption.TopDirectoryOnly);
                            for (int d = 0; d < candidateDirs.Length; d++)
                            {
                                string candidateDir = candidateDirs[d];
                                for (int ti = 0; ti < namesToTry.Length; ti++)
                                {
                                    string candidate = Path.Combine(candidateDir, namesToTry[ti]);
                                    try
                                    {
                                        if (File.Exists(candidate))
                                        {
                                            exeCache.TryAdd(name, candidate);
                                            return candidate;
                                        }
                                    }
                                    catch (Exception ex) { ErrorLogging.LogDebug(ex); }
                                }
                            }
                        }
                        catch (Exception ex) { ErrorLogging.LogDebug(ex); }
                    }

                    exeCache.TryAdd(name, string.Empty);
                }

                return string.Empty;
            }

            internal static readonly string CommandShell = FindExecutablePath("cmd.exe");

            internal static readonly string PowerShell = FindExecutablePath("pwsh.exe", "powershell.exe");

            internal static readonly string BcdEdit = FindExecutablePath("bcdedit.exe");

            internal static readonly string PowerCfg = FindExecutablePath("powercfg.exe");

            internal static readonly string Explorer = FindExecutablePath("explorer.exe");

            internal static readonly string OneDrive = FindExecutablePath("onedrivesetup.exe");

            internal static readonly string DisablingWD = Path.Combine(Folders.DefenderBackup, "DisablingWD.exe");

            internal static readonly string NSudo = Path.Combine(Folders.DefenderBackup, "NSudoLC.exe");
        }

        internal static class Files
        {
            internal static string Config = string.Empty;

            internal static readonly (string Original, string Backup) Hosts =
            (
                Path.Combine(Environment.SystemDirectory, "drivers", "etc", "hosts"),
                Path.Combine(Environment.SystemDirectory, "drivers", "etc", "hosts (Default GTweak)")
            );

            internal static readonly string PowPlan = Path.Combine(Folders.Workspace, "UltimatePerformance.pow");

            internal static readonly string BlankIcon = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Blank.ico");

            internal static readonly string ErrorLog = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GTweak_Error.log");

            internal static readonly string BackupJsonWD = Path.Combine(Folders.DefenderBackup, "BackupData.json");

            internal static readonly string BackupAclWD = Path.Combine(Folders.DefenderBackup, "AclBackup.acl");
        }

        internal static class Targets
        {
            private static (string Normal, string Block) CreatePath(string dir, string normalName, string blockName) => (Path.Combine(dir, normalName), Path.Combine(dir, blockName));

            internal static class WindowsUpdate
            {
                private static (string Normal, string Block) FindWindowsUpdateExe(string normalName, string blockName)
                {
                    static string TryFind(string name)
                    {
                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            try
                            {
                                string path = Path.Combine(Folders.SystemDrive, "Windows", "UUS", "amd64", name);

                                if (File.Exists(path))
                                {
                                    return path;
                                }

                                path = Path.Combine(Environment.SystemDirectory, name);
                                return File.Exists(path) ? path : string.Empty;
                            }
                            catch { return string.Empty; }
                        }

                        return string.Empty;
                    }

                    string normalPath = TryFind(normalName);
                    if (!string.IsNullOrEmpty(normalPath))
                    {
                        return (normalPath, Path.Combine(Path.GetDirectoryName(normalPath)!, blockName));
                    }

                    string blockPath = TryFind(blockName);
                    if (!string.IsNullOrEmpty(blockPath))
                    {
                        return (Path.Combine(Path.GetDirectoryName(blockPath)!, normalName), blockPath);
                    }

                    return (string.Empty, string.Empty);
                }

                internal static (string Normal, string Block) UsoClient => CreatePath(Environment.SystemDirectory, "usoclient.exe", "BlockUOrchestrator-GTweak.exe");

                internal static (string Normal, string Block) WorkerCore => FindWindowsUpdateExe("MoUsoCoreWorker.exe", "BlockUpdate-GTweak.exe");

                internal static (string Normal, string Block) WuauClient => FindWindowsUpdateExe("wuaucltcore.exe", "BlockUpdateCore-GTweak.exe");

                internal static (string Normal, string Block) WaaSMedic => FindWindowsUpdateExe("WaaSMedicAgent.exe", "BlockUpdateAgent-GTweak.exe");

                internal static (string Normal, string Block) MoNotificationUx => FindWindowsUpdateExe("MoNotificationUx.exe", "BlockUpdateNotify-GTweak.exe");

                internal static (string Normal, string Block)[] Mappings => new[] { UsoClient, WorkerCore, WuauClient, WaaSMedic, MoNotificationUx };

                internal static string[] CleanupFolders => new[]
                {
                    Path.Combine(Folders.SystemDrive, "Windows", "SoftwareDistribution", "Download"),
                    Path.Combine(Folders.SystemDrive, "Windows", "SoftwareDistribution", "DataStore"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "Windows", "DeliveryOptimization")
                };
            }

            internal static class Defender
            {
                internal static readonly string WindowsDefender = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Windows Defender");

                internal static (string Normal, string Block) SmartScreen => CreatePath(Environment.SystemDirectory, "smartscreen.exe", "BlockSS.exe");

                internal static (string Normal, string Block) DefenderEngine => CreatePath(WindowsDefender, "MsMpEng.exe", "BlockAntimalware.exe");

                internal static (string Normal, string Block) DefenderCore => CreatePath(WindowsDefender, "MpDefenderCoreService.exe", "BlockAntimalwareCore.exe");

                internal static (string Normal, string Block) MpCmdRun => CreatePath(WindowsDefender, "MpCmdRun.exe", "BlockMpCmdRun.exe");

                internal static (string Normal, string Block) HealthAttestation => CreatePath(Path.Combine(Environment.SystemDirectory, "HealthAttestationClient"), "HealthAttestationClientAgent.exe", "BlockHACA.exe");

                internal static (string Normal, string Block) MpCopyAccelerator => CreatePath(WindowsDefender, "MpCopyAccelerator.exe", "BlockMpCopyAccelerator.exe");

                internal static (string Normal, string Block) DlpUserAgent => CreatePath(WindowsDefender, "DlpUserAgent.exe", "BlockDlpUserAgent.exe");

                internal static (string Normal, string Block) MpDlpCmd => CreatePath(WindowsDefender, "MpDlpCmd.exe", "BlockMpDlpCmd.exe");

                internal static (string Normal, string Block) MipDlp => CreatePath(WindowsDefender, "MipDlp.exe", "BlockMDlp.exe");

                internal static (string Normal, string Block) MpDlpService => CreatePath(WindowsDefender, "MpDlpService.exe", "BlockMpDlpService.exe");

                internal static (string Normal, string Block) MpExtMs => CreatePath(WindowsDefender, "mpextms.exe", "Blockmpextms.exe");

                internal static (string Normal, string Block) NisSrv => CreatePath(WindowsDefender, "NisSrv.exe", "BlockNisSrv.exe");

                internal static (string Normal, string Block) ConfigSecurityPolicy => CreatePath(WindowsDefender, "ConfigSecurityPolicy.exe", "BlockConfigSecurityPolicy.exe");

                internal static (string Normal, string Block) WindowsDefenderX86 => CreatePath(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Windows Defender", "BlockWindowsDefenderX86");

                internal static (string Normal, string Block) DefenderATP => CreatePath(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Windows Defender Advanced Threat Protection", "BlockWindowsDefenderATP");

                internal static (string Normal, string Block)[] Mappings => new[] { SmartScreen, DefenderEngine, DefenderCore, MpCmdRun, HealthAttestation, MpCopyAccelerator, DlpUserAgent, MpDlpCmd, MipDlp, MpDlpService, MpExtMs, NisSrv, ConfigSecurityPolicy, WindowsDefenderX86, DefenderATP };

                internal static string[] CleanupFolders => new[]
                {
                    Path.Combine(Folders.SystemDrive, "ProgramData", "Microsoft", "Windows Defender", "Scans", "History"),
                    Path.Combine(Folders.SystemDrive, "ProgramData", "Microsoft", "Windows Defender", "Scans", "Workspace"),
                    Path.Combine(Folders.SystemDrive, "ProgramData", "Microsoft", "Windows Defender", "Support")
                };
            }
        }
    }
}
