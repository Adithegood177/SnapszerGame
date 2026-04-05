using System;
using System.Globalization;
using System.Windows.Data;

namespace SnapszerGame
{
    // Multiplies two numeric values provided by a MultiBinding
    public class MultiMultiplyConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2) return 0;
            if (!double.TryParse(values[0]?.ToString(), out double a)) return 0;
            if (!double.TryParse(values[1]?.ToString(), out double b)) return 0;
            return a * b;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
