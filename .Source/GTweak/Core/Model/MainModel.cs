using System.Windows;

namespace GTweak.Core.Model
{
    internal class MainModel
    {
        internal sealed class InformationSystemModel
        {
            internal string WindowsName { get; set; }
            internal string BIOSName { get; set; }
            internal string MotherBrName { get; set; }
            internal string CPUName { get; set; }
            internal string GPUName { get; set; }
            internal string RAMName { get; set; }
            internal string DiskName { get; set; }
            internal string SoundName { get; set; }
            internal string NetAdapterName { get; set; }
            internal string IpAddress { get; set; }
            internal string CountProcess { get; set; }
            internal int BlurValue { get; set; }
            internal Visibility IpVisibility { get; set; }
        }
    }
}
