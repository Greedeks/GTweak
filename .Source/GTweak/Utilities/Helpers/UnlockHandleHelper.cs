using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using GTweak.Utilities.Controls;

namespace GTweak.Utilities.Helpers
{
    internal static class UnlockHandleHelper
    {
        private const int ERROR_SUCCESS = 0;
        private const int ERROR_MORE_DATA = 234;

        [StructLayout(LayoutKind.Sequential)]
        private struct RM_UNIQUE_PROCESS
        {
            public int dwProcessId;
            public System.Runtime.InteropServices.ComTypes.FILETIME ProcessStartTime;
        }

        private enum RM_APP_TYPE
        {
            RmUnknownApp = 0,
            RmMainWindow = 1,
            RmOtherWindow = 2,
            RmService = 3,
            RmExplorer = 4,
            RmConsole = 5,
            RmCritical = 1000
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct RM_PROCESS_INFO
        {
            public RM_UNIQUE_PROCESS Process;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string strAppName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public string strServiceShortName;
            public RM_APP_TYPE ApplicationType;
            public uint AppStatus;
            public uint TSSessionId;
            [MarshalAs(UnmanagedType.Bool)]
            public bool bRestartable;
        }

        [DllImport("rstrtmgr.dll", CharSet = CharSet.Unicode)]
        private static extern int RmStartSession(out uint pSessionHandle, int dwSessionFlags, string strSessionKey);

        [DllImport("rstrtmgr.dll", CharSet = CharSet.Unicode)]
        private static extern int RmRegisterResources(uint pSessionHandle, uint nFiles, string[] rgsFilenames, uint nApplications, IntPtr rgApplications, uint nServices, string[] rgsServiceNames);

        [DllImport("rstrtmgr.dll")]
        private static extern int RmGetList(uint dwSessionHandle, out uint pnProcInfoNeeded, ref uint pnProcInfo, [In, Out] RM_PROCESS_INFO[] rgAffectedApps, out uint lpdwRebootReasons);

        [DllImport("rstrtmgr.dll")]
        private static extern int RmEndSession(uint pSessionHandle);

        internal static void UnlockDirectory(string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
            {
                return;
            }

            if (!Directory.Exists(directoryPath))
            {
                return;
            }

            HashSet<int> pids = GetUnlockProcessIds(directoryPath) ?? new HashSet<int>();

            foreach (int pid in pids)
            {
                try
                {
                    Process process = Process.GetProcessById(pid);
                    if (process.HasExited)
                    {
                        continue;
                    }

                    try
                    {
                        process.CloseMainWindow();
                        process.WaitForExit(1000);
                    }
                    catch (Exception ex) { ErrorLogging.LogDebug(ex); }

                    if (!process.HasExited)
                    {
                        process.Kill();
                        process.WaitForExit(2000);
                    }
                }
                catch (Exception ex) { ErrorLogging.LogDebug(ex); }
            }
        }

        private static HashSet<int> GetUnlockProcessIds(string directoryPath)
        {
            HashSet<int> result = new HashSet<int>();

            string[] files;
            try
            {
                files = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);
                if (files.Length == 0)
                {
                    files = new[] { Path.GetFullPath(directoryPath) };
                }
            }
            catch { files = new[] { Path.GetFullPath(directoryPath) }; }

            string sessionKey = Guid.NewGuid().ToString();

            if (RmStartSession(out uint sessionHandle, 0, sessionKey) != ERROR_SUCCESS)
            {
                return result;
            }

            try
            {
                if (RmRegisterResources(sessionHandle, (uint)files.Length, files, 0, IntPtr.Zero, 0, null) != ERROR_SUCCESS)
                {
                    return result;
                }

                uint count = 0;

                int res = RmGetList(sessionHandle, out uint needed, ref count, null, out uint rebootReasons);
                if (res != ERROR_MORE_DATA)
                {
                    return result;
                }

                RM_PROCESS_INFO[] infos = new RM_PROCESS_INFO[needed];
                count = needed;

                if (RmGetList(sessionHandle, out needed, ref count, infos, out rebootReasons) != ERROR_SUCCESS)
                {
                    return result;
                }

                foreach (var info in infos)
                {
                    result.Add(info.Process.dwProcessId);
                }
            }
            catch (Exception ex) { ErrorLogging.LogDebug(ex); }
            finally { RmEndSession(sessionHandle); }

            return result;
        }
    }
}
