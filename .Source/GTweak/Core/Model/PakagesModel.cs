using GTweak.Core.Base;

namespace GTweak.Core.Model
{
    internal sealed class PakagesModel : ViewModelBase
    {
        private string _name;
        private bool _installed;
        private bool _isUnavailable;

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public bool Installed
        {
            get => _installed;
            set { _installed = value; OnPropertyChanged(); }
        }

        public bool IsUnavailable
        {
            get => _isUnavailable;
            set { _isUnavailable = value; OnPropertyChanged(); }
        }
    }
}
