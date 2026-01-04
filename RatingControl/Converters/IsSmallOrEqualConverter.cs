using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace RatingControl.Converters;

public class IsSmallOrEqualConverter: IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if(values.Count != 2)
            throw new ArgumentException("Expected only two values");
        var value1 = values[0] as int?;
        var value2 = values[1] as int?;
        return value1 <= value2;
    }
}