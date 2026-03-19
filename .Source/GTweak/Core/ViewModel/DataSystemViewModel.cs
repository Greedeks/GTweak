using System;
using System.Collections.Generic;
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
        private readonly List<Action> _modelUpdateActions = new List<Action>();
        private readonly DataSystemModel _ipAddressModel;

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
                CreateModelCollection("OSName", () => HardwareData.OS.Name, FallbackKeys.Unknown),
                CreateModelCollection("OSVersion", () => HardwareData.OS.Version, FallbackKeys.Unknown),
                CreateModelCollection("Processes", () => HardwareData.RunningProcessesCount, isUpdatable: true),
                CreateModelCollection("Services", () => HardwareData.RunningServicesCount, isUpdatable: true),
                CreateModelCollection("Bios", () => HardwareData.Bios.Data, FallbackKeys.NoDevice),
                CreateModelCollection("Mode", () => HardwareData.Bios.Mode, FallbackKeys.Unknown),
                CreateModelCollection("Motherboard", () => HardwareData.Motherboard, FallbackKeys.NoDevice),
                CreateModelCollection("Processor", () => HardwareData.Processor.Data, FallbackKeys.NoDevice),
                CreateModelCollection("Cores", () => HardwareData.Processor.Cores, FallbackKeys.Unknown),
                CreateModelCollection("Threads", () => HardwareData.Processor.Threads, FallbackKeys.Unknown),
                CreateModelCollection("Graphics", () => HardwareData.Graphics, FallbackKeys.NoDevice),
                CreateModelCollection("RefreshRate", () => HardwareData.MonitorRefreshRate, FallbackKeys.Unknown, true),
                CreateModelCollection("Memory", () => HardwareData.Memory.Data, FallbackKeys.NoDevice),
                CreateModelCollection("Type", () => HardwareData.Memory.Type, FallbackKeys.Unknown),
                CreateModelCollection("Storage", () => HardwareData.Storage.Data, FallbackKeys.NoDevice, true),
                CreateModelCollection("UsedSpace", () => HardwareData.Storage.UsedSpace, FallbackKeys.Unknown, true),
                CreateModelCollection("FreeSpace", () => HardwareData.Storage.FreeSpace, FallbackKeys.Unknown, true),
                CreateModelCollection("Audio", () => HardwareData.AudioDevice, FallbackKeys.NoDriver, true),
                CreateModelCollection("Network", () => HardwareData.NetworkAdapter, FallbackKeys.NoDriver, true),
                (_ipAddressModel = CreateModelCollection("IpAddress", () => HardwareData.UserIPAddress, FallbackKeys.ConnectionLost, true))
            };
            RefreshStates();
        }

        internal void Update()
        {
            foreach (Action updateItemAction in _modelUpdateActions)
            {
                updateItemAction();
            }

            RefreshStates();
        }

        private void RefreshStates()
        {
            if (NetworkProvider.isIPAddressFormatValid || _ipAddressModel.Data.Any(char.IsDigit))
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

        private DataSystemModel CreateModelCollection(string name, Func<string> dataProvider, string fallbackKey = null, bool isUpdatable = false)
        {
            DataSystemModel model = new DataSystemModel { Name = name };

            void UpdateModelData()
            {
                string data = dataProvider();
                model.Data = fallbackKey == null ? (data ?? string.Empty) : (!string.IsNullOrEmpty(data) ? data : (Application.Current.Resources[fallbackKey] as string ?? string.Empty));
            }

            UpdateModelData();

            if (isUpdatable)
            {
                _modelUpdateActions.Add(UpdateModelData);
            }

            return model;
        }
    }
}
