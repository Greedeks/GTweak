using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;
using GTweak.Core.Base;
using GTweak.Core.Models;
using GTweak.Core.Service;
using GTweak.Utilities.Controls;
using GTweak.Utilities.Managers;
using Ookii.Dialogs.Wpf;

namespace GTweak.Core.ViewModel
{
    internal class ToolsetViewModel : ViewModelBase
    {
        public ObservableCollection<ToolsetItem> Tools { get; } = new ObservableCollection<ToolsetItem>();

        public ICommand SelectFolderCommand { get; }
        public ICommand OpenFolderCommand { get; }

        public string DownloadPath
        {
            get => SettingsEngine.DownloadPath;
            set
            {
                if (SettingsEngine.DownloadPath != value)
                {
                    SettingsEngine.DownloadPath = value;
                    OnPropertyChanged();
                }
            }
        }

        public ToolsetViewModel()
        {
            SelectFolderCommand = new RelayCommand(_ => SelectFolder());
            OpenFolderCommand = new RelayCommand(_ => OpenFolder());

            LoadApps();
        }

        private void LoadApps()
        {
            try
            {
                string xmlContent = Properties.Resources.AppsCatalog;

                if (!string.IsNullOrWhiteSpace(xmlContent))
                {
                    XDocument doc = XDocument.Parse(xmlContent);

                    foreach (var appElement in doc.Descendants("App"))
                    {
                        ToolsetModel model = new ToolsetModel(appElement);
                        Tools.Add(new ToolsetItem(model));
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLogging.LogDebug(ex);
            }
        }

        private void SelectFolder()
        {
            try
            {
                VistaFolderBrowserDialog folderDialog = new VistaFolderBrowserDialog
                {
                    UseDescriptionForTitle = true,
                    SelectedPath = SettingsEngine.DownloadPath ?? Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)
                };

                if (folderDialog.ShowDialog() == true)
                {
                    string selectedPath = folderDialog.SelectedPath;
                    if (!string.IsNullOrWhiteSpace(selectedPath) && Directory.Exists(selectedPath))
                    {
                        DownloadPath = selectedPath;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLogging.LogDebug(ex);
            }
        }

        private void OpenFolder()
        {
            try
            {
                if (Directory.Exists(DownloadPath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = DownloadPath,
                        UseShellExecute = true,
                        Verb = "open"
                    });
                }
            }
            catch (Exception ex)
            {
                ErrorLogging.LogDebug(ex);
            }
        }

        internal class ToolsetItem : ViewModelBase
        {
            private readonly ToolsetModel _model;
            private CancellationTokenSource _cts;

            private ImageSource _appIconSource;
            private ImageSource _authorIconSource;
            private bool _isDownloading;
            private double _progress;

            public ToolsetItem(ToolsetModel model)
            {
                _model = model;

                DownloadCommand = new RelayCommand(async _ => await DownloadAsync(), _ => !IsDownloading);
                CancelCommand = new RelayCommand(_ => CancelDownload(), _ => IsDownloading);

                _ = LoadIconsAsync();
            }

            public string AppName => _model.AppName;
            public string AuthorName => _model.AuthorName;
            public string SourceUrl => _model.SourceUrl;

            public ImageSource AppIconSource
            {
                get => _appIconSource;
                set { _appIconSource = value; OnPropertyChanged(); }
            }

            public ImageSource AuthorIconSource
            {
                get => _authorIconSource;
                set { _authorIconSource = value; OnPropertyChanged(); }
            }

            public bool IsDownloading
            {
                get => _isDownloading;
                set { _isDownloading = value; OnPropertyChanged(); }
            }

            public double Progress
            {
                get => _progress;
                set { _progress = value; OnPropertyChanged(); }
            }

            public ICommand DownloadCommand { get; }
            public ICommand CancelCommand { get; }

            private async Task LoadIconsAsync()
            {
                try
                {
                    AppIconSource = ToolsetIconManager.GetAppIconFromResource(_model.IconResourceName);
                    AuthorIconSource = ToolsetIconManager.GetPlaceholder(_model.Group);

                    ImageSource realIcon = await ToolsetIconManager.GetAuthorIconAsync(_model.Group, _model.AuthorIconUrl);
                    if (realIcon != null)
                    {
                        AuthorIconSource = realIcon;
                    }
                }
                catch (Exception ex)
                {
                    ErrorLogging.LogDebug(ex);
                }
            }

            private async Task DownloadAsync()
            {
                IsDownloading = true;
                Progress = 0;
                _cts = new CancellationTokenSource();

                Progress<double> progressHandler = new Progress<double>(value => Progress = value);

                try
                {
                    string finalUrl = await ToolsetDownloadService.ResolveDownloadUrlAsync(_model);

                    if (!string.IsNullOrEmpty(finalUrl))
                    {
                        string destinationPath = Path.Combine(SettingsEngine.DownloadPath ?? Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), Path.GetFileName(new Uri(finalUrl).LocalPath));
                        await ToolsetDownloadService.DownloadFileAsync(finalUrl, destinationPath, _model.SourceUrl, progressHandler, _cts.Token);
                    }
                }
                catch (Exception ex) { ErrorLogging.LogDebug(ex); }
                finally
                {
                    IsDownloading = false;

                    if (Progress != 100)
                    {
                        Progress = 0;
                    }

                    _cts?.Dispose();
                    _cts = null;
                }
            }

            private void CancelDownload()
            {
                _cts?.Cancel();
            }
        }
    }
}