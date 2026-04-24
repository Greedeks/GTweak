using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
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

namespace GTweak.Utilities.Configuration
{
    internal sealed class HardwareProvider : NetworkProvider
    {
        internal static bool isMsftAvailable = false;

        internal static (string Code, string Region) GetCurrentSystemLang()
        {
            CultureInfo culture = CultureInfo.CurrentUICulture;
            string[] parts = culture.Name.Split('-');
            return (culture.TwoLetterISOLanguageName.ToLowerInvariant(), parts.Length > 1 ? parts[1].ToUpperInvariant() : string.Empty);
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
            catch (Exception ex) { ErrorLogging.LogDebug(ex); }

            return (ImageSource)Application.Current.Resources["Icon_ProfileAvatar"];
        }

        internal string GetProfileName()
        {
            string nameProfile = string.Empty;

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\cimv2", $"select FullName from Win32_UserAccount where domain='{Environment.UserDomainName}' and name='{Environment.UserName.ToLowerInvariant()}'", new EnumerationOptions { ReturnImmediately = true }))
            {
                foreach (ManagementObject managementObj in searcher.Get().Cast<ManagementObject>())
                {
                    using (managementObj)
                    {
                        nameProfile = managementObj["FullName"] as string;
                    }
                }
            }

            return !string.IsNullOrWhiteSpace(nameProfile) ? nameProfile : Environment.UserName.ToLowerInvariant();
        }

        internal void GetHardwareData()
        {
            Task.Run(() =>
            {
                try
                {
                    using ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\microsoft\windows\storage", "select FriendlyName from MSFT_PhysicalDisk", new EnumerationOptions { ReturnImmediately = true });
                    using ManagementObjectCollection results = searcher.Get();
                    isMsftAvailable = results?.Count > 0;

                }
                catch { isMsftAvailable = false; }

                Parallel.Invoke(
                    () => GetWallpaperImage(),
                    () => GetBiosInfo(),
                    () => GetMotherboardInfo(),
                    () => GetProcessorInfo(),
                    () => GetGraphicsInfo(),
                    () => GetPrimaryRefreshRate(),
                    () => GetMemoryInfo(),
                    () => RefreshDevicesData()
                );
            });
        }

