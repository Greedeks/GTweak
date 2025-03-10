using GTweak.Utilities.Configuration;
using GTweak.Utilities.Tweaks;

namespace GTweak.Core.ViewModel
{
    internal class InterfaceVM : ViewModelBase
    {
        public bool IsBlockForWin10 => SystemDiagnostics.IsWindowsVersion[11];
        public bool IsBlockWithoutLicense => WindowsLicense.IsWindowsActivated;
    }
}
