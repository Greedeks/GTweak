using GTweak.Core.Model;
using GTweak.Utilities.Configuration;
using GTweak.Utilities.Controls;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace GTweak.Core.ViewModel
{
    internal class MainViewModel : ViewModelBase
    {
        public ObservableCollection<MainModel.LanguageItem> Languages { get; } = new ObservableCollection<MainModel.LanguageItem>();
        public ObservableCollection<MainModel.ThemeItem> Themes { get; } = new ObservableCollection<MainModel.ThemeItem>();

        private readonly MainModel _model = new MainModel();
        private readonly SystemDiagnostics _systemDiagnostics = new SystemDiagnostics();
        private bool _isThemeUpdateInternal = false;

        public object CurrentView
        {
            get => _model.CurrentView;
            set { _model.CurrentView = value; OnPropertyChanged(); }
        }

        public ImageSource DisplayProfileAvatar => _systemDiagnostics.GetProfileImage();

        public string DisplayProfileName => _systemDiagnostics.GetProfileName();

        public string DisplayTweakVersion => (Assembly.GetEntryAssembly() ?? throw new InvalidOperationException()).GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

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

        public string SelectedTheme
        {
            get => _model.SelectedTheme;
            set
            {
                if (_model.SelectedTheme != value)
                {
                    _model.SelectedTheme = value;
                    OnPropertyChanged();
                    if (!_isThemeUpdateInternal && _model.SelectedTheme != null)
                    {
                        SettingsEngine.Theme = value;
                        App.Theme = value;
                    }
                }
            }
        }

        public ICommand ConfidentialityCommand { get; set; }
        public ICommand InterfaceCommand { get; set; }
        public ICommand PakagesCommand { get; set; }
        public ICommand ServicesCommand { get; set; }
        public ICommand SystemCommand { get; set; }
        public ICommand DataSystemCommand { get; set; }
        public ICommand MoreCommand { get; set; }

        private void Confidentiality(object obj) => CurrentView = new ConfidentialityVM();
        private void Interface(object obj) => CurrentView = new InterfaceVM();
        private void Pakages(object obj) => CurrentView = new PakagesVM();
        private void Services(object obj) => CurrentView = new ServicesVM();
        private void System(object obj) => CurrentView = new SystemVM();
        private void DataSystem(object obj) => CurrentView = new DataSystemVM();
        private void More(object obj) => CurrentView = new MoreVM();

        public MainViewModel()
        {
            App.TweaksImported += delegate { CurrentView = new MoreVM(); };
            App.LanguageChanged += delegate { FillThemes(); };

            ConfidentialityCommand = new RelayCommand(Confidentiality);
            InterfaceCommand = new RelayCommand(Interface);
            PakagesCommand = new RelayCommand(Pakages);
            ServicesCommand = new RelayCommand(Services);
            SystemCommand = new RelayCommand(System);
            DataSystemCommand = new RelayCommand(DataSystem);
            MoreCommand = new RelayCommand(More);

            CurrentView = new MoreVM();

            FillLanguages();
            FillThemes();
        }

        private void FillLanguages()
        {
            Languages.Clear();
            ResourceDictionary dictionary = Application.Current.Resources.MergedDictionaries.FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("LanguageCatalog.xaml")) ?? Application.Current.Resources;

            foreach (string code in SettingsEngine.AvailableLangs)
            {
                string key = $"{code.Replace("-", "_")}_main";
                string display = dictionary[key] as string ?? code;
                Languages.Add(new MainModel.LanguageItem { Code = code, Display = display });
            }

            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                SelectedLanguage = SettingsEngine.AvailableLangs.Contains(SettingsEngine.Language) ? SettingsEngine.Language : SettingsEngine.AvailableLangs.FirstOrDefault();

        }

        private void FillThemes()
        {
            _isThemeUpdateInternal = true;
            try
            {
                Themes.Clear();

                foreach (string code in SettingsEngine.AvailableThemes)
                {
                    string key = $"cmbox_{code}_main";
                    string display = Application.Current.Resources[key] as string ?? code;
                    Themes.Add(new MainModel.ThemeItem { Code = code, Display = display });
                }

                SelectedTheme = SettingsEngine.AvailableThemes.Contains(SettingsEngine.Theme) ? SettingsEngine.Theme : SettingsEngine.AvailableThemes.FirstOrDefault();
            }
            finally { _isThemeUpdateInternal = false; }
        }
    }
}
