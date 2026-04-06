using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GTweak.Utilities.Controls;

namespace GTweak.Core.Services
{
    internal class ToolsetIconService
    {
        private static readonly ConcurrentDictionary<string, ImageSource> _imageCache = new ConcurrentDictionary<string, ImageSource>();
        private static readonly HttpClient _httpClient = new HttpClient();

        internal static ImageSource GetPlaceholder(string group) => Application.Current.TryFindResource(group.ToLower() == "github" ? "Icon_Git" : "Icon_Website") as ImageSource;

        internal static async Task<(ImageSource Image, bool IsFallback)> GetAuthorIcon(string group, string iconSource)
        {
            if (string.IsNullOrWhiteSpace(iconSource))
            {
                return (GetPlaceholder(group), true);
            }

            if (_imageCache.TryGetValue(iconSource, out ImageSource cachedImage))
            {
                return (cachedImage, false);
            }

            string urlToDownload = iconSource;
            bool isWeb = string.Equals(group, "web", StringComparison.OrdinalIgnoreCase);

            if (isWeb)
            {
                urlToDownload = $"https://www.google.com/s2/favicons?domain={iconSource}&sz=48";
            }

            try
            {
                byte[] imageBytes = await _httpClient.GetByteArrayAsync(urlToDownload);
                ImageSource image = LoadImage(imageBytes);

                if (image != null)
                {
                    _imageCache.TryAdd(iconSource, image);
                    return (image, false);
                }
            }
            catch
            {
                if (isWeb)
                {
                    try
                    {
                        byte[] imageBytes = await _httpClient.GetByteArrayAsync($"https://icons.duckduckgo.com/ip3/{iconSource}.ico");
                        ImageSource image = LoadImage(imageBytes);

                        if (image != null)
                        {
                            _imageCache.TryAdd(iconSource, image);
                            return (image, false);
                        }
                    }
                    catch (Exception ex) { ErrorLogging.LogDebug(ex); }
                }
            }

            return (GetPlaceholder(group), true);
        }

        private static ImageSource LoadImage(byte[] imageData)
        {
            if (imageData != null && imageData.Length != 0)
            {
                BitmapImage bitmap = new BitmapImage();
                using (MemoryStream stream = new MemoryStream(imageData))
                {
                    bitmap.BeginInit();
                    bitmap.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.UriSource = null;
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                }
                bitmap.Freeze();
                return bitmap;
            }

            return null;
        }
    }
}
