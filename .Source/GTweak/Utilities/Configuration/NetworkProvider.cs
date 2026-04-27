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
                    response = Regex.Match(response, @"\{.*\}", RegexOptions.Singleline).Value;
                    try
                    {
                        JObject jObject = JObject.Parse(response);
                        string ip = jObject["ip"]?.ToString() ?? jObject["ipAddress"]?.ToString() ?? jObject["query"]?.ToString() ?? string.Empty;
                        string country = jObject["country_code"]?.ToString() ?? jObject["countryCode"]?.ToString() ?? jObject["addr"]?.ToString() ?? string.Empty;
                        return new IPMetadata { Ip = ip, Country = country.Trim() };
                    }
                    catch (Exception ex) { ErrorLogging.LogDebug(ex); }
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

                        using HttpResponseMessage response = await sharedClient.SendAsync(request);
                        if (response.IsSuccessStatusCode)
                        {
                            string json = await response.Content.ReadAsStringAsync();
                            string foundVersion = string.Empty, foundUrl = string.Empty;

                            if (api.Contains("github"))
                            {
                                GitMetadata latest = JsonConvert.DeserializeObject<GitMetadata>(json);
                                foundVersion = latest?.CurrentVersion;
                                foundUrl = PathLocator.Links.LatestUpdate.GitHub;
                            }
                            else
                            {
                                List<GitLabMetadata> releases = JsonConvert.DeserializeObject<List<GitLabMetadata>>(json);
                                GitLabMetadata latest = releases?.FirstOrDefault();
                                if (latest != null)
                                {
                                    foundVersion = latest.CurrentVersion;
                                    Match match = Regex.Match(latest.Description ?? "", @"\[.*?\]\(/uploads/(?<hash>.+?)\)");
                                    if (match.Success)
                                    {
                                        foundUrl = $"{PathLocator.Links.LatestUpdate.GitLabBase}{match.Groups["hash"].Value}";
                                    }
                                }
                            }

                            if (!string.IsNullOrEmpty(foundUrl))
                            {
                                PathLocator.Links.LatestUpdate.Resolved = foundUrl;

                                if (new Version(foundVersion) > new Version(SettingsEngine.CurrentRelease.Short))
                                {
                                    IsNeedUpdate = true;
                                    DownloadVersion = foundVersion;
                                }
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