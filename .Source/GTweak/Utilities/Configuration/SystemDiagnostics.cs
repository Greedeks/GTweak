using GTweak.Utilities.Controls;
using GTweak.Utilities.Helpers;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace GTweak.Utilities.Configuration
{
    internal sealed class SystemDiagnostics
    {
        private sealed class GitMetadata
        {
            [JsonProperty("tag_name")]
            internal string СurrentVersion { get; set; }
        }

        private sealed class IPMetadata
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

        internal static bool IsNeedUpdate { get; private set; } = false;
        internal static string DownloadVersion { get; private set; } = string.Empty;
        internal static string WindowsClientVersion { get; set; } = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ProductName", string.Empty)?.ToString() ?? string.Empty;
        internal static string WindowsBuildVersion { get; set; } = string.Empty;

        internal static Dictionary<byte, bool> IsWindowsVersion = default;

        private static (string Storage, string Audio, string NetAdapter) _tempData = (Storage: string.Empty, Audio: string.Empty, NetAdapter: string.Empty);

        internal static Dictionary<string, string> HardwareData = new Dictionary<string, string>()
        {
           { "Windows", WindowsClientVersion },

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

        internal static bool isIPAddressFormatValid = false;


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

            foreach (var managementObj in new ManagementObjectSearcher(@"root\cimv2", $"select FullName from Win32_UserAccount where domain='{Environment.UserDomainName}' and name='{Environment.UserName.ToLower()}'", new EnumerationOptions { ReturnImmediately = true }).Get())
                nameProfile = managementObj["FullName"] as string;

            return !string.IsNullOrWhiteSpace(nameProfile) ? nameProfile : Environment.UserName.ToLower();
        }

        internal void GetHardwareData()
        {
            Parallel.Invoke(
                GetBiosInfo,
                GetMotherboardInfo,
                GetProcessorInfo,
                GeVideoInfo,
                GetMemoryInfo,
                UpdatingDevicesData,
                GetUserIpAddress
            );
        }

        internal void UpdatingDevicesData()
        {
            Parallel.Invoke(
                GetStorageDevices,
                GetAudioDevices,
                GetNetworkAdapters
            );

            HardwareData["Storage"] = _tempData.Storage;
            HardwareData["Audio"] = _tempData.Audio;
            HardwareData["NetAdapter"] = _tempData.NetAdapter;
        }

        internal void GetOperatingSystemInfo()
        {
            foreach (var managementObj in new ManagementObjectSearcher(@"root\cimv2", "select Caption, OSArchitecture, BuildNumber, Version from Win32_OperatingSystem", new EnumerationOptions { ReturnImmediately = true }).Get())
            {
                WindowsClientVersion = Convert.ToString(managementObj["Caption"]);
                WindowsBuildVersion = Convert.ToString(managementObj["BuildNumber"]);
                HardwareData["Windows"] = $"{WindowsClientVersion.Substring(WindowsClientVersion.IndexOf('W'))}, {Regex.Replace((string)managementObj["OSArchitecture"], @"\-.+", "-bit")}, V{(string)managementObj["Version"]}\n";
                IsWindowsVersion = new Dictionary<byte, bool>()
                {
                    { 11, WindowsClientVersion.Contains("11") },
                    { 10, WindowsClientVersion.Contains("10") }
                };
            }
            HardwareData["Windows"] = $"\n{HardwareData["Windows"].TrimEnd('\n')}";
        }

        /// <summary>
        /// If the manufacturer did not provide a serial number and the obtained value is "Default string", 
        /// then the serial number will not be displayed, similar to the situation with the motherboard.
        /// </summary>
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

        /// <summary>
        /// The maximum video card memory that can be obtained via WMI is 4 GB. Therefore, the memory size is obtained from the registry. 
        /// For discrete video cards or older models, the parameters will have the REG_BINARY type, and for integrated ones, REG_SZ.
        /// </summary>
        private void GeVideoInfo()
        {
            static (bool, string, string) GetMemorySize(string name)
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
                            object chipTypeValue = regKey.GetValue("HardwareInformation.ChipType");
                            string chipType = chipTypeValue as string ?? (chipTypeValue is byte[] chipTypeBytes ? Encoding.UTF8.GetString(chipTypeBytes) : string.Empty);


                            if ((!string.IsNullOrEmpty(adapterString) && adapterString.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0) ||
                                (!string.IsNullOrEmpty(driverDesc) && driverDesc.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0) ||
                                (!string.IsNullOrEmpty(chipType) && chipType.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0))
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

                                    return (true, SizeCalculationHelper(memorySize), !string.IsNullOrEmpty(driverDesc) ? driverDesc : chipType);
                                }
                            }
                        }
                    }
                }
                return (false, string.Empty, string.Empty);
            }

            foreach (var managementObj in new ManagementObjectSearcher(@"root\cimv2", "select Name, AdapterRAM from Win32_VideoController", new EnumerationOptions { ReturnImmediately = true }).Get())
            {
                string data = managementObj["Name"] as string;
                (bool isFound, string dataMemoryReg, string driverDesc) = GetMemorySize(data);
                HardwareData["GPU"] += $"{(data == null && !string.IsNullOrEmpty(driverDesc) ? driverDesc : data)}, {(isFound ? dataMemoryReg : SizeCalculationHelper((uint)managementObj["AdapterRAM"]))}\n";
            }
            HardwareData["GPU"] = HardwareData["GPU"].TrimEnd('\n');
        }

        /// <summary>
        /// The Win32_PhysicalMemory class has a limitation in retrieving the memory name and speed. In these cases, the values will be "Unknown" and "0". 
        /// Under these circumstances, the memory type will be displayed instead of the name, and the speed will not be shown.
        /// </summary>
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
                HardwareData["RAM"] += $"{(manufacturer.Equals("Unknown", StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(manufacturer) ? string.Concat(memoryType, ", ") : string.Concat(manufacturer, ", "))}{SizeCalculationHelper((ulong)managementObj["Capacity"])}{(string.IsNullOrEmpty(speedData) ? "" : $", {speedData}MHz")}\n";
            }
            HardwareData["RAM"] = HardwareData["RAM"].TrimEnd('\n');
        }

        /// <summary>
        /// The MSFT_PhysicalDisk class may be missing or malfunctioning; in such cases, it will be replaced by the universal Win32_DiskDrive class. 
        /// </summary>
        private static void GetStorageDevices()
        {
            _tempData.Storage = string.Empty;
            bool isMsftWorking = false;

            try
            {
                using var managementObj = new ManagementObjectSearcher(@"root\microsoft\windows\storage", "select FriendlyName from MSFT_PhysicalDisk", new EnumerationOptions { ReturnImmediately = true });
                var results = managementObj.Get();
                isMsftWorking = results != null && results.Count > 0;
            }
            catch { isMsftWorking = false; }

            if (isMsftWorking)
            {
                foreach (var managementObj in new ManagementObjectSearcher(@"root\microsoft\windows\storage", "select FriendlyName, Model, Description, MediaType, Size, BusType from MSFT_PhysicalDisk", new EnumerationOptions { ReturnImmediately = true }).Get())
                {
                    string data = new[] { "FriendlyName", "Model", "Description" }.Select(prop => managementObj[prop] as string).FirstOrDefault(info => !string.IsNullOrEmpty(info)) ?? string.Empty;
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

                    _tempData.Storage += $"{SizeCalculationHelper((ulong)managementObj["Size"])} [{data}] {storageType}\n";
                }
            }
            else
            {
                foreach (var managementObj in new ManagementObjectSearcher(@"root\cimv2", "select Model, Caption, Size, MediaType, InterfaceType from Win32_DiskDrive", new EnumerationOptions { ReturnImmediately = true }).Get())
                {
                    string data = new[] { "Model", "Caption" }.Select(prop => managementObj[prop] as string).FirstOrDefault(info => !string.IsNullOrEmpty(info)) ?? string.Empty;
                    string mediaType = managementObj["MediaType"] as string ?? string.Empty;
                    string storageType = mediaType switch
                    {
                        "Removable Media" => "(HDD)",
                        "Fixed hard disk media" => "(SSD)",
                        "Unknown" => "(SCM)",
                        _ => "(Unspecified)"
                    };

                    string interfaceType = managementObj["InterfaceType"] as string ?? string.Empty;
                    if ((storageType == "(Unspecified)" || storageType == "(HDD)") && (string.IsNullOrEmpty(interfaceType) || interfaceType.IndexOf("USB", StringComparison.OrdinalIgnoreCase) >= 0))
                        storageType = "(Media-Type)";

                    _tempData.Storage += $"{SizeCalculationHelper((ulong)managementObj["Size"])} [{data}] {storageType}\n";
                }
            }

            _tempData.Storage = _tempData.Storage.TrimEnd('\n');
        }

        /// <summary>
        /// Handling the retrieval of device names for USB devices: In WMI, most connected devices are often named "USB Audio Device." 
        /// Therefore, for such devices, the name lookup is performed through the registry. 
        /// The search for an identifier in Win32_PnPEntity is slow, although it is more convenient. However, it is inferior in speed.
        /// </summary>
        private void GetAudioDevices()
        {
            static (bool, string) IsUsbAudioDevice(string deviceID)
            {
                const string basePath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\MMDevices\Audio\Render";
                using (RegistryKey regKey = Registry.LocalMachine.OpenSubKey(basePath))
                {
                    if (regKey != null)
                    {
                        foreach (string subKeyName in regKey.GetSubKeyNames())
                        {
                            string propsPath = $@"HKEY_LOCAL_MACHINE\{basePath}\{subKeyName}\Properties";
                            using RegistryKey subKey = regKey.OpenSubKey(subKeyName + @"\Properties");
                            if (subKey != null)
                            {
                                string value24 = RegistryHelp.GetValue(propsPath, "{a45c254e-df1c-4efd-8020-67d146a850e0},24", string.Empty);
                                string value8 = RegistryHelp.GetValue(propsPath, "{a8b865dd-2e3d-4094-ad97-e593a70c75d6},8", string.Empty);
                                string value5 = RegistryHelp.GetValue(propsPath, "{a8b865dd-2e3d-4094-ad97-e593a70c75d6},5", string.Empty);
                                string value6 = RegistryHelp.GetValue(propsPath, "{a8b865dd-2e3d-4094-ad97-e593a70c75d6},6", string.Empty);
                                string valueID = RegistryHelp.GetValue(propsPath, "{b3f8fa53-0004-438e-9003-51a46e139bfc},2", string.Empty);

                                if (!string.IsNullOrEmpty(valueID) && valueID.IndexOf(deviceID, StringComparison.OrdinalIgnoreCase) >= 0 && ((!string.IsNullOrEmpty(value5) && value5.IndexOf("wdma_usb.inf", StringComparison.OrdinalIgnoreCase) >= 0) || (!string.IsNullOrEmpty(value8) && value8.IndexOf(@"USB\Class_01", StringComparison.OrdinalIgnoreCase) >= 0) ||
                                (!string.IsNullOrEmpty(value6) && value6.IndexOf("USBAudio.inf", StringComparison.OrdinalIgnoreCase) >= 0) || (!string.IsNullOrEmpty(value24) && value24.IndexOf("usb", StringComparison.OrdinalIgnoreCase) >= 0)))
                                    return (true, RegistryHelp.GetValue(propsPath, "{b3f8fa53-0004-438e-9003-51a46e139bfc},6", string.Empty));
                            }
                        }
                    }
                }
                return (false, string.Empty);
            }

            _tempData.Audio = string.Empty;
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\cimv2", "select DeviceID, Name, Caption, Description from Win32_SoundDevice where Status = 'OK'", new EnumerationOptions { ReturnImmediately = true }))
            {
                foreach (ManagementObject managementObj in searcher.Get().Cast<ManagementObject>())
                {
                    (bool isUsbDevice, string data) = IsUsbAudioDevice(managementObj["DeviceID"].ToString());

                    if (isUsbDevice && !string.IsNullOrEmpty(data))
                        _tempData.Audio += $"{data}\n";
                    else
                        _tempData.Audio += $"{new[] { "Name", "Caption", "Description" }.Select(prop => managementObj[prop] as string).FirstOrDefault(info => !string.IsNullOrEmpty(info))}\n" ?? string.Empty;
                }
            }
            _tempData.Audio = _tempData.Audio.TrimEnd('\n');
        }

        private static string SizeCalculationHelper<T>(T sizeInBytes) where T : struct, IConvertible
        {
            double totalSize = Convert.ToDouble(sizeInBytes) / (1024.0 * 1024.0);

            if (totalSize < 1024)
                return $"{Math.Round(totalSize)} MB";
            else if (totalSize < 1024 * 1024)
                return $"{Math.Round(totalSize / 1024.0)} GB";
            else
                return $"{Math.Round(totalSize / 1024.0, 2):G} TB";
        }

        private void GetNetworkAdapters()
        {
            _tempData.NetAdapter = string.Empty;
            foreach (var managementObj in new ManagementObjectSearcher(@"root\cimv2", "select Name, Description, ProductName, Manufacturer from Win32_NetworkAdapter where NetConnectionStatus=2 or NetConnectionStatus=7", new EnumerationOptions { ReturnImmediately = true }).Get())
                _tempData.NetAdapter += $"{new[] { "Name", "Description", "ProductName", "Manufacturer" }.Select(prop => managementObj[prop] as string).FirstOrDefault(info => !string.IsNullOrEmpty(info))}\n" ?? string.Empty;
            _tempData.NetAdapter = _tempData.NetAdapter.TrimEnd('\n');
        }

        internal bool IsNetworkAvailable()
        {
            try
            {
                string dns = CultureInfo.InstalledUICulture.Name switch
                {
                    string name when name.StartsWith("fa") => "aparat.com",
                    string name when name.StartsWith("zh") => "baidu.com",
                    string name when name.StartsWith("ru") => "yandex.ru",
                    string name when name.StartsWith("kk") => "yandex.kz",
                    string name when name.StartsWith("ko") => "naver.com",
                    string name when name.StartsWith("cs") => "seznam.cz",
                    string name when name.StartsWith("tm") => "turkmenportal.com",
                    string name when name.StartsWith("vn") => "coccoc.com",
                    string name when name.StartsWith("cu") => "ecured.cu",
                    string name when name.StartsWith("kp") => "naenara.com.kp",
                    string name when name.StartsWith("sy") => "duckduckgo.com",
                    string name when name.StartsWith("jp") => "yahoo.co.jp",
                    string name when name.StartsWith("de") => "t-online.de",
                    string name when name.StartsWith("fr") => "orange.fr",
                    string name when name.StartsWith("es") => "terra.es",
                    string name when name.StartsWith("it") => "libero.it",
                    string name when name.StartsWith("tr") || name.StartsWith("in") || name.StartsWith("ar") => "bing.com",
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

        internal void GetUserIpAddress()
        {
            Task.Run(async () =>
            {
                if (IsNetworkAvailable())
                {
                    try
                    {
                        using HttpClient client = new HttpClient { Timeout = TimeSpan.FromSeconds(5.0) };
                        string response = await client.GetStringAsync("http://ip-api.com/json/?fields=61439");
                        IPMetadata ipMetadata = JsonConvert.DeserializeObject<IPMetadata>(response);

                        if (IPAddress.TryParse(ipMetadata.Ip, out _) && !string.IsNullOrEmpty(ipMetadata?.Ip) && !string.IsNullOrEmpty(ipMetadata?.Country))
                        {
                            CurrentConnection = ConnectionStatus.Available;
                            HardwareData["UserIpAddress"] = $"{ipMetadata.Ip} ({ipMetadata.Country})";
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

                isIPAddressFormatValid = HardwareData["UserIpAddress"].Any(char.IsDigit);
            });
        }

        internal void ValidateVersionUpdates()
        {
            if (!SettingsRepository.IsUpdateCheckRequired || !IsNetworkAvailable())
                return;

            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create("https://api.github.com/repos/greedeks/gtweak/releases/latest");

                webRequest.ContentType = "application/json";
                webRequest.UserAgent = "Nothing";
                webRequest.Timeout = 5000;

                using HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
                using StreamReader sreader = new StreamReader(response.GetResponseStream());
                string DataAsJson = sreader.ReadToEnd();
                GitMetadata gitVersionUtility = JsonConvert.DeserializeObject<GitMetadata>(DataAsJson);

                if (!string.IsNullOrEmpty(gitVersionUtility.СurrentVersion) && gitVersionUtility.СurrentVersion.CompareTo(SettingsRepository.currentRelease) > 0)
                {
                    IsNeedUpdate = true;
                    DownloadVersion = gitVersionUtility.СurrentVersion;
                }
            }
            catch { IsNeedUpdate = false; }
        }
    }
}
