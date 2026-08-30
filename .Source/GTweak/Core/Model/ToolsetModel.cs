using System;
using System.Windows;
using System.Windows.Media;
using Newtonsoft.Json.Linq;

namespace GTweak.Core.Models
{
    internal enum FilterTag { All, Driver, Tool, Diagnostics, Benchmark, Overclock }

    internal class ToolsetModel
    {
        public ImageSource AppIcon { get; }
        public string AppName { get; }
        public string Group { get; }
        public FilterTag[] Tags { get; }
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
            Tags = ParseTags(appObject["tags"]?.ToObject<string[]>());
            PlaceholderIcon = Application.Current?.TryFindResource(appObject["placeholderIcon"]?.ToString() ?? string.Empty) as ImageSource;
            AuthorName = appObject["author"]?.ToString() ?? string.Empty;
            AuthorIconUrl = appObject["authorIcon"]?.ToString() ?? string.Empty;
            SourceUrl = appObject["source"]?.ToString() ?? string.Empty;
            DownloadPath = appObject["downloadPath"]?.ToString() ?? string.Empty;
            FilePattern = appObject["filePattern"]?.ToString() ?? string.Empty;
            UrlPattern = appObject["urlPattern"]?.ToString() ?? string.Empty;
            FileName = appObject["fileName"]?.ToString() ?? string.Empty;
        }

        private static FilterTag[] ParseTags(string[] values)
        {
            if (values == null || values.Length == 0)
            {
                return Array.Empty<FilterTag>();
            }

            FilterTag[] tags = new FilterTag[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                tags[i] = Enum.TryParse(values[i], true, out FilterTag tag) ? tag : FilterTag.All;
            }

            return tags;
        }
    }
}