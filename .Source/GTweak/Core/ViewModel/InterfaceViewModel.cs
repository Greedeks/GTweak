using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using GTweak.Core.Base;
using GTweak.Core.Model;
using GTweak.Modules.Configuration;
using GTweak.Modules.Tweaks;

namespace GTweak.Core.ViewModel
{
    internal class InterfaceViewModel : ViewModelPageBase<InterfaceModel, InterfaceTweaks>
    {
        public IReadOnlyDictionary<string, ImageSource> Icons { get; }
        public Visibility Win11FeatureOnly => HardwareData.OS.IsWin11 ? Visibility.Visible : Visibility.Collapsed;
        public Visibility Win11FeatureAvailable => HardwareData.OS.IsWin11 && HardwareData.OS.Build.CompareTo(22621.2361m) >= 0 ? Visibility.Visible : Visibility.Collapsed;
        public Visibility OneDriveAvailable => AppxPackageHandler.IsOneDriveInstalled ? Visibility.Visible : Visibility.Collapsed;

        protected override void Analyze(InterfaceTweaks tweaks) => tweaks?.CheckAll();
        protected override IReadOnlyDictionary<string, object> GetControlStates() => InterfaceTweaks.ControlStates;

        public InterfaceViewModel()
        {
            Icons = new Dictionary<string, ImageSource>
            {
                { "Home", IconProvider.GetStockIcon(IconProvider.StockIconType.Home) },
                { "Gallery", IconProvider.GetStockIcon(IconProvider.StockIconType.Gallery) },
                { "OneDrive", IconProvider.GetStockIcon(IconProvider.StockIconType.OneDrive) },
                { "PC", IconProvider.GetStockIcon(IconProvider.StockIconType.PC) },
                { "Network", IconProvider.GetStockIcon(IconProvider.StockIconType.Network) },
                { "Trash", IconProvider.GetStockIcon(IconProvider.StockIconType.Trash) },
                { "Panel", IconProvider.GetStockIcon(IconProvider.StockIconType.Panel) },
                { "UserFile", IconProvider.GetStockIcon(IconProvider.StockIconType.UserFile) },
                { "OneDriveFile", IconProvider.GetStockIcon(IconProvider.StockIconType.OneDrive, 64) },
                { "FolderObjects3D", IconProvider.GetStockIcon(IconProvider.StockIconType.FolderObjects3D, 40) },
                { "FolderDesktop", IconProvider.GetStockIcon(IconProvider.StockIconType.FolderDesktop, 40) },
                { "FolderDownloads", IconProvider.GetStockIcon(IconProvider.StockIconType.FolderDownloads, 40) },
                { "FolderDocuments", IconProvider.GetStockIcon(IconProvider.StockIconType.FolderDocuments, 40) },
                { "FolderPictures", IconProvider.GetStockIcon(IconProvider.StockIconType.FolderPictures, 40) },
                { "FolderMusic", IconProvider.GetStockIcon(IconProvider.StockIconType.FolderMusic, 40) },
                { "FolderVideo", IconProvider.GetStockIcon(IconProvider.StockIconType.FolderVideo, 40) }
            };
        }
    }
}
