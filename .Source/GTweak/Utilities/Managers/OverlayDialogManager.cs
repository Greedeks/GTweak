using GTweak.Utilities.Animation;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace GTweak.Utilities.Managers
{
    internal sealed class OverlayDialogManager
    {
        private readonly FrameworkElement _overlay;
        private readonly DependencyProperty _opacityProperty;
        private readonly Button _btnPrimary;
        private readonly Button _btnSecondary;
        private readonly Action _onPrimary;
        private readonly Action _onSecondary;

        internal OverlayDialogManager(FrameworkElement overlay, DependencyProperty opacityProperty, Button btnPrimary, Button btnSecondary, Action onPrimary = null, Action onSecondary = null)
        {
            _overlay = overlay;
            _opacityProperty = opacityProperty;
            _btnPrimary = btnPrimary;
            _btnSecondary = btnSecondary;
            _onPrimary = onPrimary;
            _onSecondary = onSecondary;
        }

        internal async Task<bool> Show()
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            void PrimaryHandler(object s, RoutedEventArgs e)
            {
                tcs.TrySetResult(true);
                _onPrimary?.Invoke();
                Detach();
            }
            void SecondaryHandler(object s, RoutedEventArgs e)
            {
                tcs.TrySetResult(false);
                _onSecondary?.Invoke();
                Detach();
            }

            void Detach()
            {
                _btnPrimary.PreviewMouseLeftButtonDown -= PrimaryHandler;
                _btnSecondary.PreviewMouseLeftButtonDown -= SecondaryHandler;
            }

            _overlay.Visibility = Visibility.Visible;
            _overlay.BeginAnimation(_opacityProperty, FactoryAnimation.CreateIn(0, 1, 0.3));

            _btnPrimary.PreviewMouseLeftButtonDown += PrimaryHandler;
            _btnSecondary.PreviewMouseLeftButtonDown += SecondaryHandler;

            bool result;
            try { result = await tcs.Task; }
            catch (TaskCanceledException) { result = false; }

            _overlay.BeginAnimation(_opacityProperty, FactoryAnimation.CreateTo(0.25, () => { _overlay.Visibility = Visibility.Collapsed; }));

            return result;
        }
    }
}
