using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using GTweak.Core.Base;
using GTweak.Core.Model;
using GTweak.Utilities.Configuration;
using GTweak.Utilities.Maintenance;
using GTweak.Utilities.Tweaks;

namespace GTweak.Core.ViewModel
{
    internal class InterfaceViewModel : ViewModelPageBase<InterfaceModel, InterfaceTweaks>
    {
        public Visibility Win11FeatureOnly => HardwareData.OS.IsWin11 ? Visibility.Visible : Visibility.Collapsed;
        public Visibility Win11FeatureAvailable => HardwareData.OS.IsWin11 && HardwareData.OS.Build.CompareTo(22621.2361m) >= 0 ? Visibility.Visible : Visibility.Collapsed;
        public bool IsBlockWithoutLicense => WinLicenseHandler.IsWindowsActivated;
        public Visibility OneDriveAvailable => UninstallingPakages.IsOneDriveInstalled ? Visibility.Visible : Visibility.Collapsed;
        public ImageSource HomeIcon => IconExtractProvider.GetStockIcon(IconExtractProvider.StockIconType.Home);
        public ImageSource GalleryIcon => IconExtractProvider.GetStockIcon(IconExtractProvider.StockIconType.Gallery);
        public ImageSource OneDriveIcon => IconExtractProvider.GetStockIcon(IconExtractProvider.StockIconType.OneDrive);
        public ImageSource PCIcon => IconExtractProvider.GetStockIcon(IconExtractProvider.StockIconType.PC);
        public ImageSource NetworkIcon => IconExtractProvider.GetStockIcon(IconExtractProvider.StockIconType.Network);
        public ImageSource TrashIcon => IconExtractProvider.GetStockIcon(IconExtractProvider.StockIconType.Trash);
        public ImageSource PanelIcon => IconExtractProvider.GetStockIcon(IconExtractProvider.StockIconType.Panel);
        public ImageSource UserIcon => IconExtractProvider.GetStockIcon(IconExtractProvider.StockIconType.User);

        protected override IReadOnlyDictionary<string, object> GetControlStates() => InterfaceTweaks.ControlStates;

        protected override void Analyze(InterfaceTweaks tweaks) => tweaks?.AnalyzeAndUpdate();
    }
}
