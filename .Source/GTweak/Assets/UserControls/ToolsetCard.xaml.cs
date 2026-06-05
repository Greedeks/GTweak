using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace GTweak.Assets.UserControls
{
    public partial class ToolsetCard : UserControl
    {
        internal static readonly DependencyProperty AppIconProperty = 
            DependencyProperty.Register(nameof(AppIcon), typeof(ImageSource), typeof(ToolsetCard), new PropertyMetadata(null));

        internal static readonly DependencyProperty AppNameProperty = 
            DependencyProperty.Register(nameof(AppName), typeof(string), typeof(ToolsetCard), new PropertyMetadata(string.Empty));

        internal static readonly DependencyProperty AuthorIconProperty = 
            DependencyProperty.Register(nameof(AuthorIcon), typeof(ImageSource), typeof(ToolsetCard), new PropertyMetadata(null));

        internal static readonly DependencyProperty AuthorNameProperty = 
            DependencyProperty.Register(nameof(AuthorName), typeof(string), typeof(ToolsetCard), new PropertyMetadata(string.Empty));

        internal static readonly DependencyProperty SourceUrlProperty = 
            DependencyProperty.Register(nameof(SourceUrl), typeof(string), typeof(ToolsetCard), new PropertyMetadata(string.Empty));

        internal static readonly DependencyProperty DownloadCommandProperty = 
            DependencyProperty.Register(nameof(DownloadCommand), typeof(ICommand), typeof(ToolsetCard), new PropertyMetadata(null));

        internal static readonly DependencyProperty CancelCommandProperty = 
            DependencyProperty.Register(nameof(CancelCommand), typeof(ICommand), typeof(ToolsetCard), new PropertyMetadata(null));

        internal static readonly DependencyProperty IsDownloadingProperty = 
            DependencyProperty.Register(nameof(IsDownloading), typeof(bool), typeof(ToolsetCard), new PropertyMetadata(false));

        internal static readonly DependencyProperty ProgressProperty = 
            DependencyProperty.Register(nameof(Progress), typeof(double), typeof(ToolsetCard), new PropertyMetadata(0.0));

        internal static readonly DependencyProperty IsSquareIconProperty = 
            DependencyProperty.Register(nameof(IsSquareIcon), typeof(bool), typeof(ToolsetCard), new PropertyMetadata(false));

        internal ImageSource AppIcon
        {
            get => (ImageSource)GetValue(AppIconProperty);
            set => SetValue(AppIconProperty, value);
        }

        internal string AppName
        {
            get => (string)GetValue(AppNameProperty);
            set => SetValue(AppNameProperty, value);
        }

        internal ImageSource AuthorIcon
        {
            get => (ImageSource)GetValue(AuthorIconProperty);
            set => SetValue(AuthorIconProperty, value);
        }

        internal string AuthorName
        {
            get => (string)GetValue(AuthorNameProperty);
            set => SetValue(AuthorNameProperty, value);
        }

        internal string SourceUrl
        {
            get => (string)GetValue(SourceUrlProperty);
            set => SetValue(SourceUrlProperty, value);
        }

        internal ICommand DownloadCommand
        {
            get => (ICommand)GetValue(DownloadCommandProperty);
            set => SetValue(DownloadCommandProperty, value);
        }

        internal ICommand CancelCommand
        {
            get => (ICommand)GetValue(CancelCommandProperty);
            set => SetValue(CancelCommandProperty, value);
        }

        internal bool IsDownloading
        {
            get => (bool)GetValue(IsDownloadingProperty);
            set => SetValue(IsDownloadingProperty, value);
        }

        internal double Progress
        {
            get => (double)GetValue(ProgressProperty);
            set => SetValue(ProgressProperty, value);
        }

        internal bool IsSquareIcon
        {
            get => (bool)GetValue(IsSquareIconProperty);
            set => SetValue(IsSquareIconProperty, value);
        }

        public ToolsetCard()
        {
            InitializeComponent();
        }

        private void OpenSource_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!string.IsNullOrEmpty(SourceUrl))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = SourceUrl,
                    UseShellExecute = true
                });
            }
        }
    }
}