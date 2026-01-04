using System;
using Avalonia;
using Avalonia.Media;
using NeonTracer.Core;

namespace NeonTracer.Models;

public class AutoTracker
{
    public Point Position { get; set; } 
    public Color Color { get; set; }
    public double Speed { get; set; } = 0.2; 
    private const double SearchRadius = 0.5; 

    public AutoTracker(Point startPos, Color color)
    {
        Position = startPos;
        Color = color;
    }
    
    public (Point Start, Point End)? Update(double elapsedSeconds, QuadTree quadTree)
    {
        var target = quadTree.QueryNearest(Position, SearchRadius);
        if (target != null)
        {
            Vector dir = target.Position - Position;
            double length = Math.Sqrt(dir.X * dir.X + dir.Y * dir.Y);
            
            if (length > 0.001) 
            {
                dir = dir / length; 
                Point oldPos = Position;
                Position += dir * Speed * elapsedSeconds;
                return (oldPos, Position);
            }
        }
        return null;
    }
}