using System;
using Avalonia;
using Avalonia.Media;
using NeonTracer.Core;
using Point = Avalonia.Point;

namespace NeonTracer.Models;

public class TraceSegment:IPoolable
{
     public Point StartPoint { get; set; }
     public Point EndPoint { get; set; }
     public Color Color { get; set; }
     public double Opacity { get; set; } = 1;
     public double StrokeThickness { get; set; }
     public double Age { get; set; }
     public double MaxAge { get; set; }

     public TraceSegment()
     {
          
     }
     
     public void Initialize(Point startPoint = default, Point endPoint = default, Color color = default, double strokeThickness = 1, double maxAge = 5000)
     {
          Reset();
          StartPoint = startPoint;
          EndPoint = endPoint;
          Color = color;
          StrokeThickness = strokeThickness;
          MaxAge = maxAge;
     }

     public void Update(double elapsedMilliseconds)
     {
          Age += elapsedMilliseconds;
          Opacity = Math.Max(1 - Age / MaxAge, 0);
     }

     
     
     public void Reset()
     {
          Opacity = 1;
          Age = 0;
     }
}