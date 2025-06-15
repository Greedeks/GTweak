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
            get => SettingsEngine.IsHiddenIpAddress;
            set
            {
                if (SettingsEngine.IsHiddenIpAddress != value)
                {
                    SettingsEngine.IsHiddenIpAddress = value;
                    OnPropertyChanged();
                }
            }
        }

        public DataSystemVM()
        {
            _model = new DataSystemModel();

            DisplayData = new ObservableCollection<DataSystemModel>
            {
                 new DataSystemModel { Name = "Windows", Data = SystemDiagnostics.HardwareData.OperatingSystem },
                 new DataSystemModel { Name = "Processes", Data =  new MonitoringSystem().GetNumberRunningProcesses },
                 new DataSystemModel { Name = "Bios", Data = !string.IsNullOrEmpty(SystemDiagnostics.HardwareData.Bios) ? SystemDiagnostics.HardwareData.Bios : (string)Application.Current.Resources["no_device_information_systemInformatin"] },
                 new DataSystemModel { Name = "Mode", Data = !string.IsNullOrEmpty(SystemDiagnostics.HardwareData.BiosMode) ? SystemDiagnostics.HardwareData.BiosMode : (string)Application.Current.Resources["no_device_information_systemInformatin"] },
                 new DataSystemModel { Name = "Motherboard", Data =  !string.IsNullOrEmpty(SystemDiagnostics.HardwareData.Motherboard) ? SystemDiagnostics.HardwareData.Motherboard : (string)Application.Current.Resources["no_device_information_systemInformatin"] },
                 new DataSystemModel { Name = "Processor", Data = !string.IsNullOrEmpty(SystemDiagnostics.HardwareData.Processor) ? SystemDiagnostics.HardwareData.Processor : (string)Application.Current.Resources["no_device_information_systemInformatin"] },
                 new DataSystemModel { Name = "Graphics", Data = !string.IsNullOrEmpty(SystemDiagnostics.HardwareData.Graphics) ? SystemDiagnostics.HardwareData.Graphics : (string)Application.Current.Resources["driver_not_installed_systemInformatin"] },
                 new DataSystemModel { Name = "Memory", Data = !string.IsNullOrEmpty(SystemDiagnostics.HardwareData.Memory) ? SystemDiagnostics.HardwareData.Memory : (string)Application.Current.Resources["no_device_information_systemInformatin"] },
                 new DataSystemModel { Name = "Storage", Data =  !string.IsNullOrEmpty(SystemDiagnostics.HardwareData.Storage) ? SystemDiagnostics.HardwareData.Storage : (string)Application.Current.Resources["no_device_information_systemInformatin"] },
                 new DataSystemModel { Name = "Audio", Data = !string.IsNullOrEmpty(SystemDiagnostics.HardwareData.AudioDevice) ? SystemDiagnostics.HardwareData.AudioDevice : (string)Application.Current.Resources["driver_not_installed_systemInformatin"] },
                 new DataSystemModel { Name = "Network", Data = !string.IsNullOrEmpty(SystemDiagnostics.HardwareData.NetworkAdapter) ? SystemDiagnostics.HardwareData.NetworkAdapter : (string)Application.Current.Resources["driver_not_installed_systemInformatin"] },
                 new DataSystemModel { Name = "IpAddress", Data = SystemDiagnostics.HardwareData.UserIPAddress }
            };

            SetBlurValue = (SystemDiagnostics.isIPAddressFormatValid && SettingsEngine.IsHiddenIpAddress) ? 20 : 0;
            SetVisibility = SystemDiagnostics.isIPAddressFormatValid ? Visibility.Visible : Visibility.Hidden;
        }
    }
}
