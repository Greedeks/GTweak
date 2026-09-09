using System.Windows.Input;
using System.Windows.Media;

namespace GTweak.Core.Items
{
    public sealed class ExportPackageItem
    {
        public string Name { get; }
        public ImageSource Icon { get; }
        public bool CanRemove { get; }
        public ICommand RemoveCommand { get; set; }

        public ExportPackageItem(string name, ImageSource icon, bool canRemove = true)
        {
            Name = name;
            Icon = icon;
            CanRemove = canRemove;
        }
    }
}
