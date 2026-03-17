using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using GTweak.Utilities.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GTweak.Utilities.Configuration
{
    internal class NetworkProvider : MonitoringProvider
    {
        private class GitMetadata
        {
            [JsonProperty("tag_name")]
            internal string CurrentVersion { get; set; }
        }

        private sealed class GitLabMetadata : GitMetadata
        {
            [JsonProperty("description")]
            internal string Description { get; set; }
        }

        private sealed class IPMetadata
        {
            internal string Ip { get; set; }
            internal string Country { get; set; }

            internal static IPMetadata ParseData(string response)
            {
                if (!string.IsNullOrWhiteSpace(response))
                {
                    JObject jObject = JObject.Parse(response);
                    string ip = jObject["ip"]?.ToString() ?? jObject["ipAddress"]?.ToString() ?? jObject["query"]?.ToString() ?? string.Empty;
                    string country = jObject["country_code"]?.ToString() ?? jObject["countryCode"]?.ToString() ?? string.Empty;
                    return new IPMetadata { Ip = ip, Country = country };
                }
                return new IPMetadata { Ip = string.Empty, Country = string.Empty };
            }
        }

        private static readonly HttpClient sharedClient = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };

        internal static bool IsNeedUpdate { get; private set; } = false;
        internal static string DownloadVersion { get; private set; } = string.Empty;

        internal static bool isIPAddressFormatValid = false;

        internal async Task<bool> IsNetworkAvailable()
        {
            string dns = HardwareProvider.GetCurrentSystemLang().Code switch
            {
                "fa" => "aparat.com",
                "zh" => "baidu.com",
                "ru" => "yandex.ru",
                "kk" => "yandex.kz",
                "ko" => "naver.com",
                "cs" => "seznam.cz",
                "tk" => "turkmenportal.com",
                "vi" => "fpt.vn",
                "es" => "terra.es",
                "ja" => "yahoo.co.jp",
                "de" => "t-online.de",
                "fr" => "orange.fr",
                "it" => "libero.it",
                "th" => "true.th",
                "pl" => "wp.pl",
                "nl" => "nu.nl",
                "pt" => "globo.com",
                "sv" => "aftonbladet.se",
                "no" => "vg.no",
                "fi" => "yle.fi",
                "ro" => "digisport.ro",
                "gr" => "in.gr",
                "id" => "detik.com",
                "mx" => "univision.com",
                "tr" => "bing.com",
                "hi" => "bing.com",
                "ar" => "bing.com",
                _ => "google.com"
            };

            Task<IPHostEntry> dnsTask = Dns.GetHostEntryAsync(dns);
            Task timeoutTask = Task.Delay(TimeSpan.FromSeconds(5));

            if (await Task.WhenAny(dnsTask, timeoutTask) == timeoutTask) { return false; }

            try
            {
                IPHostEntry entry = await dnsTask;
                return entry?.AddressList?.Length > 0;
            }
            catch { return false; }
        }

        internal async Task GetUserIpAddress()
        {
            if (await IsNetworkAvailable())
            {
                bool hadLimited = false, hadBlock = false;

                foreach (string url in PathLocator.Links.IpServices)
                {
                    try
                    {
                        using HttpResponseMessage response = await sharedClient.GetAsync(url);

                        if (!response.IsSuccessStatusCode)
                        {
                            hadBlock = true;
                            continue;
                        }

                        string content = await response.Content.ReadAsStringAsync();
                        IPMetadata ipMetadata = IPMetadata.ParseData(content);

                        if (ipMetadata != null && !string.IsNullOrWhiteSpace(ipMetadata.Ip) && !string.IsNullOrWhiteSpace(ipMetadata.Country) && IPAddress.TryParse(ipMetadata.Ip, out _))
                        {
                            CurrentConnection = ConnectionStatus.Available;
                            UserIPAddress = $"{ipMetadata.Ip} ({ipMetadata.Country})";
                            break;
                        }

                        hadBlock = true;
                    }
                    catch { hadLimited = true; }
                }

                if (CurrentConnection != ConnectionStatus.Available)
                {
                    if (hadLimited)
                    {
                        CurrentConnection = ConnectionStatus.Limited;
                    }
                    else if (hadBlock)
                    {
                        CurrentConnection = ConnectionStatus.Block;
                    }
                    else
                    {
                        CurrentConnection = ConnectionStatus.Lose;
                    }
                }
            }
            else
            {
                CurrentConnection = ConnectionStatus.Lose;
            }

            if (new Dictionary<ConnectionStatus, string>
            {
                { ConnectionStatus.Lose, "connection_lose_sysinfo" },
                { ConnectionStatus.Block, "connection_block_sysinfo" },
                { ConnectionStatus.Limited, "connection_limited_sysinfo" }
            }.TryGetValue(CurrentConnection, out string resourceKey)) { UserIPAddress = (string)Application.Current.Resources[resourceKey]; }

            isIPAddressFormatValid = UserIPAddress.Any(char.IsDigit);
        }

        internal async Task ValidateVersionUpdates()
        {
            if (SettingsEngine.IsUpdateCheckRequired && await IsNetworkAvailable())
            {
                foreach (string api in PathLocator.Links.ReleaseApi)
                {
                    try
                    {
                        using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, api);
                        request.Headers.Add("User-Agent", "GTweak");
                        request.Headers.Add("Accept", "application/json");

                        using HttpResponseMessage response = await sharedClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                        response.EnsureSuccessStatusCode();

                        string json = await response.Content.ReadAsStringAsync();
                        GitMetadata latest = null;

                        if (api.Contains("github"))
                        {
                            latest = JsonConvert.DeserializeObject<GitMetadata>(json);
                        }
                        else
                        {
                            List<GitLabMetadata> releases = JsonConvert.DeserializeObject<List<GitLabMetadata>>(json);
                            latest = releases?.FirstOrDefault();

                            if (latest is GitLabMetadata gitLabMetadata && !string.IsNullOrEmpty(gitLabMetadata.Description))
                            {
                                Match match = Regex.Match(gitLabMetadata.Description, @"\[(?<name>.*?)\]\(/uploads/(?<hash>.+?)\)");

                                if (match.Success)
                                {
                                    string hashFile = match.Groups["hash"].Value;
                                    PathLocator.Links.GitLabLatest = (true, $"{PathLocator.Links.GitLabLatest.Url}{hashFile}");
                                }
                            }
                        }

                        if (latest != null && !string.IsNullOrWhiteSpace(latest.CurrentVersion))
                        {
                            if (new Version(latest.CurrentVersion) > new Version(SettingsEngine.CurrentRelease.Short))
                            {
                                IsNeedUpdate = true;
                                DownloadVersion = latest.CurrentVersion;
                                break;
                            }
                        }
                    }
                    catch { continue; }
                }
            }
        }
    }
}