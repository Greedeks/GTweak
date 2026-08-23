using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using GTweak.Animations;
using GTweak.Behaviors;
using GTweak.Modules.Common;
using GTweak.Modules.Configuration;
using GTweak.Modules.Extensions;
using GTweak.Modules.Managers;
using Wpf.Ui.Controls;

namespace GTweak.Windows
{
    public partial class MainWindow : FluentWindow
    {
        private static readonly BitmapCache _bitmapCache = new BitmapCache { RenderAtScale = 1, EnableClearType = false };
        private bool _ignoreMouseClick = false;
        private RadioButton _activeBtnCache;

        public MainWindow()
        {
            InitializeComponent();
            OverlayDialogManager.Initialize(OverlayDialog, DialogTitle, DialogText, DialogQuestion, DialogBtnPrimary, DialogBtnSecondary);
            App.TweaksImported += delegate { BtnUtils.IsChecked = true; };
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            Rect area = SystemParameters.WorkArea;

            double rawWidth = area.Width * (area.Width <= 1600 ? 0.82 : 0.62);
            Width = Math.Max(1150, Math.Min(rawWidth, 1500));

            if (Width > area.Width)
            {
                Width = area.Width * 0.96;
            }

            Height = Math.Min(Width / 1.8, area.Height * 0.90);

            Left = area.Left + (area.Width - Width) / 2;
            Top = area.Top + (area.Height - Height) / 2;
        }

        #region TitleBar
        private void HandleWindowState(bool isMinimized = false) => WindowState = isMinimized ? WindowState.Minimized : WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e?.ChangedButton == MouseButton.Left)
            {
                if (e?.OriginalSource is DependencyObject source)
                {
                    DependencyObject current = source;
                    while (current != null)
                    {
                        if (current is ButtonBase)
                        {
                            return;
                        }

                        current = VisualTreeHelper.GetParent(current);
                    }
                }

                Dispatcher.BeginInvoke((Action)(() => { _ignoreMouseClick = true; TitleButtonsPanel.IsHitTestVisible = false; }));

                if (e?.ClickCount == 2)
                {
                    HandleWindowState();
                }
                else
                {
                    this.Drag();
                }

                Dispatcher.BeginInvoke((Action)(() => { _ignoreMouseClick = false; TitleButtonsPanel.IsHitTestVisible = true; }));
            }
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e) => Close();

        private void ButtonMaximize_Click(object sender, RoutedEventArgs e) => HandleWindowState();

        private void ButtonMinimize_Click(object sender, RoutedEventArgs e) => HandleWindowState(true);

        private void TglButtonSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsPanel.CacheMode = _bitmapCache;

            if (SettingsPanel.RenderTransform is TranslateTransform transform)
            {
                Dispatcher.InvokeAsync(() =>
                {
                    transform.BeginAnimation(TranslateTransform.XProperty, AnimationFactory.CreateIn(transform.X, TglButtonSettings.IsChecked == true ? 0 : 400, 0.5, () => { SettingsPanel.CacheMode = null; }, useCubicEase: true));
                }, DispatcherPriority.Render);
            }
        }

        private void TglButtonTheme_Click(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
            GlobalOptions.SelfReboot();
        }
        #endregion

        #region Navigation & Scrolling
        private void NavigationScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is ScrollViewer scrollViewer)
            {
                if (e.HeightChanged && _activeBtnCache != null && scrollViewer.ScrollableHeight > 0)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (_activeBtnCache.IsLoaded)
                        {
                            _activeBtnCache.BringIntoView();
                        }
                    }), DispatcherPriority.Loaded);
                }
            }
        }

        private void NavigationRtbn_Checked(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is RadioButton radioButton)
            {
                _activeBtnCache = radioButton;

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (radioButton.IsLoaded && radioButton.IsDescendantOf(NavPanel))
                    {
                        double margin = radioButton.ActualHeight;

                        Point btnPos = radioButton.TranslatePoint(new Point(0, 0), NavPanel);

                        double btnTop = btnPos.Y, btnBottom = btnTop + margin, viewTop = NavScroll.VerticalOffset,
                        viewBottom = viewTop + NavScroll.ViewportHeight, targetOffset = viewTop;

                        if (btnTop - margin < viewTop)
                        {
                            targetOffset = btnTop - margin;
                        }
                        else if (btnBottom + margin > viewBottom)
                        {
                            targetOffset = btnBottom + margin - NavScroll.ViewportHeight;
                        }

                        if (targetOffset != viewTop)
                        {
                            ScrollViewerBehavior.SetVerticalOffset(NavScroll, viewTop);
                            NavScroll.BeginAnimation(ScrollViewerBehavior.VerticalOffsetProperty, AnimationFactory.CreateIn(viewTop, targetOffset, 0.5, useCubicEase: true));
                        }
                    }
                }), DispatcherPriority.Loaded);
            }
        }
        #endregion

        private void BtnContacts_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Process.Start(new ProcessStartInfo(((System.Windows.Controls.Image)sender).Uid switch
            {
                "git" => PathTargets.Links.GitHub,
                "tg" => PathTargets.Links.Telegram,
                _ => PathTargets.Links.Steam
            })
            { UseShellExecute = true });
        }

        private void BtnUpdate_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            UpdateBanner.BeginAnimation(OpacityProperty, AnimationFactory.CreateIn(1, 0, 0.3, () => { UpdateBanner.Visibility = Visibility.Collapsed; }));
            Dispatcher.Invoke(() => new UpdateWindow().ShowDialog());
        }

        private void ContentControl_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_ignoreMouseClick)
            {
                e.Handled = true;
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TypewriterAnimation.Create(TitleName.Text, TitleName, TimeSpan.FromSeconds(0.4));

            if (NetworkProvider.IsNeedUpdate && GlobalOptions.IsUpdateCheckRequired)
            {
                await Task.Delay(500);

                UpdateBanner.Visibility = Visibility.Visible;
                UpdateBanner.BeginAnimation(OpacityProperty, AnimationFactory.CreateIn(0, 1, 0.2));
                if (UpdateBanner.RenderTransform is TranslateTransform transform)
                {
                    transform.BeginAnimation(TranslateTransform.YProperty, AnimationFactory.CreateIn(-20, 0, 0.3, useCubicEase: true));
                }
            }
        }
    }
}
