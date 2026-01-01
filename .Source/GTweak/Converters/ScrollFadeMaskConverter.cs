using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using GTweak.Utilities.Controls;

namespace GTweak.Converters
{
    public class ScrollFadeMaskConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 3 || values[0] == DependencyProperty.UnsetValue || values[1] == DependencyProperty.UnsetValue || values[2] == DependencyProperty.UnsetValue)
            {
                return Brushes.White;
            }

            double offset = (double)values[0];
            double viewportHeight = (double)values[1];
            double extentHeight = (double)values[2];

            if (viewportHeight <= 0 || extentHeight <= 0)
            {
                return Brushes.White;
            }

            double scrollableHeight = Math.Max(0, extentHeight - viewportHeight);

            if (scrollableHeight <= 0)
            {
                return Brushes.White;
            }

            double fadeHeight = Math.Min(30.0, viewportHeight * 0.5);

            if (fadeHeight < 4)
            {
                return Brushes.White;
            }

            double epsilon = 0.5;

            double visibleStart = offset;
            double visibleEnd = offset + viewportHeight;

            bool showTopFade = visibleStart > epsilon;
            bool showBottomFade = visibleEnd < extentHeight - epsilon;

            double topFadeEnd = fadeHeight / viewportHeight;
            double bottomFadeStart = 1.0 - (fadeHeight / viewportHeight);

            LinearGradientBrush brush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(0, 1)
            };

            if (showTopFade)
            {
                brush.GradientStops.Add(new GradientStop(Colors.Transparent, 0.0));
                brush.GradientStops.Add(new GradientStop(Color.FromArgb(0xAA, 0xFF, 0xFF, 0xFF), topFadeEnd * 0.5));
                brush.GradientStops.Add(new GradientStop(Colors.White, topFadeEnd));
            }
            else
            {
                brush.GradientStops.Add(new GradientStop(Colors.White, 0.0));
                brush.GradientStops.Add(new GradientStop(Colors.White, topFadeEnd));
            }

            brush.GradientStops.Add(new GradientStop(Colors.White, bottomFadeStart));

            if (showBottomFade)
            {
                brush.GradientStops.Add(new GradientStop(Colors.White, bottomFadeStart));
                brush.GradientStops.Add(new GradientStop(Color.FromArgb(0xAA, 0xFF, 0xFF, 0xFF), bottomFadeStart + (topFadeEnd * 0.5)));
                brush.GradientStops.Add(new GradientStop(Colors.Transparent, 1.0));
            }
            else
            {
                brush.GradientStops.Add(new GradientStop(Colors.White, 1.0));
            }

            return brush;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            ErrorLogging.LogWritingFile(new NotImplementedException());
            return Array.Empty<object>();
        }
    }
}
