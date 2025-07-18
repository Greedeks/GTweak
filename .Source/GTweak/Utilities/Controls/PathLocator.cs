using System;
using System.IO;
using System.Linq;

namespace GTweak.Utilities.Controls
{
    internal class PathLocator
    {
        internal static class Folders
        {
            internal static readonly string Workspace = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "GTweak");

            internal static readonly string SystemDrive = Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System));

            internal static readonly string DefenderBackup = Path.Combine(Environment.SystemDirectory, "Config", "WDBackup_GTweak");
        }

        internal static class Executable
        {
            private static string FindExecutablePath(string name)
            {
                string[] searchDir = Environment.GetEnvironmentVariable("PATH")?.Split(Path.PathSeparator) ?? Array.Empty<string>();
                searchDir = searchDir.Concat(new[] { Environment.SystemDirectory, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "System32"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Sysnative"), Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "SysWOW64")})
                    .Concat(Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)) ? Directory.GetDirectories(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "PowerShell*", SearchOption.TopDirectoryOnly) : Array.Empty<string>())
                    .Concat(Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)) ? Directory.GetDirectories(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "PowerShell*", SearchOption.TopDirectoryOnly) : Array.Empty<string>()
                    ).Where(dir => !string.IsNullOrEmpty(dir)).ToArray();

                return searchDir.Select(dir => Path.Combine(dir, name)).FirstOrDefault(File.Exists);
            }

            internal static readonly string CommandShell = FindExecutablePath("cmd.exe");

            internal static readonly string PowerShell = FindExecutablePath("pwsh.exe") ?? FindExecutablePath("powershell.exe");

            internal static readonly string BcdEdit = FindExecutablePath("bcdedit.exe");

            internal static readonly string PowerCfg = FindExecutablePath("powercfg.exe");

            internal static readonly string Explorer = FindExecutablePath("explorer.exe");
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

            internal static readonly string WindowsOld = Path.Combine(Folders.SystemDrive, "Windows.old");
        }

        internal static class Registry
        {
            internal const string SubKey = @"Software\GTweak";
            internal static readonly string BaseKey = @$"HKEY_CURRENT_USER\{SubKey}";
        }
    }
}
