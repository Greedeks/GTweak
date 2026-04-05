using System;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using GTweak.Core.Models;
using GTweak.Utilities.Controls;
using Newtonsoft.Json.Linq;

namespace GTweak.Core.Service
{
    internal static class ToolsetDownloadService
    {
        private static readonly HttpClient _httpClient;

        static ToolsetDownloadService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "GTweak");
        }

        public static async Task<string> ResolveDownloadUrlAsync(ToolsetModel model)
        {
            try
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

                    HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);
                    if (!response.IsSuccessStatusCode)
                    {
                        return null;
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

                            if (!string.IsNullOrEmpty(model.FilePattern) && Regex.IsMatch(downloadUrl, model.FilePattern, RegexOptions.IgnoreCase))
                            {
                                return downloadUrl;
                            }
                        }

                        return firstFoundUrl;
                    }
                }
            }
            catch (Exception ex) { ErrorLogging.LogDebug(ex); }

            return null;
        }

        public static async Task DownloadFileAsync(string url, string destinationPath, string referrer, IProgress<double> progress, CancellationToken ct)
        {
            string tempPath = destinationPath + ".tmp";
            try
            {
                using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
                if (!string.IsNullOrEmpty(referrer))
                {
                    request.Headers.Referrer = new Uri(referrer);
                }

                using HttpResponseMessage response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
                response.EnsureSuccessStatusCode();

                long totalBytes = response.Content.Headers.ContentLength ?? -1L;

                using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync())
                using (FileStream fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    byte[] buffer = new byte[8192];
                    long totalReadBytes = 0L;
                    int readBytes;
                    int lastReportedProgress = 0;

                    while ((readBytes = await streamToReadFrom.ReadAsync(buffer, 0, buffer.Length, ct)) > 0)
                    {
                        await fileStream.WriteAsync(buffer, 0, readBytes, ct);
                        totalReadBytes += readBytes;

                        if (totalBytes != -1 && progress != null)
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
                    File.Delete(destinationPath);
                }

                File.Move(tempPath, destinationPath);
                progress?.Report(100);
            }
            catch (Exception ex)
            {
                ErrorLogging.LogDebug(ex);
                if (File.Exists(tempPath))
                {
                    try { File.Delete(tempPath); }
                    catch (Exception exc) { ErrorLogging.LogDebug(exc); }
                }
                throw;
            }
        }
    }
}
