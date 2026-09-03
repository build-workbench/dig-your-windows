using System.Globalization;
using System.Windows.Data;

namespace DigYourWindows.UI.Converters
{
    /// <summary>
    /// Formats sensor readings, rendering NaN/Infinity as an em dash placeholder.
    /// Parameter is the numeric format string (default "F1").
    /// </summary>
    public class NaNSafeConverter : IValueConverter
    {
        public object Convert(object? value, System.Type targetType, object? parameter, CultureInfo culture)
        {
            var format = parameter?.ToString() ?? "F1";

            if (value is float f)
            {
                if (float.IsNaN(f) || float.IsInfinity(f))
                {
                    return "—";
                }

                return f.ToString(format, culture);
            }

            if (value is double d)
            {
                if (double.IsNaN(d) || double.IsInfinity(d))
                {
                    return "—";
                }

                return d.ToString(format, culture);
            }

            return value?.ToString() ?? "—";
        }

        public object ConvertBack(object? value, System.Type targetType, object? parameter, CultureInfo culture)
        {
            // Display-only converter; two-way editing is not supported.
            return System.Windows.DependencyProperty.UnsetValue;
        }
    }

    /// <summary>
    /// Formats sensor readings where 0 also means "no reading" (temperature, power),
    /// rendering 0 and NaN/Infinity as an em dash placeholder.
    /// Parameter is the numeric format string (default "F1").
    /// </summary>
    public class ZeroAsMissingConverter : IValueConverter
    {
        public object Convert(object? value, System.Type targetType, object? parameter, CultureInfo culture)
        {
            var format = parameter?.ToString() ?? "F1";

            if (value is float f)
            {
                if (f == 0f || float.IsNaN(f) || float.IsInfinity(f))
                {
                    return "—";
                }

                return f.ToString(format, culture);
            }

            if (value is double d)
            {
                if (d == 0d || double.IsNaN(d) || double.IsInfinity(d))
                {
                    return "—";
                }

                return d.ToString(format, culture);
            }

            return value?.ToString() ?? "—";
        }

        public object ConvertBack(object? value, System.Type targetType, object? parameter, CultureInfo culture)
        {
            // Display-only converter; two-way editing is not supported.
            return System.Windows.DependencyProperty.UnsetValue;
        }
    }

    /// <summary>
    /// Converts a megabyte value to a gigabyte display string (default "F1").
    /// </summary>
    public class MegabytesToGigabytesConverter : IValueConverter
    {
        public object Convert(object? value, System.Type targetType, object? parameter, CultureInfo culture)
        {
            var format = parameter?.ToString() ?? "F1";

            if (value is long mb)
            {
                return (mb / 1024.0).ToString(format, culture);
            }

            if (value is int mbInt)
            {
                return (mbInt / 1024.0).ToString(format, culture);
            }

            return "—";
        }

        public object ConvertBack(object? value, System.Type targetType, object? parameter, CultureInfo culture)
        {
            // Display-only converter; two-way editing is not supported.
            return System.Windows.DependencyProperty.UnsetValue;
        }
    }
}
