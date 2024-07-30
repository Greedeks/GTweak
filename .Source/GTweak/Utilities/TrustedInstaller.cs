using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.ServiceProcess;

namespace GTweak.Utilities
{
    internal sealed class TrustedInstaller
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool CloseHandle(IntPtr hObject);

        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern bool AdjustTokenPrivileges(IntPtr htok, bool disall, ref TokPrivLuid newst, int len, IntPtr prev, IntPtr relen);

        [DllImport("advapi32.dll", SetLastError = true)]
        static extern bool OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccess, out IntPtr TokenHandle);

        [DllImport("advapi32.dll", SetLastError = true)]
        static extern bool ImpersonateLoggedOnUser(IntPtr hToken);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern IntPtr OpenSCManager(string lpMachineName, string lpDatabaseName, uint dwDesiredAccess);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern IntPtr OpenService(IntPtr hSCManager, string lpServiceName, uint dwDesiredAccess);

        [DllImport("advapi32.dll", SetLastError = true)]
        static extern bool QueryServiceStatusEx(IntPtr hService, int InfoLevel, ref SERVICE_STATUS_PROCESS lpBuffer, uint cbBufSize, out uint pcbBytesNeeded);

        [DllImport("advapi32.dll", SetLastError = true)]
        static extern bool StartService(IntPtr hService, uint dwNumServiceArgs, IntPtr lpServiceArgVectors);

        [DllImport("advapi32.dll", SetLastError = true)]
        static extern bool CloseServiceHandle(IntPtr hSCObject);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CreateProcess(string lpApplicationName, string lpCommandLine, ref SECURITY_ATTRIBUTES lpProcessAttributes, ref SECURITY_ATTRIBUTES lpThreadAttributes, bool bInheritHandles, uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, [In] ref STARTUPINFOEX lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(ProcessAccessFlags processAccess, bool bInheritHandle, int processId);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UpdateProcThreadAttribute(IntPtr lpAttributeList, uint dwFlags, IntPtr Attribute, IntPtr lpValue, IntPtr cbSize, IntPtr lpPreviousValue, IntPtr lpReturnSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool InitializeProcThreadAttributeList(IntPtr lpAttributeList, int dwAttributeCount, int dwFlags, ref IntPtr lpSize);

        const uint MAXIMUM_ALLOWED = 0x02000000;

        const uint SC_MANAGER_CONNECT = 0x0001;
        const uint SC_MANAGER_ENUMERATE_SERVICE = 0x0004;
        const uint SC_MANAGER_QUERY_LOCK_STATUS = 0x0010;
        const uint SERVICE_QUERY_STATUS = 0x0004;
        const uint SERVICE_START = 0x0010;
        const int SC_STATUS_PROCESS_INFO = 0;
        const string ServicesActiveDatabase = "ServicesActive";

        const int PROC_THREAD_ATTRIBUTE_PARENT_PROCESS = 0x00020000;
        const uint EXTENDED_STARTUPINFO_PRESENT = 0x00080000;
        const uint CREATE_NO_WINDOW = 0x08000000;

        [StructLayout(LayoutKind.Sequential)]
        struct SECURITY_ATTRIBUTES
        {
            public int nLength;
            public IntPtr lpSecurityDescriptor;
            public bool bInheritHandle;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct SERVICE_STATUS_PROCESS
        {
            public uint dwServiceType;
            public uint dwCurrentState;
            public uint dwControlsAccepted;
            public uint dwWin32ExitCode;
            public uint dwServiceSpecificExitCode;
            public uint dwCheckPoint;
            public uint dwWaitHint;
            public uint dwProcessId;
            public uint dwServiceFlags;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct STARTUPINFO
        {
            public int cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public uint dwX;
            public uint dwY;
            public uint dwXSize;
            public uint dwYSize;
            public uint dwXCountChars;
            public uint dwYCountChars;
            public uint dwFillAttribute;
            public uint dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public uint dwProcessId;
            public uint dwThreadId;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct TokPrivLuid
        {
            public int Count;
            public long Luid;
            public int Attr;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct STARTUPINFOEX
        {
            public STARTUPINFO StartupInfo;
            public IntPtr lpAttributeList;
        }

        [Flags]
        public enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VirtualMemoryOperation = 0x00000008,
            VirtualMemoryRead = 0x00000010,
            VirtualMemoryWrite = 0x00000020,
            DuplicateHandle = 0x00000040,
            CreateProcess = 0x000000080,
            SetQuota = 0x00000100,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            QueryLimitedInformation = 0x00001000,
            Synchronize = 0x00100000
        }

        public static bool CheckIfAdmin()
        {
            WindowsIdentity currentIdentity = WindowsIdentity.GetCurrent();
            WindowsPrincipal currentPrincipal = new WindowsPrincipal(currentIdentity);
            return currentPrincipal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        public static bool ImpersonateSystem()
        {
            Process[] processlist = Process.GetProcesses();
            IntPtr tokenHandle = IntPtr.Zero;

            foreach (Process theProcess in processlist)
            {
                if (theProcess.ProcessName == "winlogon")
                {
                    bool token = OpenProcessToken(theProcess.Handle, MAXIMUM_ALLOWED, out tokenHandle);
                    if (!token)
                    {
                        return false;
                    }
                    else
                    {
                        token = ImpersonateLoggedOnUser(tokenHandle);
                        CloseHandle(theProcess.Handle);
                        CloseHandle(tokenHandle);
                        return true;
                    }
                }
            }
            CloseHandle(tokenHandle);
            return false;
        }
        public static int StartTrustedInstallerService()
        {
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

            uint bytesNeeded;
            SERVICE_STATUS_PROCESS statusBuffer = new SERVICE_STATUS_PROCESS();
            while (QueryServiceStatusEx(hService, SC_STATUS_PROCESS_INFO, ref statusBuffer, (uint)Marshal.SizeOf(statusBuffer), out bytesNeeded))
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
                    return (int)statusBuffer.dwProcessId;
                }
            }

            CloseServiceHandle(hService);
            CloseServiceHandle(hSCManager);
            throw new Win32Exception("QueryServiceStatusEx failed: " + Marshal.GetLastWin32Error());
        }
        public static void CreateProcessAsTrustedInstaller(int parentProcessId, string binaryPath)
        {

            ImpersonateSystem();

            var pInfo = new PROCESS_INFORMATION();
            var siEx = new STARTUPINFOEX();

            IntPtr lpValueProc = IntPtr.Zero;
            IntPtr hSourceProcessHandle = IntPtr.Zero;
            var lpSize = IntPtr.Zero;

            InitializeProcThreadAttributeList(IntPtr.Zero, 1, 0, ref lpSize);
            siEx.lpAttributeList = Marshal.AllocHGlobal(lpSize);
            InitializeProcThreadAttributeList(siEx.lpAttributeList, 1, 0, ref lpSize);

            IntPtr parentHandle = OpenProcess(ProcessAccessFlags.CreateProcess | ProcessAccessFlags.DuplicateHandle, false, parentProcessId);

            lpValueProc = Marshal.AllocHGlobal(IntPtr.Size);
            Marshal.WriteIntPtr(lpValueProc, parentHandle);

            UpdateProcThreadAttribute(siEx.lpAttributeList, 0, (IntPtr)PROC_THREAD_ATTRIBUTE_PARENT_PROCESS, lpValueProc, (IntPtr)IntPtr.Size, IntPtr.Zero, IntPtr.Zero);

            var ps = new SECURITY_ATTRIBUTES();
            var ts = new SECURITY_ATTRIBUTES();
            ps.nLength = Marshal.SizeOf(ps);
            ts.nLength = Marshal.SizeOf(ts);

            bool ret = CreateProcess(null, binaryPath, ref ps, ref ts, true, EXTENDED_STARTUPINFO_PRESENT | CREATE_NO_WINDOW, IntPtr.Zero, null, ref siEx, out pInfo);

            string stringPid = pInfo.dwProcessId.ToString();

        }
    }
}
