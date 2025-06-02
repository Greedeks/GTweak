using GTweak.Utilities.Controls;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GTweak.Utilities.Configuration
{
    internal sealed class MonitoringSystem
    {
        internal int GetMemoryUsage => GetPhysicalAvailableMemory();
        internal string GetNumberRunningProcesses => Process.GetProcesses().Length.ToString();
        internal static int GetProcessorUsage = default;

        [StructLayout(LayoutKind.Sequential)]
        private struct SystemTime
        {
            public uint dwLowDateTime;
            public uint dwHighDateTime;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private class MemoryStatus
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
            public MemoryStatus() => dwLength = (uint)Marshal.SizeOf(typeof(MemoryStatus));

        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool GlobalMemoryStatusEx([In, Out] MemoryStatus lpBuffer);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetSystemTimes(out SystemTime lpIdleTime, out SystemTime lpKernelTime, out SystemTime lpUserTime);

        private static int GetTotalMemory()
        {
            MemoryStatus memStatus = new MemoryStatus();
            if (GlobalMemoryStatusEx(memStatus))
                return (int)(memStatus.ullTotalPhys / 1048576);
            else
                return 0;
        }

        private static int GetAvailableMemory()
        {
            MemoryStatus memStatus = new MemoryStatus();
            if (GlobalMemoryStatusEx(memStatus))
                return (int)(memStatus.ullAvailPhys / 1048576);
            else
                return 0;
        }

        private static int GetPhysicalAvailableMemory()
        {
            int totalMemory = GetTotalMemory();
            int availableMemory = GetAvailableMemory();
            return (int)((float)(totalMemory - availableMemory) / totalMemory * 100);
        }

        internal async Task GetTotalProcessorUsage()
        {
            try
            {
                static ulong ConvertTimeToTicks(SystemTime systemTime) => ((ulong)systemTime.dwHighDateTime << 32) | systemTime.dwLowDateTime;

                if (!GetSystemTimes(out SystemTime idleTime, out SystemTime kernelTime, out SystemTime userTime))
                {
                    GetProcessorUsage = 1;
                    return;
                }

                ulong idleTicks = ConvertTimeToTicks(idleTime);
                ulong totalTicks = ConvertTimeToTicks(kernelTime) + ConvertTimeToTicks(userTime);

                await Task.Delay(1000);

                if (!GetSystemTimes(out idleTime, out kernelTime, out userTime))
                {
                    GetProcessorUsage = 1;
                    return;
                }

                ulong newIdleTicks = ConvertTimeToTicks(idleTime);
                ulong newTotalTicks = ConvertTimeToTicks(kernelTime) + ConvertTimeToTicks(userTime);

                ulong totalTicksDiff = newTotalTicks - totalTicks;

                GetProcessorUsage = (int)(100.0 * (totalTicksDiff - (newIdleTicks - idleTicks)) / totalTicksDiff);

            }
            catch (Exception ex)
            {
                ErrorLogging.LogDebug(ex);
                GetProcessorUsage = 1;
            }
        }
    }
}
