using GTweak.Utilities.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace GTweak.Utilities.Configuration
{
    internal class MonitoringService
    {
        internal event Action<DeviceType> HandleDevicesEvents;
        private readonly List<ManagementEventWatcher> _eventWatchers = new List<ManagementEventWatcher>();
        private readonly ServiceController[] _servicesList = ServiceController.GetServices();

        internal string GetNumberRunningProcesses => GetProcessCount().Result;
        internal string GetNumberRunningService => GetServicesCount().Result;
        internal int GetMemoryUsage => GetPhysicalAvailableMemory().Result;
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

        [DllImport("psapi.dll", SetLastError = true)]
        static extern bool EnumProcesses([Out] uint[] lpidProcess, uint cb, out uint lpcbNeeded);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool GlobalMemoryStatusEx([In, Out] MemoryStatus lpBuffer);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetSystemTimes(out SystemTime lpIdleTime, out SystemTime lpKernelTime, out SystemTime lpUserTime);


        private async Task<string> GetProcessCount()
        {
            return await Task.Run(() =>
            {
                uint capacity = 1024;

                for (int attempt = 0; attempt < 3; attempt++)
                {
                    uint[] buffer = new uint[capacity];
                    bool success = EnumProcesses(buffer, capacity * sizeof(uint), out uint bytesNeeded);

                    if (!success)
                    {
                        if (attempt == 2) break;

                        if (bytesNeeded > capacity * sizeof(uint))
                            capacity = bytesNeeded / sizeof(uint) + 1;

                        continue;
                    }

                    if (bytesNeeded < capacity * sizeof(uint))
                        return (bytesNeeded / sizeof(uint)).ToString();

                    capacity = bytesNeeded / sizeof(uint) + 1;
                }

                return Process.GetProcesses().Length.ToString();
            }).ConfigureAwait(false);
        }

        private async Task<string> GetServicesCount()
        {
            return await Task.Run(() =>
            {
                int running = 0;
                Parallel.ForEach(_servicesList, svc =>
                {
                    try
                    {
                        svc.Refresh();
                        if (svc.Status == ServiceControllerStatus.Running)
                            running++;
                    }
                    catch (Exception ex) { ErrorLogging.LogDebug(ex); }
                });
                return running.ToString();
            }).ConfigureAwait(false);
        }

        private async Task<int> GetPhysicalAvailableMemory()
        {
            return await Task.Run(() =>
            {
                MemoryStatus memStatus = new MemoryStatus();
                if (!GlobalMemoryStatusEx(memStatus))
                    return 0;

                int totalMemory = (int)(memStatus.ullTotalPhys / 1048576);
                int availableMemory = (int)(memStatus.ullAvailPhys / 1048576);
                return (int)((float)(totalMemory - availableMemory) / totalMemory * 100);
            }).ConfigureAwait(false);
        }

        internal async Task GetTotalProcessorUsage()
        {
            bool success = false;
            try
            {
                await Task.Run(async () =>
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

                    GetProcessorUsage = Math.Min(100, Math.Max(0, (int)(100.0 * (totalTicksDiff - (newIdleTicks - idleTicks)) / totalTicksDiff)));
                    success = true;
                }).ConfigureAwait(false);
            }
            catch (Exception ex) { ErrorLogging.LogDebug(ex); }
            finally { if (!success) GetProcessorUsage = 1; }
        }

        internal void StartDeviceMonitoring()
        {
            if (SystemDiagnostics.isMsftAvailable)
                SubscribeToDeviceEvents($"TargetInstance ISA 'MSFT_PhysicalDisk'", DeviceType.Storage, @"root\Microsoft\Windows\Storage").ConfigureAwait(false);
            else
                SubscribeToDeviceEvents($"TargetInstance ISA 'Win32_DiskDrive'", DeviceType.Storage).ConfigureAwait(false);
            SubscribeToDeviceEvents("TargetInstance ISA 'Win32_SoundDevice'", DeviceType.Audio).ConfigureAwait(false);
            SubscribeToDeviceEvents("TargetInstance ISA 'Win32_NetworkAdapter' AND TargetInstance.NetConnectionStatus IS NOT NULL", DeviceType.Network).ConfigureAwait(false);
        }

        private async Task SubscribeToDeviceEvents(string filter, DeviceType type, string scope = @"root\CIMV2")
        {
            try
            {
                await Task.Run(() =>
                {
                    WqlEventQuery query = new WqlEventQuery("__InstanceOperationEvent", TimeSpan.FromSeconds(1), filter);
                    ManagementEventWatcher managementEvent = new ManagementEventWatcher(new ManagementScope(scope), query);
                    managementEvent.EventArrived += (s, e) => { HandleDevicesEvents?.Invoke(type); };
                    managementEvent.Start();
                    _eventWatchers.Add(managementEvent);
                });
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
            });
        }
    }
}
