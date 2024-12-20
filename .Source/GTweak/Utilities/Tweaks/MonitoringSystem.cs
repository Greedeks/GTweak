using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GTweak.Utilities.Tweaks
{
    internal sealed class MonitoringSystem
    {
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

        [DllImport("psapi.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetPerformanceInfo([Out] out PerformanceInformation PerformanceInformation, [In] int Size);

        private static long GetPhysicalAvailableMemory()
        {
            if (GetPerformanceInfo(out PerformanceInformation _performanceInformation, Marshal.SizeOf(new PerformanceInformation())))
                return Convert.ToInt64((_performanceInformation.PhysicalAvailable.ToInt64() * _performanceInformation.PageSize.ToInt64() / 1048576));
            else
                return -1;
        }

        private static long GetTotalMemory()
        {
            if (GetPerformanceInfo(out PerformanceInformation _performanceInformation, Marshal.SizeOf(new PerformanceInformation())))
                return Convert.ToInt64((_performanceInformation.PhysicalTotal.ToInt64() * _performanceInformation.PageSize.ToInt64() / 1048576));
            else
                return -1;
        }

        internal int GetMemoryUsage() => (int)Math.Truncate(100 - ((decimal)GetPhysicalAvailableMemory() / GetTotalMemory() * 100) + (decimal)0.5);

        internal string GetNumberRunningProcesses => Process.GetProcesses().Length.ToString();

        internal static int GetProcessorUsage { get; private set; }

        internal static async Task<int> GetTotalProcessorUsage()
        {
            return await Task.Run(async () =>
            {
                using PerformanceCounter cpuCounter = new PerformanceCounter("Processor Information", "% Processor Utility", "_Total", true);
                cpuCounter.NextValue();
                await Task.Delay(1000);
                GetProcessorUsage = (int)cpuCounter.NextValue();
                return GetProcessorUsage;
            });
        }
    }
}
