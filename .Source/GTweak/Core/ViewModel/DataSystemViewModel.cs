using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using GTweak.Core.Base;
using GTweak.Utilities.Configuration;
using GTweak.Utilities.Controls;

namespace GTweak.Core.ViewModel
{
    internal class DataSystemViewModel : ViewModelBase
    {
        private readonly DataSystemModel _model = new DataSystemModel();
        private ObservableCollection<DataSystemModel> _collection;

        public ObservableCollection<DataSystemModel> DisplayData
        {
            get => _collection;
            set { _collection = value; OnPropertyChanged(); }
        }

        public DataSystemModel this[string name] => DisplayData.FirstOrDefault(d => d.Name == name);

        public ImageSource DisplayWallpaper => HardwareData.Wallpaper;

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

        public bool StateButtonVision
        {
            get => SettingsEngine.IsHiddenIpAddress;
            set
            {
                if (SettingsEngine.IsHiddenIpAddress != value)
                {
                    SettingsEngine.IsHiddenIpAddress = value;
                    OnPropertyChanged();
                    RefreshStates();
                }
            }
        }

        public DataSystemViewModel()
        {
            DisplayData = new ObservableCollection<DataSystemModel>
            {
                CreateModelCollection("OSName", HardwareData.OS.Name, "unknown_information_sysinfo"),
                CreateModelCollection("OSVersion", HardwareData.OS.Version, "unknown_information_sysinfo"),
                CreateModelCollection("Processes", HardwareData.RunningProcessesCount),
                CreateModelCollection("Services", HardwareData.RunningServicesCount),
                CreateModelCollection("Bios", HardwareData.Bios.Data, "no_device_information_sysinfo"),
                CreateModelCollection("Mode", HardwareData.Bios.Mode, "unknown_information_sysinfo"),
                CreateModelCollection("Motherboard", HardwareData.Motherboard, "no_device_information_sysinfo"),
                CreateModelCollection("Processor", HardwareData.Processor.Data, "no_device_information_sysinfo"),
                CreateModelCollection("Cores", HardwareData.Processor.Cores, "unknown_information_sysinfo"),
                CreateModelCollection("Threads", HardwareData.Processor.Threads, "unknown_information_sysinfo"),
                CreateModelCollection("Graphics", HardwareData.Graphics, "driver_not_installed_sysinfo"),
                CreateModelCollection("Memory", HardwareData.Memory.Data, "no_device_information_sysinfo"),
                CreateModelCollection("Type", HardwareData.Memory.Type, "unknown_information_sysinfo"),
                CreateModelCollection("Storage", HardwareData.Storage, "no_device_information_sysinfo"),
                CreateModelCollection("Audio", HardwareData.AudioDevice, "driver_not_installed_sysinfo"),
                CreateModelCollection("Network", HardwareData.NetworkAdapter, "driver_not_installed_sysinfo"),
                CreateModelCollection("IpAddress", HardwareData.UserIPAddress, "connection_lose_sysinfo")
            };

            RefreshStates();
        }

        public void Update()
        {
            UpdateModelData("Storage", HardwareData.Storage, "no_device_information_sysinfo");
            UpdateModelData("Audio", HardwareData.AudioDevice, "driver_not_installed_sysinfo");
            UpdateModelData("Network", HardwareData.NetworkAdapter, "driver_not_installed_sysinfo");
            UpdateModelData("IpAddress", HardwareData.UserIPAddress, "connection_lose_sysinfo");

            RefreshStates();
        }

        private void RefreshStates()
        {
            DataSystemModel ipModel = this["IpAddress"];
            if (ipModel != null)
            {
                if (NetworkProvider.isIPAddressFormatValid || ipModel.Data.Any(char.IsDigit))
                {
                    SetBlurValue = SettingsEngine.IsHiddenIpAddress ? 20 : 0;
                    SetVisibility = Visibility.Visible;
                }
                else
                {
                    SetBlurValue = 0;
                    SetVisibility = Visibility.Hidden;
                }
            }
        }

        private void UpdateModelData(string name, string newData, string fallbackResourceKey)
        {
            DataSystemModel model = this[name];
            if (model != null)
            {
                model.Data = !string.IsNullOrEmpty(newData) ? newData : (string)Application.Current.Resources[fallbackResourceKey];
            }
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
