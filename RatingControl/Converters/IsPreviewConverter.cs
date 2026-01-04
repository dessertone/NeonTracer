using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace RatingControl.Converters;

public class IsPreviewConverter:IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if(values.Count != 3) throw new ArgumentException("Expected three values");
        var value1  = values[0] as int?;
        var value2 = values[1] as int?;
        var value3 = values[2] as int?;
        return value1 > value2 && value1 <= value3 || value1 <= value2 && value1 > value3;
    }
}