        internal void RefreshDevicesData(DeviceType deviceType = DeviceType.All)
        {
            if (deviceType == DeviceType.Storage || deviceType == DeviceType.All)
            {
                Storage.Data = GetStorageDevices();
            }

            if (deviceType == DeviceType.StorageSpace || deviceType == DeviceType.All || deviceType == DeviceType.Storage)
            {
                (string FreePercentage, string UsedPercentage) = GetStorageSpace();
                Storage.FreeSpace = FreePercentage;
                Storage.UsedSpace = UsedPercentage;
            }

            if (deviceType == DeviceType.Audio || deviceType == DeviceType.All)
            {
                AudioDevice = GetAudioDevices();
            }

            if (deviceType == DeviceType.Network || deviceType == DeviceType.All)
            {
                NetworkAdapter = GetNetworkAdapters();
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
                    bmp.DecodePixelWidth = 200;
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

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\cimv2", "select Caption, Description, OSArchitecture, BuildNumber, Version from Win32_OperatingSystem", new EnumerationOptions { ReturnImmediately = true }))
            {
                foreach (ManagementObject managementObj in searcher.Get().Cast<ManagementObject>())
                {
                    using (managementObj)
                    {
                        string data = new string[] { "Caption", "Description" }.Select(p => managementObj[p] as string).FirstOrDefault(s => !string.IsNullOrWhiteSpace(s)) ?? string.Empty;
                        OS.Name = $"{(data.Contains('W') ? data.Substring(data.IndexOf('W')) : data)} {Regex.Replace(managementObj["OSArchitecture"]?.ToString() ?? string.Empty, @"\-.+", "-bit")} {(!string.IsNullOrWhiteSpace(release) ? $"({release})" : string.Empty)}\n";
                        OS.Version = $"{managementObj["Version"]?.ToString() ?? "0"}.{revisionNumber}\n";
                        OS.Build = decimal.TryParse($"{Convert.ToString(managementObj["BuildNumber"])}.{revisionNumber}", NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal result) ? result : decimal.TryParse(Registry.GetValue(regPath, "CurrentBuild", "0")?.ToString(), out result) ? result : 0;
                    }
                }
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
            try
            {
                string output = CommandExecutor.GetCommandOutput(PathLocator.Executable.BcdEdit).GetAwaiter().GetResult();
                Bios.Mode = output.IndexOf("efi", StringComparison.OrdinalIgnoreCase) >= 0 ? "UEFI" : "Legacy Boot";
            }
            catch { Bios.Mode = string.Empty; }

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\cimv2", "select Name, Caption, Description, SMBIOSBIOSVersion, SerialNumber from Win32_BIOS", new EnumerationOptions { ReturnImmediately = true }))
            {
                foreach (ManagementObject managementObj in searcher.Get().Cast<ManagementObject>())
                {
                    using (managementObj)
                    {
                        string data = new string[] { "Name", "Caption", "Description", "SMBIOSBIOSVersion" }.Select(prop => managementObj[prop] as string).FirstOrDefault(info => !string.IsNullOrEmpty(info)) ?? string.Empty;
                        string dataSN = managementObj["SerialNumber"]?.ToString() ?? string.Empty;
                        Bios.Data += !string.IsNullOrWhiteSpace(dataSN) && !dataSN.Any(char.IsWhiteSpace) ? $"{data}, S/N-{dataSN}\n" : $"{data}\n";
                    }
                }
            }

            Bios.Data = Bios.Data.TrimEnd('\n', '\r');
        }

        private void GetMotherboardInfo()
        {
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\cimv2", "select Manufacturer, Product, Version, SerialNumber from Win32_BaseBoard", new EnumerationOptions { ReturnImmediately = true }))
            {
                foreach (ManagementObject managementObj in searcher.Get().Cast<ManagementObject>())
                {
                    using (managementObj)
                    {
                        string data = $"{managementObj["Manufacturer"]?.ToString() ?? string.Empty} {managementObj["Product"]?.ToString() ?? string.Empty}";
                        string dataVersion = managementObj["Version"]?.ToString() ?? string.Empty;
                        string dataSN = managementObj["SerialNumber"]?.ToString() ?? string.Empty;
                        Motherboard.Data = $"{data}".Trim();

                        if (!string.IsNullOrWhiteSpace(dataVersion) && !dataVersion.Any(char.IsWhiteSpace))
                        {
                            Motherboard.Data += $", V{dataVersion}";
                        }
                        else if (!string.IsNullOrWhiteSpace(dataSN) && !dataSN.Any(char.IsWhiteSpace))
                        {
                            Motherboard.Data += $", S/N-{dataSN}";
                        }


                    }
                }
            }
            Motherboard.Data = Motherboard.Data.TrimEnd('\n', '\r');

            bool chipsetFound = false;
            using RegistryKey baseKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Enum\PCI");
            if (baseKey != null)
            {
                foreach (string deviceId in baseKey.GetSubKeyNames())
                {
                    if (!chipsetFound)
                    {
                        using RegistryKey deviceKey = baseKey.OpenSubKey(deviceId);
                        if (deviceKey != null)
                        {
                            foreach (string subId in deviceKey.GetSubKeyNames())
                            {
                                using RegistryKey subKey = deviceKey.OpenSubKey(subId);
                                string deviceDesc = subKey?.GetValue("DeviceDesc")?.ToString() ?? string.Empty;
                                string friendlyName = subKey?.GetValue("FriendlyName")?.ToString() ?? string.Empty;
                                string targetString = deviceDesc.Contains("LPC") ? deviceDesc : (friendlyName.Contains("LPC") ? friendlyName : string.Empty);

                                if (!string.IsNullOrWhiteSpace(targetString))
                                {
                                    string chipset = ParseChipset(targetString) ?? string.Empty;

                                    if (!string.IsNullOrEmpty(chipset))
                                    {
                                        Motherboard.Chipset = chipset;
                                        chipsetFound = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void GetProcessorInfo()
        {
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\cimv2", "select Name, CurrentClockSpeed, MaxClockSpeed, NumberOfCores, NumberOfLogicalProcessors from Win32_Processor", new EnumerationOptions { ReturnImmediately = true }))
            {
                foreach (ManagementObject managementObj in searcher.Get().Cast<ManagementObject>())
                {
                    using (managementObj)
                    {
                        Processor.Data = managementObj["Name"]?.ToString()?.Split('@')[0].Replace("CPU", string.Empty).Trim() ?? string.Empty;
                        Processor.Cores = managementObj["NumberOfCores"]?.ToString() ?? string.Empty;
                        Processor.Threads = managementObj["NumberOfLogicalProcessors"]?.ToString() ?? string.Empty;
                        Processor.Frequency = (managementObj["CurrentClockSpeed"] ?? managementObj["MaxClockSpeed"]) != null ? $"{(Convert.ToDouble(managementObj["CurrentClockSpeed"] ?? managementObj["MaxClockSpeed"]) / 1000.0).ToString("0.00", CultureInfo.InvariantCulture)} GHz" : string.Empty;
                    }
                }
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

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\cimv2", "select Name, AdapterRAM, PNPDeviceID from Win32_VideoController", new EnumerationOptions { ReturnImmediately = true }))
            {
                foreach (ManagementObject managementObj in searcher.Get().Cast<ManagementObject>())
                {
                    using (managementObj)
                    {
                        string data = managementObj["Name"] as string ?? string.Empty;
                        (bool isFound, string dataMemoryReg, string driverDesc) = GetMemorySize(data);
                        Graphics += $"{(string.IsNullOrEmpty(data) && !string.IsNullOrEmpty(driverDesc) ? driverDesc : data)}, {(isFound && !string.IsNullOrEmpty(dataMemoryReg) ? dataMemoryReg : managementObj["AdapterRAM"] is uint valueRAM && managementObj["AdapterRAM"] != null ? SizeCalculationHelper(valueRAM) : "N/A")}\n";
                        VendorDetection.Nvidia |= managementObj["PNPDeviceID"]?.ToString().IndexOf("VEN_10DE", StringComparison.OrdinalIgnoreCase) >= 0;
                    }
                }
            }

            Graphics = Graphics.TrimEnd('\n', '\r');
        }

        /// <summary>
        /// The Win32_PhysicalMemory class has a limitation in retrieving the memory name and speed. In these cases, the values will be "Unknown" and "0". 
        /// Under these circumstances, the memory type will be displayed instead of the name, and the speed will not be shown.
        /// </summary>
        private void GetMemoryInfo()
        {
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\cimv2", "select Manufacturer, Name, Caption, Description, Tag, Capacity, ConfiguredClockSpeed, Speed, SMBIOSMemoryType from Win32_PhysicalMemory", new EnumerationOptions { ReturnImmediately = true }))
            {
                foreach (ManagementObject managementObj in searcher.Get().Cast<ManagementObject>())
                {
                    using (managementObj)
                    {
                        string speedData = new string[] { "ConfiguredClockSpeed", "Speed" }.Select(prop => managementObj[prop]?.ToString()).FirstOrDefault(s => !string.IsNullOrEmpty(s) && s != "0");
                        string data = new string[] { "Manufacturer", "Name", "Caption", "Description", "Tag" }.Select(prop => managementObj[prop] as string).FirstOrDefault(value => !string.IsNullOrWhiteSpace(value) && !string.Equals(value, "Unspecified", StringComparison.OrdinalIgnoreCase)) ?? string.Empty;
                        Memory.Type = HardwareMappings.GetMemoryType(Convert.ToUInt32(managementObj["SMBIOSMemoryType"] ?? 0u));
                        Memory.Data += $"{string.Concat(data, ", ")}{SizeCalculationHelper(Convert.ToUInt64(managementObj["Capacity"] ?? 0UL))}{(string.IsNullOrEmpty(speedData) ? "" : $", {speedData}MHz")}\n";
                    }
                }
            }

            Memory.Data = Memory.Data.TrimEnd('\n', '\r');
        }

        /// <summary>
        /// The MSFT_PhysicalDisk class may be missing or malfunctioning; in such cases, it will be replaced by the universal Win32_DiskDrive class. 
        /// </summary>
        private string GetStorageDevices()
        {
            StringBuilder result = new StringBuilder();

            static string GetStorageType(object mediaType, string deviceId, ushort busType, string interfaceType)
            {
                string storageType = HardwareMappings.MediaTypeMap.FirstOrDefault(x => x.Keys != null && x.Keys.Any(k => k is string ks && mediaType is string ms ? string.Equals(ks, ms, StringComparison.OrdinalIgnoreCase) : Equals(k, mediaType))).Type ?? StorageTypeLabels.Unspecified;

                if (isMsftAvailable)
                {
                    storageType = HardwareMappings.BusTypeMap.FirstOrDefault(map => map.BusType == busType).StorageType ?? StorageTypeLabels.Unspecified;
                }
                else
                {
                    if ((storageType == StorageTypeLabels.Unspecified || storageType == StorageTypeLabels.HDD) && (string.IsNullOrEmpty(interfaceType) || interfaceType.IndexOf("USB", StringComparison.OrdinalIgnoreCase) >= 0))
                    {
                        return StorageTypeLabels.USB;
                    }
                }

                if (storageType != StorageTypeLabels.Unspecified)
                {
                    string lowLevelType = StorageTypeIdentifier.GetStorageKind(deviceId ?? string.Empty) ?? string.Empty;
                    if (!string.IsNullOrEmpty(lowLevelType) && lowLevelType != StorageTypeLabels.Unspecified)
                    {
                        return lowLevelType;
                    }
                }

                if (storageType == StorageTypeLabels.Unspecified && !string.IsNullOrEmpty(deviceId))
                {
                    return StorageTypeIdentifier.GetStorageKind(deviceId ?? string.Empty) ?? StorageTypeLabels.Unspecified;
                }

                return storageType;
            }

            if (isMsftAvailable)
            {
                using ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\microsoft\windows\storage", "select DeviceId, FriendlyName, Model, Description, MediaType, Size, BusType from MSFT_PhysicalDisk", new EnumerationOptions { ReturnImmediately = true });
                foreach (ManagementObject managementObj in searcher.Get().Cast<ManagementObject>())
                {
                    using (managementObj)
                    {
                        string data = new string[] { "FriendlyName", "Model", "Description" }.Select(prop => managementObj[prop] as string).FirstOrDefault(info => !string.IsNullOrEmpty(info)) ?? string.Empty;
                        ushort mediaType = managementObj["MediaType"] is IConvertible m ? Convert.ToUInt16(m) : (ushort)0;
                        ushort busType = managementObj["BusType"] is IConvertible b ? Convert.ToUInt16(b) : (ushort)0;
                        string storageType = GetStorageType(mediaType, $@"\\.\PhysicalDrive{managementObj["DeviceId"]?.ToString() ?? "0"}", busType, default);

                        result.AppendLine($"{SizeCalculationHelper(Convert.ToUInt64(managementObj["Size"] ?? 0UL))} [{data}] {storageType}");
                    }
                }
            }
            else
            {
                using ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\cimv2", "select DeviceID, Model, Caption, Size, MediaType, InterfaceType from Win32_DiskDrive", new EnumerationOptions { ReturnImmediately = true });
                foreach (ManagementObject managementObj in searcher.Get().Cast<ManagementObject>())
                {
                    using (managementObj)
                    {
                        string data = new string[] { "Model", "Caption" }.Select(prop => managementObj[prop] as string).FirstOrDefault(info => !string.IsNullOrEmpty(info)) ?? string.Empty;
                        string mediaType = managementObj["MediaType"]?.ToString() ?? string.Empty;
                        string interfaceType = managementObj["InterfaceType"]?.ToString() ?? string.Empty;
                        string storageType = GetStorageType(mediaType, managementObj["DeviceID"] as string ?? string.Empty, default, interfaceType);

                        result.AppendLine($"{SizeCalculationHelper(Convert.ToUInt64(managementObj["Size"] ?? 0UL))} [{data}] {storageType}");
                    }
                }
            }

            return result.ToString().TrimEnd('\n', '\r');
        }

        private (string FreePercentage, string UsedPercentage) GetStorageSpace()
        {
            long totalSize = 0;
            long totalFreeSpace = 0;

            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                try
                {
                    if (drive.IsReady && (drive.DriveType == DriveType.Fixed || drive.DriveType == DriveType.Removable))
                    {
                        totalSize += drive.TotalSize;
                        totalFreeSpace += drive.TotalFreeSpace;
                    }
                }
                catch (Exception ex) { ErrorLogging.LogDebug(ex); }
            }

            string freePercent = totalSize > 0 ? $"{(Math.Round((double)totalFreeSpace / totalSize * 100, 1)).ToString("0.#", CultureInfo.InvariantCulture)}%" : string.Empty;
            string usedPercent = totalSize > 0 ? $"{(Math.Round(100 - ((double)totalFreeSpace / totalSize * 100), 1)).ToString("0.#", CultureInfo.InvariantCulture)}%" : string.Empty;

            return (freePercent, usedPercent);
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
                foreach (string basePath in new[] { @"SOFTWARE\Microsoft\Windows\CurrentVersion\MMDevices\Audio\Render", @"SOFTWARE\Microsoft\Windows\CurrentVersion\MMDevices\Audio\Capture" })
                {
                    using RegistryKey regKey = Registry.LocalMachine.OpenSubKey(basePath);
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

                                if (!string.IsNullOrEmpty(valueID) && valueID.IndexOf(deviceID, StringComparison.OrdinalIgnoreCase) >= 0 &&
                                    ((!string.IsNullOrEmpty(value5) && value5.IndexOf("wdma_usb.inf", StringComparison.OrdinalIgnoreCase) >= 0) ||
                                     (!string.IsNullOrEmpty(value8) && value8.IndexOf(@"USB\Class_01", StringComparison.OrdinalIgnoreCase) >= 0) ||
                                     (!string.IsNullOrEmpty(value6) && value6.IndexOf("USBAudio.inf", StringComparison.OrdinalIgnoreCase) >= 0) ||
                                     (!string.IsNullOrEmpty(value24) && value24.IndexOf("usb", StringComparison.OrdinalIgnoreCase) >= 0)))
                                {

                                    string nameValue6 = RegistryHelp.GetValue(propsPath, "{b3f8fa53-0004-438e-9003-51a46e139bfc},6", string.Empty).Trim();
                                    string typeNameValue2 = RegistryHelp.GetValue(propsPath, "{a45c254e-df1c-4efd-8020-67d146a850e0},2", string.Empty).Trim();

                                    if (nameValue6.Length > 10 && !string.Equals(nameValue6, typeNameValue2, StringComparison.OrdinalIgnoreCase))
                                    {
                                        return (true, nameValue6);
                                    }
                                    else
                                    {
                                        return (true, $"{typeNameValue2} {nameValue6}");
                                    }
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
                    using (managementObj)
                    {
                        (bool isUsbDevice, string data) = IsUsbAudioDevice(managementObj["DeviceID"]?.ToString() ?? string.Empty);

                        if (isUsbDevice && !string.IsNullOrEmpty(data))
                        {
                            result.AppendLine(data);
                        }
                        else
                        {
                            result.AppendLine(new string[] { "Name", "Caption", "Description" }.Select(prop => managementObj[prop] as string).FirstOrDefault(info => !string.IsNullOrEmpty(info)) ?? string.Empty);
                        }

                        VendorDetection.Realtek |= (managementObj["PNPDeviceID"]?.ToString().IndexOf("VEN_10EC", StringComparison.OrdinalIgnoreCase) ?? -1) >= 0;
                    }
                }
            }

            return result.ToString().TrimEnd('\n', '\r');
        }

        private string GetNetworkAdapters()
        {
            StringBuilder result = new StringBuilder();

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\cimv2", "select Name, Description, ProductName, Manufacturer from Win32_NetworkAdapter where NetConnectionStatus=2 or NetConnectionStatus=7", new EnumerationOptions { ReturnImmediately = true }))
            {
                foreach (ManagementObject managementObj in searcher.Get().Cast<ManagementObject>())
                {
                    using (managementObj)
                    {
                        result.AppendLine(new string[] { "Name", "Description", "ProductName", "Manufacturer" }.Select(prop => managementObj[prop]?.ToString()).FirstOrDefault(info => !string.IsNullOrEmpty(info)) ?? string.Empty); ;
                    }
                }
            }

            return result.ToString().TrimEnd('\n', '\r');
        }

        private static string SizeCalculationHelper<T>(T sizeInBytes) where T : struct, IConvertible
        {
            decimal bytes = ((IConvertible)sizeInBytes).ToDecimal(null);

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

        private string ParseChipset(string rawCaption)
        {
            if (!string.IsNullOrWhiteSpace(rawCaption))
            {
                Match match = Regex.Match(rawCaption, @"\((?<res>(?=[^)]*\d)[^)]{3,})\)|\b(?<res>[A-Z]{1,2}\d{2,4}[A-Z]?)\b");
                if (match.Success)
                {
                    return match.Groups["res"].Value;
                }

                string clean = Regex.Replace(rawCaption, @"\([RTMtm]+\)|(?i:\b(Intel|AMD|NVIDIA|VIA|Series|Chipset|Family|LPC|Controller|Interface|Bridge|Host|Standard)\b)", "");
                clean = Regex.Replace(clean, @"[()\[\]\-\s]+", " ").Trim();

                return clean.Length > 2 ? clean : rawCaption;
            }

            return string.Empty;
        }
    }
}
