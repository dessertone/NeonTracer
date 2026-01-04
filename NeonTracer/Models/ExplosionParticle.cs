using System;
using Avalonia;
using Avalonia.Media;

namespace NeonTracer.Models;

public class ExplosionParticle: ParticleBase
{

    /// <summary>
    /// 阻力
    /// </summary>
    public double Residence { get; set; }

    /// <summary>
    /// 重力
    /// </summary>
    public double Gravity { get; set; }
    public override bool IsAlive => Age < MaxAge;

    public ExplosionParticle() {}
    

    public void Initialize(Point position, Vector velocity, double radius, Color color, double maxAge, double residence = 0.93, double gravity = 1e-6)
    {
        base.Reset();
        Position = position;
        Velocity = velocity;
        Radius = radius;
        Color = color;
        MaxAge = maxAge;
        Residence = residence;
        Gravity = gravity;
        
    }

    /// <summary>
    /// 更新元素状态
    /// </summary>
    /// <param name="elapsedMilliseconds"></param>
    /// <param name="bounds"></param>
    public override void Update(double elapsedMilliseconds, Rect bounds)
    {
        if (!IsAlive) return;
        
        // 更新位置信息
        
        Velocity = new Vector(Velocity.X * Residence, Velocity.Y * Residence + Gravity);
        Position += Velocity * elapsedMilliseconds;
        Opacity = Math.Max(1 - Age / MaxAge, 0);
        Velocity *= Residence;
        
        Age += elapsedMilliseconds;
        
    }
}