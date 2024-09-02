using GTweak.Core.Model;
using GTweak.Utilities.Tweaks;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace GTweak.Core.ViewModel
{
    internal class MainViewModel : ViewModelBase
    {
        private readonly MainModel.MainWindowModel _model;
        private object _currentView;

        public object CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(); }
        }

        public ImageSource DisplayProfileAvatar
        {
            get => _model.ProfileAvatar;
            set { _model.ProfileAvatar = value; OnPropertyChanged(); }
        }

        public string DisplayProfileName
        {
            get => _model.ProfileName;
            set { _model.ProfileName = value; OnPropertyChanged(); }
        }

        public string DisplayTweakVersion
        {
            get => _model.TweakVersion;
            set { _model.TweakVersion = value; OnPropertyChanged(); }
        }

        public ICommand ConfidentialityCommand { get; set; }
        public ICommand InterfaceCommand { get; set; }
        public ICommand ApplicationsCommand { get; set; }
        public ICommand ServicesCommand { get; set; }
        public ICommand SystemCommand { get; set; }
        public ICommand InformationSystemCommand { get; set; }
        public ICommand MoreCommand { get; set; }


        private void Confidentiality(object obj) => CurrentView = new ConfidentialityVM();
        private void Interface(object obj) => CurrentView = new InterfaceVM();
        private void Apps(object obj) => CurrentView = new ApplicationsVM();
        private void Services(object obj) => CurrentView = new ServicesVM();
        private void System(object obj) => CurrentView = new SystemVM();
        private void InformationSystem(object obj) => CurrentView = new InformationSystemVM();
        private void More(object obj) => CurrentView = new MoreVM();

        public MainViewModel()
        {
            _model = new MainModel.MainWindowModel();

            DisplayProfileAvatar = SystemData.ProfileData.GetProfileImage() ?? Application.Current.Resources["DI_AvatarProfile"] as DrawingImage;
            DisplayProfileName = SystemData.ProfileData.GetProfileName();
            DisplayTweakVersion = (Assembly.GetEntryAssembly() ?? throw new InvalidOperationException()).GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

            ConfidentialityCommand = new RelayCommand(Confidentiality);
            InterfaceCommand = new RelayCommand(Interface);
            ApplicationsCommand = new RelayCommand(Apps);
            ServicesCommand = new RelayCommand(Services);
            SystemCommand = new RelayCommand(System);
            InformationSystemCommand = new RelayCommand(InformationSystem);
            MoreCommand = new RelayCommand(More);

            CurrentView = new MoreVM();

            App.ImportTweaksUpdate += (s, e) => { CurrentView = new MoreVM(); };
        }
    }
}
