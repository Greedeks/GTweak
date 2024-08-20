using GTweak.Utilities;
using GTweak.Core.Model;
using System.Windows;
using GTweak.Utilities.Tweaks;

namespace GTweak.Core.ViewModel
{
    internal class ConfidentialityVM : ViewModelBase { }
    internal class InterfaceVM : ViewModelBase
    {
        private readonly MainModel.InterfaceModel _model;
        public bool SetBlockForWin10
        {
            get => _model.BlockForWin10;
            set { _model.BlockForWin10 = value; OnPropertyChanged(); }
        }

        public bool SetBlockWithoutLicense
        {
            get => _model.BlockWithoutLicense;
            set { _model.BlockWithoutLicense = value; OnPropertyChanged(); }
        }

        public InterfaceVM()
        {
            _model = new MainModel.InterfaceModel();

            SetBlockForWin10 = SystemData.СomputerСonfiguration.clientWinVersion.Contains("11");

            SetBlockWithoutLicense = WindowsLicense.statusLicense == 1;

            if (WindowsLicense.statusLicense != 1)
            {
                App.ViewLang();
                new ViewNotification().Show("", (string)Application.Current.Resources["title1_notification"], (string)Application.Current.Resources["viewlicense_notification"]);
            }
        }
    }

    internal class ApplicationsVM : ViewModelBase { }
    internal class ServicesVM : ViewModelBase { }
    internal class SystemVM : ViewModelBase { }
    internal class MoreVM : ViewModelBase { }

    internal class InformationSystemVM : ViewModelBase
    {
        private readonly MainModel.InformationSystemModel _model;
        public string DisplayWindowsName
        {
            get => _model.WindowsName;
            set { _model.WindowsName = value; OnPropertyChanged(); }
        }

        public string DisplayBiosName
        {
            get => _model.BIOSName;
            set { _model.BIOSName = value; OnPropertyChanged(); }
        }

        public string DisplayMotherBrName
        {
            get => _model.MotherBrName;
            set { _model.MotherBrName = value; OnPropertyChanged(); }
        }

        public string DisplayCpuName
        {
            get => _model.CPUName;
            set { _model.CPUName = value; OnPropertyChanged(); }
        }

        public string DisplayGpuName
        {
            get => _model.GPUName;
            set { _model.GPUName = value; OnPropertyChanged(); }
        }

        public string DisplayRamName
        {
            get => _model.RAMName;
            set { _model.RAMName = value; OnPropertyChanged(); }
        }

        public string DisplayDiskName
        {
            get => _model.DiskName;
            set { _model.DiskName = value; OnPropertyChanged(); }
        }

        public string DisplaySoundName
        {
            get => _model.SoundName;
            set { _model.SoundName = value; OnPropertyChanged(); }
        }

        public string DisplayNetAdapterName
        {
            get => _model.NetAdapterName;
            set { _model.NetAdapterName = value; OnPropertyChanged(); }
        }

        public string DisplayIpAddress
        {
            get => _model.IpAddress;
            set { _model.IpAddress = value; OnPropertyChanged(); }
        }

        public string DisplayCountProcess
        {
            get => _model.CountProcess;
            set { _model.CountProcess = value; OnPropertyChanged(); }
        }

        public InformationSystemVM()
        {
            _model = new MainModel.InformationSystemModel();

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
        }
    }
}
