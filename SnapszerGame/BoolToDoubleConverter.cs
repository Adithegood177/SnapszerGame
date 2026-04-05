using System;
using System.Globalization;
using System.Windows.Data;

namespace SnapszerGame
{
    // Converts a boolean to one of two double values provided in ConverterParameter as "trueValue;falseValue"
    public class BoolToDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool b = false;
            if (value is bool bv) b = bv;
            else bool.TryParse(value?.ToString(), out b);

            double trueVal = 1.0;
            double falseVal = 1.0;
            if (parameter != null)
            {
                var parts = parameter.ToString().Split(';');
                if (parts.Length > 0) double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out trueVal);
                if (parts.Length > 1) double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out falseVal);
            }

            return b ? trueVal : falseVal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
