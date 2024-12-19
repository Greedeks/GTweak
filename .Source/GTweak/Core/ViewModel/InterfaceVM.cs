using GTweak.Utilities.Tweaks;

namespace GTweak.Core.ViewModel
{
    internal class InterfaceVM : ViewModelBase
    {
        public bool IsBlockForWin10 => SystemСonfiguration.WindowsClientVersion.Contains("11");
        public bool IsBlockWithoutLicense => WindowsLicense.IsWindowsActivated;
    }
}
