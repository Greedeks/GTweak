using GTweak.Utilities.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GTweak.Utilities.Configuration
{
    internal class MonitoringService
    {
        internal event Action<DeviceType> HandleDevicesEvents;
        private readonly List<ManagementEventWatcher> _eventWatchers = new List<ManagementEventWatcher>();

        internal int GetMemoryUsage => GetPhysicalAvailableMemory();
        internal string GetNumberRunningProcesses => Process.GetProcesses().Length.ToString();
        internal static int GetProcessorUsage { get; private set; } = 1;

        internal enum DeviceType
        {
            All,
            Storage,
            Audio,
            Network
        }


        [StructLayout(LayoutKind.Sequential)]
        private struct SystemTime
        {
            internal uint dwLowDateTime;
            internal uint dwHighDateTime;
        }

        [StructLayout(LayoutKind.Sequential)]
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
            internal MemoryStatus() => dwLength = (uint)Marshal.SizeOf(typeof(MemoryStatus));
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool GlobalMemoryStatusEx([In, Out] MemoryStatus lpBuffer);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetSystemTimes(out SystemTime lpIdleTime, out SystemTime lpKernelTime, out SystemTime lpUserTime);

        private static int GetPhysicalAvailableMemory()
        {
            MemoryStatus memStatus = new MemoryStatus();
            if (!GlobalMemoryStatusEx(memStatus))
                return 0;

            int totalMemory = (int)(memStatus.ullTotalPhys / 1048576);
            int availableMemory = (int)(memStatus.ullAvailPhys / 1048576);
            return (int)((float)(totalMemory - availableMemory) / totalMemory * 100);
        }

        internal async Task GetTotalProcessorUsage()
        {
            bool success = false;
            try
            {
                static ulong ConvertTimeToTicks(SystemTime systemTime) => ((ulong)systemTime.dwHighDateTime << 32) | systemTime.dwLowDateTime;

                if (!GetSystemTimes(out SystemTime idleTime, out SystemTime kernelTime, out SystemTime userTime))
                    return;

                ulong idleTicks = ConvertTimeToTicks(idleTime);
                ulong totalTicks = ConvertTimeToTicks(kernelTime) + ConvertTimeToTicks(userTime);

                await Task.Delay(1000);

                if (!GetSystemTimes(out idleTime, out kernelTime, out userTime))
                    return;

                ulong newIdleTicks = ConvertTimeToTicks(idleTime);
                ulong newTotalTicks = ConvertTimeToTicks(kernelTime) + ConvertTimeToTicks(userTime);

                ulong totalTicksDiff = newTotalTicks - totalTicks;

                GetProcessorUsage = (int)(100.0 * (totalTicksDiff - (newIdleTicks - idleTicks)) / totalTicksDiff);
                success = true;
            }
            catch (Exception ex) { ErrorLogging.LogDebug(ex); }
            finally { if (!success) GetProcessorUsage = 1; }
        }

        internal void StartDeviceMonitoring()
        {
            Task.Run(() =>
            {
                foreach (var wmiClass in new[] { "Win32_DiskDrive", "MSFT_PhysicalDisk" })
                {
                    SubscribeToDeviceEvents($"TargetInstance ISA '{wmiClass}'", DeviceType.Storage);
                }
                SubscribeToDeviceEvents("TargetInstance ISA 'Win32_SoundDevice'", DeviceType.Audio);
                SubscribeToDeviceEvents("TargetInstance ISA 'Win32_NetworkAdapter' AND TargetInstance.NetConnectionStatus IS NOT NULL", DeviceType.Network);
            }).ConfigureAwait(false);
        }


        private void SubscribeToDeviceEvents(string filter, DeviceType type)
        {
            try
            {
                WqlEventQuery query = new WqlEventQuery("__InstanceOperationEvent", TimeSpan.FromSeconds(1), filter);
                ManagementEventWatcher managementEvent = new ManagementEventWatcher(query);
                managementEvent.EventArrived += (s, e) => { HandleDevicesEvents?.Invoke(type); };
                managementEvent.Start();
                _eventWatchers.Add(managementEvent);
            }
            catch (Exception ex) { ErrorLogging.LogDebug(ex); }
        }

        internal void StopDeviceMonitoring()
        {
            Task.Run(() =>
            {
                foreach (ManagementEventWatcher managementEvent in _eventWatchers)
                {
                    try
                    {
                        managementEvent.Stop();
                        managementEvent.EventArrived -= null;
                        managementEvent.Dispose();
                    }
                    catch (Exception ex) { ErrorLogging.LogDebug(ex); }
                }
                _eventWatchers.Clear();
            }).ConfigureAwait(false);
        }
    }
}
