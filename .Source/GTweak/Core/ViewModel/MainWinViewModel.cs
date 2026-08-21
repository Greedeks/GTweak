using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using GTweak.Core.Base;
using GTweak.Core.Model;
using GTweak.Modules.Common;
using GTweak.Modules.Configuration;
using GTweak.View;

namespace GTweak.Core.ViewModel
{
    internal class MainWinViewModel : ViewModelBase
    {
        private readonly MainWinModel _model = new MainWinModel();
        private readonly HardwareProvider _hardwareProvider = new HardwareProvider();
        private readonly Dictionary<Type, object> _viewCache = new Dictionary<Type, object>();

        public ObservableCollection<MainWinModel.LanguageItem> Languages { get; } = new ObservableCollection<MainWinModel.LanguageItem>();

        public object CurrentView
        {
            get => _model.CurrentView;
            set { _model.CurrentView = value; OnPropertyChanged(); }
        }

        public ImageSource DisplayProfileAvatar => _hardwareProvider.GetProfileImage();

        public string DisplayProfileName => _hardwareProvider.GetProfileName();

        public string DisplayTweakVersion => GlobalOptions.CurrentRelease.Full;

        public string CurrentVersion => GlobalOptions.CurrentRelease.Short;

        public string DownloadVersion => NetworkProvider.DownloadVersion;

        public bool StateButtonTheme
        {
            get => !string.Equals(GlobalOptions.Theme, GlobalOptions.AvailableThemes.First(), StringComparison.OrdinalIgnoreCase);
            set
            {
                App.Theme = GlobalOptions.Theme = value == false ? GlobalOptions.AvailableThemes.First() : GlobalOptions.AvailableThemes.Last();
                OnPropertyChanged();
            }
        }

        public bool IsViewNotification
        {
            get => GlobalOptions.IsViewNotification;
            set
            {
                if (GlobalOptions.IsViewNotification != value)
                {
                    GlobalOptions.IsViewNotification = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsUpdateCheckRequired
        {
            get => GlobalOptions.IsUpdateCheckRequired;
            set
            {
                if (GlobalOptions.IsUpdateCheckRequired != value)
                {
                    GlobalOptions.IsUpdateCheckRequired = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsTopMost
        {
            get => GlobalOptions.IsTopMost;
            set
            {
                if (GlobalOptions.IsTopMost != value)
                {
                    GlobalOptions.IsTopMost = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsPlayingSound
        {
            get => GlobalOptions.IsPlayingSound;
            set
            {
                if (GlobalOptions.IsPlayingSound != value)
                {
                    GlobalOptions.IsPlayingSound = value;
                    OnPropertyChanged();
                }
            }
        }

        public int CurrentVolume
        {
            get => GlobalOptions.Volume;
            set
            {
                if (GlobalOptions.Volume != value)
                {
                    GlobalOptions.Volume = value;
                    GlobalOptions.waveOutSetVolume(IntPtr.Zero, ((uint)(double)(ushort.MaxValue / 100 * value) & 0x0000ffff) | ((uint)(double)(ushort.MaxValue / 100 * value) << 16));
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
                    GlobalOptions.Language = value;
                    App.Language = value;
                }
            }
        }

        public ICommand ExportConfigCommand { get; }
        public ICommand ImportConfigCommand { get; }
        public ICommand SelfRemovalCommand { get; }

        public ICommand NavigateToUtilsCommand { get; }
        public ICommand NavigateToConfidentialityCommand { get; }
        public ICommand NavigateToInterfaceCommand { get; }
        public ICommand NavigateToPackagesCommand { get; }
        public ICommand NavigateToServicesCommand { get; }
        public ICommand NavigateToSystemCommand { get; }
        public ICommand NavigateToDataSystemCommand { get; }
        public ICommand NavigateToAddonsCommand { get; }
        public ICommand NavigateToToolsetCommand { get; }

        public MainWinViewModel()
        {
            App.TweaksImported += delegate
            {
                CurrentView = GetView<UtilsView>();
            };

            CurrentView = GetView<UtilsView>();

            ExportConfigCommand = new RelayCommand(obj => GlobalOptions.SaveFileConfig());
            ImportConfigCommand = new RelayCommand(obj => GlobalOptions.OpenFileConfig());
            SelfRemovalCommand = new RelayCommand(obj => GlobalOptions.SelfRemoval());

            NavigateToUtilsCommand = new RelayCommand(_ => CurrentView = GetView<UtilsView>());
            NavigateToConfidentialityCommand = new RelayCommand(_ => CurrentView = GetView<ConfidentialityViewModel>());
            NavigateToInterfaceCommand = new RelayCommand(_ => CurrentView = GetView<InterfaceViewModel>());
            NavigateToPackagesCommand = new RelayCommand(_ => CurrentView = GetView<PackagesViewModel>());
            NavigateToServicesCommand = new RelayCommand(_ => CurrentView = GetView<ServicesViewModel>());
            NavigateToSystemCommand = new RelayCommand(_ => CurrentView = GetView<SystemViewModel>());
            NavigateToDataSystemCommand = new RelayCommand(_ => CurrentView = GetView<DataSystemViewModel>());
            NavigateToAddonsCommand = new RelayCommand(_ => CurrentView = GetView<AddonsViewModel>());
            NavigateToToolsetCommand = new RelayCommand(_ => CurrentView = GetView<ToolsetViewModel>());

            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => { FillLanguages(); }));
            }
        }

        private T GetView<T>() where T : new() => (T)(_viewCache.TryGetValue(typeof(T), out var v) ? v : _viewCache[typeof(T)] = new T());

        private void FillLanguages()
        {
            Languages.Clear();
            ResourceDictionary dictionary = new ResourceDictionary { Source = new Uri($"Languages/LanguageCatalog.xaml", UriKind.Relative) };

            foreach (string code in GlobalOptions.AvailableLangs)
            {
                Languages.Add(new MainWinModel.LanguageItem { Code = code, Display = dictionary[$"{code.Replace("-", "_")}"] as string ?? code });
            }

            SelectedLanguage = GlobalOptions.AvailableLangs.Contains(GlobalOptions.Language) ? GlobalOptions.Language : GlobalOptions.AvailableLangs.FirstOrDefault();
        }
    }
}
