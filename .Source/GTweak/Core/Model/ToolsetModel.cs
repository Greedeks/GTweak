using System.Windows;
using System.Windows.Media;
using Newtonsoft.Json.Linq;

namespace GTweak.Core.Models
{
    internal class ToolsetModel
    {
        public ImageSource AppIcon { get; }
        public string AppName { get; }
        public string Group { get; }
        public ImageSource PlaceholderIcon { get; }
        public string AuthorName { get; }
        public string AuthorIconUrl { get; }
        public string SourceUrl { get; }
        public string DownloadPath { get; }
        public string FilePattern { get; }
        public string UrlPattern { get; }
        public string FileName { get; }

        public (bool IsDirectUrl, bool IsSquareIcon) AuthorIconInfo
        {
            get
            {
                return Group?.ToLowerInvariant() switch
                {
                    "github" => (IsDirectUrl: true, IsSquareIcon: false),
                    _ => (IsDirectUrl: false, IsSquareIcon: true)
                };
            }
        }

        public ToolsetModel(JObject appObject)
        {
            AppIcon = Application.Current?.TryFindResource(appObject["icon"]?.ToString() ?? string.Empty) as ImageSource;
            AppName = appObject["name"]?.ToString() ?? string.Empty;
            Group = appObject["group"]?.ToString() ?? string.Empty;
            PlaceholderIcon = Application.Current?.TryFindResource(appObject["placeholderIcon"]?.ToString() ?? string.Empty) as ImageSource;
            AuthorName = appObject["author"]?.ToString() ?? string.Empty;
            AuthorIconUrl = appObject["authorIcon"]?.ToString() ?? string.Empty;
            SourceUrl = appObject["source"]?.ToString() ?? string.Empty;
            DownloadPath = appObject["downloadPath"]?.ToString() ?? string.Empty;
            FilePattern = appObject["filePattern"]?.ToString() ?? string.Empty;
            UrlPattern = appObject["urlPattern"]?.ToString() ?? string.Empty;
            FileName = appObject["fileName"]?.ToString() ?? string.Empty;
        }
    }
}