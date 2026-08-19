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
        private readonly KeyboardHookBlocker keyboardHook = new KeyboardHookBlocker();
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

                if (keyboardHook.ptrHook != IntPtr.Zero)
                {
                    KeyboardHookBlocker.UnhookWindowsHookEx(keyboardHook.ptrHook);
                    keyboardHook.ptrHook = IntPtr.Zero;
                }

                BeginAnimation(OpacityProperty, AnimationFactory.CreateTo(0.15, () => { Close(); }));
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ProcessModule objCurrentModule = Process.GetCurrentProcess().MainModule;
            keyboardHook.objKeyboardProcess = new KeyboardHookBlocker.LowLevelKeyboardProc(keyboardHook.CaptureKey);
            keyboardHook.ptrHook = KeyboardHookBlocker.SetWindowsHookEx(13, keyboardHook.objKeyboardProcess, KeyboardHookBlocker.GetModuleHandle(objCurrentModule.ModuleName), 0);
            BeginAnimation(OpacityProperty, AnimationFactory.CreateIn(0, 0.5, 0.3));
        }
    }
}
