using System.Windows;
using System.Windows.Controls;

namespace GTweak.Utilities.Behaviors
{
    internal class TextBoxClearBehavior
    {
        public static readonly DependencyProperty ClearButtonProperty =
            DependencyProperty.RegisterAttached("ClearButton", typeof(bool), typeof(TextBoxClearBehavior), new PropertyMetadata(false, OnClearButtonChanged));

        public static bool GetClearButton(UIElement element) => (bool)element.GetValue(ClearButtonProperty);
        public static void SetClearButton(UIElement element, bool value) => element.SetValue(ClearButtonProperty, value);

        private static void OnClearButtonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox)
            {
                textBox.Loaded -= TextBox_Loaded;
                textBox.TextChanged -= TextBox_TextChanged;

                if (e.NewValue is bool isEnabled && isEnabled)
                {
                    textBox.Loaded += TextBox_Loaded;
                    textBox.TextChanged += TextBox_TextChanged;
                }
            }
        }

        private static void TextBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                UpdateVisibility(textBox);

                if (textBox.Template?.FindName("PART_ClearButton", textBox) is Button clearButton)
                {
                    clearButton.Click -= ClearButton_Click;
                    clearButton.Click += ClearButton_Click;
                }
            }
        }

        private static void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                UpdateVisibility(textBox);
            }
        }

        private static void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button clearButton && clearButton.TemplatedParent is TextBox textBox)
            {
                textBox.Clear();
                textBox.Focus();
            }
        }

        private static void UpdateVisibility(TextBox textBox)
        {
            if (textBox.Template?.FindName("PART_ClearButton", textBox) is Button clearButton)
            {
                clearButton.Visibility = string.IsNullOrEmpty(textBox.Text) ? Visibility.Collapsed : Visibility.Visible;
            }
        }
    }
}