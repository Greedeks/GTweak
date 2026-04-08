using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using GTweak.Core.Models;
using GTweak.Core.ViewModel.Components;
using GTweak.Utilities.Controls;
using Newtonsoft.Json.Linq;

namespace GTweak.Core.Services
{
    internal static class ToolsetDownloadService
    {
        private static readonly HttpClient _httpClient;
        private static readonly ConcurrentDictionary<string, DownloadSession> _sessions = new ConcurrentDictionary<string, DownloadSession>();

        static ToolsetDownloadService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "GTweak");
        }

        internal static DownloadSession GetOrCreateSession(ToolsetModel model) => _sessions.GetOrAdd(!string.IsNullOrWhiteSpace(model.SourceUrl) ? model.SourceUrl : model.AppName, _ => new DownloadSession(model));

        internal static async Task<string> GetResolvedDownloadUrl(ToolsetModel model)
        {
            if (model.Group.Equals("web", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrEmpty(model.UrlPattern))
                {
                    return model.DownloadPath;
                }

                string htmlCode = await _httpClient.GetStringAsync(model.DownloadPath);
                Match match = Regex.Match(htmlCode, model.UrlPattern, RegexOptions.IgnoreCase);

                if (match.Success)
                {
                    string url = match.Value;
                    if (Uri.TryCreate(url, UriKind.Relative, out _))
                    {
                        if (Uri.TryCreate(new Uri(model.DownloadPath), url, out Uri absoluteUri))
                        {
                            return absoluteUri.ToString();
                        }
                    }
                    return url;
                }

                throw new HttpRequestException();
            }

            if (model.Group.Equals("github", StringComparison.OrdinalIgnoreCase))
            {
                string apiUrl = $"https://api.github.com/repos/{model.DownloadPath}/releases/latest";
                using HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.Forbidden || (int)response.StatusCode == 429)
                    {
                        throw new Exception("GitHubRateLimit");
                    }

                    throw new HttpRequestException();
                }

                string content = await response.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(content);

                if (json["assets"] is JArray assets && assets.Count > 0)
                {
                    List<string> urls = assets.Select(a => a["browser_download_url"]?.ToString()).Where(u => !string.IsNullOrEmpty(u)).ToList();

                    if (!string.IsNullOrEmpty(model.FilePattern))
                    {
                        string matchedUrl = urls.FirstOrDefault(u => Regex.IsMatch(u, model.FilePattern, RegexOptions.IgnoreCase));
                        if (matchedUrl != null)
                        {
                            return matchedUrl;
                        }
                    }

                    string fallbackUrl = urls.FirstOrDefault(u => !u.EndsWith(".sig", StringComparison.OrdinalIgnoreCase) && !u.EndsWith(".sha256", StringComparison.OrdinalIgnoreCase) && !u.EndsWith(".asc", StringComparison.OrdinalIgnoreCase) && !u.EndsWith(".txt", StringComparison.OrdinalIgnoreCase));

                    return fallbackUrl ?? urls.FirstOrDefault();
                }
                throw new HttpRequestException();
            }

            if (model.Group.Equals("sourceforge", StringComparison.OrdinalIgnoreCase))
            {
                string projectName = model.DownloadPath;
                Match match = Regex.Match(projectName, @"projects/([^/]+)");
                projectName = match.Success ? match.Groups[1].Value : projectName.Trim('/');

                string bestReleaseUrl = $"https://sourceforge.net/projects/{projectName}/best_release.json";
                try
                {
                    using HttpResponseMessage response = await _httpClient.GetAsync(bestReleaseUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        string content = await response.Content.ReadAsStringAsync();
                        JObject json = JObject.Parse(content);
                        string filename = json["release"]?["filename"]?.ToString();

                        if (!string.IsNullOrEmpty(filename))
                        {
                            if (string.IsNullOrEmpty(model.FilePattern) || Regex.IsMatch(filename, model.FilePattern, RegexOptions.IgnoreCase))
                            {
                                return $"https://downloads.sourceforge.net/project/{projectName}{filename}";
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorLogging.LogDebug(ex);
                }

                try
                {
                    string rssUrl = $"https://sourceforge.net/projects/{projectName}/rss?path=/";
                    string rssContent = await _httpClient.GetStringAsync(rssUrl);

                    MatchCollection rssMatches = Regex.Matches(rssContent, $@"<link>https://sourceforge\.net/projects/{projectName}/files/([^<]+?)/download</link>", RegexOptions.IgnoreCase);

                    if (rssMatches.Count > 0)
                    {
                        if (!string.IsNullOrEmpty(model.FilePattern))
                        {
                            foreach (Match rssMatch in rssMatches)
                            {
                                string filePath = rssMatch.Groups[1].Value;
                                if (Regex.IsMatch(filePath, model.FilePattern, RegexOptions.IgnoreCase))
                                {

                                    return $"https://downloads.sourceforge.net/project/{projectName}/{filePath}";
                                }
                            }
                        }

                        string firstFilePath = rssMatches[0].Groups[1].Value;
                        return $"https://downloads.sourceforge.net/project/{projectName}/{firstFilePath}";
                    }
                }
                catch (Exception ex)
                {
                    ErrorLogging.LogDebug(ex);
                }

                throw new HttpRequestException();
            }

            throw new HttpRequestException();
        }

        internal static async Task DownloadFile(string url, string destinationPath, string referrer, IProgress<double> progress, CancellationToken ct)
        {
            string tempPath = $"{destinationPath}.download";
            long existingBytes = 0;
            bool append = false;

            if (File.Exists(tempPath))
            {
                existingBytes = new FileInfo(tempPath).Length;
            }

            if (existingBytes == 0)
            {
                progress?.Report(0);
            }

            try
            {
                using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);

                if (!string.IsNullOrEmpty(referrer))
                {
                    request.Headers.Referrer = new Uri(referrer);
                }

                if (existingBytes > 0)
                {
                    request.Headers.Range = new RangeHeaderValue(existingBytes, null);
                }

                using HttpResponseMessage response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);

                if (existingBytes > 0 && response.StatusCode == HttpStatusCode.OK)
                {
                    existingBytes = 0;
                    append = false;
                    progress?.Report(0);
                }
                else if (response.StatusCode == HttpStatusCode.PartialContent && existingBytes > 0)
                {
                    append = true;
                }

                response.EnsureSuccessStatusCode();

                long contentLength = response.Content.Headers.ContentLength ?? -1L;
                long totalBytes = contentLength;

                if (response.StatusCode == HttpStatusCode.PartialContent && contentLength > 0)
                {
                    totalBytes = existingBytes + contentLength;
                }

                long totalReadBytes = existingBytes;
                int lastReportedProgress = -1;

                using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync())
                using (FileStream fileStream = new FileStream(tempPath, append ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    byte[] buffer = new byte[8192];
                    int readBytes;

                    if (progress != null && totalBytes > 0 && existingBytes > 0)
                    {
                        int initialProgress = (int)Math.Round((double)existingBytes / totalBytes * 100);
                        progress.Report(initialProgress);
                        lastReportedProgress = initialProgress;
                    }

                    while ((readBytes = await streamToReadFrom.ReadAsync(buffer, 0, buffer.Length, ct)) > 0)
                    {
                        await fileStream.WriteAsync(buffer, 0, readBytes, ct);
                        totalReadBytes += readBytes;

                        if (totalBytes > 0 && progress != null)
                        {
                            int newProgress = (int)Math.Round((double)totalReadBytes / totalBytes * 100);
                            if (newProgress != lastReportedProgress)
                            {
                                progress.Report(newProgress);
                                lastReportedProgress = newProgress;
                            }
                        }
                    }
                }

                if (File.Exists(destinationPath))
                {
                    File.Replace(tempPath, destinationPath, null);
                }
                else
                {
                    File.Move(tempPath, destinationPath);
                }

                progress?.Report(100);
            }
            catch (Exception ex)
            {
                ErrorLogging.LogDebug(ex);
                throw;
            }
        }
    }
}