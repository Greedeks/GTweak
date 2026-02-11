using System.Windows.Media;

namespace GTweak.Utilities.Configuration
{
    internal class HardwareData
    {
        internal sealed class OperatingSystemInfo
        {
            internal string Name { get; set; } = string.Empty;
            internal string Version { get; set; } = string.Empty;
            internal decimal Build { get; set; } = default;
            internal bool IsWin10 => Name.Contains("10");
            internal bool IsWin11 => Name.Contains("11");
        }

        internal sealed class BiosInfo
        {
            internal string Data { get; set; } = string.Empty;
            internal string Mode { get; set; } = string.Empty;
        }

        internal sealed class ProcessorInfo
        {
            internal string Data { get; set; } = string.Empty;
            internal int Usage { get; set; } = default;
            internal string Cores { get; set; } = string.Empty;
            internal string Threads { get; set; } = string.Empty;
        }

        internal sealed class MemoryInfo
        {
            internal string Data { get; set; } = string.Empty;
            internal int Usage { get; set; } = default;
            internal string Type { get; set; } = string.Empty;
        }

        internal enum ConnectionStatus { Available, Lose, Block, Limited, }

        internal static OperatingSystemInfo OS { get; set; } = new OperatingSystemInfo();
        internal static ImageSource Wallpaper { get; set; } = default;
        internal static string RunningProcessesCount { get; set; } = string.Empty;
        internal static string RunningServicesCount { get; set; } = string.Empty;
        internal static BiosInfo Bios { get; set; } = new BiosInfo();
        internal static ProcessorInfo Processor { get; set; } = new ProcessorInfo();
        internal static string Motherboard { get; set; } = string.Empty;
        internal static string Graphics { get; set; } = string.Empty;
        internal static MemoryInfo Memory { get; set; } = new MemoryInfo();
        internal static string Storage { get; set; } = string.Empty;
        internal static string AudioDevice { get; set; } = string.Empty;
        internal static string NetworkAdapter { get; set; } = string.Empty;
        internal static string UserIPAddress { get; set; } = string.Empty;
        internal static ConnectionStatus CurrentConnection = ConnectionStatus.Lose;

        internal static class VendorDetection
        {
            internal static bool Nvidia { get; set; } = default;
            internal static bool Realtek { get; set; } = default;
        }

        internal static class StorageTypeLabels
        {
            internal const string Unspecified = "(Unspecified)";
            internal const string SCM = "(SCM)";
            internal const string HDD = "(HDD)";
            internal const string SSD = "(SSD)";
            internal const string NVMe = "(NVMe SSD)";
            internal const string SCSI = "(SCSI)";
            internal const string USB = "(USB-Media)";
            internal const string SD = "(SD-Card)";
            internal const string CD = "(CD/DVD)";
            internal const string VHD = "(VHD)";
            internal const string VHDX = "(VHDX)";
        }

        internal static class StorageTypeMappings
        {
            internal static readonly (object[] Keys, string Type)[] MediaTypeMap = new (object[] Keys, string Type)[]
            {
                (new object[] { (ushort)3, "Removable Media" }, StorageTypeLabels.HDD),
                (new object[] { (ushort)4, "Fixed hard disk media" }, StorageTypeLabels.SSD),
                (new object[] { (ushort)5, "Unspecified" }, StorageTypeLabels.SCM)
            };

            internal static readonly (ushort BusType, string StorageType)[] BusTypeMap = new (ushort, string)[]
            {
                (7, StorageTypeLabels.USB),
                (12, StorageTypeLabels.SD),
                (17, StorageTypeLabels.NVMe)
            };
        }
    }
}