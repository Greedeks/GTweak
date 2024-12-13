using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GTweak.Utilities.Tweaks
{
    internal sealed class SystemData
    {
        internal sealed class ProfileData
        {
            internal static ImageSource GetProfileImage()
            {
                try
                {
                    RegistryKey regKey = default, ourKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\AccountPicture\Users", true);

                    foreach (string keyname in ourKey!.GetSubKeyNames())
                        regKey = ourKey!.OpenSubKey(keyname);

                    return new BitmapImage(new Uri(regKey?.GetValue("Image1080").ToString() ?? string.Empty));
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

        internal class MonitoringSystem
        {
            internal static int PercentageProcessorUsage = 1;

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

            internal string GetNumberRunningProcesses => Process.GetProcesses().Length.ToString();

            internal async void GetProcessorUsage()
            {
                PerformanceCounter cpuCounter = new PerformanceCounter("Processor Information", "% Processor Utility", "_Total", true);
                cpuCounter.NextValue();
                await Task.Delay(1000);
                PercentageProcessorUsage = (int)cpuCounter.NextValue();
            }

            internal int GetMemoryUsage() => (int)Math.Truncate(100 - ((decimal)GetPhysicalAvailableMemory() / GetTotalMemory() * 100) + (decimal)0.5);
        }

        internal class СomputerСonfiguration
        {
            internal sealed class ClientInternetProtocol
            {
                [JsonProperty("query")]
                internal string Ip { get; set; }

                [JsonProperty("countryCode")]
                internal string Country { get; set; }
            }

            /// <summary>
            /// No connection problems - [0];
            /// Connection lose - [1];
            /// Conection block - [2];
            /// Connection limited - [3].
            /// </summary>
            internal static byte ConnectionStatus = 1;

            internal static string WindowsClientVersion { get; set; } = string.Empty;

            internal static readonly Dictionary<string, string> СonfigurationData = new Dictionary<string, string>()
            {
                {"Windows",        Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ProductName", string.Empty).ToString() },
                {"BIOS",           string.Empty },
                {"MotherBr",       string.Empty },
                {"CPU",            string.Empty },
                {"GPU",            string.Empty },
                {"RAM",            string.Empty },
                {"Storage",        string.Empty },
                {"Audio",          string.Empty },
                {"NetAdapter",     string.Empty },
                {"UserIpAddress",  (string)Application.Current.Resources["connection_lose_systemInformation"] }
            };

            internal void GetConfiguration()
            {
                Parallel.Invoke(delegate
                {
                    foreach (var managementObj in new ManagementObjectSearcher(@"root\cimv2", "select Caption, OSArchitecture, Version from Win32_OperatingSystem", new EnumerationOptions { ReturnImmediately = true }).Get())
                        СonfigurationData["Windows"] = $"{Convert.ToString(managementObj["Caption"]).Substring(Convert.ToString(managementObj["Caption"]).IndexOf('W'))}, {Regex.Replace((string)managementObj["OSArchitecture"], @"\-.+", "-bit")}, V{(string)managementObj["Version"]}\n";
                    СonfigurationData["Windows"] = $"\n{СonfigurationData["Windows"].TrimEnd('\n')}";
                },
                delegate
                {
                    foreach (var managementObj in new ManagementObjectSearcher(@"root\cimv2", "select Name, Caption, Description, SerialNumber from Win32_BIOS", new EnumerationOptions { ReturnImmediately = true }).Get())
                    {
                        if (!string.IsNullOrEmpty((string)managementObj["Name"]))
                            СonfigurationData["BIOS"] += !string.IsNullOrEmpty((string)managementObj["SerialNumber"]) ? (string)managementObj["Name"] + ", S/N-" + (string)managementObj["SerialNumber"] + "\n" : (string)managementObj["Name"] + "\n";
                        else if (!string.IsNullOrEmpty((string)managementObj["Caption"]))
                            СonfigurationData["BIOS"] += !string.IsNullOrEmpty((string)managementObj["SerialNumber"]) ? (string)managementObj["Caption"] + ", S/N-" + (string)managementObj["SerialNumber"] + "\n" : (string)managementObj["Caption"] + "\n";
                        else if (!string.IsNullOrEmpty((string)managementObj["Description"]))
                            СonfigurationData["BIOS"] += !string.IsNullOrEmpty((string)managementObj["SerialNumber"]) ? (string)managementObj["Description"] + ", S/N-" + (string)managementObj["SerialNumber"] + "\n" : (string)managementObj["Description"] + "\n";
                    }
                    СonfigurationData["BIOS"] = СonfigurationData["BIOS"].TrimEnd('\n');
                },
                delegate
                {
                    foreach (var managementObj in new ManagementObjectSearcher(@"root\cimv2", "select Manufacturer, Product, Version from Win32_BaseBoard", new EnumerationOptions { ReturnImmediately = true }).Get())
                        СonfigurationData["MotherBr"] = (string)managementObj["Manufacturer"] + (string)managementObj["Product"] + ", V" + (string)managementObj["Version"] + "\n";
                    СonfigurationData["MotherBr"] = СonfigurationData["MotherBr"].TrimEnd('\n');
                },
                delegate
                {
                    foreach (var managementObj in new ManagementObjectSearcher(@"root\cimv2", "select Name from Win32_Processor", new EnumerationOptions { ReturnImmediately = true }).Get())
                        СonfigurationData["CPU"] = (string)managementObj["Name"] + "\n";
                    СonfigurationData["CPU"] = СonfigurationData["CPU"].TrimEnd('\n');
                },
                delegate
                {
                    foreach (var managementObj in new ManagementObjectSearcher(@"root\cimv2", "select Name, AdapterRAM from Win32_VideoController", new EnumerationOptions { ReturnImmediately = true }).Get())
                        СonfigurationData["GPU"] += ((string)managementObj["Name"] + ", " + Convert.ToString(((uint)managementObj["AdapterRAM"] / 1024000000)) + " GB\n");
                    СonfigurationData["GPU"] = СonfigurationData["GPU"].TrimEnd('\n');
                },
                delegate
                {
                    foreach (var managementObj in new ManagementObjectSearcher(@"root\cimv2", "select  Manufacturer, Capacity, ConfiguredClockSpeed from Win32_PhysicalMemory", new EnumerationOptions { ReturnImmediately = true }).Get())
                        СonfigurationData["RAM"] += (string)managementObj["Manufacturer"] + ", " + Convert.ToString((ulong)managementObj["Capacity"] / 1024000000) + " GB, " + Convert.ToString((uint)managementObj["ConfiguredClockSpeed"]) + "MHz\n";
                    СonfigurationData["RAM"] = СonfigurationData["RAM"].TrimEnd('\n');
                },
                UpdatingDeviceData,
                GettingUserIP);
            }

            internal static void UpdatingDeviceData()
            {
                Parallel.Invoke(delegate
                {
                    string storageType = СonfigurationData["Storage"] = string.Empty;
                    foreach (var managementObj in new ManagementObjectSearcher(@"\\.\root\microsoft\windows\storage", "select FriendlyName,MediaType,Size,BusType from MSFT_PhysicalDisk", new EnumerationOptions { ReturnImmediately = true }).Get())
                    {
                        storageType = (ushort)(managementObj["MediaType"]) switch
                        {
                            3 => "(HDD)",
                            4 => "(SSD)",
                            5 => "(SCM)",
                            _ => "(Unspecified)",
                        };
                        if (storageType == "(Unspecified)" && ((ushort)(managementObj["BusType"])) == 7) storageType = "(Media-Type)";
                        СonfigurationData["Storage"] += Convert.ToString((ulong)managementObj["Size"] / 1024000000) + " GB " + "[" + (string)managementObj["FriendlyName"] + "] " + storageType + "\n";
                    }
                    СonfigurationData["Storage"] = СonfigurationData["Storage"].TrimEnd('\n');
                },
                delegate
                {
                    СonfigurationData["Audio"] = string.Empty;
                    foreach (var managementObj in new ManagementObjectSearcher(@"root\cimv2", "select Name,Caption,Description from Win32_SoundDevice", new EnumerationOptions { ReturnImmediately = true }).Get())
                    {
                        if (!string.IsNullOrEmpty((string)managementObj["Name"]))
                            СonfigurationData["Audio"] += (string)managementObj["Name"] + "\n";
                        else if (!string.IsNullOrEmpty((string)managementObj["Caption"]))
                            СonfigurationData["Audio"] += (string)managementObj["Caption"] + "\n";
                        else if (!string.IsNullOrEmpty((string)managementObj["Description"]))
                            СonfigurationData["Audio"] += (string)managementObj["Description"] + "\n";
                    }
                    СonfigurationData["Audio"] = СonfigurationData["Audio"].TrimEnd('\n');
                },
               delegate
               {
                   СonfigurationData["NetAdapter"] = string.Empty;
                   foreach (var managementObj in new ManagementObjectSearcher(@"root\cimv2", "select Name from Win32_NetworkAdapter where NetConnectionStatus=2 or NetConnectionStatus=7", new EnumerationOptions { ReturnImmediately = true }).Get())
                       СonfigurationData["NetAdapter"] += (string)managementObj["Name"] + "\n";
                   СonfigurationData["NetAdapter"] = СonfigurationData["NetAdapter"].TrimEnd('\n');
               });
            }


            internal static bool IsNetworkAvailable()
            {
                try
                {
                    string dns = CultureInfo.InstalledUICulture switch
                    {
                        { Name: string name } when name.StartsWith("fa") => "aparat.com",
                        { Name: string name } when name.StartsWith("zh") => "baidu.com",
                        { Name: string name } when name.StartsWith("ru") => "yandex.ru",
                        _ => "google.com",
                    };

                    TimeSpan timeout = TimeSpan.FromSeconds(5.0);
                    using Task<IPAddress> task = Task.Run(() =>
                    {
                        return Dns.GetHostEntry(dns).AddressList[0];
                    });
                    if (!task.Wait(timeout))
                        return false;

                    return true;
                }
                catch { return false; }
            }

            internal static void GettingUserIP()
            {
                Parallel.Invoke(delegate
                {
                    if (IsNetworkAvailable())
                    {
                        ClientInternetProtocol clientInternetProtocol = new ClientInternetProtocol();

                        try
                        {
                            HttpClient client = new HttpClient { Timeout = TimeSpan.FromSeconds(5.0) };
                            clientInternetProtocol = JsonConvert.DeserializeObject<ClientInternetProtocol>(client.GetStringAsync("http://ip-api.com/json/?fields=61439").Result);
                        }
                        catch
                        {
                            СonfigurationData["UserIpAddress"] = (string)Application.Current.Resources["limited_systemInformation"];
                            ConnectionStatus = 3;
                        }
                        finally
                        {
                            if (IPAddress.TryParse(clientInternetProtocol.Ip, out _) && !string.IsNullOrEmpty(clientInternetProtocol.Ip) && !string.IsNullOrEmpty(clientInternetProtocol.Country))
                            {
                                ConnectionStatus = 0;
                                СonfigurationData["UserIpAddress"] = clientInternetProtocol.Ip + " (" + clientInternetProtocol.Country + ")";
                            }
                            else
                            {
                                ConnectionStatus = 2;
                                СonfigurationData["UserIpAddress"] = (string)Application.Current.Resources["connection_block_systemInformation"];
                            }
                        }
                    }
                    else
                    {
                        ConnectionStatus = 1;
                        СonfigurationData["UserIpAddress"] = (string)Application.Current.Resources["connection_lose_systemInformation"];
                    }
                });
            }
        }
    }
}
