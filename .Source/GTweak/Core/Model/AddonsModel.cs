using System.Windows.Input;
using System.Windows.Media;

namespace GTweak.Core.ViewModel
{
    public class AddonModel
    {
        public string FilePath { get; }
        public string FileName { get; }
        public ImageSource IconImage { get; }
        public ICommand RunCommand { get; }
        public bool RequiresElevation { get; }

        public AddonModel(string filePath, string fileName, ImageSource iconImage, ICommand runCommand, bool requiresElevation = false)
        {
            FilePath = filePath;
            FileName = fileName;
            IconImage = iconImage;
            RunCommand = runCommand;
            RequiresElevation = requiresElevation;
        }
    }
}