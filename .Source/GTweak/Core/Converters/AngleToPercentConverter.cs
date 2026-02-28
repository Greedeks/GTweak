using System;
using System.Globalization;
using System.Windows.Data;

namespace GTweak.Core.Converters
{
    internal class AngleToPercentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double angle)
            {
                double percent = (angle / 360.0) * 100.0;
                return Math.Round(Math.Max(0, Math.Min(100, percent)));
            }
            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && double.TryParse(value.ToString(), NumberStyles.Any, culture, out double percent))
            {
                double angle = (percent / 100.0) * 360.0;
                return Math.Max(0, Math.Min(360, angle));
            }
            return 0.0;
        }
    }
}
