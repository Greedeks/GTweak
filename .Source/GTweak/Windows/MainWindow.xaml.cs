using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using GTweak.Utilities.Animation;
using GTweak.Utilities.Configuration;
using GTweak.Utilities.Controls;
using Wpf.Ui.Controls;

namespace GTweak.Windows
{
    public partial class MainWindow : FluentWindow
    {
        private bool _settingsOpen = false, _draggingFromMaximized = false, _ignoreMouseClick = false;
        private Point? _lastNormalPosition;
        private Size? _lastNormalSize;
        private Point _mouseDownWindowPoint;

        public MainWindow()
        {
            InitializeComponent();
            App.TweaksImported += delegate { BtnUtils.IsChecked = true; };
        }

        private void AnimateSettings()
        {
            TranslateTransform transform = (TranslateTransform)SettingsPanel.RenderTransform;
            double toX = _settingsOpen ? 400 : 0;
            _settingsOpen = !_settingsOpen;
            transform.BeginAnimation(TranslateTransform.XProperty, FactoryAnimation.CreateIn(transform.X, toX, 0.5, useCubicEase: true));
        }

        #region TitleBar
        private void HandleWindowState()
        {
            if (WindowState == WindowState.Normal)
            {
                _lastNormalPosition = new Point(Left, Top);
                _lastNormalSize = new Size(Width, Height);
                WindowState = WindowState.Maximized;
            }
            else
            {
                WindowState = WindowState.Normal;
            }
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e?.ChangedButton == MouseButton.Left && e?.ClickCount != 2)
            {
                if (WindowState == WindowState.Maximized)
                {
                    if (e != null)
                    {
                        _mouseDownWindowPoint = e.GetPosition(this);
                        _draggingFromMaximized = true;
                    }
                }
                else
                {
                    DragMove();
                }
            }
        }

        private void TitleBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (_draggingFromMaximized)
            {
                if (e?.LeftButton != MouseButtonState.Pressed)
                {
                    _draggingFromMaximized = false;
                    return;
                }

                Point currentScreen = PointToScreen(e.GetPosition(this));
                Vector delta = currentScreen - PointToScreen(_mouseDownWindowPoint);

                if (Math.Abs(delta.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(delta.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    _draggingFromMaximized = false;

                    Size restoreSize = _lastNormalSize ?? new Size(RestoreBounds.Width, RestoreBounds.Height);
                    double restoreWidth = restoreSize.Width;
                    double restoreHeight = restoreSize.Height;

                    double relX = ActualWidth > 0 ? _mouseDownWindowPoint.X / ActualWidth : 0.5;

                    WindowState = WindowState.Normal;
                    Width = restoreWidth;
                    Height = restoreHeight;

                    double left = currentScreen.X - restoreWidth * relX;
                    double top = currentScreen.Y - _mouseDownWindowPoint.Y;

                    left = Math.Max(0, Math.Min(Math.Max(0, SystemParameters.PrimaryScreenWidth - restoreWidth), left));
                    top = Math.Max(0, Math.Min(Math.Max(0, SystemParameters.PrimaryScreenHeight - restoreHeight), top));

                    Left = left;
                    Top = top;

                    Dispatcher.BeginInvoke((Action)(() => { DragMove(); }));
                }
            }
        }

        private void TitleBar_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) => _draggingFromMaximized = false;

        private async void TitleBar_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e?.ChangedButton == MouseButton.Left && e?.ClickCount == 2)
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
                HandleWindowState();

                if (WindowState == WindowState.Maximized && _lastNormalPosition.HasValue && _lastNormalSize.HasValue)
                {
                    Point pos = _lastNormalPosition.Value;
                    Size sz = _lastNormalSize.Value;

                    Left = pos.X;
                    Top = pos.Y;
                    Width = sz.Width;
                    Height = sz.Height;
                }

                await Task.Delay(100).ConfigureAwait(false);
                _ignoreMouseClick = false;

                if (e != null)
                {
                    e.Handled = true;
                }
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

        #region SettingsPanel
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
                "git" => "https://github.com/Greedeks",
                "tg" => "https://t.me/Greedeks",
                _ => "https://steamcommunity.com/id/greedeks/"
            })
            { UseShellExecute = true });
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
            (double screenWidth, double screenHeight) = (SystemParameters.PrimaryScreenWidth, SystemParameters.PrimaryScreenHeight);

            Width = screenWidth * 0.61;
            Height = screenHeight * 0.60;

            Left = (screenWidth - Width) / 2;
            Top = (screenHeight - Height) / 2;

            TypewriterAnimation.Create(TitleName.Text, TitleName, TimeSpan.FromSeconds(0.4));

            if (SystemDiagnostics.IsNeedUpdate && SettingsEngine.IsUpdateCheckRequired)
            {
                await Task.Delay(500);

                UpdateBanner.Visibility = Visibility.Visible;
                UpdateBanner.BeginAnimation(OpacityProperty, FactoryAnimation.CreateIn(0, 1, 0.3));
                (UpdateBanner.RenderTransform as TranslateTransform).BeginAnimation(TranslateTransform.YProperty, FactoryAnimation.CreateIn(-20, 0, 0.3, useCubicEase: true));
            }
        }
    }
}
