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
        public bool IsBlockWithoutLicense => WindowsLicense.IsWindowsActivated;
    }

    internal class InformationSystemVM : ViewModelBase
    {
        private readonly InformationSystemModel model;

        public string DisplayWindows
        {
            get => model.WindowsDescriptions;
            set { model.WindowsDescriptions = value; OnPropertyChanged(); }
        }

        public string DisplayBios
        {
            get => model.BiosDescriptions;
            set { model.BiosDescriptions = value; OnPropertyChanged(); }
        }

        public string DisplayMotherBr
        {
            get => model.MotherBrDescriptions;
            set { model.MotherBrDescriptions = value; OnPropertyChanged(); }
        }

        public string DisplayCpu
        {
            get => model.CpuDescriptions;
            set { model.CpuDescriptions = value; OnPropertyChanged(); }
        }

        public string DisplayGpu
        {
            get => model.GpuDescriptions;
            set { model.GpuDescriptions = value; OnPropertyChanged(); }
        }

        public string DisplayRam
        {
            get => model.RamDescriptions;
            set { model.RamDescriptions = value; OnPropertyChanged(); }
        }

        public string DisplayStorage
        {
            get => model.StorageDescriptions;
            set { model.StorageDescriptions = value; OnPropertyChanged(); }
        }

        public string DisplayAudio
        {
            get => model.AudioDescriptions;
            set { model.AudioDescriptions = value; OnPropertyChanged(); }
        }

        public string DisplayNetwork
        {
            get => model.NetworkDescriptions;
            set { model.NetworkDescriptions = value; OnPropertyChanged(); }
        }

        public string DisplayIpAddress
        {
            get => model.UserIpAddress;
            set { model.UserIpAddress = value; OnPropertyChanged(); }
        }

        public string DisplayNumberProcesses
        {
            get => model.NumberProcesses;
            set { model.NumberProcesses = value; OnPropertyChanged(); }
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

            DisplayWindows = SystemData.СomputerСonfiguration.СonfigurationData["Windows"];
            DisplayNumberProcesses = new SystemData.MonitoringSystem().CountProcess.ToString();

            DisplayBios = !string.IsNullOrEmpty(SystemData.СomputerСonfiguration.СonfigurationData["BIOS"]) ? SystemData.СomputerСonfiguration.СonfigurationData["BIOS"] : (string)Application.Current.Resources["no_device_information_systemInformatin"];
            DisplayMotherBr = !string.IsNullOrEmpty(SystemData.СomputerСonfiguration.СonfigurationData["MotherBr"]) ? SystemData.СomputerСonfiguration.СonfigurationData["MotherBr"] : (string)Application.Current.Resources["no_device_information_systemInformatin"];
            DisplayCpu = !string.IsNullOrEmpty(SystemData.СomputerСonfiguration.СonfigurationData["CPU"]) ? SystemData.СomputerСonfiguration.СonfigurationData["CPU"] : (string)Application.Current.Resources["no_device_information_systemInformatin"];
            DisplayGpu = !string.IsNullOrEmpty(SystemData.СomputerСonfiguration.СonfigurationData["GPU"]) ? SystemData.СomputerСonfiguration.СonfigurationData["GPU"] : (string)Application.Current.Resources["driver_not_installed_systemInformatin"];
            DisplayRam = !string.IsNullOrEmpty(SystemData.СomputerСonfiguration.СonfigurationData["RAM"]) ? SystemData.СomputerСonfiguration.СonfigurationData["RAM"] : (string)Application.Current.Resources["no_device_information_systemInformatin"];
            DisplayStorage = !string.IsNullOrEmpty(SystemData.СomputerСonfiguration.СonfigurationData["Storage"]) ? SystemData.СomputerСonfiguration.СonfigurationData["Storage"] : (string)Application.Current.Resources["no_device_information_systemInformatin"];
            DisplayAudio = !string.IsNullOrEmpty(SystemData.СomputerСonfiguration.СonfigurationData["Audio"]) ? SystemData.СomputerСonfiguration.СonfigurationData["Audio"] : (string)Application.Current.Resources["driver_not_installed_systemInformatin"];
            DisplayNetwork = !string.IsNullOrEmpty(SystemData.СomputerСonfiguration.СonfigurationData["NetAdapter"]) ? SystemData.СomputerСonfiguration.СonfigurationData["NetAdapter"] : (string)Application.Current.Resources["driver_not_installed_systemInformatin"];

            DisplayIpAddress = SystemData.СomputerСonfiguration.СonfigurationData["UserIpAddress"];

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
