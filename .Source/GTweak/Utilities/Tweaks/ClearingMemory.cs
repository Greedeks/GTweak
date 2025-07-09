using GTweak.Utilities.Controls;
using GTweak.Utilities.Helpers;
using GTweak.Utilities.Managers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading.Tasks;

namespace GTweak.Utilities.Tweaks
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct SYSTEM_CACHE_INFORMATION
    {
        internal uint CurrentSize;
        internal uint PeakSize;
        internal uint PageFaultCount;
        internal uint MinimumWorkingSet;
        internal uint MaximumWorkingSet;
        internal uint Unused1;
        internal uint Unused2;
        internal uint Unused3;
        internal uint Unused4;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct SYSTEM_CACHE_INFORMATION_64_BIT
    {
        internal long CurrentSize;
        internal long PeakSize;
        internal long PageFaultCount;
        internal long MinimumWorkingSet;
        internal long MaximumWorkingSet;
        internal long Unused1;
        internal long Unused2;
        internal long Unused3;
        internal long Unused4;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct TokPriv1Luid
    {
        internal int Count;
        internal long Luid;
        internal int Attr;
    }

    internal class ClearingMemory
    {
        internal static bool IsWinOldExists => Directory.Exists(Path.Combine(PathLocator.Folders.SystemDrive, "Windows.old"));

        private const int SE_PRIVILEGE_ENABLED = 2;
        private const string SE_INCREASE_QUOTA_NAME = "SeIncreaseQuotaPrivilege";
        private const string SE_PROFILE_SINGLE_PROCESS_NAME = "SeProfileSingleProcessPrivilege";
        private const int SystemFileCacheInformation = 0x0015;
        private const int SystemMemoryListInformation = 0x0050;
        private const int MemoryPurgeStandbyList = 4;

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool LookupPrivilegeValue(string host, string name, ref long pluid);

        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern bool AdjustTokenPrivileges(IntPtr htok, bool disall, ref TokPriv1Luid newst, int len, IntPtr prev, IntPtr relen);

        [DllImport("ntdll.dll")]
        internal static extern uint NtSetSystemInformation(int InfoClass, IntPtr Info, int Length);

        [DllImport("psapi.dll")]
        private static extern int EmptyWorkingSet(IntPtr hwProc);


        internal static void EmptyWorkingSetFunction()
        {
            string ProcessName = string.Empty;
            Process[] allProcesses = Process.GetProcesses();
            List<string> successProcesses = new List<string>();
            List<string> failProcesses = new List<string>();
            List<string> skipProcesses = new List<string>();
            HashSet<string> skipSet = new HashSet<string>() { "services", "csrss", "wininit", "csrss", "Registry", "Secure System", "smss", "MsMpEng", "System", "Idle" };


            for (int i = 0; i < allProcesses.Length; i++)
            {
                Process _process = allProcesses[i];
                try
                {
                    ProcessName = _process.ProcessName;
                    if (skipSet.Contains(ProcessName))
                    {
                        skipProcesses.Add(ProcessName);
                    }
                    else
                    {
                        EmptyWorkingSet(_process.Handle);
                        successProcesses.Add(ProcessName);
                    }
                }
                catch (Exception ex) { failProcesses.Add(ProcessName + ": " + ex.Message); }
            }

        }

        internal static bool Is64BitMode() => Marshal.SizeOf(typeof(IntPtr)) == 8;

        internal static void ClearFileSystemCache(bool ClearStandbyCache)
        {
            try
            {
                if (SetIncreasePrivilege(SE_INCREASE_QUOTA_NAME))
                {
                    uint num1;
                    int SystemInfoLength;
                    GCHandle gcHandle;
                    if (!Is64BitMode())
                    {
                        SYSTEM_CACHE_INFORMATION cacheInformation = new SYSTEM_CACHE_INFORMATION()
                        {
                            MinimumWorkingSet = uint.MaxValue,
                            MaximumWorkingSet = uint.MaxValue,
                        };
                        SystemInfoLength = Marshal.SizeOf(cacheInformation);
                        gcHandle = GCHandle.Alloc(cacheInformation, GCHandleType.Pinned);
                        num1 = NtSetSystemInformation(SystemFileCacheInformation, gcHandle.AddrOfPinnedObject(), SystemInfoLength);
                        gcHandle.Free();
                    }
                    else
                    {
                        SYSTEM_CACHE_INFORMATION_64_BIT information64Bit = new SYSTEM_CACHE_INFORMATION_64_BIT()
                        {
                            MinimumWorkingSet = -1L,
                            MaximumWorkingSet = -1L
                        };
                        SystemInfoLength = Marshal.SizeOf(information64Bit);
                        gcHandle = GCHandle.Alloc(information64Bit, GCHandleType.Pinned);
                        num1 = NtSetSystemInformation(SystemFileCacheInformation, gcHandle.AddrOfPinnedObject(), SystemInfoLength);
                        gcHandle.Free();
                    }
                    if (num1 != 0)
                    {
                        throw new Exception("NtSetSystemInformation(SYSTEMCACHEINFORMATION) error: ", new Win32Exception(Marshal.GetLastWin32Error()));
                    }
                }

                if (ClearStandbyCache && SetIncreasePrivilege(SE_PROFILE_SINGLE_PROCESS_NAME))
                {
                    int SystemInfoLength = Marshal.SizeOf(MemoryPurgeStandbyList);
                    GCHandle gcHandle = GCHandle.Alloc(MemoryPurgeStandbyList, GCHandleType.Pinned);
                    uint num2 = NtSetSystemInformation(SystemMemoryListInformation, gcHandle.AddrOfPinnedObject(), SystemInfoLength);
                    gcHandle.Free();
                    if (num2 != 0)
                    {
                        throw new Exception("NtSetSystemInformation(SYSTEMMEMORYLISTINFORMATION) error: ", new Win32Exception(Marshal.GetLastWin32Error()));
                    }
                }
            }
            catch (Exception ex) { ErrorLogging.LogDebug(ex); }
        }

        private static bool SetIncreasePrivilege(string privilegeName)
        {
            using WindowsIdentity current = WindowsIdentity.GetCurrent(TokenAccessLevels.Query | TokenAccessLevels.AdjustPrivileges);
            TokPriv1Luid newst;
            newst.Count = 1;
            newst.Luid = 0L;
            newst.Attr = SE_PRIVILEGE_ENABLED;

            if (!LookupPrivilegeValue(null, privilegeName, ref newst.Luid))
                throw new Exception("Error in LookupPrivilegeValue: ", new Win32Exception(Marshal.GetLastWin32Error()));

            int num = AdjustTokenPrivileges(current.Token, false, ref newst, 0, IntPtr.Zero, IntPtr.Zero) ? 1 : 0;

            if (num == 0)
                throw new Exception("Error in AdjustTokenPrivileges: ", new Win32Exception(Marshal.GetLastWin32Error()));

            return num != 0;
        }

        internal static void StartMemoryCleanupAsync(bool shouldRemoveWinOld = false)
        {
            ClearFileSystemCache(true);
            EmptyWorkingSetFunction();

            if (shouldRemoveWinOld)
            {
                string filePath = Path.Combine(PathLocator.Folders.SystemDrive, "Windows.old");
                CommandExecutor.RunCommandAsTrustedInstaller($"/c takeown /f \"{filePath}\"");
                CommandExecutor.RunCommandAsTrustedInstaller($"/c icacls \"{filePath}\" /inheritance:r /remove S-1-5-32-544 S-1-5-11 S-1-5-32-545 S-1-5-18");
                CommandExecutor.RunCommandAsTrustedInstaller($"/c icacls \"{filePath}\" /grant {Environment.UserName}:F");
                CommandExecutor.RunCommandAsTrustedInstaller($"/c rd /s /q \"{filePath}\"");
            }

            CommandExecutor.RunCommandAsTrustedInstaller(@$"/c rd /s /q {PathLocator.Folders.SystemDrive}Windows\Temp & " +
                @$"rd /s /q %localappdata%\Temp & rd /s /q {PathLocator.Folders.SystemDrive}Windows\ff*.tmp & rd /s /q {PathLocator.Folders.SystemDrive}Windows\History\* & " +
                $@"rd /s /q {PathLocator.Folders.SystemDrive}Windows\CbsTemp\* & rd /s /q {PathLocator.Folders.SystemDrive}Windows\System32\SleepStudy\* & " +
                $@"rd /s /q {PathLocator.Folders.SystemDrive}Users\%USERNAME%\AppData\Local\Microsoft\Windows\INetCache\IE\* & rd /s /q {PathLocator.Folders.SystemDrive}Windows\Downloaded Program Files\* & " +
                $@"del /f /q {PathLocator.Folders.SystemDrive}Windows\setupapi.log & rd /s /q {PathLocator.Folders.SystemDrive}Windows\Panther\* & " +
                $@"for /D %D in (""{PathLocator.Folders.SystemDrive}ProgramData\Microsoft\Windows\WER\*"") do (del /s /q ""%D\*.*"" & for /D %E in (""%D\*"") do rd /s /q ""%E"") & " +
                $@"del /f /q {PathLocator.Folders.SystemDrive}Windows\INF\setupapi.app.log & del /f /q {PathLocator.Folders.SystemDrive}Windows\INF\setupapi.dev.log & del /f /q {PathLocator.Folders.SystemDrive}Windows\INF\setupapi.offline.log");

            ExplorerManager.Restart(new Process(), async () =>
            {
                CommandExecutor.RunCommandAsTrustedInstaller(@"/c attrib -h -r -s %localappdata%\IconCache.db & del /a /f /q %localappdata%\IconCache.db & " +
                        @"del /a /f /q %localappdata%\Microsoft\Windows\Explorer\iconcache* & del /a /f /q %localappdata%\Microsoft\Windows\Explorer\thumbcache* & " +
                        @"del /a /f /q %localappdata%\Microsoft\Windows\Explorer\thumbcache_*.db & del /a /f /q %localappdata%\Microsoft\Windows\Explorer\thumbcache_exif.db & " +
                        @"del /a /f /q %localappdata%\Microsoft\Windows\Explorer\thumbcache_idx.db & del /a /f /q %localappdata%\Microsoft\Windows\Explorer\thumbcache_sr.db & " +
                        @"del /a /f /q %localappdata%\Microsoft\Windows\Explorer\thumbcache_wide.db & del /a /f /q %localappdata%\Microsoft\Windows\Explorer\thumbcache_96.db & " +
                        @"del /a /f /q %localappdata%\Microsoft\Windows\Explorer\thumbcache_256.db & del /a /f /q %localappdata%\Microsoft\Windows\Explorer\thumbcache_1024.db & " +
                        "ie4uinit.exe -show");

                await Task.Delay(200);
            });
        }
    }
}
