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
            HardwareData["Mode"] = output.IndexOf("efi", StringComparison.OrdinalIgnoreCase) >= 0 ? "UEFI" : "Legacy Boot";

            foreach (var managementObj in new ManagementObjectSearcher(@"root\cimv2", "select Name, Caption, Description, SMBIOSBIOSVersion, SerialNumber from Win32_BIOS", new EnumerationOptions { ReturnImmediately = true }).Get())
            {
                string data = new[] { "Name", "Caption", "Description", "SMBIOSBIOSVersion" }.Select(prop => managementObj[prop] as string).FirstOrDefault(info => !string.IsNullOrEmpty(info)) ?? string.Empty;
                string dataSN = (string)managementObj["SerialNumber"];
                HardwareData["BIOS"] += !string.IsNullOrWhiteSpace(dataSN) && !dataSN.Any(char.IsWhiteSpace) ? $"{data}, S/N-{dataSN}\n" : $"{data}\n";
            }
            HardwareData["BIOS"] = HardwareData["BIOS"].TrimEnd('\n');
        }

        private void GetMotherboardInfo()
        {
            foreach (var managementObj in new ManagementObjectSearcher(@"root\cimv2", "select Manufacturer, Product, Version from Win32_BaseBoard", new EnumerationOptions { ReturnImmediately = true }).Get())
            {
                string data = $"{(string)managementObj["Manufacturer"]}{(string)managementObj["Product"]}";
                string dataVersion = (string)managementObj["Version"];
                HardwareData["MotherBr"] += !string.IsNullOrWhiteSpace(dataVersion) && !dataVersion.Any(char.IsWhiteSpace) ? $"{data}, V{dataVersion}\n" : $"{data}\n";
            }
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
            static (bool, int) GetMemorySize(string name)
            {
                using RegistryKey baseKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\ControlSet001\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}");
                if (baseKey != null)
                {
                    foreach (string subKeyName in baseKey.GetSubKeyNames())
                    {
                        using RegistryKey regKey = baseKey.OpenSubKey(subKeyName);
                        if (regKey != null)
                        {
                            string adapterString = regKey.GetValue("HardwareInformation.AdapterString") as string;
                            string driverDesc = regKey.GetValue("DriverDesc") as string;
                            string chipType = regKey.GetValue("HardwareInformation.ChipType") as string;

                            if ((!string.IsNullOrEmpty(adapterString) && adapterString.Contains(name)) || (!string.IsNullOrEmpty(driverDesc) && driverDesc.Contains(name)) || (!string.IsNullOrEmpty(chipType) && chipType.Contains(name)))
                            {
                                object memorySizeValue = regKey.GetValue("HardwareInformation.qwMemorySize") ?? regKey.GetValue("HardwareInformation.MemorySize");
                                if (memorySizeValue != null)
                                {
                                    ulong memorySize = 0;

                                    if (memorySizeValue is byte[] byteArray)
                                    {
                                        byte[] filledArray = new byte[8];
                                        Array.Copy(byteArray, filledArray, Math.Min(byteArray.Length, filledArray.Length));
                                        memorySize = BitConverter.ToUInt64(filledArray, 0);
                                    }
                                    else
                                    {
                                        memorySize = memorySizeValue switch
                                        {
                                            int intValue => unchecked((uint)intValue),
                                            uint uintValue => uintValue,
                                            long longValue => unchecked((ulong)longValue),
                                            ulong ulongValue => ulongValue,
                                            _ => 0
                                        };
                                    }
                                    return (true, (int)Math.Round(memorySize / (1024.0 * 1024.0 * 1024.0)));
                                }
                            }
                        }
                    }
                }
                return (false, 0);
            }

            foreach (var managementObj in new ManagementObjectSearcher(@"root\cimv2", "select Name, AdapterRAM from Win32_VideoController", new EnumerationOptions { ReturnImmediately = true }).Get())
            {
                string data = (string)managementObj["Name"];
                int dataMemory = (int)Math.Round((uint)managementObj["AdapterRAM"] / (1024.0 * 1024.0 * 1024.0));
                (bool isFound, int dataMemoryReg) = GetMemorySize(data);
                HardwareData["GPU"] += $"{data}, {(isFound ? dataMemoryReg : dataMemory)} GB\n";
            }
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
                    24 => "DDR3",
                    26 => "DDR4",
                    29 => "LPDDR3",
                    30 => "LPDDR4",
                    34 => "DDR5",
                    35 => "LPDDR5",
                    _ => string.Empty
                };
                HardwareData["RAM"] += $"{(manufacturer.Equals("Unknown", StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(manufacturer) ? string.Concat(memoryType, ", ") : string.Concat(manufacturer, ", "))}{(int)Math.Round((ulong)managementObj["Capacity"] / 1024.0 / 1024.0 / 1024.0)} GB{(string.IsNullOrEmpty(speedData) ? "" : $", {speedData}MHz")}\n";
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

                if (storageType == "(Unspecified)" && ((ushort)managementObj["BusType"]) == 7)
                    storageType = "(Media-Type)";

                ulong getSizeGB = (ulong)managementObj["Size"] / (1024 * 1024 * 1024);
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
                                if ((subKey.GetValue("{a45c254e-df1c-4efd-8020-67d146a850e0},24")?.ToString().ToLowerInvariant() == "usb" || subKey.GetValue("{a8b865dd-2e3d-4094-ad97-e593a70c75d6},6")?.ToString().ToLowerInvariant().Contains("usb") == true ||
                                subKey.GetValue("{a8b865dd-2e3d-4094-ad97-e593a70c75d6},8")?.ToString().ToLowerInvariant().Contains("usb") == true) && subKey.GetValue("{b3f8fa53-0004-438e-9003-51a46e139bfc},2")?.ToString()?.Contains(deviceID) == true)
                                    return (true, subKey.GetValue("{b3f8fa53-0004-438e-9003-51a46e139bfc},6")?.ToString());
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
                        HardwareData["Audio"] += $"{new[] { "Name", "Caption", "Description" }.Select(prop => managementObj[prop] as string).FirstOrDefault(info => !string.IsNullOrEmpty(info))}\n" ?? string.Empty;
                }
            }
            HardwareData["Audio"] = HardwareData["Audio"].TrimEnd('\n');
        }

        private void GetNetworkAdapters()
        {
            HardwareData["NetAdapter"] = string.Empty;
            foreach (var managementObj in new ManagementObjectSearcher(@"root\cimv2", "select Name, Description, ProductName, Manufacturer from Win32_NetworkAdapter where NetConnectionStatus=2 or NetConnectionStatus=7", new EnumerationOptions { ReturnImmediately = true }).Get())
                HardwareData["NetAdapter"] += $"{new[] { "Name", "Description", "ProductName", "Manufacturer" }.Select(prop => managementObj[prop] as string).FirstOrDefault(info => !string.IsNullOrEmpty(info))}\n" ?? string.Empty;
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
                    { Name: string name } when name.StartsWith("tm") => "turkmenportal.com",
                    { Name: string name } when name.StartsWith("vn") => "coccoc.com",
                    { Name: string name } when name.StartsWith("cu") => "ecured.cu",
                    { Name: string name } when name.StartsWith("kp") => "naenara.com.kp",
                    { Name: string name } when name.StartsWith("sy") => "duckduckgo.com",
                    { Name: string name } when name.StartsWith("jp") => "yahoo.co.jp",
                    { Name: string name } when name.StartsWith("de") => "t-online.de",
                    { Name: string name } when name.StartsWith("fr") => "orange.fr",
                    { Name: string name } when name.StartsWith("es") => "terra.es",
                    { Name: string name } when name.StartsWith("it") => "libero.it",
                    { Name: string name } when name.StartsWith("tr") || name.StartsWith("in") => "bing.com",
                    _ => "google.com"
                };

                TimeSpan timeout = TimeSpan.FromSeconds(5.0);

                using Task<IPAddress> task = Task.Run(() =>
                {
                    try { return Dns.GetHostEntry(dns).AddressList[0]; }
                    catch { return null; }
                });

                if (!task.Wait(timeout) || task.Result == null)
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
