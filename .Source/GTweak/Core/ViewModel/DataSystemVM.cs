using GTweak.Core.Model;
using GTweak.Utilities.Configuration;
using GTweak.Utilities.Controls;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace GTweak.Core.ViewModel
{
    internal class DataSystemVM : ViewModelBase
    {
        private readonly DataSystemModel _model;
        private ObservableCollection<DataSystemModel> _collection;

        public ObservableCollection<DataSystemModel> DisplayData
        {
            get => _collection;
            set { _collection = value; OnPropertyChanged(); }
        }

        public DataSystemModel this[string name] => DisplayData.FirstOrDefault(d => d.Name == name);

        public int SetBlurValue
        {
            get => _model.BlurValue;
            set
            {
                if (_model.BlurValue != value)
                {
                    _model.BlurValue = value;
                    OnPropertyChanged();
                }
            }
        }

        public Visibility SetVisibility
        {
            get => _model.IpVisibility;
            set
            {
                if (_model.IpVisibility != value)
                {
                    _model.IpVisibility = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool StateButton
        {
            get => SettingsRepository.IsHiddenIpAddress;
            set
            {
                if (SettingsRepository.IsHiddenIpAddress != value)
                {
                    SettingsRepository.IsHiddenIpAddress = value;
                    OnPropertyChanged();
                }
            }
        }

        public DataSystemVM()
        {
            _model = new DataSystemModel();

            DisplayData = new ObservableCollection<DataSystemModel>
            {
                 new DataSystemModel { Name = "Windows", Data = SystemDiagnostics.HardwareData["Windows"] },
                 new DataSystemModel { Name = "Processes", Data =  new MonitoringSystem().GetNumberRunningProcesses },
                 new DataSystemModel { Name = "Bios", Data = !string.IsNullOrEmpty(SystemDiagnostics.HardwareData["BIOS"]) ? SystemDiagnostics.HardwareData["BIOS"] : (string)Application.Current.Resources["no_device_information_systemInformatin"] },
                 new DataSystemModel { Name = "Mode", Data = !string.IsNullOrEmpty(SystemDiagnostics.HardwareData["Mode"]) ? SystemDiagnostics.HardwareData["Mode"] : (string)Application.Current.Resources["no_device_information_systemInformatin"] },
                 new DataSystemModel { Name = "Motherboard", Data =  !string.IsNullOrEmpty(SystemDiagnostics.HardwareData["MotherBr"]) ? SystemDiagnostics.HardwareData["MotherBr"] : (string)Application.Current.Resources["no_device_information_systemInformatin"] },
                 new DataSystemModel { Name = "Processor", Data = !string.IsNullOrEmpty(SystemDiagnostics.HardwareData["CPU"]) ? SystemDiagnostics.HardwareData["CPU"] : (string)Application.Current.Resources["no_device_information_systemInformatin"] },
                 new DataSystemModel { Name = "Graphics", Data = !string.IsNullOrEmpty(SystemDiagnostics.HardwareData["GPU"]) ? SystemDiagnostics.HardwareData["GPU"] : (string)Application.Current.Resources["driver_not_installed_systemInformatin"] },
                 new DataSystemModel { Name = "Memory", Data = !string.IsNullOrEmpty(SystemDiagnostics.HardwareData["RAM"]) ? SystemDiagnostics.HardwareData["RAM"] : (string)Application.Current.Resources["no_device_information_systemInformatin"] },
                 new DataSystemModel { Name = "Storage", Data =  !string.IsNullOrEmpty(SystemDiagnostics.HardwareData["Storage"]) ? SystemDiagnostics.HardwareData["Storage"] : (string)Application.Current.Resources["no_device_information_systemInformatin"] },
                 new DataSystemModel { Name = "Audio", Data = !string.IsNullOrEmpty(SystemDiagnostics.HardwareData["Audio"]) ? SystemDiagnostics.HardwareData["Audio"] : (string)Application.Current.Resources["driver_not_installed_systemInformatin"] },
                 new DataSystemModel { Name = "Network", Data = !string.IsNullOrEmpty(SystemDiagnostics.HardwareData["NetAdapter"]) ? SystemDiagnostics.HardwareData["NetAdapter"] : (string)Application.Current.Resources["driver_not_installed_systemInformatin"] },
                 new DataSystemModel { Name = "IpAddress", Data = SystemDiagnostics.HardwareData["UserIpAddress"] }
            };

            SetBlurValue = (SystemDiagnostics.isIPAddressFormatValid && SettingsRepository.IsHiddenIpAddress) ? 20 : 0;
            SetVisibility = SystemDiagnostics.isIPAddressFormatValid ? Visibility.Visible : Visibility.Hidden;
        }
    }
}
