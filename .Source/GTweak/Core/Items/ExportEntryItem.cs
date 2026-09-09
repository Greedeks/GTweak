using System.Windows.Input;
using GTweak.Core.Base;

namespace GTweak.Core.Items
{
    internal sealed class ExportEntryItem : PropertyChangedBase
    {
        private string _label;
        private object _value;

        public string Key { get; }
        public string Label
        {
            get => _label;
            set { _label = value; OnPropertyChanged(); }
        }

        public object Value
        {
            get => _value;
            set { _value = value; OnPropertyChanged(); }
        }

        public ICommand RemoveCommand { get; set; }

        public ExportEntryItem(string key, string label, object value)
        {
            Key = key;
            Label = label;
            Value = value;
        }
    }
}
