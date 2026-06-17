using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GTweak.Core.Converters
{
    internal class UsageToAngleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str && double.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out double usage))
            {
                return Math.Min(usage * 3.6, 359.9);
            }

            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => DependencyProperty.UnsetValue;
    }
}
