using System.Windows;

namespace GTweak.Core.Model
{
    internal sealed class DataSystemModel
    {
        internal string WindowsDescriptions { get; set; }
        internal string BiosDescriptions { get; set; }
        internal string MotherBrDescriptions { get; set; }
        internal string CpuDescriptions { get; set; }
        internal string GpuDescriptions { get; set; }
        internal string RamDescriptions { get; set; }
        internal string StorageDescriptions { get; set; }
        internal string AudioDescriptions { get; set; }
        internal string NetworkDescriptions { get; set; }
        internal string UserIpAddress { get; set; }
        internal string NumberProcesses { get; set; }
        internal int BlurValue { get; set; }
        internal Visibility IpVisibility { get; set; }
    }
}
