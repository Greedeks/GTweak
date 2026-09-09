using System.Windows.Input;

namespace GTweak.Core.Items
{
    internal sealed class ExportCheckItem
    {
        public string Label { get; }
        public bool IsChecked { get; }
        public ICommand RemoveCommand { get; set; }

        public ExportCheckItem(string label, bool isChecked)
        {
            Label = label;
            IsChecked = isChecked;
        }
    }
}
