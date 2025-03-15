using GTweak.Utilities.Control;
using Newtonsoft.Json;
using System.IO;
using System.Net;

namespace GTweak.Utilities.Configuration
{
    internal sealed class QueryUpdates
    {
        private sealed class GitMetadata
        {
            [JsonProperty("tag_name")]
            internal string СurrentVersion { get; set; }
        }

        internal static bool IsNeedUpdate { get; private set; } = false;
        internal static string DownloadVersion { get; private set; } = string.Empty;

        internal void RunSearch()
        {
            if (!SettingsRepository.IsСheckingUpdate)
                return;

            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create("https://api.github.com/repos/greedeks/gtweak/releases/latest");

                webRequest.ContentType = "application/json";
                webRequest.UserAgent = "Nothing";
                webRequest.Timeout = 5000;

                using HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
                using StreamReader sreader = new StreamReader(response.GetResponseStream());
                string DataAsJson = sreader.ReadToEnd();
                GitMetadata gitVersionUtility = JsonConvert.DeserializeObject<GitMetadata>(DataAsJson);

                if (!string.IsNullOrEmpty(gitVersionUtility.СurrentVersion) && gitVersionUtility.СurrentVersion.CompareTo(SettingsRepository.currentRelease) > 0)
                {
                    IsNeedUpdate = true;
                    DownloadVersion = gitVersionUtility.СurrentVersion;
                }
            }
            catch
            {
                IsNeedUpdate = false;
            }
        }
    }
}
