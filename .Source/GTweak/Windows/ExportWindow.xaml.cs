using System;
using System.Windows;
using System.Windows.Input;
using GTweak.Modules.Extensions;
using Wpf.Ui.Controls;

namespace GTweak.Windows
{
    public partial class ExportWindow : FluentWindow
    {
        public ExportWindow()
        {
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            Rect area = SystemParameters.WorkArea;

            double targetWidth = Math.Min(860, area.Width * 0.88);
            Width = Math.Max(MinWidth, targetWidth);

            if (Width > area.Width)
            {
                Width = area.Width * 0.96;
                MinWidth = Math.Min(MinWidth, Width);
            }

            double targetHeight = Math.Min(640, area.Height * 0.85);
            Height = Math.Max(MinHeight, targetHeight);

            if (Height > area.Height)
            {
                Height = area.Height * 0.92;
                MinHeight = Math.Min(MinHeight, Height);
            }

            Left = area.Left + (area.Width - Width) / 2;
            Top = area.Top + (area.Height - Height) / 2;
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e?.ClickCount == 2)
            {
                HandleWindowState();
            }
            else
            {
                this.Drag();
            }
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e) => Close();

        private void ButtonMaximize_Click(object sender, RoutedEventArgs e) => HandleWindowState();

        private void ButtonMinimize_Click(object sender, RoutedEventArgs e) => HandleWindowState(true);

        private void HandleWindowState(bool isMinimized = false) => WindowState = isMinimized ? WindowState.Minimized : WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

    }
}