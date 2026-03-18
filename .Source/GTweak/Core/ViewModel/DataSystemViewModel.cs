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
        private enum FallbackState { None, Unknown, NoDevice, NoDriver, ConnectionLost }

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

        private string GetFallbackResourceString(FallbackState state)
        {
            string key = state switch
            {
                FallbackState.Unknown => "unknown_information_sysinfo",
                FallbackState.NoDevice => "no_device_information_sysinfo",
                FallbackState.NoDriver => "driver_not_installed_sysinfo",
                FallbackState.ConnectionLost => "connection_lose_sysinfo",
                _ => null
            };

            return key == null ? string.Empty : (string)Application.Current.Resources[key];
        }

        public DataSystemViewModel()
        {
            DisplayData = new ObservableCollection<DataSystemModel>
            {
                CreateModelCollection("OSName", HardwareData.OS.Name, FallbackState.Unknown),
                CreateModelCollection("OSVersion", HardwareData.OS.Version, FallbackState.Unknown),
                CreateModelCollection("Processes", HardwareData.RunningProcessesCount),
                CreateModelCollection("Services", HardwareData.RunningServicesCount),
                CreateModelCollection("Bios", HardwareData.Bios.Data, FallbackState.NoDevice),
                CreateModelCollection("Mode", HardwareData.Bios.Mode, FallbackState.Unknown),
                CreateModelCollection("Motherboard", HardwareData.Motherboard, FallbackState.NoDevice),
                CreateModelCollection("Processor", HardwareData.Processor.Data, FallbackState.NoDevice),
                CreateModelCollection("Cores", HardwareData.Processor.Cores, FallbackState.Unknown),
                CreateModelCollection("Threads", HardwareData.Processor.Threads, FallbackState.Unknown),
                CreateModelCollection("Graphics", HardwareData.Graphics, FallbackState.NoDriver),
                CreateModelCollection("RefreshRate", HardwareData.MonitorRefreshRate, FallbackState.Unknown),
                CreateModelCollection("Memory", HardwareData.Memory.Data, FallbackState.NoDevice),
                CreateModelCollection("Type", HardwareData.Memory.Type, FallbackState.Unknown),
                CreateModelCollection("Storage", HardwareData.Storage, FallbackState.NoDevice),
                CreateModelCollection("Audio", HardwareData.AudioDevice, FallbackState.NoDriver),
                CreateModelCollection("Network", HardwareData.NetworkAdapter, FallbackState.NoDriver),
                CreateModelCollection("IpAddress", HardwareData.UserIPAddress, FallbackState.ConnectionLost)
            };

            RefreshStates();
        }

        internal void Update()
        {
            UpdateModelData("RefreshRate", HardwareData.MonitorRefreshRate, FallbackState.Unknown);
            UpdateModelData("Storage", HardwareData.Storage, FallbackState.NoDevice);
            UpdateModelData("Audio", HardwareData.AudioDevice, FallbackState.NoDriver);
            UpdateModelData("Network", HardwareData.NetworkAdapter, FallbackState.NoDriver);
            UpdateModelData("IpAddress", HardwareData.UserIPAddress, FallbackState.ConnectionLost);

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

        private void UpdateModelData(string name, string newData, FallbackState fallbackState = FallbackState.None)
        {
            DataSystemModel model = this[name];
            if (model != null)
            {
                model.Data = (fallbackState == FallbackState.None) ? newData : (!string.IsNullOrEmpty(newData) ? newData : GetFallbackResourceString(fallbackState));
            }
        }

        private DataSystemModel CreateModelCollection(string name, string data, FallbackState fallbackState = FallbackState.None)
        {
            return new DataSystemModel
            {
                Name = name,
                Data = (fallbackState == FallbackState.None) ? data : (!string.IsNullOrEmpty(data) ? data : GetFallbackResourceString(fallbackState))
            };
        }
    }
}
