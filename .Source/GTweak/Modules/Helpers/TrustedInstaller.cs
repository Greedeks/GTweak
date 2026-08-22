using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;

namespace GTweak.Modules.Helpers
{
    internal sealed class TrustedInstaller
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetCurrentProcess();

        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern bool AdjustTokenPrivileges(IntPtr htok, bool disall, ref TOKEN_PRIVILEGES newst, int len, IntPtr prev, IntPtr relen);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool LookupPrivilegeValue(string lpSystemName, string lpName, out LUID lpLuid);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccess, out IntPtr TokenHandle);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool DuplicateTokenEx(IntPtr hExistingToken, uint dwDesiredAccess, IntPtr lpTokenAttributes, SECURITY_IMPERSONATION_LEVEL ImpersonationLevel, TOKEN_TYPE TokenType, out IntPtr phNewToken);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool ImpersonateLoggedOnUser(IntPtr hToken);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool RevertToSelf();

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr OpenSCManager(string lpMachineName, string lpDatabaseName, uint dwDesiredAccess);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr OpenService(IntPtr hSCManager, string lpServiceName, uint dwDesiredAccess);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool QueryServiceStatusEx(IntPtr hService, int InfoLevel, ref SERVICE_STATUS_PROCESS lpBuffer, uint cbBufSize, out uint pcbBytesNeeded);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool StartService(IntPtr hService, uint dwNumServiceArgs, IntPtr lpServiceArgVectors);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool CloseServiceHandle(IntPtr hSCObject);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CreateProcess(string lpApplicationName, string lpCommandLine, ref SECURITY_ATTRIBUTES lpProcessAttributes, ref SECURITY_ATTRIBUTES lpThreadAttributes, bool bInheritHandles, uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, [In] ref STARTUPINFOEX lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(ProcessAccessFlags processAccess, bool bInheritHandle, int processId);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UpdateProcThreadAttribute(IntPtr lpAttributeList, uint dwFlags, IntPtr Attribute, IntPtr lpValue, UIntPtr cbSize, IntPtr lpPreviousValue, IntPtr lpReturnSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool InitializeProcThreadAttributeList(IntPtr lpAttributeList, int dwAttributeCount, int dwFlags, ref UIntPtr lpSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern void DeleteProcThreadAttributeList(IntPtr lpAttributeList);

        private const uint TOKEN_ADJUST_PRIVILEGES = 0x0020;
        private const uint TOKEN_QUERY = 0x0008;
        private const uint TOKEN_DUPLICATE = 0x0002;
        private const uint MAXIMUM_ALLOWED = 0x02000000;
        private const int SE_PRIVILEGE_ENABLED = 0x00000002;

        private const uint SC_MANAGER_CONNECT = 0x0001;
        private const uint SC_MANAGER_ENUMERATE_SERVICE = 0x0004;
        private const uint SC_MANAGER_QUERY_LOCK_STATUS = 0x0010;
        private const uint SERVICE_QUERY_STATUS = 0x0004;
        private const uint SERVICE_START = 0x0010;
        private const int SC_STATUS_PROCESS_INFO = 0;
        private const string ServicesActiveDatabase = "ServicesActive";

        private const uint PROC_THREAD_ATTRIBUTE_PARENT_PROCESS = 0x00020000;
        private const uint EXTENDED_STARTUPINFO_PRESENT = 0x00080000;

        private enum SECURITY_IMPERSONATION_LEVEL
        {
            SecurityAnonymous,
            SecurityIdentification,
            SecurityImpersonation,
            SecurityDelegation
        }

        private enum TOKEN_TYPE
        {
            TokenPrimary = 1,
            TokenImpersonation
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct LUID
        {
            public uint LowPart;
            public int HighPart;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct LUID_AND_ATTRIBUTES
        {
            public LUID Luid;
            public uint Attributes;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TOKEN_PRIVILEGES
        {
            public uint PrivilegeCount;
            public LUID_AND_ATTRIBUTES Privileges;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SECURITY_ATTRIBUTES
        {
            internal int nLength;
            internal IntPtr lpSecurityDescriptor;
            internal bool bInheritHandle;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SERVICE_STATUS_PROCESS
        {
            internal uint dwServiceType;
            internal uint dwCurrentState;
            internal uint dwControlsAccepted;
            internal uint dwWin32ExitCode;
            internal uint dwServiceSpecificExitCode;
            internal uint dwCheckPoint;
            internal uint dwWaitHint;
            internal uint dwProcessId;
            internal uint dwServiceFlags;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct STARTUPINFO
        {
            internal int cb;
            internal string lpReserved;
            internal string lpDesktop;
            internal string lpTitle;
            internal uint dwX;
            internal uint dwY;
            internal uint dwXSize;
            internal uint dwYSize;
            internal uint dwXCountChars;
            internal uint dwYCountChars;
            internal uint dwFillAttribute;
            internal uint dwFlags;
            internal short wShowWindow;
            internal short cbReserved2;
            internal IntPtr lpReserved2;
            internal IntPtr hStdInput;
            internal IntPtr hStdOutput;
            internal IntPtr hStdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PROCESS_INFORMATION
        {
            internal IntPtr hProcess;
            internal IntPtr hThread;
            internal uint dwProcessId;
            internal uint dwThreadId;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct STARTUPINFOEX
        {
            internal STARTUPINFO StartupInfo;
            internal IntPtr lpAttributeList;
        }

        [Flags]
        private enum ProcessAccessFlags : uint
        {
            CreateProcess = 0x00000080,
            QueryLimitedInformation = 0x00001000
        }

        private static bool EnablePrivilege(string privilegeName)
        {
            if (!OpenProcessToken(GetCurrentProcess(), TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, out IntPtr tokenHandle))
            {
                return false;
            }

            try
            {
                if (!LookupPrivilegeValue(null, privilegeName, out LUID luid))
                {
                    return false;
                }

                TOKEN_PRIVILEGES tp = new TOKEN_PRIVILEGES
                {
                    PrivilegeCount = 1,
                    Privileges = new LUID_AND_ATTRIBUTES
                    {
                        Luid = luid,
                        Attributes = SE_PRIVILEGE_ENABLED
                    }
                };

                return AdjustTokenPrivileges(tokenHandle, false, ref tp, Marshal.SizeOf(tp), IntPtr.Zero, IntPtr.Zero);
            }
            finally
            {
                CloseHandle(tokenHandle);
            }
        }

        private static bool IsProcessRunning(int processId)
        {
            if (processId <= 0)
            {
                return false;
            }

            try
            {
                using Process process = Process.GetProcessById(processId);
                return !process.HasExited && string.Equals(process.ProcessName, "TrustedInstaller", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private static bool ImpersonateSystem()
        {
            EnablePrivilege("SeDebugPrivilege");
            EnablePrivilege("SeImpersonatePrivilege");

            IntPtr primaryToken = IntPtr.Zero;
            IntPtr duplicateToken = IntPtr.Zero;
            IntPtr winlogonHandle = IntPtr.Zero;

            try
            {
                int currentSession = Process.GetCurrentProcess().SessionId;
                Process[] winlogons = Process.GetProcessesByName("winlogon");

                Process targetWinlogon = winlogons.FirstOrDefault(p => p.SessionId == currentSession) ?? winlogons.FirstOrDefault();

                if (targetWinlogon == null)
                {
                    return false;
                }

                winlogonHandle = OpenProcess(ProcessAccessFlags.QueryLimitedInformation, false, targetWinlogon.Id);
                if (winlogonHandle == IntPtr.Zero)
                {
                    return false;
                }

                if (!OpenProcessToken(winlogonHandle, TOKEN_DUPLICATE | TOKEN_QUERY, out primaryToken))
                {
                    return false;
                }

                if (!DuplicateTokenEx(primaryToken, MAXIMUM_ALLOWED, IntPtr.Zero,
                    SECURITY_IMPERSONATION_LEVEL.SecurityImpersonation,
                    TOKEN_TYPE.TokenImpersonation, out duplicateToken))
                {
                    return false;
                }

                return ImpersonateLoggedOnUser(duplicateToken);
            }
            finally
            {
                if (primaryToken != IntPtr.Zero)
                {
                    CloseHandle(primaryToken);
                }

                if (duplicateToken != IntPtr.Zero)
                {
                    CloseHandle(duplicateToken);
                }

                if (winlogonHandle != IntPtr.Zero)
                {
                    CloseHandle(winlogonHandle);
                }
            }
        }

        internal static void StartTrustedInstallerService()
        {
            CommandExecutor.RunCommand("/c sc config TrustedInstaller start= demand && sc start TrustedInstaller");
            IntPtr hSCManager = OpenSCManager(null, ServicesActiveDatabase, SC_MANAGER_CONNECT | SC_MANAGER_ENUMERATE_SERVICE | SC_MANAGER_QUERY_LOCK_STATUS);
            if (hSCManager == IntPtr.Zero)
            {
                throw new Win32Exception("OpenSCManager failed: " + Marshal.GetLastWin32Error());
            }

            IntPtr hService = OpenService(hSCManager, "TrustedInstaller", SERVICE_QUERY_STATUS | SERVICE_START);
            if (hService == IntPtr.Zero)
            {
                CloseServiceHandle(hSCManager);
                throw new Win32Exception("OpenService failed: " + Marshal.GetLastWin32Error());
            }

            SERVICE_STATUS_PROCESS statusBuffer = new SERVICE_STATUS_PROCESS();
            while (QueryServiceStatusEx(hService, SC_STATUS_PROCESS_INFO, ref statusBuffer, (uint)Marshal.SizeOf(statusBuffer), out _))
            {
                if (statusBuffer.dwCurrentState == (uint)ServiceControllerStatus.Stopped)
                {
                    if (!StartService(hService, 0, IntPtr.Zero))
                    {
                        CloseServiceHandle(hService);
                        CloseServiceHandle(hSCManager);
                        throw new Win32Exception("StartService failed: " + Marshal.GetLastWin32Error());
                    }
                }
                if (statusBuffer.dwCurrentState == (uint)ServiceControllerStatus.StartPending || statusBuffer.dwCurrentState == (uint)ServiceControllerStatus.StopPending)
                {
                    System.Threading.Thread.Sleep((int)statusBuffer.dwWaitHint);
                    continue;
                }
                if (statusBuffer.dwCurrentState == (uint)ServiceControllerStatus.Running)
                {
                    CloseServiceHandle(hService);
                    CloseServiceHandle(hSCManager);

                    int pid = (int)statusBuffer.dwProcessId;
                    if (pid <= 0)
                    {
                        Process ti = Process.GetProcessesByName("TrustedInstaller").FirstOrDefault();
                        if (ti != null)
                        {
                            pid = ti.Id;
                            ti.Dispose();
                        }
                    }

                    CommandExecutor.PID = pid;
                    return;
                }
            }

            CloseServiceHandle(hService);
            CloseServiceHandle(hSCManager);
            throw new Win32Exception("QueryServiceStatusEx failed: " + Marshal.GetLastWin32Error());
        }

        internal static void CreateProcessAsTrustedInstaller(int parentProcessId, string binaryPath, bool showWindow = false)
        {
            Exception lastException = null;
            bool impersonated = false;

            UIntPtr lpSize = UIntPtr.Zero;
            InitializeProcThreadAttributeList(IntPtr.Zero, 1, 0, ref lpSize);
            if (lpSize == UIntPtr.Zero)
            {
                throw new Win32Exception("InitializeProcThreadAttributeList returned zero size");
            }

            for (int attempt = 0; attempt < 3; attempt++)
            {
                IntPtr lpValueProc = IntPtr.Zero;
                IntPtr parentHandle = IntPtr.Zero;
                IntPtr attributeList = IntPtr.Zero;

                try
                {
                    if (!IsProcessRunning(parentProcessId))
                    {
                        StartTrustedInstallerService();
                        parentProcessId = CommandExecutor.PID;
                    }

                    if (!impersonated)
                    {
                        impersonated = ImpersonateSystem();
                    }

                    if (!impersonated)
                    {
                        throw new Win32Exception("Failed to impersonate SYSTEM identity");
                    }

                    attributeList = Marshal.AllocHGlobal((IntPtr)(long)lpSize);

                    if (!InitializeProcThreadAttributeList(attributeList, 1, 0, ref lpSize))
                    {
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    }

                    parentHandle = OpenProcess(ProcessAccessFlags.CreateProcess, false, parentProcessId);
                    if (parentHandle == IntPtr.Zero)
                    {
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    }

                    lpValueProc = Marshal.AllocHGlobal(IntPtr.Size);
                    Marshal.WriteIntPtr(lpValueProc, parentHandle);

                    if (!UpdateProcThreadAttribute(attributeList, 0, new IntPtr(unchecked(PROC_THREAD_ATTRIBUTE_PARENT_PROCESS)), lpValueProc, new UIntPtr((uint)IntPtr.Size), IntPtr.Zero, IntPtr.Zero))
                    {
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    }

                    STARTUPINFOEX siEx = new STARTUPINFOEX();
                    siEx.StartupInfo.cb = Marshal.SizeOf(typeof(STARTUPINFOEX));
                    siEx.StartupInfo.dwFlags = 0x00000001;
                    siEx.StartupInfo.wShowWindow = showWindow ? (short)5 : (short)0;
                    siEx.lpAttributeList = attributeList;

                    SECURITY_ATTRIBUTES ps = new SECURITY_ATTRIBUTES { nLength = Marshal.SizeOf(typeof(SECURITY_ATTRIBUTES)) };
                    SECURITY_ATTRIBUTES ts = new SECURITY_ATTRIBUTES { nLength = Marshal.SizeOf(typeof(SECURITY_ATTRIBUTES)) };

                    if (!CreateProcess(null, binaryPath, ref ps, ref ts, false, EXTENDED_STARTUPINFO_PRESENT, IntPtr.Zero, null, ref siEx, out PROCESS_INFORMATION pInfo))
                    {
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    }

                    CloseHandle(pInfo.hProcess);
                    CloseHandle(pInfo.hThread);
                    return;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    parentProcessId = 0;
                }
                finally
                {
                    if (lpValueProc != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(lpValueProc);
                    }

                    if (parentHandle != IntPtr.Zero)
                    {
                        CloseHandle(parentHandle);
                    }

                    if (attributeList != IntPtr.Zero)
                    {
                        DeleteProcThreadAttributeList(attributeList);
                        Marshal.FreeHGlobal(attributeList);
                    }
                    if (impersonated)
                    {
                        RevertToSelf();
                        impersonated = false;
                    }
                }
            }

            throw lastException ?? new Win32Exception("CreateProcessAsTrustedInstaller failed after 3 attempts");
        }
    }
}