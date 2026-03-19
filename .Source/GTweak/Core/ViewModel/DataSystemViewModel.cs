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
        private static class FallbackKeys
        {
            internal const string Unknown = "unknown_information_sysinfo";
            internal const string NoDevice = "no_device_information_sysinfo";
            internal const string NoDriver = "driver_not_installed_sysinfo";
            internal const string ConnectionLost = "connection_lose_sysinfo";
        }

        private readonly DataSystemModel _model = new DataSystemModel();
        private ObservableCollection<DataSystemModel> _collection;

        public ObservableCollection<DataSystemModel> DisplayData
        {
            get => _collection;
            set { _collection = value; OnPropertyChanged(); }
        }

        public DataSystemModel this[string name] => DisplayData?.FirstOrDefault(d => d.Name == name);

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
                UpsertModelCollection("OSName", HardwareData.OS.Name, FallbackKeys.Unknown),
                UpsertModelCollection("OSVersion", HardwareData.OS.Version, FallbackKeys.Unknown),
                UpsertModelCollection("Processes", HardwareData.RunningProcessesCount),
                UpsertModelCollection("Services", HardwareData.RunningServicesCount),
                UpsertModelCollection("Bios", HardwareData.Bios.Data, FallbackKeys.NoDevice),
                UpsertModelCollection("Mode", HardwareData.Bios.Mode, FallbackKeys.Unknown),
                UpsertModelCollection("Motherboard", HardwareData.Motherboard, FallbackKeys.NoDevice),
                UpsertModelCollection("Processor", HardwareData.Processor.Data, FallbackKeys.NoDevice),
                UpsertModelCollection("Cores", HardwareData.Processor.Cores, FallbackKeys.Unknown),
                UpsertModelCollection("Threads", HardwareData.Processor.Threads, FallbackKeys.Unknown),
                UpsertModelCollection("Graphics", HardwareData.Graphics, FallbackKeys.NoDriver),
                UpsertModelCollection("RefreshRate", HardwareData.MonitorRefreshRate, FallbackKeys.Unknown),
                UpsertModelCollection("Memory", HardwareData.Memory.Data, FallbackKeys.NoDevice),
                UpsertModelCollection("Type", HardwareData.Memory.Type, FallbackKeys.Unknown),
                UpsertModelCollection("Storage", HardwareData.Storage, FallbackKeys.NoDevice),
                UpsertModelCollection("Audio", HardwareData.AudioDevice, FallbackKeys.NoDriver),
                UpsertModelCollection("Network", HardwareData.NetworkAdapter, FallbackKeys.NoDriver),
                UpsertModelCollection("IpAddress", HardwareData.UserIPAddress, FallbackKeys.ConnectionLost)
            };

            RefreshStates();
        }

        internal void Update()
        {
            UpsertModelCollection("RefreshRate", HardwareData.MonitorRefreshRate, FallbackKeys.Unknown);
            UpsertModelCollection("Storage", HardwareData.Storage, FallbackKeys.NoDevice);
            UpsertModelCollection("Audio", HardwareData.AudioDevice, FallbackKeys.NoDriver);
            UpsertModelCollection("Network", HardwareData.NetworkAdapter, FallbackKeys.NoDriver);
            UpsertModelCollection("IpAddress", HardwareData.UserIPAddress, FallbackKeys.ConnectionLost);

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

        private DataSystemModel UpsertModelCollection(string name, string data, string fallbackKey = null)
        {
            string targetData = (fallbackKey == null) ? (data ?? string.Empty) : (!string.IsNullOrEmpty(data) ? data : (Application.Current.Resources[fallbackKey] as string ?? string.Empty));
            DataSystemModel model = this[name];

            if (model != null)
            {
                model.Data = targetData;
                return model;
            }

            return new DataSystemModel { Name = name, Data = targetData };
        }
    }
}
