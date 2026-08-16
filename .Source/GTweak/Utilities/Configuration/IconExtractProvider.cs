using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GTweak.Utilities.Configuration
{
    internal static class IconExtractProvider
    {
        internal enum StockIconType
        {
            Home,
            Gallery,
            OneDrive,
            PC,
            Network,
            Trash,
            Panel,
            UserFile,
            FolderObjects3D,
            FolderDesktop,
            FolderDownloads,
            FolderDocuments,
            FolderPictures,
            FolderMusic,
            FolderVideo
        }

        private static readonly ConcurrentDictionary<(string, int, int), ImageSource> _iconCache = new ConcurrentDictionary<(string, int, int), ImageSource>();

        [DllImport("Shell32.dll", EntryPoint = "SHDefExtractIconW", CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern int SHDefExtractIcon(string pszIconFile, int iIndex, uint uFlags, out IntPtr phiconLarge, out IntPtr phiconSmall, uint nIconSize);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr hIcon);

        internal static ImageSource GetStockIcon(StockIconType type, int size = 64)
        {
            (string file, int index) = type switch
            {
                StockIconType.Home => ("shell32.dll", -51380),
                StockIconType.Gallery => ("shell32.dll", -51586),
                StockIconType.OneDrive => ("imageres.dll", -1040),
                StockIconType.PC => ("imageres.dll", -109),
                StockIconType.Network => ("imageres.dll", -25),
                StockIconType.Trash => ("imageres.dll", -54),
                StockIconType.Panel => ("imageres.dll", -27),
                StockIconType.UserFile => ("imageres.dll", -123),
                StockIconType.FolderObjects3D => ("imageres.dll", -198),
                StockIconType.FolderDesktop => ("imageres.dll", -183),
                StockIconType.FolderDownloads => ("imageres.dll", -184),
                StockIconType.FolderDocuments => ("imageres.dll", -112),
                StockIconType.FolderPictures => ("imageres.dll", -113),
                StockIconType.FolderMusic => ("imageres.dll", -108),
                StockIconType.FolderVideo => ("imageres.dll", -189),
                _ => (null, 0)
            };

            return file != null ? GetIcon(file, index, size) : null;
        }

        internal static ImageSource GetIcon(string file, int index, int size)
        {
            (string, int index, int size) key = (file.ToLowerInvariant(), index, size);

            if (_iconCache.TryGetValue(key, out ImageSource cached))
            {
                return cached;
            }

            ImageSource icon = ExtractIconInternal(file, index, size);
            if (icon != null)
            {
                _iconCache[key] = icon;
            }
            return icon;
        }

        private static ImageSource ExtractIconInternal(string file, int index, int size)
        {
            IntPtr hLargeIcon = IntPtr.Zero;
            IntPtr hSmallIcon = IntPtr.Zero;

            try
            {
                uint nIconSize = (uint)((size << 16) | size);

                int result = SHDefExtractIcon(file, index, 0, out hLargeIcon, out hSmallIcon, nIconSize);

                if (result != 0 || hLargeIcon == IntPtr.Zero)
                {
                    return null;
                }

                ImageSource imageSource = Imaging.CreateBitmapSourceFromHIcon(hLargeIcon, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                imageSource.Freeze();

                return imageSource;
            }
            catch { return null; }
            finally
            {
                if (hLargeIcon != IntPtr.Zero)
                {
                    DestroyIcon(hLargeIcon);
                }

                if (hSmallIcon != IntPtr.Zero)
                {
                    DestroyIcon(hSmallIcon);
                }
            }
        }
    }
}