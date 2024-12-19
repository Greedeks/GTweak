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

        public DataSystemModel this[string name] => DisplayData.FirstOrDefault(d => d.Name == name);

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
                 new DataSystemModel { Name = "Windows", Data = SystemСonfiguration.СonfigurationData["Windows"] },
                 new DataSystemModel { Name = "Processes", Data = new MonitoringSystem().GetNumberRunningProcesses },
                 new DataSystemModel { Name = "Bios", Data = !string.IsNullOrEmpty(SystemСonfiguration.СonfigurationData["BIOS"]) ? SystemСonfiguration.СonfigurationData["BIOS"] : (string)Application.Current.Resources["no_device_information_systemInformatin"] },
                 new DataSystemModel { Name = "Motherboard", Data =  !string.IsNullOrEmpty(SystemСonfiguration.СonfigurationData["MotherBr"]) ? SystemСonfiguration.СonfigurationData["MotherBr"] : (string)Application.Current.Resources["no_device_information_systemInformatin"] },
                 new DataSystemModel { Name = "Processor", Data = !string.IsNullOrEmpty(SystemСonfiguration.СonfigurationData["CPU"]) ? SystemСonfiguration.СonfigurationData["CPU"] : (string)Application.Current.Resources["no_device_information_systemInformatin"] },
                 new DataSystemModel { Name = "Graphics", Data = !string.IsNullOrEmpty(SystemСonfiguration.СonfigurationData["GPU"]) ? SystemСonfiguration.СonfigurationData["GPU"] : (string)Application.Current.Resources["driver_not_installed_systemInformatin"] },
                 new DataSystemModel { Name = "Memory", Data = !string.IsNullOrEmpty(SystemСonfiguration.СonfigurationData["RAM"]) ? SystemСonfiguration.СonfigurationData["RAM"] : (string)Application.Current.Resources["no_device_information_systemInformatin"] },
                 new DataSystemModel { Name = "Storage", Data =  !string.IsNullOrEmpty(SystemСonfiguration.СonfigurationData["Storage"]) ? SystemСonfiguration.СonfigurationData["Storage"] : (string)Application.Current.Resources["no_device_information_systemInformatin"] },
                 new DataSystemModel { Name = "Audio", Data = !string.IsNullOrEmpty(SystemСonfiguration.СonfigurationData["Audio"]) ? SystemСonfiguration.СonfigurationData["Audio"] : (string)Application.Current.Resources["driver_not_installed_systemInformatin"] },
                 new DataSystemModel { Name = "Network", Data = !string.IsNullOrEmpty(SystemСonfiguration.СonfigurationData["NetAdapter"]) ? SystemСonfiguration.СonfigurationData["NetAdapter"] : (string)Application.Current.Resources["driver_not_installed_systemInformatin"] },
                 new DataSystemModel { Name = "IpAddress", Data = SystemСonfiguration.СonfigurationData["UserIpAddress"] }
            };

            if (Settings.IsHiddenIpAddress & SystemСonfiguration.CurrentConnection == SystemСonfiguration.ConnectionStatus.Available)
            {
                SetBlurValue = 20;
                SetVisibility = Visibility.Visible;
            }
            else
            {
                SetBlurValue = 0;
                SetVisibility = SystemСonfiguration.CurrentConnection == SystemСonfiguration.ConnectionStatus.Available ? Visibility.Visible : Visibility.Hidden;
            }
        }
    }
}
