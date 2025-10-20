using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace KlarfApplication.Converter
{
    public class ColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isGood)
            {
                return isGood ? Brushes.LimeGreen : Brushes.Red;
            }
            return Brushes.Gray; // null fallback
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
