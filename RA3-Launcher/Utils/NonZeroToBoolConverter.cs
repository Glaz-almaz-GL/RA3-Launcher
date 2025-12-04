using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Utils
{
    public class NonZeroToBoolConverter : IValueConverter
    {
        public static readonly NonZeroToBoolConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is int intValue && intValue != 0;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
