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
using Microsoft.Win32;

namespace GTweak.Utilities.Configuration
{
    internal class MonitoringProvider : HardwareData
    {
        internal event Action<DeviceType> HandleDevicesEvents;
        private readonly List<(ManagementEventWatcher watcher, EventArrivedEventHandler handler)> _watcherHandler = new List<(ManagementEventWatcher watcher, EventArrivedEventHandler handler)>();
        private readonly ServiceController[] _servicesList = ServiceController.GetServices();

        internal enum DeviceType { All, Storage, Audio, Network }

        [StructLayout(LayoutKind.Sequential)]
        private struct DEVMODE
        {
            private const int CCHDEVICENAME = 32;
            private const int CCHFORMNAME = 32;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)]
            internal string dmDeviceName;
            internal short dmSpecVersion;
            internal short dmDriverVersion;
            internal short dmSize;
            internal short dmDriverExtra;
            internal int dmFields;

            internal int dmPositionX;
            internal int dmPositionY;
            internal int dmDisplayOrientation;
            internal int dmDisplayFixedOutput;

            internal short dmColor;
            internal short dmDuplex;
            internal short dmYResolution;
            internal short dmTTOption;
            internal short dmCollate;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHFORMNAME)]
            internal string dmFormName;

            internal short dmLogPixels;
            internal int dmBitsPerPel;
            internal int dmPelsWidth;
            internal int dmPelsHeight;

            internal int dmDisplayFlags;
            internal int dmDisplayFrequency;
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
            internal uint dwLength;
            internal uint dwMemoryLoad;
            internal ulong ullTotalPhys;
            internal ulong ullAvailPhys;
            internal ulong ullTotalPageFile;
            internal ulong ullAvailPageFile;
            internal ulong ullTotalVirtual;
            internal ulong ullAvailVirtual;
            internal ulong ullAvailExtendedVirtual;
            internal MemoryStatus() => dwLength = (uint)Marshal.SizeOf(typeof(MemoryStatus));
        }

        [DllImport("user32.dll")]
        private static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);

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

        internal void GetPrimaryRefreshRate()
        {
            DEVMODE vDevMode = new DEVMODE
            {
                dmSize = (short)Marshal.SizeOf(typeof(DEVMODE))
            };

            MonitorRefreshRate = EnumDisplaySettings(null, -1, ref vDevMode) && vDevMode.dmDisplayFrequency > 0 ? $"{vDevMode.dmDisplayFrequency} Hz" : string.Empty;
        }

        internal async Task StartDeviceMonitoring()
        {
            await Task.WhenAll(new List<(string filter, DeviceType type, string scope)>
            {
                ($"TargetInstance ISA {(HardwareProvider.isMsftAvailable ? "'MSFT_PhysicalDisk'" : "'Win32_DiskDrive'")}", DeviceType.Storage, HardwareProvider.isMsftAvailable ? @"root\microsoft\windows\storage" : null),
                ("TargetInstance ISA 'Win32_SoundDevice'", DeviceType.Audio, null),
                ("TargetInstance ISA 'Win32_NetworkAdapter' AND TargetInstance.NetConnectionStatus IS NOT NULL", DeviceType.Network, null)
            }.Select(device => Task.Run(() => SubscribeToDeviceEvents(device.filter, device.type, device.scope))).ToArray());

            SystemEvents.DisplaySettingsChanged += OnDisplaySettingsChanged;
        }

        private void OnDisplaySettingsChanged(object sender, EventArgs e) => GetPrimaryRefreshRate();

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
            SystemEvents.DisplaySettingsChanged -= OnDisplaySettingsChanged;
        }
    }
}
