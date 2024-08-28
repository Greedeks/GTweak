﻿using GTweak.Core.Model;
using GTweak.Utilities;
using GTweak.Utilities.Tweaks;
using System.Windows;
using System.Windows.Media;

namespace GTweak.Core.ViewModel
{
    internal class ConfidentialityVM : ViewModelBase { }
    internal class InterfaceVM : ViewModelBase
    {
        private readonly MainModel.InterfaceModel _model;
        public bool IsBlockForWin10
        {
            get => _model.IsBlockWin10;
            set { _model.IsBlockWin10 = value; OnPropertyChanged(); }
        }

        public bool IsBlockWithoutLicense
        {
            get => _model.IsBlockLicense;
            set { _model.IsBlockLicense = value; OnPropertyChanged(); }
        }

        public InterfaceVM()
        {
            _model = new MainModel.InterfaceModel();
            IsBlockForWin10 = SystemData.СomputerСonfiguration.clientWinVersion.Contains("11");
            IsBlockWithoutLicense = WindowsLicense.statusLicense == 1;
        }
    }

    internal class ApplicationsVM : ViewModelBase { }
    internal class ServicesVM : ViewModelBase { }
    internal class SystemVM : ViewModelBase { }
    internal class MoreVM : ViewModelBase { }

    internal class InformationSystemVM : ViewModelBase
    {
        private readonly MainModel.InformationSystemModel _model;
        public string DisplayWindowsName
        {
            get => _model.WindowsName;
            set { _model.WindowsName = value; OnPropertyChanged(); }
        }

        public string DisplayBiosName
        {
            get => _model.BIOSName;
            set { _model.BIOSName = value; OnPropertyChanged(); }
        }

        public string DisplayMotherBrName
        {
            get => _model.MotherBrName;
            set { _model.MotherBrName = value; OnPropertyChanged(); }
        }

        public string DisplayCpuName
        {
            get => _model.CPUName;
            set { _model.CPUName = value; OnPropertyChanged(); }
        }

        public string DisplayGpuName
        {
            get => _model.GPUName;
            set { _model.GPUName = value; OnPropertyChanged(); }
        }

        public string DisplayRamName
        {
            get => _model.RAMName;
            set { _model.RAMName = value; OnPropertyChanged(); }
        }

        public string DisplayDiskName
        {
            get => _model.DiskName;
            set { _model.DiskName = value; OnPropertyChanged(); }
        }

        public string DisplaySoundName
        {
            get => _model.SoundName;
            set { _model.SoundName = value; OnPropertyChanged(); }
        }

        public string DisplayNetAdapterName
        {
            get => _model.NetAdapterName;
            set { _model.NetAdapterName = value; OnPropertyChanged(); }
        }

        public string DisplayIpAddress
        {
            get => _model.IpAddress;
            set { _model.IpAddress = value; OnPropertyChanged(); }
        }

        public string DisplayCountProcess
        {
            get => _model.CountProcess;
            set { _model.CountProcess = value; OnPropertyChanged(); }
        }

        public int SetBlurValue
        {
            get => _model.BlurValue;
            set { _model.BlurValue = value; OnPropertyChanged(); }
        }

        public DrawingImage DisplayImageHidden
        {
            get => _model.ImageHidden;
            set { _model.ImageHidden = value; OnPropertyChanged(); }
        }


        public InformationSystemVM()
        {
            _model = new MainModel.InformationSystemModel();

            DisplayWindowsName = SystemData.СomputerСonfiguration.СonfigurationData["Windows"];
            DisplayBiosName = SystemData.СomputerСonfiguration.СonfigurationData["BIOS"];
            DisplayMotherBrName = SystemData.СomputerСonfiguration.СonfigurationData["MotherBr"];
            DisplayCpuName = SystemData.СomputerСonfiguration.СonfigurationData["CPU"];

            if (!string.IsNullOrEmpty(SystemData.СomputerСonfiguration.СonfigurationData["GPU"]))
                DisplayGpuName = SystemData.СomputerСonfiguration.СonfigurationData["GPU"];
            else
                DisplayGpuName = (string)Application.Current.Resources["driver_not_installed_systemInformatin"];

            DisplayRamName = SystemData.СomputerСonfiguration.СonfigurationData["RAM"];
            DisplayDiskName = SystemData.СomputerСonfiguration.СonfigurationData["Disk"];

            if (!string.IsNullOrEmpty(SystemData.СomputerСonfiguration.СonfigurationData["Sound"]))
                DisplaySoundName = SystemData.СomputerСonfiguration.СonfigurationData["Sound"];
            else
                DisplaySoundName = (string)Application.Current.Resources["driver_not_installed_systemInformatin"];

            if (!string.IsNullOrEmpty(SystemData.СomputerСonfiguration.СonfigurationData["NetAdapter"]))
                DisplayNetAdapterName = SystemData.СomputerСonfiguration.СonfigurationData["NetAdapter"];
            else
                DisplayNetAdapterName = (string)Application.Current.Resources["driver_not_installed_systemInformatin"];

            DisplayIpAddress = SystemData.СomputerСonfiguration.СonfigurationData["IpAddress"];
            DisplayCountProcess = new SystemData.MonitoringSystem().CountProcess.ToString();

            if (Settings.IsHiddenIpAddress & !SystemData.СomputerСonfiguration.isNoInternetConnection & !SystemData.СomputerСonfiguration.isInternetLimited)
            {
                SetBlurValue = 15;
                DisplayImageHidden = (DrawingImage)Application.Current.Resources["DI_Show"];
            }
            else
            {
                SetBlurValue = 0;
                DisplayImageHidden = (DrawingImage)Application.Current.Resources["DI_Hide"];
            }
        }
    }
}
