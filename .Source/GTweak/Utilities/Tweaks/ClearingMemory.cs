using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

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
                catch (Exception ex)
                {
                    failProcesses.Add(ProcessName + ": " + ex.Message);
                }
            }

        }

        internal static bool Is64BitMode()
        {
            return Marshal.SizeOf(typeof(IntPtr)) == 8;
        }

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
            catch (Exception ex) { Debug.WriteLine(ex.Message.ToString()); }
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

        private static void ClearTempSystemCache()
        {
            Process.Start(new ProcessStartInfo()
            {
                Arguments = @"/c rd /s /q " + Settings.PathSystemDisk + @"\Windows\Temp & rd /s /q %localappdata%\Temp & 
                del /a /q ""%localappdata%\IconCache.db"" & del /a /f /q ""%localappdata%\Microsoft\Windows\Explorer\iconcache*""",
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                FileName = "cmd.exe"
            });
        }

        internal static void StartMemoryCleanup()
        {
            BackgroundWorker backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += (s, e) =>
            {
                Parallel.Invoke(() => { ClearFileSystemCache(true); },
                 EmptyWorkingSetFunction,
                 ClearTempSystemCache);
            };
            backgroundWorker.RunWorkerCompleted += (s, e) =>
            {
                new Thread(() => backgroundWorker.Dispose()).Start();
                new Thread(() => backgroundWorker.Dispose()).IsBackground = true;
                new ViewNotification().Show("", (string)Application.Current.Resources["title1_notification"], (string)Application.Current.Resources["clear_ram_notification"]);
            };
            backgroundWorker.RunWorkerAsync();
        }
    }
}
