using System;
using System.Collections.Concurrent;
using System.IO;
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
                    return model.DownloadPathStr;
                }

                string htmlCode = await _httpClient.GetStringAsync(model.DownloadPathStr);
                Match match = Regex.Match(htmlCode, model.UrlPattern);
                return match.Success ? match.Value : null;
            }

            if (model.Group.Equals("github", StringComparison.OrdinalIgnoreCase))
            {
                string apiUrl = $"https://api.github.com/repos/{model.DownloadPathStr}/releases/latest";

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
                    string firstFoundUrl = null;
                    foreach (JToken asset in assets)
                    {
                        string downloadUrl = asset["browser_download_url"]?.ToString();
                        if (string.IsNullOrEmpty(downloadUrl))
                        {
                            continue;
                        }

                        firstFoundUrl ??= downloadUrl;

                        if (!string.IsNullOrEmpty(model.FilePattern) &&
                            Regex.IsMatch(downloadUrl, model.FilePattern, RegexOptions.IgnoreCase))
                        {
                            return downloadUrl;
                        }
                    }
                    return firstFoundUrl;
                }
            }
            return null;
        }

        internal static async Task DownloadFile(string url, string destinationPath, string referrer, IProgress<double> progress, CancellationToken ct)
        {
            string tempPath = $"{destinationPath}.download";
            long existingBytes = 0;
            bool append = false;

            try
            {
                if (File.Exists(tempPath))
                {
                    existingBytes = new FileInfo(tempPath).Length;
                }

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