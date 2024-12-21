using Newtonsoft.Json;
using System.IO;
using System.Net;

namespace GTweak.Utilities.Control
{
    internal sealed class SearchUpdates
    {
        internal class GitVersionUtility
        {
            [JsonProperty("tag_name")]
            internal string СurrentVersion { get; set; }
        }

        internal static bool IsNeedUpdate { get; private set; } = false;
        internal static string DownloadVersion { get; private set; } = string.Empty;

        internal void StartСhecking()
        {
            if (!Settings.IsСheckingUpdate)
                return;

            if (!(WebRequest.Create("https://api.github.com/repos/greedeks/gtweak/releases/latest") is HttpWebRequest webRequest))
                return;

            webRequest.ContentType = "application/json";
            webRequest.UserAgent = "Nothing";

            using StreamReader sreader = new StreamReader(webRequest.GetResponse().GetResponseStream());
            string DataAsJson = sreader.ReadToEnd();
            GitVersionUtility gitVersionUtility = JsonConvert.DeserializeObject<GitVersionUtility>(DataAsJson);

            if (!string.IsNullOrEmpty(gitVersionUtility.СurrentVersion) && gitVersionUtility.СurrentVersion.CompareTo(Settings.currentRelease) > 0)
            {
                IsNeedUpdate = true;
                DownloadVersion = gitVersionUtility.СurrentVersion;
            }
        }
    }
}
