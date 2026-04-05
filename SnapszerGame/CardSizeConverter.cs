using System;
using System.Globalization;
using System.Windows.Data;

namespace SnapszerGame
{
    // MultiValueConverter: values[0] = ActualWidth (double), values[1] = HasAduChosen (bool)
    // parameter: two numbers separated by ';' -> baseFactor;chosenScale
    public class CardSizeConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2) return 0.0;
            if (!double.TryParse(values[0]?.ToString(), out double actualWidth)) return 0.0;
            bool hasAdu = false;
            if (values[1] is bool b) hasAdu = b;
            else bool.TryParse(values[1]?.ToString(), out hasAdu);

            double baseFactor = 0.06;
            double chosenScale = 1.5;
            if (parameter != null)
            {
                var parts = parameter.ToString().Split(new[] {';',','}, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 1) double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out baseFactor);
                if (parts.Length >= 2) double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out chosenScale);
            }

            double scale = hasAdu ? chosenScale : 1.0;
            return actualWidth * baseFactor * scale;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
