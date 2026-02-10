using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
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
        private readonly SystemDataCollector _systemDataCollector  = new SystemDataCollector();

        public object CurrentView
        {
            get => _model.CurrentView;
            set { _model.CurrentView = value; OnPropertyChanged(); }
        }

        public ImageSource DisplayProfileAvatar => _systemDataCollector.GetProfileImage();

        public string DisplayProfileName => _systemDataCollector.GetProfileName();

        public string DisplayTweakVersion => (Assembly.GetEntryAssembly() ?? throw new InvalidOperationException()).GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

        public bool StateButtonTheme
        {
            get => !string.Equals(SettingsEngine.Theme, SettingsEngine.AvailableThemes.First(), StringComparison.OrdinalIgnoreCase);
            set
            {
                App.Theme = SettingsEngine.Theme = value == false ? SettingsEngine.AvailableThemes.First() : SettingsEngine.AvailableThemes.Last();
                OnPropertyChanged();
            }
        }

        public string CurrentVersion => SettingsEngine.currentRelease;

        public string DownloadVersion => SystemDataCollector.DownloadVersion;

        public bool IsViewNotification => SettingsEngine.IsViewNotification;

        public bool IsUpdateCheckRequired => SettingsEngine.IsUpdateCheckRequired;

        public bool IsTopMost => SettingsEngine.IsTopMost;

        public bool IsPlayingSound => SettingsEngine.IsPlayingSound;

        public int CurrentVolume => SettingsEngine.Volume;

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

        private void NavigateToUtils(object obj) => CurrentView = new UtilsView();
        private void NavigateToConfidentiality(object obj) => CurrentView = new ConfidentialityViewModel();
        private void NavigateToInterface(object obj) => CurrentView = new InterfaceViewModel();
        private void NavigateToPackages(object obj) => CurrentView = new PakagesViewModel();
        private void NavigateToServices(object obj) => CurrentView = new ServicesViewModel();
        private void NavigateToSystem(object obj) => CurrentView = new SystemViewModel();
        private void NavigateToDataSystem(object obj) => CurrentView = new DataSystemViewModel();
        private void NavigateToAddons(object obj) => CurrentView = new AddonsViewModel();

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
