using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GTweak.Utilities.Configuration
{
    internal sealed class MonitoringSystem
    {
        internal int GetMemoryUsage => (int)Math.Truncate(100 - ((decimal)GetPhysicalAvailableMemory() / GetTotalMemory() * 100) + (decimal)0.5);
        internal string GetNumberRunningProcesses => Process.GetProcesses().Length.ToString();
        internal static int GetProcessorUsage = default;

        [DllImport("psapi.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetPerformanceInfo([Out] out PerformanceInformation PerformanceInformation, [In] int Size);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetSystemTimes(out SystemTime lpIdleTime, out SystemTime lpKernelTime, out SystemTime lpUserTime);

        [StructLayout(LayoutKind.Sequential)]
        private struct PerformanceInformation
        {
            internal int Size;
            internal IntPtr CommitTotal;
            internal IntPtr CommitLimit;
            internal IntPtr CommitPeak;
            internal IntPtr PhysicalTotal;
            internal IntPtr PhysicalAvailable;
            internal IntPtr SystemCache;
            internal IntPtr KernelTotal;
            internal IntPtr KernelPaged;
            internal IntPtr KernelNonPaged;
            internal IntPtr PageSize;
            internal int HandlesCount;
            internal int ProcessCount;
            internal int ThreadCount;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SystemTime
        {
            public uint dwLowDateTime;
            public uint dwHighDateTime;
        }

        private long GetPhysicalAvailableMemory()
        {
            if (GetPerformanceInfo(out PerformanceInformation _performanceInformation, Marshal.SizeOf(new PerformanceInformation())))
                return Convert.ToInt64((_performanceInformation.PhysicalAvailable.ToInt64() * _performanceInformation.PageSize.ToInt64() / 1048576));
            else
                return -1;
        }

        private long GetTotalMemory()
        {
            if (GetPerformanceInfo(out PerformanceInformation _performanceInformation, Marshal.SizeOf(new PerformanceInformation())))
                return Convert.ToInt64((_performanceInformation.PhysicalTotal.ToInt64() * _performanceInformation.PageSize.ToInt64() / 1048576));
            else
                return -1;
        }

        internal async Task<int> GetTotalProcessorUsage()
        {
            try
            {
                static ulong ConvertTimeToTicks(SystemTime systemTime) => ((ulong)systemTime.dwHighDateTime << 32) | systemTime.dwLowDateTime;

                if (!GetSystemTimes(out SystemTime idleTime, out SystemTime kernelTime, out SystemTime userTime))
                    return 0;

                ulong idleTicks = ConvertTimeToTicks(idleTime);
                ulong totalTicks = ConvertTimeToTicks(kernelTime) + ConvertTimeToTicks(userTime);

                await Task.Delay(500);

                if (!GetSystemTimes(out idleTime, out kernelTime, out userTime))
                    return 0;

                ulong newIdleTicks = ConvertTimeToTicks(idleTime);
                ulong newTotalTicks = ConvertTimeToTicks(kernelTime) + ConvertTimeToTicks(userTime);

                ulong totalTicksDiff = newTotalTicks - totalTicks;

                GetProcessorUsage = (int)(100.0 * (totalTicksDiff - (newIdleTicks - idleTicks)) / totalTicksDiff);
                return GetProcessorUsage;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return 0;
            }
        }
    }
}
