using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using GTweak.Utilities.Animation;
using GTweak.Utilities.Configuration;
using GTweak.Utilities.Controls;
using GTweak.Utilities.Managers;
using Wpf.Ui.Controls;

namespace GTweak.Windows
{
    public partial class MainWindow : FluentWindow
    {
        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        internal static readonly DependencyProperty CurrentVerticalOffsetProperty =
            DependencyProperty.RegisterAttached("CurrentVerticalOffset", typeof(double), typeof(MainWindow), new PropertyMetadata(0.0, OnCurrentVerticalOffsetChanged));

        internal static void SetCurrentVerticalOffset(DependencyObject target, double value) => target.SetValue(CurrentVerticalOffsetProperty, value);

        private static void OnCurrentVerticalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollViewer scrollViewer)
            {
                scrollViewer.ScrollToVerticalOffset((double)e.NewValue);
            }
        }

        private const int WM_NCLBUTTONDOWN = 0xA1, HTCAPTION = 0x2;
        private bool _settingsOpen = false, _ignoreMouseClick = false;
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

        private void AnimateSettings()
        {
            TranslateTransform transform = (TranslateTransform)SettingsPanel.RenderTransform;
            double toX = _settingsOpen ? 400 : 0;
            _settingsOpen = !_settingsOpen;
            transform.BeginAnimation(TranslateTransform.XProperty, FactoryAnimation.CreateIn(transform.X, toX, 0.5, useCubicEase: true));
        }

        private void HandleWindowState() => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

        #region TitleBar
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

                _ignoreMouseClick = true;
                TitleButtonsPanel.IsHitTestVisible = false;

                if (e?.ClickCount == 2)
                {
                    HandleWindowState();
                    return;
                }

                IntPtr hwnd = new WindowInteropHelper(this).Handle;
                ReleaseCapture();
                SendMessage(hwnd, WM_NCLBUTTONDOWN, (IntPtr)HTCAPTION, IntPtr.Zero);

                Dispatcher.BeginInvoke((Action)(() => { _ignoreMouseClick = false; TitleButtonsPanel.IsHitTestVisible = true; }));
            }
        }

        private void TitleButton_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement)?.Name)
            {
                case nameof(ButtonClose):
                    Close();
                    break;
                case nameof(ButtonMaximize):
                    HandleWindowState();
                    break;
                case nameof(ButtonMinimize):
                    WindowState = WindowState.Minimized;
                    break;
                case nameof(ButtonSettings):
                    AnimateSettings();
                    break;
                case nameof(ButtonTheme):
                    SettingsEngine.SelfReboot();
                    Visibility = Visibility.Collapsed;
                    break;
                default:
                    break;
            }
        }

        private void TglButton_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e?.Key == Key.Space || e?.Key == Key.Enter)
            {
                e.Handled = true;
            }
        }
        #endregion

        #region Settings Panel
        private void BtnNotification_ChangedState(object sender, RoutedEventArgs e) => SettingsEngine.IsViewNotification = !BtnNotification.State;

        private void BtnUpdate_ChangedState(object sender, RoutedEventArgs e) => SettingsEngine.IsUpdateCheckRequired = !BtnUpdate.State;

        private void BtnTopMost_ChangedState(object sender, RoutedEventArgs e) => SettingsEngine.IsTopMost = Topmost = !BtnTopMost.State;

        private void BtnVolume_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) => SettingsEngine.IsPlayingSound = (bool)!BtnVolume.IsChecked;

        private void SliderVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SliderVolume.Value = SliderVolume.Value == 0 ? 1 : SliderVolume.Value;
            SettingsEngine.Volume = (int)SliderVolume.Value;
            SettingsEngine.waveOutSetVolume(IntPtr.Zero, ((uint)(double)(ushort.MaxValue / 100 * SliderVolume.Value) & 0x0000ffff) | ((uint)(double)(ushort.MaxValue / 100 * SliderVolume.Value) << 16));
        }

        private void BtnExport_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) => SettingsEngine.SaveFileConfig();

        private void BtnImport_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) => SettingsEngine.OpenFileConfig();

        private void BtnDelete_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) => SettingsEngine.SelfRemoval();

        private void BtnContacts_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Process.Start(new ProcessStartInfo(((System.Windows.Controls.Image)sender).Uid switch
            {
                "git" => PathLocator.Links.GitHub,
                "tg" => PathLocator.Links.Telegram,
                _ => PathLocator.Links.Steam
            })
            { UseShellExecute = true });
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
                            SetCurrentVerticalOffset(NavScroll, viewTop);
                            NavScroll.BeginAnimation(CurrentVerticalOffsetProperty, FactoryAnimation.CreateIn(viewTop, targetOffset, 0.5, useCubicEase: true));
                        }
                    }
                }), DispatcherPriority.Loaded);
            }
        }
        #endregion

        private void BtnUpdate_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            UpdateBanner.BeginAnimation(OpacityProperty, FactoryAnimation.CreateIn(1, 0, 0.3, () => { UpdateBanner.Visibility = Visibility.Collapsed; }));
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

            if (NetworkProvider.IsNeedUpdate && SettingsEngine.IsUpdateCheckRequired)
            {
                await Task.Delay(500);

                UpdateBanner.Visibility = Visibility.Visible;

                UpdateBanner.BeginAnimation(OpacityProperty, FactoryAnimation.CreateIn(0, 1, 0.3));

                if (UpdateBanner.RenderTransform is TranslateTransform transform)
                {
                    transform.BeginAnimation(TranslateTransform.YProperty, FactoryAnimation.CreateIn(-20, 0, 0.3, useCubicEase: true));
                }
            }
        }
    }
}
