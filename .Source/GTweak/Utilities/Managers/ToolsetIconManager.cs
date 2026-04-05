using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GTweak.Utilities.Controls;

namespace GTweak.Utilities.Managers
{
    internal static class ToolsetIconManager
    {
        private static readonly ConcurrentDictionary<string, ImageSource> _imageCache = new ConcurrentDictionary<string, ImageSource>();
        private static readonly HttpClient _httpClient = new HttpClient();

        public static ImageSource GetPlaceholder(string group) => Application.Current.TryFindResource(group.ToLower() == "github" ? "Icon_Git" : "Icon_Website") as ImageSource;

        public static async Task<(ImageSource Image, bool IsFallback)> GetAuthorIconAsync(string group, string authorIconUrlOrDomain)
        {
            if (string.IsNullOrWhiteSpace(authorIconUrlOrDomain))
            {
                return (GetPlaceholder(group), true);
            }

            if (_imageCache.TryGetValue(authorIconUrlOrDomain, out ImageSource cachedImage))
            {
                return (cachedImage, false);
            }

            string urlToDownload = authorIconUrlOrDomain;
            bool isWeb = string.Equals(group, "web", StringComparison.OrdinalIgnoreCase);

            if (isWeb)
            {
                urlToDownload = $"https://www.google.com/s2/favicons?domain={authorIconUrlOrDomain}&sz=48";
            }

            try
            {
                byte[] imageBytes = await _httpClient.GetByteArrayAsync(urlToDownload);
                ImageSource image = LoadImage(imageBytes);

                if (image != null)
                {
                    _imageCache.TryAdd(authorIconUrlOrDomain, image);
                    return (image, false);
                }
            }
            catch
            {
                if (isWeb)
                {
                    try
                    {
                        string backupUrl = $"https://icons.duckduckgo.com/ip3/{authorIconUrlOrDomain}.ico";
                        byte[] imageBytes = await _httpClient.GetByteArrayAsync(backupUrl);
                        ImageSource image = LoadImage(imageBytes);

                        if (image != null)
                        {
                            _imageCache.TryAdd(authorIconUrlOrDomain, image);
                            return (image, false);
                        }
                    }
                    catch (Exception ex) { ErrorLogging.LogDebug(ex);  }
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

        public static ImageSource GetAppIconFromResource(string resourceName) => Application.Current.TryFindResource(resourceName) as ImageSource;
    }
}
