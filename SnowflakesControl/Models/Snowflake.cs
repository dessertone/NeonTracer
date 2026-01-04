using Avalonia;

namespace SnowflakesControl.Models;

public class Snowflake
{
    public Snowflake(double x, double y, double radius, double speed)
    {
        X = x;
        Y = y;
        Radius = radius;
        Speed = speed;
    }

    public double X { get; set; }
    public double Y { get; set; }
    public double Radius { get; set; }
    public double Speed { get; set; }

    public double Score => 1 / Radius * 200 + Speed / 10;
    
    
    public void Move(double elapsedMilliseconds)
    {
        Y += Speed / 1000 * elapsedMilliseconds;
        if (Y > 1) Y = 0;
    }
    
    public Point GetCenterForViewport(Rect viewport)
    {
        return new Point(viewport.Width * X + viewport.Left, viewport.Height * Y +  viewport.Top);
    }

    public bool IsHit(Point point, Rect viewport)
    {
        return Point.Distance(point, GetCenterForViewport(viewport)) <= Radius;
    }
}