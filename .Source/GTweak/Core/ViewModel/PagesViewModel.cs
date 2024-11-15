using GTweak.Utilities;
using GTweak.Utilities.Tweaks;
using System.Windows;

namespace GTweak.Core.ViewModel
{
    internal class ConfidentialityVM : ViewModelBase { }
    internal class ApplicationsVM : ViewModelBase { }
    internal class ServicesVM : ViewModelBase { }
    internal class SystemVM : ViewModelBase { }
    internal class MoreVM : ViewModelBase { }


    internal class InterfaceVM : ViewModelBase
    {
        public bool IsBlockForWin10 => SystemData.СomputerСonfiguration.WindowsClientVersion.Contains("11");
        public bool IsBlockWithoutLicense => WindowsLicense.statusLicense == 1;
    }

    internal class InformationSystemVM : ViewModelBase
    {
        private readonly InformationSystemModel model;

        public string DisplayWindowsName
        {
            get => model.WindowsName;
            set { model.WindowsName = value; OnPropertyChanged(); }
        }

        public string DisplayBiosName
        {
            get => model.BIOSName;
            set { model.BIOSName = value; OnPropertyChanged(); }
        }

        public string DisplayMotherBrName
        {
            get => model.MotherBrName;
            set { model.MotherBrName = value; OnPropertyChanged(); }
        }

        public string DisplayCpuName
        {
            get => model.CPUName;
            set { model.CPUName = value; OnPropertyChanged(); }
        }

        public string DisplayGpuName
        {
            get => model.GPUName;
            set { model.GPUName = value; OnPropertyChanged(); }
        }

        public string DisplayRamName
        {
            get => model.RAMName;
            set { model.RAMName = value; OnPropertyChanged(); }
        }

        public string DisplayDiskName
        {
            get => model.DiskName;
            set { model.DiskName = value; OnPropertyChanged(); }
        }

        public string DisplaySoundName
        {
            get => model.SoundName;
            set { model.SoundName = value; OnPropertyChanged(); }
        }

        public string DisplayNetAdapterName
        {
            get => model.NetAdapterName;
            set { model.NetAdapterName = value; OnPropertyChanged(); }
        }

        public string DisplayIpAddress
        {
            get => model.IpAddress;
            set { model.IpAddress = value; OnPropertyChanged(); }
        }

        public string DisplayCountProcess
        {
            get => model.CountProcess;
            set { model.CountProcess = value; OnPropertyChanged(); }
        }

        public int SetBlurValue
        {
            get => model.BlurValue;
            set { model.BlurValue = value; OnPropertyChanged("SetBlurValue"); }
        }

        public Visibility SetVisibility
        {
            get => model.IpVisibility;
            set { model.IpVisibility = value; OnPropertyChanged(); }
        }

        public bool StateButton
        {
            get => Settings.IsHiddenIpAddress;
            set { Settings.IsHiddenIpAddress = value; OnPropertyChanged(); }
        }


        public InformationSystemVM()
        {
            model = new InformationSystemModel();
            DisplayWindowsName = SystemData.СomputerСonfiguration.СonfigurationData["Windows"];
            DisplayBiosName = SystemData.СomputerСonfiguration.СonfigurationData["BIOS"];
            DisplayMotherBrName = SystemData.СomputerСonfiguration.СonfigurationData["MotherBr"];
            DisplayCpuName = SystemData.СomputerСonfiguration.СonfigurationData["CPU"];

            if (!string.IsNullOrEmpty(SystemData.СomputerСonfiguration.СonfigurationData["GPU"]))
                DisplayGpuName = SystemData.СomputerСonfiguration.СonfigurationData["GPU"];
            else
                DisplayGpuName = (string)Application.Current.Resources["driver_not_installed_systemInformatin"];

            DisplayRamName = SystemData.СomputerСonfiguration.СonfigurationData["RAM"];
            DisplayDiskName = SystemData.СomputerСonfiguration.СonfigurationData["Disk"];

            if (!string.IsNullOrEmpty(SystemData.СomputerСonfiguration.СonfigurationData["Sound"]))
                DisplaySoundName = SystemData.СomputerСonfiguration.СonfigurationData["Sound"];
            else
                DisplaySoundName = (string)Application.Current.Resources["driver_not_installed_systemInformatin"];

            if (!string.IsNullOrEmpty(SystemData.СomputerСonfiguration.СonfigurationData["NetAdapter"]))
                DisplayNetAdapterName = SystemData.СomputerСonfiguration.СonfigurationData["NetAdapter"];
            else
                DisplayNetAdapterName = (string)Application.Current.Resources["driver_not_installed_systemInformatin"];

            DisplayIpAddress = SystemData.СomputerСonfiguration.СonfigurationData["IpAddress"];
            DisplayCountProcess = new SystemData.MonitoringSystem().CountProcess.ToString();

            if (Settings.IsHiddenIpAddress & SystemData.СomputerСonfiguration.ConnectionStatus == 0)
            {
                SetBlurValue = 20;
                SetVisibility = Visibility.Visible;
            }
            else
            {
                SetBlurValue = 0;
                SetVisibility = SystemData.СomputerСonfiguration.ConnectionStatus == 0 ? Visibility.Visible : Visibility.Hidden;
            }
        }
    }
}
