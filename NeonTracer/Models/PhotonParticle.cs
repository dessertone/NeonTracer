using System;
using Avalonia;
using Avalonia.Media;
using Point = Avalonia.Point;


namespace NeonTracer.Models;

public class PhotonParticle:ParticleBase
{
    public PhotonParticle(){ }

    
    /// <summary>
    /// 重置或初始化对象
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="vel"></param>
    /// <param name="r"></param>
    /// <param name="c"></param>
    /// <param name="maxAge"></param>
    public void Initialize(Point pos, Vector vel, double r, Color c, double maxAge)
    {
        base.Reset(); 
        Position = pos;
        Velocity = vel;
        Radius = r;
        Color = c;
        MaxAge = maxAge;
    }
    
    /// <summary>
    /// update the Particle's state
    /// </summary>
    /// <param name="elapsedMilliseconds">tick时间</param>
    /// <param name="bounds">边界</param>
    public override void Update(double elapsedMilliseconds, Rect bounds)
    {
         
        if(!IsAlive) return;
        
        // 更新坐标
        Position += Velocity * elapsedMilliseconds;
        var relativeVerticalRadius = Radius / bounds.Height;
        var relativeHorizontalRadius = Radius / bounds.Width;
        var relativeTop = bounds.Y / (bounds.Y + bounds.Height);
        var relativeBottom = bounds.Y / (bounds.X + bounds.Height);
        // 左右边界
        if (Position.X + relativeHorizontalRadius > 1|| Position.X - relativeHorizontalRadius < 0)
        {
            var a = Math.Clamp(Position.X, relativeHorizontalRadius, 1 - relativeHorizontalRadius);
            Position = new(a, Position.Y);
            Velocity = new(-Velocity.X, Velocity.Y);
        }
        // 上下边界
        if (Position.Y + relativeVerticalRadius > 1 || Position.Y - relativeVerticalRadius < 0)
        {   
            Position= new(Position.X, Math.Clamp(Position.Y, relativeVerticalRadius, 1 - relativeVerticalRadius));
            Velocity = new(Velocity.X, -Velocity.Y);
        }

        
        Age += elapsedMilliseconds;
        if (Age > MaxAge * 0.7) 
        {
            Opacity = 1.0 - ((Age - MaxAge * 0.7) / (MaxAge * 0.3));
            if (Opacity < 0) Opacity = 0;
        }
        if (Age >= MaxAge) IsAlive = false;
        
    }

    
    /// <summary>
    /// check if point is in particle
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public bool ContainsPoint(Point point, Rect bounds)
    {
        
        var pos = GetCenterForBounds(bounds);
        return Point.Distance(pos, point) <= Radius;
    }
    
    /// <summary>
    /// check if particle is intersected by line
    /// 首先计算圆心在线段上投影，根据圆心到线段投影距离与半径比较判断是否成功切割
    /// </summary>
    /// <param name="lineStart"></param>
    /// <param name="lineEnd"></param>
    /// <returns></returns>
    public bool IntersectsLine(Point lineStart, Point lineEnd, Rect bounds)
    {
        var pos = GetCenterForBounds(bounds);
        Vector lineVector = lineEnd - lineStart;
        Vector startToCenter = pos - lineStart;

        // 计算线段长度的平方
        double lineLengthSquared = lineVector.X * lineVector.X + lineVector.Y * lineVector.Y;

        // 如果起始点和终点重合，直接退化为点圆检测
        if (lineLengthSquared == 0)
        {
            return ContainsPoint(lineStart, bounds);
        }
        
        // 计算投影比例 t (0 <= t <= 1)
        // t 是圆心在直线上投影点的位置比例：t = (AC·AB) / |AB|^2
        double t = (startToCenter.X * lineVector.X + startToCenter.Y * lineVector.Y) / lineLengthSquared;

        // 将 t 限制在 [0, 1] 范围内，确保最短距离点在线段上
        t = Math.Clamp(t, 0, 1);

        //  找到线段上距离 圆心最近的点
        Point closestPoint = new Point(
            lineStart.X + t * lineVector.X,
            lineStart.Y + t * lineVector.Y
        );
         
        Vector distVector = pos - closestPoint;
        double distanceSquared = distVector.X * distVector.X + distVector.Y * distVector.Y;
        
        return distanceSquared <= (Radius * Radius);
        
    }
}