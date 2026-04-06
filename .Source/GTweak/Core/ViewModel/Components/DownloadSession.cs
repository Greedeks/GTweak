using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GTweak.Core.Base;
using GTweak.Core.Models;
using GTweak.Core.Services;
using GTweak.Utilities.Controls;

namespace GTweak.Core.ViewModel.Components
{
    internal sealed class DownloadSession : ViewModelBase
    {
        private readonly ToolsetModel _model;
        private CancellationTokenSource _cts;
        private bool _isDownloading;
        private double _progress;

        public bool IsDownloading
        {
            get => _isDownloading;
            private set
            {
                if (_isDownloading != value)
                {
                    _isDownloading = value;
                    OnPropertyChanged();
                    System.Windows.Input.CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public double Progress
        {
            get => _progress;
            private set
            {
                if (Math.Abs(_progress - value) > 0.01)
                {
                    _progress = value;
                    OnPropertyChanged();
                }
            }
        }

        internal DownloadSession(ToolsetModel model)
        {
            _model = model;
        }

        public async Task Start()
        {
            if (!IsDownloading)
            {
                IsDownloading = true;

                _cts?.Dispose();
                _cts = new CancellationTokenSource();

                try
                {
                    string finalUrl = await ToolsetDownloadService.GetResolvedDownloadUrl(_model);

                    if (!string.IsNullOrEmpty(finalUrl))
                    {
                        string destinationFolder = SettingsEngine.DownloadPath ?? Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                        await ToolsetDownloadService.DownloadFile(finalUrl, Path.Combine(destinationFolder, GetFileNameFromUrl(finalUrl)), _model.SourceUrl, new Progress<double>(value => Progress = value), _cts.Token);
                    }
                }
                catch (Exception ex) { ErrorLogging.LogDebug(ex); }
                finally
                {
                    IsDownloading = false;
                    _cts?.Dispose();
                    _cts = null;
                    System.Windows.Input.CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public void Cancel()
        {
            _cts?.Cancel();
        }

        private string GetFileNameFromUrl(string url)
        {
            try
            {
                string fileName = Path.GetFileName(new Uri(url).LocalPath);

                if (string.IsNullOrEmpty(Path.GetExtension(fileName)))
                {
                    return !string.IsNullOrEmpty(_model.FilePattern) ? _model.FilePattern : $"{_model.AppName}.exe";
                }

                return fileName;
            }
            catch { return $"{_model.AppName}.exe"; }
        }
    }
}
