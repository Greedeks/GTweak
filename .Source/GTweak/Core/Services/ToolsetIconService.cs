using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GTweak.Modules.Common;

namespace GTweak.Core.Services
{
    internal class ToolsetIconService
    {
        private static readonly ConcurrentDictionary<string, ImageSource> _imageCache = new ConcurrentDictionary<string, ImageSource>();
        private static readonly HttpClient _httpClient = new HttpClient();

        internal static async Task<ImageSource> GetAuthorIcon(string iconSource, bool isDirectUrl)
        {
            if (string.IsNullOrWhiteSpace(iconSource))
            {
                return null;
            }

            if (_imageCache.TryGetValue(iconSource, out ImageSource cachedImage))
            {
                return cachedImage;
            }

            string[] urlsToDownload = isDirectUrl ? new[] { iconSource } : new[] { PathTargets.Links.Favicons.Google(iconSource), PathTargets.Links.Favicons.DuckDuckGo(iconSource) };

            foreach (string url in urlsToDownload)
            {
                try
                {
                    byte[] imageBytes = await _httpClient.GetByteArrayAsync(url);
                    ImageSource image = LoadImage(imageBytes);

                    if (image != null)
                    {
                        _imageCache.TryAdd(iconSource, image);
                        return image;
                    }
                }
                catch (Exception ex) { ErrorLogger.LogDebug(ex); }
            }

            return null;
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
