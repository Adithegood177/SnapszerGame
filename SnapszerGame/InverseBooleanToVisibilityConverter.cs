using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SnapszerGame
{
    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool flag = false;
            if (value is bool b) flag = b;
            else bool.TryParse(value?.ToString(), out flag);
            return flag ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility v)
            {
                return v != Visibility.Visible;
            }
            return Binding.DoNothing;
        }
    }
}
