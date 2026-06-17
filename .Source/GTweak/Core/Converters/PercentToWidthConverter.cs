using System;
using System.Globalization;
using System.Windows.Data;

namespace GTweak.Core.Converters
{
    internal class PercentToWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is string str && double.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out double percent) && values[1] is double totalWidth)
            {
                return Math.Max(0, Math.Min(totalWidth, totalWidth * percent / 100));
            }

            return 0.0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
