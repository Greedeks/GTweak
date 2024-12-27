using GTweak.Core.Model;
using GTweak.Utilities.Configuration;
using GTweak.Utilities.Control;
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
            get => SettingsRepository.IsHiddenIpAddress;
            set { SettingsRepository.IsHiddenIpAddress = value; OnPropertyChanged(); }
        }

        public DataSystemVM()
        {
            model = new DataSystemModel();

            DisplayData = new ObservableCollection<DataSystemModel>
            {
                 new DataSystemModel { Name = "Windows", Data = SystemDiagnostics.СonfigurationData["Windows"] },
                 new DataSystemModel { Name = "Processes", Data =  new MonitoringSystem().GetNumberRunningProcesses },
                 new DataSystemModel { Name = "Bios", Data = !string.IsNullOrEmpty(SystemDiagnostics.СonfigurationData["BIOS"]) ? SystemDiagnostics.СonfigurationData["BIOS"] : (string)Application.Current.Resources["no_device_information_systemInformatin"] },
                 new DataSystemModel { Name = "Motherboard", Data =  !string.IsNullOrEmpty(SystemDiagnostics.СonfigurationData["MotherBr"]) ? SystemDiagnostics.СonfigurationData["MotherBr"] : (string)Application.Current.Resources["no_device_information_systemInformatin"] },
                 new DataSystemModel { Name = "Processor", Data = !string.IsNullOrEmpty(SystemDiagnostics.СonfigurationData["CPU"]) ? SystemDiagnostics.СonfigurationData["CPU"] : (string)Application.Current.Resources["no_device_information_systemInformatin"] },
                 new DataSystemModel { Name = "Graphics", Data = !string.IsNullOrEmpty(SystemDiagnostics.СonfigurationData["GPU"]) ? SystemDiagnostics.СonfigurationData["GPU"] : (string)Application.Current.Resources["driver_not_installed_systemInformatin"] },
                 new DataSystemModel { Name = "Memory", Data = !string.IsNullOrEmpty(SystemDiagnostics.СonfigurationData["RAM"]) ? SystemDiagnostics.СonfigurationData["RAM"] : (string)Application.Current.Resources["no_device_information_systemInformatin"] },
                 new DataSystemModel { Name = "Storage", Data =  !string.IsNullOrEmpty(SystemDiagnostics.СonfigurationData["Storage"]) ? SystemDiagnostics.СonfigurationData["Storage"] : (string)Application.Current.Resources["no_device_information_systemInformatin"] },
                 new DataSystemModel { Name = "Audio", Data = !string.IsNullOrEmpty(SystemDiagnostics.СonfigurationData["Audio"]) ? SystemDiagnostics.СonfigurationData["Audio"] : (string)Application.Current.Resources["driver_not_installed_systemInformatin"] },
                 new DataSystemModel { Name = "Network", Data = !string.IsNullOrEmpty(SystemDiagnostics.СonfigurationData["NetAdapter"]) ? SystemDiagnostics.СonfigurationData["NetAdapter"] : (string)Application.Current.Resources["driver_not_installed_systemInformatin"] },
                 new DataSystemModel { Name = "IpAddress", Data = SystemDiagnostics.СonfigurationData["UserIpAddress"] }
            };

            if (SystemDiagnostics.CurrentConnection == SystemDiagnostics.ConnectionStatus.Available)
            {
                SetBlurValue = SettingsRepository.IsHiddenIpAddress ? 20 : 0;
                SetVisibility = Visibility.Visible;
            }
            else
            {
                SetBlurValue = 0;
                SetVisibility = Visibility.Hidden;
            }
        }
    }
}
