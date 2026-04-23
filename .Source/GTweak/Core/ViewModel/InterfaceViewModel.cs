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
        public IReadOnlyDictionary<string, ImageSource> Icons { get; }
        public Visibility Win11FeatureOnly => HardwareData.OS.IsWin11 ? Visibility.Visible : Visibility.Collapsed;
        public Visibility Win11FeatureAvailable => HardwareData.OS.IsWin11 && HardwareData.OS.Build.CompareTo(22621.2361m) >= 0 ? Visibility.Visible : Visibility.Collapsed;
        public bool IsBlockWithoutLicense => WinLicenseHandler.IsWindowsActivated;
        public Visibility OneDriveAvailable => UninstallingPakages.IsOneDriveInstalled ? Visibility.Visible : Visibility.Collapsed;

        protected override void Analyze(InterfaceTweaks tweaks) => tweaks?.AnalyzeAndUpdate();
        protected override IReadOnlyDictionary<string, object> GetControlStates() => InterfaceTweaks.ControlStates;

        public InterfaceViewModel()
        {
            Icons = new Dictionary<string, ImageSource>
            {
                { "Home", IconExtractProvider.GetStockIcon(IconExtractProvider.StockIconType.Home) },
                { "Gallery", IconExtractProvider.GetStockIcon(IconExtractProvider.StockIconType.Gallery) },
                { "OneDrive", IconExtractProvider.GetStockIcon(IconExtractProvider.StockIconType.OneDrive) },
                { "PC", IconExtractProvider.GetStockIcon(IconExtractProvider.StockIconType.PC) },
                { "Network", IconExtractProvider.GetStockIcon(IconExtractProvider.StockIconType.Network) },
                { "Trash", IconExtractProvider.GetStockIcon(IconExtractProvider.StockIconType.Trash) },
                { "Panel", IconExtractProvider.GetStockIcon(IconExtractProvider.StockIconType.Panel) },
                { "UserFile", IconExtractProvider.GetStockIcon(IconExtractProvider.StockIconType.UserFile) }
            };
        }
    }
}
