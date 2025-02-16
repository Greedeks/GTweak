using GTweak.Utilities.Helpers;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace GTweak.Utilities.Configuration
{
    internal sealed class SystemDiagnostics
    {
        internal sealed class IPMetadata
        {
            [JsonProperty("query")]
            internal string Ip { get; set; }

            [JsonProperty("countryCode")]
            internal string Country { get; set; }
        }

        internal enum ConnectionStatus
        {
            Available,
            Lose,
            Block,
            Limited,
        }

        internal static ConnectionStatus CurrentConnection = ConnectionStatus.Lose;

        internal static string WindowsClientVersion { get; set; } = string.Empty;

        internal static readonly Dictionary<string, string> HardwareData = new Dictionary<string, string>()
        {
           { "Windows", Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ProductName", string.Empty)?.ToString() ?? string.Empty },

           { "BIOS", string.Empty },
           { "Mode", string.Empty },
           { "MotherBr", string.Empty },
           { "CPU", string.Empty },
           { "GPU", string.Empty },
           { "RAM", string.Empty },
           { "Storage", string.Empty },
           { "Audio", string.Empty },
           { "NetAdapter", string.Empty },

           { "UserIpAddress", Application.Current.Resources["connection_lose_systemInformation"].ToString() }
        };

        internal ImageSource GetProfileImage()
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

        internal string GetProfileName()
        {
            string nameProfile = string.Empty;
            foreach (var managementObj in new ManagementObjectSearcher(@"root\cimv2", $"select FullName from Win32_UserAccount where domain='{Environment.UserDomainName}' and name='{Environment.UserName.ToLower()}'").Get())
                nameProfile = (string)managementObj["FullName"];

            return nameProfile != string.Empty ? nameProfile : Environment.UserName.ToLower();
        }

        internal void GetHardwareData()
        {
            Parallel.Invoke(
                GetOperatingSystemInfo,
                GetBiosInfo,
                GetMotherboardInfo,
                GetProcessorInfo,
                GeVideoInfo,
                GetMemoryInfo,
                GetUpdatingDevices,
                GetUserIpAddress
            );
        }

        internal void GetUpdatingDevices()
        {
            Parallel.Invoke(
                GetStorageDevices,
                GetAudioDevices,
                GetNetworkAdapters
            );
        }

        private void GetOperatingSystemInfo()
        {
            foreach (var managementObj in new ManagementObjectSearcher(@"root\cimv2", "select Caption, OSArchitecture, Version from Win32_OperatingSystem", new EnumerationOptions { ReturnImmediately = true }).Get())
                HardwareData["Windows"] = $"{Convert.ToString(managementObj["Caption"]).Substring(Convert.ToString(managementObj["Caption"]).IndexOf('W'))}, {Regex.Replace((string)managementObj["OSArchitecture"], @"\-.+", "-bit")}, V{(string)managementObj["Version"]}\n";
            HardwareData["Windows"] = $"\n{HardwareData["Windows"].TrimEnd('\n')}";
        }

        private async void GetBiosInfo()
        {
            string output = await CommandExecutor.GetCommandOutput("bcdedit");
            HardwareData["Mode"] = output.Contains(@"efi") ? "UEFI" : "Legacy BIOS";

            foreach (var managementObj in new ManagementObjectSearcher(@"root\cimv2", "select Name, Caption, Description, SMBIOSBIOSVersion, SerialNumber from Win32_BIOS", new EnumerationOptions { ReturnImmediately = true }).Get())
            {
                string data = new[] { "Name", "Caption", "Description", "SMBIOSBIOSVersion" }.Select(prop => (string)managementObj[prop]).FirstOrDefault(info => !string.IsNullOrEmpty(info)) ?? string.Empty;
                HardwareData["BIOS"] += !string.IsNullOrEmpty((string)managementObj["SerialNumber"]) ? $"{data}, S/N-{(string)managementObj["SerialNumber"]}\n" : $"{data}\n";
            }
            HardwareData["BIOS"] = HardwareData["BIOS"].TrimEnd('\n');
        }

        private void GetMotherboardInfo()
        {
            foreach (var managementObj in new ManagementObjectSearcher(@"root\cimv2", "select Manufacturer, Product, Version from Win32_BaseBoard", new EnumerationOptions { ReturnImmediately = true }).Get())
                HardwareData["MotherBr"] = $"{(string)managementObj["Manufacturer"]}{(string)managementObj["Product"]}, V{(string)managementObj["Version"]}\n";
            HardwareData["MotherBr"] = HardwareData["MotherBr"].TrimEnd('\n');
        }

        private void GetProcessorInfo()
        {
            foreach (var managementObj in new ManagementObjectSearcher(@"root\cimv2", "select Name from Win32_Processor", new EnumerationOptions { ReturnImmediately = true }).Get())
                HardwareData["CPU"] = $"{(string)managementObj["Name"]}\n";
            HardwareData["CPU"] = HardwareData["CPU"].TrimEnd('\n');
        }

        private void GeVideoInfo()
        {
            foreach (var managementObj in new ManagementObjectSearcher(@"root\cimv2", "select Name, AdapterRAM from Win32_VideoController", new EnumerationOptions { ReturnImmediately = true }).Get())
                HardwareData["GPU"] += $"{(string)managementObj["Name"]}, {Convert.ToString((uint)managementObj["AdapterRAM"] / 1024000000)} GB\n";
            HardwareData["GPU"] = HardwareData["GPU"].TrimEnd('\n');
        }

        private void GetMemoryInfo()
        {
            foreach (var managementObj in new ManagementObjectSearcher(@"root\cimv2", "select Manufacturer, Capacity, ConfiguredClockSpeed, Speed, SMBIOSMemoryType from Win32_PhysicalMemory", new EnumerationOptions { ReturnImmediately = true }).Get())
            {
                string speedData = new[] { "ConfiguredClockSpeed", "Speed" }.Select(prop => managementObj[prop] != null ? Convert.ToString(managementObj[prop]) : null).FirstOrDefault(info => !string.IsNullOrEmpty(info) && info != "0");
                string manufacturer = (string)managementObj["Manufacturer"];
                string memoryType = (uint)managementObj["SMBIOSMemoryType"] switch
                {
                    24 => "DDR3,",
                    26 => "DDR4,",
                    29 => "LPDDR3,",
                    30 => "LPDDR4,",
                    31 => "DDR5,",
                    _ => string.Empty
                };
               HardwareData["RAM"] += $"{(manufacturer == "Unknown" || string.IsNullOrEmpty(manufacturer) ? memoryType : string.Concat(manufacturer, ","))} {Convert.ToString((ulong)managementObj["Capacity"] / 1024000000)} GB{(string.IsNullOrEmpty(speedData) ? "" : $", { speedData}MHz")}\n";
            }
            HardwareData["RAM"] = HardwareData["RAM"].TrimEnd('\n');
        }

        private static void GetStorageDevices()
        {
            HardwareData["Storage"] = string.Empty;
            foreach (var managementObj in new ManagementObjectSearcher(@"\\.\root\microsoft\windows\storage", "select FriendlyName, MediaType, Size, BusType from MSFT_PhysicalDisk", new EnumerationOptions { ReturnImmediately = true }).Get())
            {
                ushort mediaType = managementObj["MediaType"] != null ? (ushort)managementObj["MediaType"] : (ushort)0;
                string storageType = mediaType switch
                {
                    3 => "(HDD)",
                    4 => "(SSD)",
                    5 => "(SCM)",
                    _ => "(Unspecified)"
                };

                if (storageType == "(Unspecified)" && ((ushort)(managementObj["BusType"])) == 7) storageType = "(Media-Type)";

                ulong getSizeGB = (ulong)managementObj["Size"] / 1024000000;
                string size = getSizeGB >= 1024 ? $"{Math.Round(getSizeGB / 1024.0, 2):G} TB" : $"{getSizeGB} GB";

                HardwareData["Storage"] += $"{size} [{(string)managementObj["FriendlyName"]}] {storageType}\n";
            }

            HardwareData["Storage"] = HardwareData["Storage"].TrimEnd('\n');
        }

        private void GetAudioDevices()
        {
            static (bool, string) IsUsbAudioDevice(string deviceID)
            {
                using (RegistryKey regKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\MMDevices\Audio\Render"))
                {
                    if (regKey != null)
                    {
                        foreach (string subKeyName in regKey.GetSubKeyNames())
                        {
                            using RegistryKey subKey = regKey.OpenSubKey(subKeyName + @"\Properties");
                            if (subKey != null)
                            {
                                if (subKey.GetValue("{a45c254e-df1c-4efd-8020-67d146a850e0},24")?.ToString() == "USB" && subKey.GetValue("{b3f8fa53-0004-438e-9003-51a46e139bfc},2")?.ToString()?.Contains(deviceID) == true)
                                {
                                    string value = subKey.GetValue("{b3f8fa53-0004-438e-9003-51a46e139bfc},6")?.ToString();
                                    return (true, value);
                                }
                            }
                        }
                    }
                }
                return (false, string.Empty);
            }

            HardwareData["Audio"] = string.Empty;
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\cimv2", "select DeviceID, Name, Caption, Description from Win32_SoundDevice where Status = 'OK'", new EnumerationOptions { ReturnImmediately = true }))
            {
                foreach (ManagementObject managementObj in searcher.Get().Cast<ManagementObject>())
                {
                    (bool isUsbDevice, string data) = IsUsbAudioDevice(managementObj["DeviceID"].ToString());

                    if (isUsbDevice && !string.IsNullOrEmpty(data))
                        HardwareData["Audio"] += $"{data}\n";
                    else
                        HardwareData["Audio"] += $"{new[] { "Name", "Caption", "Description" }.Select(prop => (string)managementObj[prop]).FirstOrDefault(info => !string.IsNullOrEmpty(info))}\n" ?? string.Empty;
                }
            }
            HardwareData["Audio"] = HardwareData["Audio"].TrimEnd('\n');
        }

        private void GetNetworkAdapters()
        {
            HardwareData["NetAdapter"] = string.Empty;
            foreach (var managementObj in new ManagementObjectSearcher(@"root\cimv2", "select Name from Win32_NetworkAdapter where NetConnectionStatus=2 or NetConnectionStatus=7", new EnumerationOptions { ReturnImmediately = true }).Get())
                HardwareData["NetAdapter"] += $"{(string)managementObj["Name"]}\n";
            HardwareData["NetAdapter"] = HardwareData["NetAdapter"].TrimEnd('\n');
        }

        private bool IsNetworkAvailable()
        {
            try
            {
                string dns = CultureInfo.InstalledUICulture switch
                {
                    { Name: string name } when name.StartsWith("fa") => "aparat.com",
                    { Name: string name } when name.StartsWith("zh") => "baidu.com",
                    { Name: string name } when name.StartsWith("ru") => "yandex.ru",
                    { Name: string name } when name.StartsWith("ko") => "naver.com",
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

        internal async void GetUserIpAddress()
        {
            await Task.Run(async () =>
            {
                if (IsNetworkAvailable())
                {
                    try
                    {
                        using HttpClient client = new HttpClient { Timeout = TimeSpan.FromSeconds(5.0) };
                        string response = await client.GetStringAsync("http://ip-api.com/json/?fields=61439");
                        IPMetadata ipMetadata = JsonConvert.DeserializeObject<IPMetadata>(response);

                        if (IPAddress.TryParse(ipMetadata.Ip, out _) && !string.IsNullOrEmpty(ipMetadata.Ip) && !string.IsNullOrEmpty(ipMetadata.Country))
                        {
                            CurrentConnection = ConnectionStatus.Available;
                            HardwareData["UserIpAddress"] = $"{ipMetadata.Ip} ({ipMetadata.Country})";
                            return;
                        }
                        else
                            CurrentConnection = ConnectionStatus.Block;
                    }
                    catch { CurrentConnection = ConnectionStatus.Limited; }
                }
                else
                    CurrentConnection = ConnectionStatus.Lose;

                if (new Dictionary<ConnectionStatus, string>
                {
                    { ConnectionStatus.Lose, "connection_lose_systemInformation" },
                    { ConnectionStatus.Block, "connection_block_systemInformation" },
                    { ConnectionStatus.Limited, "limited_systemInformation" }
                }.TryGetValue(CurrentConnection, out string resourceKey)) { HardwareData["UserIpAddress"] = (string)Application.Current.Resources[resourceKey]; }
            });
        }
    }
}
