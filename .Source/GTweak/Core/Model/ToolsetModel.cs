using System.Xml.Linq;

namespace GTweak.Core.Models
{
    internal class ToolsetModel
    {
        public string AppName { get; }
        public string AuthorName { get; }
        public string SourceUrl { get; }
        public string Group { get; }
        public string DownloadPathStr { get; }
        public string FilePattern { get; }
        public string UrlPattern { get; }
        public string IconResourceName { get; }
        public string AuthorIconUrl { get; }

        public ToolsetModel(XElement appElement)
        {
            AppName = appElement.Element("Name")?.Value ?? string.Empty;
            AuthorName = appElement.Element("Author")?.Value ?? string.Empty;
            SourceUrl = appElement.Element("Source")?.Value ?? string.Empty;
            Group = appElement.Element("Group")?.Value ?? string.Empty;
            DownloadPathStr = appElement.Element("DownloadPath")?.Value ?? string.Empty;
            FilePattern = appElement.Element("FilePattern")?.Value ?? string.Empty;
            UrlPattern = appElement.Element("UrlPattern")?.Value ?? string.Empty;
            IconResourceName = appElement.Element("Icon")?.Value ?? string.Empty;
            AuthorIconUrl = appElement.Element("AuthorIcon")?.Value ?? string.Empty;
        }
    }
}