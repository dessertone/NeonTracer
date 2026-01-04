using System;
using System.Collections.Generic;
using Avalonia;

namespace SnowflakesControl.Models;

public class SnowHint
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Score { get; set; }
    public double Opacity { get; set; }
    
    private ICollection<SnowHint> _snowHints;

    private double _elapsedMillisecondsTotal;
    public SnowHint(Snowflake snowflake, ICollection<SnowHint> snowHints)
    {
        _snowHints = snowHints;
        X = snowflake.X;
        Y = snowflake.Y;
        Score = snowflake.Score;
        Opacity = 1;
    }
    
    internal Point GetTopLeftForViewport(Rect viewport, Size textSize)
    {
        var left = (X * viewport.Width + viewport.Left) - textSize.Width / 2.0;
        var top = (Y * viewport.Height + viewport.Top) - textSize.Height;
        
        // Make sure text is not out of bounds
        if (left < 0) left = 0;
        if (top < 0) top = 0;
        if (left + textSize.Width > viewport.Width) left = viewport.Width - textSize.Width;
        
        return new Point(left, top);
    }
    
    public void Update(double milliseconds)
    {
        _elapsedMillisecondsTotal += milliseconds;
        if (_elapsedMillisecondsTotal >= 1000)
        {
            _snowHints.Remove(this);
        }
        if (_elapsedMillisecondsTotal > 500)
        {
            var percentage = (_elapsedMillisecondsTotal - 500) / 500;
            Opacity = Math.Max(1 - percentage, 0);
            Y -= 0.01 * percentage;;
        }
    }
    public override string ToString()
    {
        return $"+{Score:N0}";
    }
}