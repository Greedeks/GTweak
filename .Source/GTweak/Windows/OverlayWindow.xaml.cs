using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using GTweak.Animations;
using GTweak.Modules.Helpers;

namespace GTweak.Windows
{
    /// <summary>
    /// Darkened screen 
    /// </summary>
    public partial class OverlayWindow
    {
        private readonly KeyboardHookBlocker _keyboardHook = new KeyboardHookBlocker();
        public OverlayWindow()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (e != null)
            {
                Closing -= Window_Closing;
                e!.Cancel = true;

                if (_keyboardHook.ptrHook != IntPtr.Zero)
                {
                    KeyboardHookBlocker.UnhookWindowsHookEx(_keyboardHook.ptrHook);
                    _keyboardHook.ptrHook = IntPtr.Zero;
                }

                BeginAnimation(OpacityProperty, AnimationFactory.CreateTo(0.15, () => { Close(); }));
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ProcessModule objCurrentModule = Process.GetCurrentProcess().MainModule;
            _keyboardHook.objKeyboardProcess = new KeyboardHookBlocker.LowLevelKeyboardProc(_keyboardHook.CaptureKey);
            _keyboardHook.ptrHook = KeyboardHookBlocker.SetWindowsHookEx(13, _keyboardHook.objKeyboardProcess, KeyboardHookBlocker.GetModuleHandle(objCurrentModule.ModuleName), 0);
            BeginAnimation(OpacityProperty, AnimationFactory.CreateIn(0, 0.5, 0.3));
        }
    }
}
