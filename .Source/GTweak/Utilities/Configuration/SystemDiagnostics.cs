using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GTweak.Utilities.Controls;
using GTweak.Utilities.Helpers;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GTweak.Utilities.Configuration
{
    internal sealed class SystemDiagnostics : MonitoringService
    {
        private sealed class GitMetadata
        {
            [JsonProperty("tag_name")]
            internal string СurrentVersion { get; set; }
        }

        private sealed class IPMetadata
        {
            internal string Ip { get; set; }
            internal string Country { get; set; }

            internal static IPMetadata ParseData(string response)
            {
                JObject jObject = JObject.Parse(response);
                string ip = (string)(jObject["ip"] ?? jObject["ipAddress"] ?? jObject["query"]);
                string country = (string)(jObject["country_code"] ?? jObject["countryCode"]);
                return new IPMetadata { Ip = ip, Country = country };
            }
        }

        internal static bool IsNeedUpdate { get; private set; } = false;
        internal static string DownloadVersion { get; private set; } = string.Empty;

        internal static bool isIPAddressFormatValid = false, isMsftAvailable = false;

        private static readonly (object[] Keys, string Type)[] MediaTypeMap = new (object[] Keys, string Type)[]
        {
            (new object[] { (ushort)3, "Removable Media" }, DiskTypeLabels.HDD),
            (new object[] { (ushort)4, "Fixed hard disk media" }, DiskTypeLabels.SSD),
            (new object[] { (ushort)5, "Unspecified" }, DiskTypeLabels.SCM)
        };

        private static readonly Dictionary<ushort, string> BusTypeMap = new Dictionary<ushort, string>()
        {
            { 7,  DiskTypeLabels.USB },
            { 12, DiskTypeLabels.SD },
            { 17, DiskTypeLabels.NVMe }
        };

        internal static (string Code, string Region) GetCurrentSystemLang()
        {
            CultureInfo culture = CultureInfo.CurrentUICulture;
            string[] parts = culture.Name.Split('-');
            return (culture.TwoLetterISOLanguageName.ToLowerInvariant(), parts.Length > 1 ? parts[1].ToLowerInvariant() : string.Empty);
        }

        internal ImageSource GetProfileImage()
        {
            try
            {
                string avatarPath = RegistryHelp.GetValue($@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\AccountPicture\Users\{WindowsIdentity.GetCurrent().User?.Value}", "Image1080", string.Empty);

                if (!string.IsNullOrWhiteSpace(avatarPath) && File.Exists(avatarPath) && new FileInfo(avatarPath).Length != 0)
                {
                    BitmapImage bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.UriSource = new Uri(avatarPath, UriKind.Absolute);
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.EndInit();
                    bmp.Freeze();
                    return bmp;
                }
            }
            catch (Exception ex)
            {
                ErrorLogging.LogDebug(ex);
            }

            return (ImageSource)Application.Current.Resources["Icon_ProfileAvatar"];
        }

        internal string GetProfileName()
        {
            string nameProfile = string.Empty;

            foreach (var managementObj in new ManagementObjectSearcher(@"root\cimv2", $"select FullName from Win32_UserAccount where domain='{Environment.UserDomainName}' and name='{Environment.UserName.ToLowerInvariant()}'", new EnumerationOptions { ReturnImmediately = true }).Get())
            {
                nameProfile = managementObj["FullName"] as string;
            }

            return !string.IsNullOrWhiteSpace(nameProfile) ? nameProfile : Environment.UserName.ToLowerInvariant();
        }

        internal void GetHardwareData()
        {
            Parallel.Invoke(() =>
            {
                try
                {
                    using var managementObj = new ManagementObjectSearcher(@"root\microsoft\windows\storage", "select FriendlyName from MSFT_PhysicalDisk", new EnumerationOptions { ReturnImmediately = true });
                    var results = managementObj.Get();
                    isMsftAvailable = results != null && results.Count > 0;
                }
                catch { isMsftAvailable = false; }
            });

            Parallel.Invoke(
                GetWallpaperImage,
                GetBiosInfo,
                GetMotherboardInfo,
                GetProcessorInfo,
                GetGraphicsInfo,
                GetMemoryInfo,
                GetUserIpAddress
            );

            UpdatingDevicesData();
        }

        internal void UpdatingDevicesData(DeviceType deviceType = DeviceType.All)
        {
            string storage = string.Empty, audio = string.Empty, netAdapter = string.Empty;

            switch (deviceType)
            {
                case DeviceType.Storage:
                    Parallel.Invoke(() => storage = GetStorageDevices());
                    Storage = storage;
                    break;
                case DeviceType.Audio:
                    Parallel.Invoke(() => audio = GetAudioDevices());
                    AudioDevice = audio;
                    break;
                case DeviceType.Network:
                    Parallel.Invoke(() => netAdapter = GetNetworkAdapters());
                    NetworkAdapter = netAdapter;
                    break;
                default:
                    Parallel.Invoke
                    (
                        () => storage = GetStorageDevices(),
                        () => audio = GetAudioDevices(),
                        () => netAdapter = GetNetworkAdapters()
                    );
                    Storage = storage;
                    AudioDevice = audio;
                    NetworkAdapter = netAdapter;
                    break;
            }
        }

        private void GetWallpaperImage()
        {
            try
            {
                string wallpaperPath = RegistryHelp.GetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "WallPaper", string.Empty);

                string filePath = !string.IsNullOrWhiteSpace(wallpaperPath) && File.Exists(wallpaperPath) ? wallpaperPath :
                    Directory.GetFiles(PathLocator.Folders.WallpaperCache, "TranscodedWallpaper*").Select(f => new FileInfo(f)).Where(f => f.Exists).OrderByDescending(f => f.LastWriteTime).FirstOrDefault()?.FullName ?? string.Empty;

                if (!string.IsNullOrWhiteSpace(filePath))
                {
                    BitmapImage bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.UriSource = new Uri(filePath, UriKind.Absolute);
                    bmp.DecodePixelWidth = 100;
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                    bmp.EndInit();
                    bmp.Freeze();
                    Wallpaper = bmp;
                }
                else
                {
                    Wallpaper = null;
                }
            }
            catch (Exception ex)
            {
                ErrorLogging.LogDebug(ex);
                Wallpaper = null;
            }
        }

        internal void GetOperatingSystemInfo()
        {
            string regPath = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion";
            int revisionNumber = RegistryHelp.GetValue(regPath, "UBR", 0);
            string release = RegistryHelp.GetValue(regPath, "DisplayVersion", string.Empty);

            foreach (var managementObj in new ManagementObjectSearcher(@"root\cimv2", "select Caption, Description, OSArchitecture, BuildNumber, Version from Win32_OperatingSystem", new EnumerationOptions { ReturnImmediately = true }).Get())
            {
                string data = new[] { "Caption", "Description" }.Select(p => managementObj[p] as string).FirstOrDefault(s => !string.IsNullOrWhiteSpace(s)) ?? string.Empty;
                OS.Name = $"{(data.Contains('W') ? data.Substring(data.IndexOf('W')) : data)} {Regex.Replace(managementObj["OSArchitecture"]?.ToString() ?? string.Empty, @"\-.+", "-bit")} {(!string.IsNullOrWhiteSpace(release) ? $"({release})" : string.Empty)}\n";
                OS.Version = $"{managementObj["Version"]?.ToString() ?? "0"}.{revisionNumber}\n";
                OS.Build = decimal.TryParse($"{Convert.ToString(managementObj["BuildNumber"])}.{revisionNumber}", NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal result) ? result : decimal.TryParse(Registry.GetValue(regPath, "CurrentBuild", "0")?.ToString(), out result) ? result : 0;

            }
            OS.Name = !string.IsNullOrWhiteSpace(OS.Name) ? OS.Name.TrimEnd('\n', '\r') : Registry.GetValue(regPath, "ProductName", "Windows")?.ToString();
            OS.Version = !string.IsNullOrWhiteSpace(OS.Version) ? OS.Version.TrimEnd('\n', '\r') : Registry.GetValue(regPath, "LCUVer", 0)?.ToString();
        }

        /// <summary>
        /// If the manufacturer did not provide a serial number and the obtained value is "Default string", 
        /// then the serial number will not be displayed, similar to the situation with the motherboard.
        /// </summary>
        private void GetBiosInfo()
        {
            string output = CommandExecutor.GetCommandOutput(PathLocator.Executable.BcdEdit).GetAwaiter().GetResult();
            Bios.Mode = output.IndexOf("efi", StringComparison.OrdinalIgnoreCase) >= 0 ? "UEFI" : "Legacy Boot";

            foreach (var managementObj in new ManagementObjectSearcher(@"root\cimv2", "select Name, Caption, Description, SMBIOSBIOSVersion, SerialNumber from Win32_BIOS", new EnumerationOptions { ReturnImmediately = true }).Get())
            {
                string data = new[] { "Name", "Caption", "Description", "SMBIOSBIOSVersion" }.Select(prop => managementObj[prop] as string).FirstOrDefault(info => !string.IsNullOrEmpty(info)) ?? string.Empty;
                string dataSN = managementObj["SerialNumber"]?.ToString() ?? string.Empty;
                Bios.Data += !string.IsNullOrWhiteSpace(dataSN) && !dataSN.Any(char.IsWhiteSpace) ? $"{data}, S/N-{dataSN}\n" : $"{data}\n";
            }
            Bios.Data = Bios.Data.TrimEnd('\n', '\r');
        }

        private void GetMotherboardInfo()
        {
            foreach (var managementObj in new ManagementObjectSearcher(@"root\cimv2", "select Manufacturer, Product, Version from Win32_BaseBoard", new EnumerationOptions { ReturnImmediately = true }).Get())
            {
                string data = $"{managementObj["Manufacturer"]?.ToString() ?? string.Empty}{managementObj["Product"]?.ToString() ?? string.Empty}";
                string dataVersion = managementObj["Version"]?.ToString() ?? string.Empty;
                Motherboard += !string.IsNullOrWhiteSpace(dataVersion) && !dataVersion.Any(char.IsWhiteSpace) ? $"{data}, V{dataVersion}\n" : $"{data}\n";
            }
            Motherboard = Motherboard.TrimEnd('\n', '\r');
        }

        private void GetProcessorInfo()
        {
            foreach (var managementObj in new ManagementObjectSearcher(@"root\cimv2", "select Name, NumberOfCores, NumberOfLogicalProcessors from Win32_Processor", new EnumerationOptions { ReturnImmediately = true }).Get())
            {
                Processor.Data = $"{managementObj["Name"]?.ToString() ?? string.Empty}\n";
                Processor.Cores = managementObj["NumberOfCores"]?.ToString() ?? string.Empty;
                Processor.Threads = managementObj["NumberOfLogicalProcessors"]?.ToString() ?? string.Empty;
            }
            Processor.Data = Processor.Data.TrimEnd('\n', '\r');
        }

        /// <summary>
        /// The maximum video card memory that can be obtained via WMI is 4 GB. Therefore, the memory size is obtained from the registry. 
        /// For discrete video cards or older models, the parameters will have the REG_BINARY type, and for integrated ones, REG_SZ.
        /// </summary>
        private void GetGraphicsInfo()
        {
            static (bool, string, string) GetMemorySize(string name)
            {
                using RegistryKey baseKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\ControlSet001\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}");
                if (baseKey != null)
                {
                    foreach (string subKeyName in baseKey.GetSubKeyNames())
                    {
                        if (subKeyName == "Properties")
                        {
                            continue;
                        }

                        using RegistryKey regKey = baseKey.OpenSubKey(subKeyName);
                        if (regKey != null)
                        {
                            string adapterString = regKey.GetValue("HardwareInformation.AdapterString") as string ?? string.Empty;
                            string driverDesc = regKey.GetValue("DriverDesc") as string ?? string.Empty;
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
                                else
                                {
                                    return (false, string.Empty, string.Empty);
                                }
                            }
                        }
                    }
                }
                return (false, string.Empty, string.Empty);
            }


            foreach (var managementObj in new ManagementObjectSearcher(@"root\cimv2", "select Name, AdapterRAM, PNPDeviceID from Win32_VideoController", new EnumerationOptions { ReturnImmediately = true }).Get())
            {
                string data = managementObj["Name"] as string ?? string.Empty;
                (bool isFound, string dataMemoryReg, string driverDesc) = GetMemorySize(data);
                Graphics += $"{(data == null && !string.IsNullOrEmpty(driverDesc) ? driverDesc : data)}, {(isFound && !string.IsNullOrEmpty(dataMemoryReg) ? dataMemoryReg : managementObj["AdapterRAM"] is uint valueRAM && managementObj["AdapterRAM"] != null ? SizeCalculationHelper(valueRAM) : "N/A")}\n";
                VendorDetection.Nvidia |= managementObj["PNPDeviceID"]?.ToString().IndexOf("VEN_10DE", StringComparison.OrdinalIgnoreCase) >= 0;
            }
            Graphics = Graphics.TrimEnd('\n', '\r');
        }

        /// <summary>
        /// The Win32_PhysicalMemory class has a limitation in retrieving the memory name and speed. In these cases, the values will be "Unknown" and "0". 
        /// Under these circumstances, the memory type will be displayed instead of the name, and the speed will not be shown.
        /// </summary>
        private void GetMemoryInfo()
        {
            foreach (var managementObj in new ManagementObjectSearcher(@"root\cimv2", "select Manufacturer, Name, Caption, Description, Tag, Capacity, ConfiguredClockSpeed, Speed, SMBIOSMemoryType from Win32_PhysicalMemory", new EnumerationOptions { ReturnImmediately = true }).Get())
            {
                string speedData = new[] { "ConfiguredClockSpeed", "Speed" }.Select(prop => managementObj[prop]?.ToString()).FirstOrDefault(s => !string.IsNullOrEmpty(s) && s != "0");
                string data = new[] { "Manufacturer", "Name", "Caption", "Description", "Tag" }.Select(prop => managementObj[prop] as string).FirstOrDefault(value => !string.IsNullOrWhiteSpace(value) && !string.Equals(value, "Unspecified", StringComparison.OrdinalIgnoreCase)) ?? string.Empty;
                string memoryType = (uint)managementObj["SMBIOSMemoryType"] switch
                {
                    2 => "DRAM",
                    3 => "EDRAM",
                    4 => "VRAM",
                    5 => "SRAM",
                    6 => "RAM",
                    7 => "ROM",
                    8 => "Flash",
                    9 => "EEPROM",
                    10 => "FEPROM",
                    11 => "EPROM",
                    12 => "CDRAM",
                    13 => "3DRAM",
                    14 => "SDRAM",
                    15 => "SGRAM",
                    16 => "RDRAM",
                    17 => "DDR",
                    18 => "DDR2",
                    19 => "DDR2 FB-DIMM",
                    20 => "Reserved",
                    21 => "Reserved",
                    22 => "FBD2",
                    23 => "DDR3",
                    24 => "DDR3",
                    uint type when type == 25 || type == 26 => "DDR4",
                    27 => "LPDDR",
                    28 => "LPDDR2",
                    29 => "LPDDR3",
                    30 => "LPDDR4",
                    31 => "LPDDR4X",
                    32 => "Logical Non-Volatile",
                    33 => "HBM",
                    34 => "DDR5",
                    35 => "LPDDR5",
                    36 => "LPDDR5X",
                    _ => string.Empty,
                };
                Memory.Type = memoryType;
                Memory.Data += $"{string.Concat(data, ", ")}{SizeCalculationHelper((ulong)managementObj["Capacity"])}{(string.IsNullOrEmpty(speedData) ? "" : $", {speedData}MHz")}\n";
            }
            Memory.Data = Memory.Data.TrimEnd('\n', '\r');
        }

        /// <summary>
        /// The MSFT_PhysicalDisk class may be missing or malfunctioning; in such cases, it will be replaced by the universal Win32_DiskDrive class. 
        /// </summary>
        /// 
        private string GetStorageDevices()
        {
            StringBuilder result = new StringBuilder();

            static string GetStorageType(object mediaType, string deviceId, ushort busType, string interfaceType)
            {
                string storageType = MediaTypeMap.Where(x => x.Keys != null && mediaType != null && x.Keys.Contains(mediaType)).Select(x => x.Type).FirstOrDefault() ?? DiskTypeLabels.Unspecified;

                if (isMsftAvailable)
                {
                    if (BusTypeMap.TryGetValue(busType, out var busStorageType))
                    {
                        storageType = busStorageType;
                    }
                }
                else
                {
                    if ((storageType == DiskTypeLabels.Unspecified || storageType == DiskTypeLabels.HDD) && (string.IsNullOrEmpty(interfaceType) || interfaceType.IndexOf("USB", StringComparison.OrdinalIgnoreCase) >= 0))
                    {
                        return DiskTypeLabels.USB;
                    }
                }

                if (storageType != DiskTypeLabels.Unspecified)
                {
                    string lowLevelType = DiskTypeIdentifier.GetStorageKind(deviceId);
                    if (!string.IsNullOrEmpty(lowLevelType) && lowLevelType != DiskTypeLabels.Unspecified)
                    {
                        return lowLevelType;
                    }
                }

                if (storageType == DiskTypeLabels.Unspecified && !string.IsNullOrEmpty(deviceId))
                {
                    return DiskTypeIdentifier.GetStorageKind(deviceId);
                }

                return storageType;
            }

            if (isMsftAvailable)
            {
                foreach (var managementObj in new ManagementObjectSearcher(@"root\microsoft\windows\storage", "select DeviceId, FriendlyName, Model, Description, MediaType, Size, BusType from MSFT_PhysicalDisk", new EnumerationOptions { ReturnImmediately = true }).Get())
                {
                    string data = new[] { "FriendlyName", "Model", "Description" }.Select(prop => managementObj[prop] as string).FirstOrDefault(info => !string.IsNullOrEmpty(info)) ?? string.Empty;
                    ushort mediaType = managementObj["MediaType"] is IConvertible m ? Convert.ToUInt16(m) : (ushort)0;
                    ushort busType = managementObj["BusType"] is IConvertible b ? Convert.ToUInt16(b) : (ushort)0;
                    string storageType = GetStorageType(mediaType, $@"\\.\PhysicalDrive{managementObj["DeviceId"]?.ToString() ?? "0"}", busType, default);

                    result.AppendLine($"{SizeCalculationHelper((ulong)managementObj["Size"])} [{data}] {storageType}");
                }
            }
            else
            {
                foreach (var managementObj in new ManagementObjectSearcher(@"root\cimv2", "select DeviceID, Model, Caption, Size, MediaType, InterfaceType from Win32_DiskDrive", new EnumerationOptions { ReturnImmediately = true }).Get())
                {
                    string data = new[] { "Model", "Caption" }.Select(prop => managementObj[prop] as string).FirstOrDefault(info => !string.IsNullOrEmpty(info)) ?? string.Empty;
                    string mediaType = managementObj["MediaType"]?.ToString() ?? string.Empty;
                    string interfaceType = managementObj["InterfaceType"].ToString() ?? string.Empty;
                    string storageType = GetStorageType(mediaType, managementObj["DeviceID"] as string ?? string.Empty, default, interfaceType);

                    result.AppendLine($"{SizeCalculationHelper((ulong)managementObj["Size"])} [{data}] {storageType}");
                }
            }

            return result.ToString().TrimEnd('\n', '\r');
        }

        /// <summary>
        /// Handling the retrieval of device names for USB devices: In WMI, most connected devices are often named "USB Audio Device." 
        /// Therefore, for such devices, the name lookup is performed through the registry. 
        /// The search for an identifier in Win32_PnPEntity is slow, although it is more convenient. However, it is inferior in speed.
        /// </summary>
        private string GetAudioDevices()
        {
            StringBuilder result = new StringBuilder();

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
                                {
                                    return (true, RegistryHelp.GetValue(propsPath, "{b3f8fa53-0004-438e-9003-51a46e139bfc},6", string.Empty));
                                }
                            }
                        }
                    }
                }
                return (false, string.Empty);
            }

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\cimv2", "select DeviceID, Name, Caption, Description, PNPDeviceID from Win32_SoundDevice where Status = 'OK'", new EnumerationOptions { ReturnImmediately = true }))
            {
                foreach (ManagementObject managementObj in searcher.Get().Cast<ManagementObject>())
                {
                    (bool isUsbDevice, string data) = IsUsbAudioDevice(managementObj["DeviceID"].ToString());

                    if (isUsbDevice && !string.IsNullOrEmpty(data))
                    {
                        result.AppendLine(data);
                    }
                    else
                    {
                        result.AppendLine(new[] { "Name", "Caption", "Description" }.Select(prop => managementObj[prop] as string).FirstOrDefault(info => !string.IsNullOrEmpty(info)) ?? string.Empty);
                    }

                    VendorDetection.Realtek |= managementObj["PNPDeviceID"]?.ToString().IndexOf("VEN_10EC", StringComparison.OrdinalIgnoreCase) >= 0;
                }
            }

            return result.ToString().TrimEnd('\n', '\r');
        }

        private static string SizeCalculationHelper<T>(T sizeInBytes) where T : struct, IConvertible
        {
            decimal bytes = Convert.ToDecimal(sizeInBytes);

            foreach (string unit in new[] { "B", "KB", "MB", "GB", "TB" })
            {
                if (bytes < 1024 || unit == "TB")
                {
                    return unit == "TB" ? $"{Math.Round(bytes, 2):G} {unit}" : $"{Math.Round(bytes):N0} {unit}";
                }
                bytes /= 1024;
            }

            return $"{Math.Round(bytes, 2):G} TB";
        }

        private string GetNetworkAdapters()
        {
            StringBuilder result = new StringBuilder();

            foreach (var managementObj in new ManagementObjectSearcher(@"root\cimv2", "select Name, Description, ProductName, Manufacturer from Win32_NetworkAdapter where NetConnectionStatus=2 or NetConnectionStatus=7", new EnumerationOptions { ReturnImmediately = true }).Get())
            {
                result.AppendLine(new[] { "Name", "Description", "ProductName", "Manufacturer" }.Select(prop => managementObj[prop] as string).FirstOrDefault(info => !string.IsNullOrEmpty(info)) ?? string.Empty);
            }

            return result.ToString().TrimEnd('\n', '\r');
        }

        internal bool IsNetworkAvailable()
        {
            try
            {
                string dns = GetCurrentSystemLang().Code switch
                {
                    "fa" => "aparat.com",
                    "zh" => "baidu.com",
                    "ru" => "yandex.ru",
                    "kk" => "yandex.kz",
                    "ko" => "naver.com",
                    "cs" => "seznam.cz",
                    "tk" => "turkmenportal.com",
                    "vi" => "fpt.vn",
                    "es" => "terra.es",
                    "ja" => "yahoo.co.jp",
                    "de" => "t-online.de",
                    "fr" => "orange.fr",
                    "it" => "libero.it",
                    "th" => "true.th",
                    "pl" => "wp.pl",
                    "nl" => "nu.nl",
                    "pt" => "globo.com",
                    "sv" => "aftonbladet.se",
                    "no" => "vg.no",
                    "fi" => "yle.fi",
                    "ro" => "digisport.ro",
                    "gr" => "in.gr",
                    "id" => "detik.com",
                    "mx" => "univision.com",
                    "ar" => "claro.com.ar",
                    string name when new[] { "tr", "hi", "ar" }.Contains(name) => "bing.com",
                    _ => "google.com"
                };

                TimeSpan timeout = TimeSpan.FromSeconds(5.0);

                using Task<IPAddress> task = Task.Run(() =>
                {
                    try { return Dns.GetHostEntry(dns).AddressList[0]; }
                    catch { return null; }
                });

                if (!task.Wait(timeout) || task.Result == null)
                {
                    return false;
                }

                return true;
            }
            catch { return false; }
        }

        internal void GetUserIpAddress()
        {
            if (IsNetworkAvailable())
            {
                bool hadLimited = false, hadBlock = false;

                using HttpClient client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };

                foreach (string url in new[] { "https://free.freeipapi.com/api/json/", "https://api.db-ip.com/v2/free/self", "https://ipapi.co/json/", "https://reallyfreegeoip.org/json/", "https://get.geojs.io/v1/ip/geo.json", "http://ip-api.com/json/" })
                {
                    try
                    {
                        using HttpResponseMessage response = client.GetAsync(url).GetAwaiter().GetResult();

                        if (!response.IsSuccessStatusCode)
                        {
                            hadBlock = true;
                            continue;
                        }

                        IPMetadata ipMetadata = IPMetadata.ParseData(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                        if (IPAddress.TryParse(ipMetadata.Ip, out _) && !string.IsNullOrWhiteSpace(ipMetadata?.Ip) && !string.IsNullOrWhiteSpace(ipMetadata?.Country))
                        {
                            CurrentConnection = ConnectionStatus.Available;
                            UserIPAddress = $"{ipMetadata.Ip} ({ipMetadata.Country})";
                            break;
                        }
                        else
                        {
                            hadBlock = true;
                        }
                    }
                    catch { hadLimited = true; }
                }

                if (CurrentConnection != ConnectionStatus.Available)
                {
                    if (hadLimited)
                    {
                        CurrentConnection = ConnectionStatus.Limited;
                    }
                    else if (hadBlock)
                    {
                        CurrentConnection = ConnectionStatus.Block;
                    }
                    else
                    {
                        CurrentConnection = ConnectionStatus.Lose;
                    }
                }
            }
            else
            {
                CurrentConnection = ConnectionStatus.Lose;
            }

            if (new Dictionary<ConnectionStatus, string>
                {
                    { ConnectionStatus.Lose, "connection_lose_sysinfo" },
                    { ConnectionStatus.Block, "connection_block_sysinfo" },
                    { ConnectionStatus.Limited, "connection_limited_sysinfo" }
                }.TryGetValue(CurrentConnection, out string resourceKey)) { UserIPAddress = (string)Application.Current.Resources[resourceKey]; }

            isIPAddressFormatValid = UserIPAddress.Any(char.IsDigit);
        }

        internal void ValidateVersionUpdates()
        {
            if (!SettingsEngine.IsUpdateCheckRequired || !IsNetworkAvailable())
            {
                return;
            }

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

                if (!string.IsNullOrWhiteSpace(gitVersionUtility.СurrentVersion) && gitVersionUtility.СurrentVersion.CompareTo(SettingsEngine.currentRelease) > 0)
                {
                    IsNeedUpdate = true;
                    DownloadVersion = gitVersionUtility.СurrentVersion;
                }
            }
            catch { IsNeedUpdate = false; }
        }
    }
}
