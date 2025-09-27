using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace GTweak.Utilities.Interop
{
    /// <summary>
    /// Restores standard system window behaviors (animations, edge snapping, system menu)
    /// for windows with <see cref="WindowStyle.None"/>.
    /// To enable runtime window resizing, add the WS_THICKFRAME style (0x00040000).
    /// </summary>
    internal static class WindowStyleHelper
    {
        private const int GWL_STYLE = -16;
        private const int WS_CAPTION = 0x00C00000;
        private const int WS_SYSMENU = 0x00080000;
        private const int WS_MINIMIZEBOX = 0x00020000;
        private const int WS_MAXIMIZEBOX = 0x00010000;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        internal static readonly DependencyProperty EnableSystemAnimationsProperty = DependencyProperty.RegisterAttached("EnableSystemAnimations", typeof(bool), typeof(WindowStyleHelper), new PropertyMetadata(false, OnEnableSystemAnimationsChanged));

        internal static void SetEnableSystemAnimations(DependencyObject element, bool value) => element.SetValue(EnableSystemAnimationsProperty, value);

        internal static bool GetEnableSystemAnimations(DependencyObject element) => (bool)element.GetValue(EnableSystemAnimationsProperty);

        private static void OnEnableSystemAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Window window && (bool)e.NewValue)
            {
                window.SourceInitialized += (s, ev) =>
                {
                    IntPtr hwnd = new WindowInteropHelper(window).Handle;
                    int style = GetWindowLong(hwnd, GWL_STYLE);

                    style |= WS_CAPTION | WS_SYSMENU | WS_MINIMIZEBOX | WS_MAXIMIZEBOX;

                    SetWindowLong(hwnd, GWL_STYLE, style);
                };
            }
        }
    }
}
