using GTweak.Core.Model;
using GTweak.Utilities;
using GTweak.Utilities.Tweaks;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;


namespace GTweak.Core.ViewModel
{
    internal class DataSystemVM : ViewModelBase
    {
        private readonly DataSystemModel model;
        private ObservableCollection<DataSystemModel> collection;

        public ObservableCollection<DataSystemModel> DisplayData
        {
            get => collection;
            set { collection = value; OnPropertyChanged(); }
        }
        public DataSystemModel this[string name] => DisplayData.FirstOrDefault(d => d.ItemName == name);

        public int SetBlurValue
        {
            get => model.BlurValue;
            set { model.BlurValue = value; OnPropertyChanged(); }
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

        public DataSystemVM()
        {
            model = new DataSystemModel();

            DisplayData = new ObservableCollection<DataSystemModel>
            {
                 new DataSystemModel { ItemName = "Windows", Data = SystemData.СomputerСonfiguration.СonfigurationData["Windows"] },
                 new DataSystemModel { ItemName = "Processes", Data = new SystemData.MonitoringSystem().GetNumberRunningProcesses },
                 new DataSystemModel { ItemName = "Bios", Data = !string.IsNullOrEmpty(SystemData.СomputerСonfiguration.СonfigurationData["BIOS"]) ? SystemData.СomputerСonfiguration.СonfigurationData["BIOS"] : (string)Application.Current.Resources["no_device_information_systemInformatin"] },
                 new DataSystemModel { ItemName = "MotherBr", Data =  !string.IsNullOrEmpty(SystemData.СomputerСonfiguration.СonfigurationData["MotherBr"]) ? SystemData.СomputerСonfiguration.СonfigurationData["MotherBr"] : (string)Application.Current.Resources["no_device_information_systemInformatin"] },
                 new DataSystemModel { ItemName = "Cpu", Data = !string.IsNullOrEmpty(SystemData.СomputerСonfiguration.СonfigurationData["CPU"]) ? SystemData.СomputerСonfiguration.СonfigurationData["CPU"] : (string)Application.Current.Resources["no_device_information_systemInformatin"] },
                 new DataSystemModel { ItemName = "Gpu", Data = !string.IsNullOrEmpty(SystemData.СomputerСonfiguration.СonfigurationData["GPU"]) ? SystemData.СomputerСonfiguration.СonfigurationData["GPU"] : (string)Application.Current.Resources["driver_not_installed_systemInformatin"] },
                 new DataSystemModel { ItemName = "Ram", Data = !string.IsNullOrEmpty(SystemData.СomputerСonfiguration.СonfigurationData["RAM"]) ? SystemData.СomputerСonfiguration.СonfigurationData["RAM"] : (string)Application.Current.Resources["no_device_information_systemInformatin"] },
                 new DataSystemModel { ItemName = "Storage", Data =  !string.IsNullOrEmpty(SystemData.СomputerСonfiguration.СonfigurationData["Storage"]) ? SystemData.СomputerСonfiguration.СonfigurationData["Storage"] : (string)Application.Current.Resources["no_device_information_systemInformatin"] },
                 new DataSystemModel { ItemName = "Audio", Data = !string.IsNullOrEmpty(SystemData.СomputerСonfiguration.СonfigurationData["Audio"]) ? SystemData.СomputerСonfiguration.СonfigurationData["Audio"] : (string)Application.Current.Resources["driver_not_installed_systemInformatin"] },
                 new DataSystemModel { ItemName = "Network", Data = !string.IsNullOrEmpty(SystemData.СomputerСonfiguration.СonfigurationData["NetAdapter"]) ? SystemData.СomputerСonfiguration.СonfigurationData["NetAdapter"] : (string)Application.Current.Resources["driver_not_installed_systemInformatin"] },
                 new DataSystemModel { ItemName = "Ip", Data = SystemData.СomputerСonfiguration.СonfigurationData["UserIpAddress"] }
            };

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
