using Avalonia;
using Avalonia.Media;
using NeonTracer.Core;

namespace NeonTracer.Models;

public abstract class ParticleBase: IPoolable
{
    public Point Position { get; set; }
    public Vector Velocity { get; set; }
    public double Radius { get; set; }
    public Color Color { get; set; }
    public double Opacity { get; set; } = 1;
    public virtual bool IsAlive { get; set; } = true;
    public double Age { get; set; } = 0;
    public double MaxAge { get; set; }

    public abstract void Update(double elapsedMilliseconds, Rect bounds);

    public virtual Point GetCenterForBounds(Rect bounds)
    {
        return new Point(bounds.Width * Position.X , bounds.Height * Position.Y );
    }

    protected ParticleBase(Point position, Vector velocity, double radius, Color color, double maxAge)
    {
        Position = position;
        Velocity = velocity;
        Radius = radius;
        Color = color;
        MaxAge = maxAge;
    }

    protected ParticleBase()
    {
        
    }

    public virtual void Reset()
    {
        IsAlive = true;
        Opacity = 1;
        Age = 0;
    }
}