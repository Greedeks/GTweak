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
                CreateModelCollection("Windows", SystemDiagnostics.HardwareData.OperatingSystem),
                CreateModelCollection("Processes", new MonitoringService().GetNumberRunningProcesses),
                CreateModelCollection("Services", new MonitoringService().GetNumberRunningService),
                CreateModelCollection("Bios", SystemDiagnostics.HardwareData.Bios, "no_device_information_systemInformatin"),
                CreateModelCollection("Mode", SystemDiagnostics.HardwareData.BiosMode, "unknown_information_systemInformatin"),
                CreateModelCollection("Motherboard", SystemDiagnostics.HardwareData.Motherboard, "no_device_information_systemInformatin"),
                CreateModelCollection("Processor", SystemDiagnostics.HardwareData.Processor, "no_device_information_systemInformatin"),
                CreateModelCollection("Cores", SystemDiagnostics.HardwareData.Cores, "unknown_information_systemInformatin"),
                CreateModelCollection("Threads", SystemDiagnostics.HardwareData.Threads, "unknown_information_systemInformatin"),
                CreateModelCollection("Graphics", SystemDiagnostics.HardwareData.Graphics, "driver_not_installed_systemInformatin"),
                CreateModelCollection("Memory", SystemDiagnostics.HardwareData.Memory, "no_device_information_systemInformatin"),
                CreateModelCollection("Type", SystemDiagnostics.HardwareData.MemoryType, "unknown_information_systemInformatin"),
                CreateModelCollection("Storage", SystemDiagnostics.HardwareData.Storage, "no_device_information_systemInformatin"),
                CreateModelCollection("Audio", SystemDiagnostics.HardwareData.AudioDevice, "driver_not_installed_systemInformatin"),
                CreateModelCollection("Network", SystemDiagnostics.HardwareData.NetworkAdapter, "driver_not_installed_systemInformatin"),
                CreateModelCollection("IpAddress", SystemDiagnostics.HardwareData.UserIPAddress)
            };

            SetBlurValue = (SystemDiagnostics.isIPAddressFormatValid && SettingsEngine.IsHiddenIpAddress) ? 20 : 0;
            SetVisibility = SystemDiagnostics.isIPAddressFormatValid ? Visibility.Visible : Visibility.Hidden;
        }

        private DataSystemModel CreateModelCollection(string name, object data, string fallbackResourceKey = null)
        {
            return new DataSystemModel
            {
                Name = name,
                Data = (string)(fallbackResourceKey == null ? data : (!string.IsNullOrEmpty(data as string) ? data : (string)Application.Current.Resources[fallbackResourceKey]))
            };
        }
    }
}
