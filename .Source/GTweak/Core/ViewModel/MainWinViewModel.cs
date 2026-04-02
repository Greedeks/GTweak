using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using GTweak.Core.Base;
using GTweak.Core.Model;
using GTweak.Utilities.Configuration;
using GTweak.Utilities.Controls;
using GTweak.View;

namespace GTweak.Core.ViewModel
{
    internal class MainWinViewModel : ViewModelBase
    {
        public ObservableCollection<MainWinModel.LanguageItem> Languages { get; } = new ObservableCollection<MainWinModel.LanguageItem>();

        private readonly MainWinModel _model = new MainWinModel();
        private readonly HardwareProvider _hardwareProvider = new HardwareProvider();

        public object CurrentView
        {
            get => _model.CurrentView;
            set { _model.CurrentView = value; OnPropertyChanged(); }
        }

        public ImageSource DisplayProfileAvatar => _hardwareProvider.GetProfileImage();

        public string DisplayProfileName => _hardwareProvider.GetProfileName();

        public string DisplayTweakVersion => SettingsEngine.CurrentRelease.Full;

        public string CurrentVersion => SettingsEngine.CurrentRelease.Short;

        public string DownloadVersion => NetworkProvider.DownloadVersion;

        public bool StateButtonTheme
        {
            get => !string.Equals(SettingsEngine.Theme, SettingsEngine.AvailableThemes.First(), StringComparison.OrdinalIgnoreCase);
            set
            {
                App.Theme = SettingsEngine.Theme = value == false ? SettingsEngine.AvailableThemes.First() : SettingsEngine.AvailableThemes.Last();
                OnPropertyChanged();
            }
        }

        public bool IsViewNotification
        {
            get => SettingsEngine.IsViewNotification;
            set
            {
                if (SettingsEngine.IsViewNotification != value)
                {
                    SettingsEngine.IsViewNotification = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsUpdateCheckRequired
        {
            get => SettingsEngine.IsUpdateCheckRequired;
            set
            {
                if (SettingsEngine.IsUpdateCheckRequired != value)
                {
                    SettingsEngine.IsUpdateCheckRequired = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsTopMost
        {
            get => SettingsEngine.IsTopMost;
            set
            {
                if (SettingsEngine.IsTopMost != value)
                {
                    SettingsEngine.IsTopMost = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsPlayingSound
        {
            get => SettingsEngine.IsPlayingSound;
            set
            {
                if (SettingsEngine.IsPlayingSound != value)
                {
                    SettingsEngine.IsPlayingSound = value;
                    OnPropertyChanged();
                }
            }
        }

        public int CurrentVolume
        {
            get => SettingsEngine.Volume;
            set
            {
                if (SettingsEngine.Volume != value)
                {
                    SettingsEngine.Volume = value;
                    SettingsEngine.waveOutSetVolume(IntPtr.Zero, ((uint)(double)(ushort.MaxValue / 100 * value) & 0x0000ffff) | ((uint)(double)(ushort.MaxValue / 100 * value) << 16));
                    OnPropertyChanged();
                }
            }
        }

        public string SelectedLanguage
        {
            get => _model.SelectedLanguage;
            set
            {
                if (_model.SelectedLanguage != value)
                {
                    _model.SelectedLanguage = value;
                    OnPropertyChanged();
                    SettingsEngine.Language = value;
                    App.Language = value;
                }
            }
        }

        public ICommand NavigateToUtilsCommand { get; set; }
        public ICommand NavigateToConfidentialityCommand { get; set; }
        public ICommand NavigateToInterfaceCommand { get; set; }
        public ICommand NavigateToPackagesCommand { get; set; }
        public ICommand NavigateToServicesCommand { get; set; }
        public ICommand NavigateToSystemCommand { get; set; }
        public ICommand NavigateToDataSystemCommand { get; set; }
        public ICommand NavigateToAddonsCommand { get; set; }
        public ICommand NavigateToToolsetCommand { get; set; }

        private void NavigateToUtils(object obj) => CurrentView = new UtilsView();
        private void NavigateToConfidentiality(object obj) => CurrentView = new ConfidentialityViewModel();
        private void NavigateToInterface(object obj) => CurrentView = new InterfaceViewModel();
        private void NavigateToPackages(object obj) => CurrentView = new PackagesViewModel();
        private void NavigateToServices(object obj) => CurrentView = new ServicesViewModel();
        private void NavigateToSystem(object obj) => CurrentView = new SystemViewModel();
        private void NavigateToDataSystem(object obj) => CurrentView = new DataSystemViewModel();
        private void NavigateToAddons(object obj) => CurrentView = new AddonsViewModel();
        private void NavigateToToolset(object obj) => CurrentView = new ToolsetViewModel();

        public MainWinViewModel()
        {
            App.TweaksImported += delegate { CurrentView = new UtilsView(); };

            CurrentView = new UtilsView();

            NavigateToUtilsCommand = new RelayCommand(NavigateToUtils);
            NavigateToConfidentialityCommand = new RelayCommand(NavigateToConfidentiality);
            NavigateToInterfaceCommand = new RelayCommand(NavigateToInterface);
            NavigateToPackagesCommand = new RelayCommand(NavigateToPackages);
            NavigateToServicesCommand = new RelayCommand(NavigateToServices);
            NavigateToSystemCommand = new RelayCommand(NavigateToSystem);
            NavigateToDataSystemCommand = new RelayCommand(NavigateToDataSystem);
            NavigateToAddonsCommand = new RelayCommand(NavigateToAddons);
            NavigateToToolsetCommand = new RelayCommand(NavigateToToolset);

            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => { FillLanguages(); }));
            }
        }

        private void FillLanguages()
        {
            Languages.Clear();
            ResourceDictionary dictionary = new ResourceDictionary { Source = new Uri($"Languages/LanguageCatalog.xaml", UriKind.Relative) };

            foreach (string code in SettingsEngine.AvailableLangs)
            {
                Languages.Add(new MainWinModel.LanguageItem { Code = code, Display = dictionary[$"{code.Replace("-", "_")}"] as string ?? code });
            }

            SelectedLanguage = SettingsEngine.AvailableLangs.Contains(SettingsEngine.Language) ? SettingsEngine.Language : SettingsEngine.AvailableLangs.FirstOrDefault();
        }
    }
}
