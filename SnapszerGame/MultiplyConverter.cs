using System;
using System.Globalization;
using System.Windows.Data;

namespace SnapszerGame
{
    // Converts a numeric value by multiplying with ConverterParameter (double)
    public class MultiplyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return 0;
            if (!double.TryParse(value.ToString(), out double v)) return 0;

            double factor = 1.0;
            if (parameter != null)
            {
                double.TryParse(parameter.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out factor);
            }

            return v * factor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
