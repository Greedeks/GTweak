using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GTweak.Utilities.Configuration
{
    internal static class IconExtractProvider
    {
        [DllImport("Shell32.dll", EntryPoint = "ExtractIconExW", CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern int ExtractIconEx(string sFile, int iIndex, out IntPtr piLargeVersion, out IntPtr piSmallVersion, int amountIcons);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr hIcon);

        private static readonly ConcurrentDictionary<(string, int, bool), ImageSource> _iconCache = new ConcurrentDictionary<(string, int, bool), ImageSource>();

        internal enum StockIconType { Home, Gallery, PC, Network, Trash, Panel, User, OneDrive }

        private static readonly Dictionary<StockIconType, (string file, int index)> _stockIconMappings = new Dictionary<StockIconType, (string, int)>
        {
            { StockIconType.Home, ("shell32.dll", -51380) },
            { StockIconType.Gallery, ("shell32.dll", -51586) },
            { StockIconType.PC, ("imageres.dll", -109) },
            { StockIconType.Network, ("imageres.dll", -25) },
            { StockIconType.Trash, ("imageres.dll", -54) },
            { StockIconType.Panel, ("imageres.dll", -27) },
            { StockIconType.User, ("imageres.dll", -123) },
            { StockIconType.OneDrive, ("imageres.dll", -1040) }
        };

        internal static ImageSource GetStockIcon(StockIconType type)
        {
            if (_stockIconMappings.TryGetValue(type, out (string file, int index) iconInfo))
            {
                return GetIcon(iconInfo.file, iconInfo.index);
            }
            return null;
        }

        internal static ImageSource GetIcon(string file, int index, bool large = true)
        {
            (string, int index, bool large) key = (file.ToLowerInvariant(), index, large);

            if (_iconCache.TryGetValue(key, out ImageSource cached))
            {
                return cached;
            }

            ImageSource icon = ExtractIconInternal(file, index, large);
            if (icon != null)
            {
                _iconCache[key] = icon;
            }
            return icon;
        }

        private static ImageSource ExtractIconInternal(string file, int index, bool large)
        {
            IntPtr hLargeIcon = IntPtr.Zero;
            IntPtr hSmallIcon = IntPtr.Zero;

            try
            {
                int readIconCount = ExtractIconEx(file, index, out hLargeIcon, out hSmallIcon, 1);

                if (readIconCount <= 0)
                {
                    return null;
                }

                IntPtr hIconToUse = large ? hLargeIcon : hSmallIcon;

                if (hIconToUse == IntPtr.Zero)
                {
                    return null;
                }

                ImageSource imageSource = Imaging.CreateBitmapSourceFromHIcon(hIconToUse, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
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