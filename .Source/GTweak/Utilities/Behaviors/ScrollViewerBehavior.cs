using System.Windows;
using System.Windows.Controls;

namespace GTweak.Utilities.Behaviors
{
    internal static class ScrollViewerBehavior
    {
        internal static readonly DependencyProperty VerticalOffsetProperty =
            DependencyProperty.RegisterAttached("VerticalOffset", typeof(double), typeof(ScrollViewerBehavior), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnVerticalOffsetChanged));

        internal static double GetVerticalOffset(ScrollViewer viewer) => (double)(viewer?.GetValue(VerticalOffsetProperty) ?? 0.0);
        internal static void SetVerticalOffset(ScrollViewer viewer, double value) => viewer?.SetValue(VerticalOffsetProperty, value);

        private static void OnVerticalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollViewer viewer && e.NewValue is double offset)
            {
                viewer.ScrollToVerticalOffset(offset);
            }
        }
    }
}
