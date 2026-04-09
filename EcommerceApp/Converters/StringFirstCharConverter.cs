using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace EcommerceApp.Converters;

public class StringFirstCharConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var s = value as string;
        if (!string.IsNullOrEmpty(s))
            return s[0].ToString();
        return "?";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
