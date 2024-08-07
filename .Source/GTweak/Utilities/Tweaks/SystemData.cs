using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GTweak.Utilities
{
    internal sealed class SystemData
    {
        internal struct ProfileData
        {
            internal static ImageSource GetProfileImage()
            {
                try
                {
                    RegistryKey _regKey = default, _ourKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\AccountPicture\Users", true);

                    foreach (string keyname in _ourKey.GetSubKeyNames())
                        _regKey = _ourKey.OpenSubKey(keyname);

                    return new BitmapImage(new Uri(_regKey?.GetValue("Image1080").ToString() ?? string.Empty));
                }
                catch { return null; }
            }

            internal static string GetProfileName()
            {
                string nameProfile = string.Empty;
                foreach (var managementObj in new ManagementObjectSearcher(@"root\cimv2", $"select FullName from Win32_UserAccount where domain='{Environment.UserDomainName}' and name='{Environment.UserName.ToLower()}'").Get())
                    nameProfile = (string)managementObj["FullName"];

                return nameProfile != string.Empty ? nameProfile : Environment.UserName.ToLower();
            }
        }

        internal struct MonitoringSystem
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

            internal readonly int CountProcess => GetCountProcess();
            internal static int CpuUsage = 1;
            internal readonly int RamUsage => GetRamUsage();


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

            internal readonly int GetCountProcess()
            {
                Process[] countProcess = Process.GetProcesses();
                return countProcess.Length;
            }

            internal readonly void GetCpuUsage()
            {
                Parallel.Invoke(async () => { 
                    PerformanceCounter cpuCounter = new PerformanceCounter("Processor Information", "% Processor Utility", "_Total", true);
                    cpuCounter.NextValue();
                    await Task.Delay(1000);
                    CpuUsage = (int)cpuCounter.NextValue();
                });
            }

            internal readonly int GetRamUsage() => (int)Math.Truncate(100 - ((decimal)GetPhysicalAvailableMemory() / GetTotalMemory() * 100) + (decimal)0.5);
        }

        internal sealed class ClientInternetProtocol
        {
            [JsonProperty("ip")]
            internal string Ip { get; set; }

            [JsonProperty("country")]
            internal string Country { get; set; }

            [JsonProperty("query")]
            internal string Ip_Reserve { get; set; }

            [JsonProperty("countryCode")]
            internal string Country_Reserve { get; set; }
        }

        internal sealed class СomputerСonfiguration
        {
            internal static string clientWinVersion = default;
            internal static bool isNoInternetConnection = false, isInternetLimited = false;
            private static string _type = string.Empty;
            private static readonly EnumerationOptions optionsObj = new EnumerationOptions { ReturnImmediately = true };

            internal static readonly Dictionary<string, string> СonfigurationData = new Dictionary<string, string>()
            {
                {"Windows",    string.Empty },
                {"BIOS",       string.Empty },
                {"MotherBr",   string.Empty },
                {"CPU",        string.Empty },
                {"GPU",        string.Empty },
                {"RAM",        string.Empty },
                {"Disk",       string.Empty },
                {"Sound",      string.Empty },
                {"NetAdapter", string.Empty },
                {"IpAddress",  string.Empty }
            };

            internal void GetСonfigurationComputer()
            {
                Parallel.Invoke(() =>
                {
                    foreach (var managementObj in new ManagementObjectSearcher(@"root\cimv2", "select Caption, OSArchitecture, Version from Win32_OperatingSystem", optionsObj).Get())
                        СonfigurationData["Windows"] = $"{Convert.ToString(managementObj["Caption"]).Substring(Convert.ToString(managementObj["Caption"]).IndexOf('W'))}, {Regex.Replace((string)managementObj["OSArchitecture"], @"\-.+", "-bit")}, V{(string)managementObj["Version"]}\n";
                    СonfigurationData["Windows"] = $"\n{СonfigurationData["Windows"].TrimEnd('\n')}";
                },
                () =>
                {
                    foreach (var managementObj in new ManagementObjectSearcher(@"root\cimv2", "select Name, Caption, Description, SerialNumber from Win32_BIOS", optionsObj).Get()) 
                    {
                        if (!string.IsNullOrEmpty((string)managementObj["Name"]))
                            СonfigurationData["BIOS"] += !string.IsNullOrEmpty((string)managementObj["SerialNumber"]) ? (string)managementObj["Name"] + ", S/N-" + (string)managementObj["SerialNumber"] + "\n" : (string)managementObj["Name"] + "\n";
                        else if (!string.IsNullOrEmpty((string)managementObj["Caption"]))
                            СonfigurationData["BIOS"] += !string.IsNullOrEmpty((string)managementObj["SerialNumber"]) ?  (string)managementObj["Caption"] + ", S/N-" + (string)managementObj["SerialNumber"] + "\n" : (string)managementObj["Caption"] + "\n";
                        else if (!string.IsNullOrEmpty((string)managementObj["Description"]))
                            СonfigurationData["BIOS"] += !string.IsNullOrEmpty((string)managementObj["SerialNumber"]) ?  (string)managementObj["Description"] + ", S/N-" + (string)managementObj["SerialNumber"] + "\n" : (string)managementObj["Description"] + "\n";
                    }
                    СonfigurationData["BIOS"] = СonfigurationData["BIOS"].TrimEnd('\n');
                }, 
                () =>
                {
                    foreach (var managementObj in new ManagementObjectSearcher(@"root\cimv2", "select Manufacturer, Product, Version from Win32_BaseBoard", optionsObj).Get())
                        СonfigurationData["MotherBr"] = (string)managementObj["Manufacturer"] + (string)managementObj["Product"] + ", V" + (string)managementObj["Version"] + "\n";
                    СonfigurationData["MotherBr"] = СonfigurationData["MotherBr"].TrimEnd('\n');
                },
                () =>
                {
                    foreach (var managementObj in new ManagementObjectSearcher(@"root\cimv2", "select Name from Win32_Processor", optionsObj).Get())
                        СonfigurationData["CPU"] = (string)managementObj["Name"] + "\n";
                    СonfigurationData["CPU"] = СonfigurationData["CPU"].TrimEnd('\n');
                },
                () =>
                {
                    foreach (var managementObj in new ManagementObjectSearcher(@"root\cimv2", "select Name, AdapterRAM from Win32_VideoController", optionsObj).Get())
                        СonfigurationData["GPU"] += ((string)managementObj["Name"] + ", " + Convert.ToString(((uint)managementObj["AdapterRAM"] / 1024000000)) + " GB\n");
                    СonfigurationData["GPU"] = СonfigurationData["GPU"].TrimEnd('\n');
                }, 
                () =>
                {
                    foreach (var managementObj in new ManagementObjectSearcher(@"root\cimv2", "select  Manufacturer, Capacity, ConfiguredClockSpeed from Win32_PhysicalMemory", optionsObj).Get())
                        СonfigurationData["RAM"] += (string)managementObj["Manufacturer"] + ", " + Convert.ToString((ulong)managementObj["Capacity"] / 1024000000) + " GB, " + Convert.ToString((uint)managementObj["ConfiguredClockSpeed"]) + "MHz\n";
                    СonfigurationData["RAM"] = СonfigurationData["RAM"].TrimEnd('\n');
                },
                UpdatingDeviceData,
                GetUserIP);
            }

            internal static void UpdatingDeviceData()
            {
                Parallel.Invoke(
                () =>
                {
                    СonfigurationData["Disk"] = string.Empty;
                    foreach (var managementObj in new ManagementObjectSearcher(@"\\.\root\microsoft\windows\storage", "select FriendlyName,MediaType,Size,BusType from MSFT_PhysicalDisk", optionsObj).Get())
                    {
                        _type = (ushort)(managementObj["MediaType"]) switch
                        {
                            3 => "(HDD)",
                            4 => "(SSD)",
                            5 => "(SCM)",
                            _ => "(Unspecified)",
                        };
                        if (_type == "(Unspecified)" && ((ushort)(managementObj["BusType"])) == 7) _type = "(Media-Type)";
                        СonfigurationData["Disk"] += Convert.ToString((ulong)managementObj["Size"] / 1024000000) + " GB " + "[" + (string)managementObj["FriendlyName"] + "] " + _type + "\n";
                    }
                    СonfigurationData["Disk"] = СonfigurationData["Disk"].TrimEnd('\n');
                },
               () =>
               {
                   СonfigurationData["Sound"] = string.Empty;
                   foreach (var managementObj in new ManagementObjectSearcher(@"root\cimv2", "select Name,Caption,Description from Win32_SoundDevice", optionsObj).Get())
                   {
                       if (!string.IsNullOrEmpty((string)managementObj["Name"]))
                           СonfigurationData["Sound"] += (string)managementObj["Name"] + "\n";
                       else if (!string.IsNullOrEmpty((string)managementObj["Caption"]))
                           СonfigurationData["Sound"] += (string)managementObj["Caption"] + "\n";
                       else if (!string.IsNullOrEmpty((string)managementObj["Description"]))
                           СonfigurationData["Sound"] += (string)managementObj["Description"] + "\n";
                   }
                   СonfigurationData["Sound"] = СonfigurationData["Sound"].TrimEnd('\n');
               },
               () =>
               {
                   СonfigurationData["NetAdapter"] = string.Empty;
                   foreach (var managementObj in new ManagementObjectSearcher(@"root\cimv2", "select Name from Win32_NetworkAdapter where NetConnectionStatus=2 or NetConnectionStatus=7", optionsObj).Get())
                       СonfigurationData["NetAdapter"] += (string)managementObj["Name"] + "\n";
                   СonfigurationData["NetAdapter"] = СonfigurationData["NetAdapter"].TrimEnd('\n');
               });
            }

            internal static bool IsCheckInternetConnection()
            {
                try {
                    TimeSpan timeout = TimeSpan.FromSeconds(5.0);
                    Task<IPAddress> _task = Task.Run(() => {
                        return !string.IsNullOrEmpty(Dns.GetHostEntry("google.com").AddressList[0].ToString())
                            ? Dns.GetHostEntry("google.com").AddressList[0]
                            : Dns.GetHostEntry("baidu.com").AddressList[0];
                    });
                    if (!_task.Wait(timeout))
                        return false;
                    return true;
                }
                catch { return false; }
            }

            internal static void GetUserIP()
            {
                Parallel.Invoke(() =>
                {
                    if (IsCheckInternetConnection())
                    {
                        ClientInternetProtocol clientInternetProtocol = new ClientInternetProtocol();

                        try
                        {
                            HttpClient client = new HttpClient { Timeout = TimeSpan.FromSeconds(5.0) };
                            clientInternetProtocol = JsonConvert.DeserializeObject<ClientInternetProtocol>(client.GetStringAsync("https://ipinfo.io/json").Result);

                            if (string.IsNullOrEmpty(clientInternetProtocol.Ip) || string.IsNullOrEmpty(clientInternetProtocol.Country))
                                clientInternetProtocol = JsonConvert.DeserializeObject<ClientInternetProtocol>(client.GetStringAsync("http://ip-api.com/json/?fields=61439").Result);
                        }
                        catch
                        {
                            App.ViewLang();
                            СonfigurationData["IpAddress"] = (string)Application.Current.Resources["limited_systemInformation"];
                            isInternetLimited = true;
                        }
                        finally 
                        {
                            if (isInternetLimited)
                                isInternetLimited = false;

                            if (!string.IsNullOrEmpty(clientInternetProtocol.Ip) || !string.IsNullOrEmpty(clientInternetProtocol.Country))
                            {
                               СonfigurationData["IpAddress"] = clientInternetProtocol.Ip + " (" + clientInternetProtocol.Country + ")";
                               isNoInternetConnection = false && isNoInternetConnection;
                            }
                            else if (!string.IsNullOrEmpty(clientInternetProtocol.Ip_Reserve) || !string.IsNullOrEmpty(clientInternetProtocol.Country_Reserve))
                            {
                                СonfigurationData["IpAddress"] = clientInternetProtocol.Ip_Reserve + " (" + clientInternetProtocol.Country_Reserve + ")";
                                isNoInternetConnection = false && isNoInternetConnection;
                            }
                            else
                            {
                                App.ViewLang();
                                СonfigurationData["IpAddress"] = (string)Application.Current.Resources["connection_systemInformation"];
                                isNoInternetConnection = true;
                            }
                        }
                    }
                    else
                    {
                        App.ViewLang();
                        СonfigurationData["IpAddress"] = (string)Application.Current.Resources["connection_systemInformation"];
                        isNoInternetConnection = true;
                    }
                });
            }


        }
    }
}
