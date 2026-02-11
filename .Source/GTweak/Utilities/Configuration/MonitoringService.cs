using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using GTweak.Utilities.Controls;

namespace GTweak.Utilities.Configuration
{
    internal class MonitoringService : HardwareData
    {
        internal event Action<DeviceType> HandleDevicesEvents;
        private readonly List<(ManagementEventWatcher watcher, EventArrivedEventHandler handler)> _watcherHandler = new List<(ManagementEventWatcher watcher, EventArrivedEventHandler handler)>();
        private readonly ServiceController[] _servicesList = ServiceController.GetServices();

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
        private static extern bool EnumProcesses([Out] uint[] lpidProcess, uint cb, out uint lpcbNeeded);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool GlobalMemoryStatusEx([In, Out] MemoryStatus lpBuffer);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetSystemTimes(out SystemTime lpIdleTime, out SystemTime lpKernelTime, out SystemTime lpUserTime);

        internal async Task<string> GetProcessCount()
        {
            return await Task.Run(() =>
            {
                const uint initialCapacity = 1024;
                uint capacity = initialCapacity;
                uint[] buffer = new uint[capacity];
                bool success = false;

                for (int attempt = 0; attempt < 3; attempt++)
                {
                    success = EnumProcesses(buffer, capacity * sizeof(uint), out uint bytesNeeded);

                    if (!success)
                    {
                        if (attempt == 2)
                        {
                            break;
                        }

                        if (bytesNeeded > capacity * sizeof(uint))
                        {
                            capacity = bytesNeeded / sizeof(uint) + 1;
                            buffer = new uint[capacity];
                        }

                        continue;
                    }

                    if (bytesNeeded < capacity * sizeof(uint))
                    {
                        return (bytesNeeded / sizeof(uint)).ToString();
                    }

                    capacity = bytesNeeded / sizeof(uint) + 1;
                    buffer = new uint[capacity];
                }

                return Process.GetProcesses().Length.ToString();
            }).ConfigureAwait(false);
        }

        internal async Task<string> GetServicesCount()
        {
            int running = 0;

            await Task.WhenAll(_servicesList.Select(async svc =>
            {
                try
                {
                    svc.Refresh();
                    if (svc.Status == ServiceControllerStatus.Running)
                    {
                        Interlocked.Increment(ref running);
                    }
                }
                catch (Exception ex) { ErrorLogging.LogDebug(ex); }
            }).ToArray()).ConfigureAwait(false);

            return running.ToString();
        }

        internal async Task GetPhysicalAvailableMemory()
        {
            await Task.Run(() =>
            {
                MemoryStatus memStatus = new MemoryStatus();
                if (!GlobalMemoryStatusEx(memStatus))
                {
                    Memory.Usage = 0;
                }

                int totalMemory = (int)(memStatus.ullTotalPhys / 1048576);
                int availableMemory = (int)(memStatus.ullAvailPhys / 1048576);
                Memory.Usage = (int)((float)(totalMemory - availableMemory) / totalMemory * 100);
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
                    {
                        return;
                    }

                    ulong idleTicks = ConvertTimeToTicks(idleTime);
                    ulong totalTicks = ConvertTimeToTicks(kernelTime) + ConvertTimeToTicks(userTime);

                    await Task.Delay(1000);

                    if (!GetSystemTimes(out idleTime, out kernelTime, out userTime))
                    {
                        return;
                    }

                    ulong newIdleTicks = ConvertTimeToTicks(idleTime);
                    ulong newTotalTicks = ConvertTimeToTicks(kernelTime) + ConvertTimeToTicks(userTime);

                    ulong totalTicksDiff = newTotalTicks - totalTicks;

                    Processor.Usage = Math.Min(100, Math.Max(0, (int)(100.0 * (totalTicksDiff - (newIdleTicks - idleTicks)) / totalTicksDiff)));
                    success = true;
                }).ConfigureAwait(false);
            }
            catch (Exception ex) { ErrorLogging.LogDebug(ex); }
            finally
            {
                if (!success)
                {
                    Processor.Usage = 1;
                }
            }
        }

        internal async Task StartDeviceMonitoring()
        {
            await Task.WhenAll(new List<(string filter, DeviceType type, string scope)>
            {
                ($"TargetInstance ISA {(SystemDataCollector.isMsftAvailable ? "'MSFT_PhysicalDisk'" : "'Win32_DiskDrive'")}", DeviceType.Storage, SystemDataCollector.isMsftAvailable ? @"root\microsoft\windows\storage" : null),
                ("TargetInstance ISA 'Win32_SoundDevice'", DeviceType.Audio, null),
                ("TargetInstance ISA 'Win32_NetworkAdapter' AND TargetInstance.NetConnectionStatus IS NOT NULL", DeviceType.Network, null)
            }.Select(device => Task.Run(() => SubscribeToDeviceEvents(device.filter, device.type, device.scope))).ToArray());
        }

        private async Task SubscribeToDeviceEvents(string filter, DeviceType type, string scope)
        {
            try
            {
                await Task.Run(() =>
                {
                    WqlEventQuery query = new WqlEventQuery("__InstanceOperationEvent", TimeSpan.FromSeconds(1), filter);
                    ManagementEventWatcher managementEvent = new ManagementEventWatcher(new ManagementScope(scope ?? @"root\cimv2"), query);
                    void handler(object s, EventArrivedEventArgs e)
                    {
                        Action<DeviceType> handlerEvent = HandleDevicesEvents;
                        handlerEvent?.Invoke(type);
                    }
                    managementEvent.EventArrived += handler;
                    managementEvent.Start();
                    _watcherHandler.Add((managementEvent, handler));
                }).ConfigureAwait(false);
            }
            catch (Exception ex) { ErrorLogging.LogDebug(ex); }
        }

        internal void StopDeviceMonitoring()
        {
            foreach ((ManagementEventWatcher watcher, EventArrivedEventHandler handler) in _watcherHandler)
            {
                try
                {
                    watcher.EventArrived -= handler;
                    watcher.Stop();
                    watcher.Dispose();
                }
                catch (Exception ex) { ErrorLogging.LogDebug(ex); }
            }
            _watcherHandler.Clear();
        }
    }
}
