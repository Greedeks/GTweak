using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using GTweak.Utilities.Animation;

namespace GTweak.Utilities.Managers
{
    internal static class OverlayDialogManager
    {
        private static FrameworkElement _overlay;
        private static TextBlock _tbTitle, _tbText, _tbQuestion;
        private static Button _btnPrimary, _btnSecondary;

        private static TaskCompletionSource<bool?> _tcs;

        internal static void Initialize(FrameworkElement overlay, TextBlock tbTitle, TextBlock tbText, TextBlock tbQuestion, Button btnPrimary, Button btnSecondary)
        {
            _overlay = overlay;
            _tbTitle = tbTitle;
            _tbText = tbText;
            _tbQuestion = tbQuestion;
            _btnPrimary = btnPrimary;
            _btnSecondary = btnSecondary;
        }

        internal static async Task<bool?> Show(string titleKey, string textKey, string questionKey, string btnPrimaryKey, string btnSecondaryKey)
        {
            if (_overlay != null)
            {
                Close();

                _tbTitle.SetResourceReference(TextBlock.TextProperty, titleKey);
                _tbText.SetResourceReference(TextBlock.TextProperty, textKey);
                _tbQuestion.SetResourceReference(TextBlock.TextProperty, questionKey);
                _btnPrimary.SetResourceReference(ContentControl.ContentProperty, btnPrimaryKey);
                _btnSecondary.SetResourceReference(ContentControl.ContentProperty, btnSecondaryKey);

                _tcs = new TaskCompletionSource<bool?>();

                static void onPrimary(object s, RoutedEventArgs e) => _tcs?.TrySetResult(true);
                static void onSecondary(object s, RoutedEventArgs e) => _tcs?.TrySetResult(false);

                _btnPrimary.Click += onPrimary;
                _btnSecondary.Click += onSecondary;

                _overlay.Visibility = Visibility.Visible;
                _overlay.BeginAnimation(UIElement.OpacityProperty, FactoryAnimation.CreateIn(0, 1, 0.3));

                bool? result = await _tcs.Task;

                _btnPrimary.Click -= onPrimary;
                _btnSecondary.Click -= onSecondary;

                Close();

                return result;
            }

            return null;
        }

        internal static void Close()
        {
            if (_tcs != null && !_tcs.Task.IsCompleted)
            {
                _tcs.TrySetResult(null);
            }
            else if (_overlay != null && _overlay.Visibility == Visibility.Visible)
            {
                _overlay.Visibility = Visibility.Collapsed;
            }
        }
    }
}