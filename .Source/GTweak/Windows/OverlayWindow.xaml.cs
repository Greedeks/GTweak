using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using GTweak.Utilities.Animation;
using GTweak.Utilities.Helpers;

namespace GTweak.Windows
{
    /// <summary>
    /// Darkened screen 
    /// </summary>

    public partial class OverlayWindow
    {
        private readonly DisablingWinKeys disablingWinKeys = new DisablingWinKeys();
        public OverlayWindow()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Closing -= Window_Closing;
            e.Cancel = true;
            BeginAnimation(OpacityProperty, FactoryAnimation.CreateTo(0.15, () =>
            {
                ProcessModule objCurrentModule = Process.GetCurrentProcess().MainModule;
                disablingWinKeys.objKeyboardProcess = new DisablingWinKeys.LowLevelKeyboardProc(disablingWinKeys.CaptureKey);
                disablingWinKeys.ptrHook = DisablingWinKeys.SetWindowsHookEx(13, disablingWinKeys.objKeyboardProcess, DisablingWinKeys.GetModuleHandle(objCurrentModule.ModuleName), 1);
                Close();
            }));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ProcessModule objCurrentModule = Process.GetCurrentProcess().MainModule;
            disablingWinKeys.objKeyboardProcess = new DisablingWinKeys.LowLevelKeyboardProc(disablingWinKeys.CaptureKey);
            disablingWinKeys.ptrHook = DisablingWinKeys.SetWindowsHookEx(13, disablingWinKeys.objKeyboardProcess, DisablingWinKeys.GetModuleHandle(objCurrentModule.ModuleName), 0);
            BeginAnimation(OpacityProperty, FactoryAnimation.CreateIn(0, 0.5, 0.3));
        }
    }
}
