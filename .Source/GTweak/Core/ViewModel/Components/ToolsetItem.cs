using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using GTweak.Core.Base;
using GTweak.Core.Models;
using GTweak.Core.Services;
using GTweak.Core.ViewModel.Components;
using GTweak.Modules.Common;

namespace GTweak.Core.Item
{
    internal class ToolsetItem : ViewModelBase
    {
        private readonly ToolsetModel _model;
        private readonly DownloadSession _downloadSession;

        private ImageSource _appIconSource;
        private ImageSource _authorIconSource;
        private bool _isSquareIcon;

        public ImageSource AppIconSource { get => _appIconSource; set { _appIconSource = value; OnPropertyChanged(); } }
        public string AppName => _model.AppName;
        public FilterTag[] Tags => _model.Tags;
        public string AuthorName => _model.AuthorName;
        public ImageSource AuthorIconSource { get => _authorIconSource; set { _authorIconSource = value; OnPropertyChanged(); } }
        public bool IsSquareIcon { get => _isSquareIcon; set { _isSquareIcon = value; OnPropertyChanged(); } }
        public string SourceUrl => _model.SourceUrl;
        public bool IsDownloading => _downloadSession.IsDownloading;
        public bool IsDownloadCompleted => _downloadSession.IsDownloadCompleted;
        public double Progress => _downloadSession.Progress;
        public ICommand DownloadCommand { get; }
        public ICommand CancelCommand { get; }

        public ToolsetItem(ToolsetModel model)
        {
            _model = model;
            _downloadSession = ToolsetDownloadService.GetOrCreateSession(model);

            DownloadCommand = new RelayCommand(async _ => await _downloadSession.Start(), _ => !IsDownloading);
            CancelCommand = new RelayCommand(_ => _downloadSession.Cancel(), _ => IsDownloading);

            PropertyChangedEventManager.AddHandler(_downloadSession, DownloadSession_PropertyChanged, string.Empty);

            _ = LoadIcons();
        }

        private void DownloadSession_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DownloadSession.IsDownloading))
            {
                OnPropertyChanged(nameof(IsDownloading));
                CommandManager.InvalidateRequerySuggested();
            }
            else if (e.PropertyName == nameof(DownloadSession.Progress))
            {
                OnPropertyChanged(nameof(Progress));
            }
            else if (e.PropertyName == nameof(DownloadSession.IsDownloadCompleted))
            {
                OnPropertyChanged(nameof(IsDownloadCompleted));
            }
        }

        private async Task LoadIcons()
        {
            try
            {
                AppIconSource = _model.AppIcon;
                AuthorIconSource = _model.PlaceholderIcon;

                ImageSource imageSource = await ToolsetIconService.GetAuthorIcon(_model.AuthorIconUrl, _model.AuthorIconInfo.IsDirectUrl);

                if (imageSource != null)
                {
                    AuthorIconSource = imageSource;
                    IsSquareIcon = _model.AuthorIconInfo.IsSquareIcon;
                }
            }
            catch (Exception ex) { ErrorLogger.LogDebug(ex); }
        }
    }
}
