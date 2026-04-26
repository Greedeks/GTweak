using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GTweak.Utilities.Behaviors
{
    internal static class ScrollFadeBehavior
    {
        public static readonly DependencyProperty EnableFadeProperty = DependencyProperty.RegisterAttached("EnableFade", typeof(bool), typeof(ScrollFadeBehavior), new PropertyMetadata(false, OnEnableFadeChanged));

        public static bool GetEnableFade(DependencyObject obj) => (bool)obj.GetValue(EnableFadeProperty);
        public static void SetEnableFade(DependencyObject obj, bool value) => obj.SetValue(EnableFadeProperty, value);

        private static void OnEnableFadeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollViewer scrollViewer)
            {
                if ((bool)e.NewValue)
                {
                    scrollViewer.Loaded += ScrollViewer_Loaded;
                    scrollViewer.Unloaded += ScrollViewer_Unloaded;

                    if (scrollViewer.IsLoaded)
                    {
                        AttachScrollEvents(scrollViewer);
                        InitializeOpacityMask(scrollViewer);
                        UpdateMask(scrollViewer);
                    }
                }
                else
                {
                    scrollViewer.Loaded -= ScrollViewer_Loaded;
                    scrollViewer.Unloaded -= ScrollViewer_Unloaded;
                    DetachScrollEvents(scrollViewer);
                    scrollViewer.OpacityMask = null;
                }
            }
        }

        private static void ScrollViewer_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is ScrollViewer scrollViewer)
            {
                AttachScrollEvents(scrollViewer);
                InitializeOpacityMask(scrollViewer);

                scrollViewer.Dispatcher.BeginInvoke(new Action(() => UpdateMask(scrollViewer)), System.Windows.Threading.DispatcherPriority.Render);
            }
        }

        private static void ScrollViewer_Unloaded(object sender, RoutedEventArgs e)
        {
            if (sender is ScrollViewer scrollViewer)
            {
                DetachScrollEvents(scrollViewer);
                scrollViewer.OpacityMask = null;
            }
        }

        private static void AttachScrollEvents(ScrollViewer scrollViewer)
        {
            scrollViewer.ScrollChanged -= ScrollViewer_ScrollChanged;
            scrollViewer.SizeChanged -= ScrollViewer_SizeChanged;

            scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
            scrollViewer.SizeChanged += ScrollViewer_SizeChanged;
        }

        private static void DetachScrollEvents(ScrollViewer scrollViewer)
        {
            scrollViewer.ScrollChanged -= ScrollViewer_ScrollChanged;
            scrollViewer.SizeChanged -= ScrollViewer_SizeChanged;
        }

        private static void ScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is ScrollViewer scrollViewer)
            {
                UpdateMask(scrollViewer);
            }
        }

        private static void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.VerticalChange != 0 || e.ViewportHeightChange != 0 || e.ExtentHeightChange != 0)
            {
                if (sender is ScrollViewer scrollViewer)
                {
                    UpdateMask(scrollViewer);
                }
            }
        }

        private static void InitializeOpacityMask(ScrollViewer scrollViewer)
        {
            if (scrollViewer.OpacityMask is LinearGradientBrush)
            {
                return;
            }

            LinearGradientBrush brush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(0, 1)
            };

            brush.GradientStops.Add(new GradientStop(Colors.Transparent, 0.0));
            brush.GradientStops.Add(new GradientStop(Colors.Black, 0.05));
            brush.GradientStops.Add(new GradientStop(Colors.Black, 0.95));
            brush.GradientStops.Add(new GradientStop(Colors.Transparent, 1.0));

            scrollViewer.OpacityMask = brush;
        }

        private static void UpdateMask(ScrollViewer scrollViewer)
        {
            if (scrollViewer.OpacityMask is LinearGradientBrush brush && brush.GradientStops.Count == 4)
            {
                if (scrollViewer.ScrollableHeight == 0)
                {
                    UpdateGradientStopIfNotEqual(brush.GradientStops[0], Colors.Black);
                    UpdateGradientStopIfNotEqual(brush.GradientStops[3], Colors.Black);
                    return;
                }

                Color targetTopColor = scrollViewer.VerticalOffset <= 0 ? Colors.Black : Colors.Transparent;
                Color targetBottomColor = scrollViewer.VerticalOffset >= (scrollViewer.ScrollableHeight - 1.0) ? Colors.Black : Colors.Transparent;

                UpdateGradientStopIfNotEqual(brush.GradientStops[0], targetTopColor);
                UpdateGradientStopIfNotEqual(brush.GradientStops[3], targetBottomColor);
            }
        }

        private static void UpdateGradientStopIfNotEqual(GradientStop stop, Color targetColor)
        {
            if (stop.Color != targetColor)
            {
                stop.Color = targetColor;
            }
        }
    }
}