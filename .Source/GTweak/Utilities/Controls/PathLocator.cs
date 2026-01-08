using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace GTweak.Utilities.Controls
{
    internal class PathLocator
    {
        internal static class Folders
        {
            internal static readonly string Workspace = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "GTweak");

            internal static readonly string SystemDrive = Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System));

            internal static readonly string DefenderBackup = Path.Combine(Environment.SystemDirectory, "Config", "WDBackup_GTweak");

            internal static readonly string WindowsDefender = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Windows Defender");

            internal static readonly string WindowsOld = Path.Combine(SystemDrive, "Windows.old");

            internal static readonly string Tasks = Path.Combine(Environment.SystemDirectory, "Tasks");
        }

        internal static class Executable
        {
            private static readonly ConcurrentDictionary<string, string> exeCache = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            internal static string FindExecutablePath(params string[] names)
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
                            catch (Exception ex)
                            {
                                ErrorLogging.LogDebug(ex);
                            }
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
                                catch (Exception ex)
                                {
                                    ErrorLogging.LogDebug(ex);
                                }
                            }

                            if (found)
                            {
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            ErrorLogging.LogDebug(ex);
                        }
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

            internal static (string Normal, string Block) FindWindowsUpdateExe(string normalName, string blockName)
            {
                static string FindFile(string fileName)
                {
                    string uusPath = Path.Combine(Folders.SystemDrive, "Windows", "UUS", "amd64", fileName);
                    string system32Path = Path.Combine(Environment.SystemDirectory, fileName);

                    if (File.Exists(uusPath))
                    {
                        return uusPath;
                    }

                    if (File.Exists(system32Path))
                    {
                        return system32Path;
                    }

                    return null;
                }

                string normalPath = FindFile(normalName);
                string blockPath = FindFile(blockName);

                if (normalPath != null)
                {
                    return (normalPath, Path.Combine(Path.GetDirectoryName(normalPath), blockName));
                }

                if (blockPath != null)
                {
                    return (Path.Combine(Path.GetDirectoryName(blockPath), normalName), blockPath);
                }

                return (string.Empty, string.Empty);
            }


            internal static readonly string CommandShell = FindExecutablePath("cmd.exe");

            internal static readonly string PowerShell = FindExecutablePath("pwsh.exe", "powershell.exe");

            internal static readonly string BcdEdit = FindExecutablePath("bcdedit.exe");

            internal static readonly string PowerCfg = FindExecutablePath("powercfg.exe");

            internal static readonly string Explorer = FindExecutablePath("explorer.exe");

            internal static readonly string OneDrive = FindExecutablePath("onedrivesetup.exe");

            internal static readonly string NSudo = Path.Combine(Folders.DefenderBackup, "NSudoLC.exe");

            internal static (string Normal, string Block) UsoClient =>
            (
                Path.Combine(Environment.SystemDirectory, "usoclient.exe"),
                Path.Combine(Environment.SystemDirectory, "BlockUOrchestrator-GTweak.exe")
            );

            internal static (string Normal, string Block) WorkerCore => FindWindowsUpdateExe("MoUsoCoreWorker.exe", "BlockUpdate-GTweak.exe");

            internal static (string Normal, string Block) WuauClient => FindWindowsUpdateExe("wuaucltcore.exe", "BlockUpdateCore-GTweak.exe");

            internal static (string Normal, string Block) WaaSMedic => FindWindowsUpdateExe("WaaSMedicAgent.exe", "BlockUpdateAgent-GTweak.exe");

            internal static readonly string MpCmdRun = Path.Combine(Folders.SystemDrive, "Program Files", "Windows Defender", "MpCmdRun.exe");

            internal static readonly string DisablingWD = Path.Combine(Folders.DefenderBackup, "DisablingWD.exe");
        }

        internal static class Files
        {
            internal static string Config = string.Empty;

            internal static readonly string Hosts = Path.Combine(Environment.SystemDirectory, "drivers", "etc", "hosts");

            internal static readonly string PowPlan = Path.Combine(Folders.Workspace, "UltimatePerformance.pow");

            internal static readonly string BlankIcon = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Blank.ico");

            internal static readonly string ErrorLog = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GTweak_Error.log");

            internal static readonly string BackupDataJson = Path.Combine(Folders.DefenderBackup, "BackupData.json");

            internal static readonly string BackupRightsAcl = Path.Combine(Folders.DefenderBackup, "BackupRights.acl");
        }

        internal static class Registry
        {
            internal const string SubKey = @"Software\GTweak";
            internal static readonly string BaseKey = @$"HKEY_CURRENT_USER\{SubKey}";
        }
    }
}
