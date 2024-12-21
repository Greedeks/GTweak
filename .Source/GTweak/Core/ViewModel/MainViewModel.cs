using GTweak.Utilities.Configuration;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace GTweak.Core.ViewModel
{
    internal class MainViewModel : ViewModelBase
    {
        private object _currentView;

        public object CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(); }
        }

        public ImageSource DisplayProfileAvatar => SystemDiagnostics.GetProfileImage() ?? Application.Current.Resources["DI_AvatarProfile"] as DrawingImage;

        public string DisplayProfileName => SystemDiagnostics.GetProfileName();

        public string DisplayTweakVersion => (Assembly.GetEntryAssembly() ?? throw new InvalidOperationException()).GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

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
            ConfidentialityCommand = new RelayCommand(Confidentiality);
            InterfaceCommand = new RelayCommand(Interface);
            PakagesCommand = new RelayCommand(Pakages);
            ServicesCommand = new RelayCommand(Services);
            SystemCommand = new RelayCommand(System);
            DataSystemCommand = new RelayCommand(DataSystem);
            MoreCommand = new RelayCommand(More);

            CurrentView = new MoreVM();

            App.ImportTweaksUpdate += delegate { CurrentView = new MoreVM(); };
        }
    }
}
