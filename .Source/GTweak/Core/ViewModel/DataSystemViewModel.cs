using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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
        }

        private readonly DataSystemModel _model = new DataSystemModel();
        private ObservableCollection<DataSystemModel> _collection;
        private readonly List<Action> _modelUpdateActions = new List<Action>();
        private readonly List<Action> _fallbackRefresh = new List<Action>();
        private readonly DataSystemModel _ipAddressModel;

        public ObservableCollection<DataSystemModel> DisplayData
        {
            get => _collection;
            set { _collection = value; OnPropertyChanged(); }
        }

        public DataSystemModel this[string name] => DisplayData?.FirstOrDefault(d => d.Name == name);

        public ImageSource DisplayWallpaper => HardwareData.Wallpaper;

        public int IpBlurRadius
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

        public Visibility IpSectionVisibility
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

        public bool IsIpHidden
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
                CreateModelCollection("CPUAngle", () => HardwareData.Processor.Usage.ToString(), isUpdatable: true),
                CreateModelCollection("RAMAngle", () => HardwareData.Memory.Usage.ToString(), isUpdatable: true),
                CreateModelCollection("Bios", () => HardwareData.Bios.Data, FallbackKeys.NoDevice),
                CreateModelCollection("BiosSerial", () => HardwareData.Bios.SerialNumber, FallbackKeys.Unknown),
                CreateModelCollection("Mode", () => HardwareData.Bios.Mode, FallbackKeys.Unknown),
                CreateModelCollection("Motherboard", () => HardwareData.Motherboard.Data, FallbackKeys.NoDevice),
                CreateModelCollection("MotherboardVersion", () => HardwareData.Motherboard.Version, FallbackKeys.Unknown),
                CreateModelCollection("MotherboardSerial", () => HardwareData.Motherboard.SerialNumber, FallbackKeys.Unknown),
                CreateModelCollection("Chipset", () => HardwareData.Motherboard.Chipset, FallbackKeys.Unknown),
                CreateModelCollection("Processor", () => HardwareData.Processor.Data, FallbackKeys.NoDevice),
                CreateModelCollection("Cores", () => HardwareData.Processor.Cores, FallbackKeys.Unknown),
                CreateModelCollection("Threads", () => HardwareData.Processor.Threads, FallbackKeys.Unknown),
                CreateModelCollection("Frequency", () => HardwareData.Processor.Frequency, FallbackKeys.Unknown),
                CreateListCollection("Graphics", () => HardwareData.Graphics.Select(g => new[] { g.Data, g.Memory }).ToList(), isUpdatable:false, FallbackKeys.NoDevice, FallbackKeys.Unknown),
                CreateModelCollection("RefreshRate", () => HardwareData.MonitorRefreshRate, FallbackKeys.Unknown, true),
                CreateListCollection("Memory", () => HardwareData.Memory.Modules.Select(m => new[] { m.Data, m.Frequency, m.Capacity }).ToList(), isUpdatable:false, FallbackKeys.NoDevice, FallbackKeys.Unknown, FallbackKeys.Unknown),
                CreateModelCollection("Type", () => HardwareData.Memory.Type, FallbackKeys.Unknown),
                CreateListCollection("Storage", () => HardwareData.Storage.Select(s => new[] { s.Data, s.Capacity, s.StorageType, s.UsedPercent.ToString(CultureInfo.InvariantCulture), s.FreeSpace, s.UsedSpace }).ToList(),true,FallbackKeys.NoDevice,string.Empty,FallbackKeys.Unknown, string.Empty,string.Empty, string.Empty),
                CreateListCollection("Audio", () => HardwareData.AudioDevices.Select(a => new[] { a.Data, a.IsCapture ? "1" : "0" }).ToList(),true, FallbackKeys.NoDriver),
                CreateListCollection("Network", () => HardwareData.NetworkAdapters.Select(n => new[] { n.Data, n.IsConnected ? "1" : "0" }).ToList(),true, FallbackKeys.NoDriver),
                CreateModelCollection("CountryCode", () => HardwareData.UserCountryCode, isUpdatable: true),
                (_ipAddressModel = CreateModelCollection("IpAddress", () => HardwareData.UserIPAddress, NetworkProvider.GetConnectionResourceKey(), true))
            };

            RefreshStates();
        }

        internal void RefreshFallback()
        {
            string key = NetworkProvider.GetConnectionResourceKey();
            if (key != null)
            {
                HardwareData.UserIPAddress = Application.Current.Resources[key] as string;
            }
            _fallbackRefresh.ForEach(action => action());
        }

        internal void UpdateModel() => _modelUpdateActions.ForEach(action => action());

        internal void RefreshStates()
        {
            if (NetworkProvider.isIPAddressFormatValid || (_ipAddressModel?.Data?.Any(char.IsDigit) == true))
            {
                IpBlurRadius = SettingsEngine.IsHiddenIpAddress ? 20 : 0;
                IpSectionVisibility = Visibility.Visible;
                _ipAddressModel.IsEnabled = true;
            }
            else
            {
                IpBlurRadius = 0;
                IpSectionVisibility = Visibility.Collapsed;
                _ipAddressModel.IsEnabled = false;
            }
        }

        private DataSystemModel CreateModelCollection(string name, Func<string> dataProvider, string fallbackKey = null, bool isUpdatable = false)
        {
            DataSystemModel model = new DataSystemModel { Name = name };

            void UpdateModelData()
            {
                string data = dataProvider();
                if (!string.IsNullOrEmpty(data))
                {
                    model.Data = data;
                    model.IsEnabled = true;
                }
                else
                {
                    model.Data = ResolveFallback(fallbackKey);
                    model.IsEnabled = false;
                }
            }

            UpdateModelData();

            if (isUpdatable)
            {
                _modelUpdateActions.Add(UpdateModelData);
            }

            _fallbackRefresh.Add(UpdateModelData);

            return model;
        }

        private DataSystemModel CreateListCollection(string name, Func<List<string[]>> listProvider, bool isUpdatable = false, params string[] fallbacks)
        {
            DataSystemModel model = new DataSystemModel { Name = name };

            void UpdateModelData()
            {
                List<string[]> list = listProvider();
                if (list != null && list.Count > 0)
                {
                    model.Items = list.Select(item => new DataSystemModel
                    {
                        DataItems = item,
                        IsEnabled = true
                    }).ToList();
                }
                else
                {
                    model.Items = new List<DataSystemModel>
                    {
                        new DataSystemModel
                        {
                            DataItems = fallbacks.Select(ResolveFallback).ToArray(),
                            IsEnabled = false
                        }
                    };
                }
            }

            UpdateModelData();

            if (isUpdatable)
            {
                _modelUpdateActions.Add(UpdateModelData);
            }

            _fallbackRefresh.Add(UpdateModelData);

            return model;
        }

        private string ResolveFallback(string fallbackKeyOrValue) => string.IsNullOrEmpty(fallbackKeyOrValue) ? string.Empty : (Application.Current.Resources[fallbackKeyOrValue] as string ?? fallbackKeyOrValue);
    }
}
