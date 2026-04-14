using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SnapszerGame
{
    // Kis UI trükk: ha van cucc → látszik, ha nincs → eltűnik
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // ha null → nincs mit mutatni, szóval "eltüntetjük"
            return value == null ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // visszafelé ezt nem használjuk, szóval hagyjuk a fenébe
            throw new NotSupportedException();
        }
    }
}
