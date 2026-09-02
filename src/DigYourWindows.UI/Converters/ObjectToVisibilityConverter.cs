using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DigYourWindows.UI.Converters
{
    /// <summary>
    /// Converts any reference value to Visibility: non-null returns Visible, null returns Collapsed.
    /// </summary>
    public class ObjectToVisibilityConverter : IValueConverter
    {
        public object Convert(object? value, System.Type targetType, object parameter, CultureInfo culture)
        {
            return value is not null ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            return System.Windows.DependencyProperty.UnsetValue;
        }
    }

    /// <summary>
    /// Converts bool to Visibility inverted: false/null returns Visible, true returns Collapsed.
    /// </summary>
    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            return value is true ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            return System.Windows.DependencyProperty.UnsetValue;
        }
    }
}
