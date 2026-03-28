using System;
using System.Globalization;
using Avalonia.Data.Converters;
namespace AIRPG;

public class FloorIntConverter : IValueConverter
{
public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
{
    if (value is int i) return (decimal)i;
    return value;
}
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
            return 0;

        double d = System.Convert.ToDouble(value);
        return (int)Math.Floor(d); // ✅ always round down
    }
}