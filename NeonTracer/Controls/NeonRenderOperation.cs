using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using NeonTracer.Models;
using SkiaSharp;

namespace NeonTracer.Controls;

public class NeonRenderOperation : ICustomDrawOperation
{
    /// <summary>
    /// 粒子快照
    /// </summary>
    private readonly PhotonParticle[] _particles;
    /// <summary>
    /// 线段快照
    /// </summary>
    private readonly TraceSegment[] _segments;
    /// <summary>
    /// 爆炸粒子快照
    /// </summary>
    private readonly ExplosionParticle[] _explosions;
    
    /// <summary>
    /// 游戏时间
    /// </summary>
    private readonly double _totalTime;
    
    /// <summary>
    /// 游戏边界
    /// </summary>
    public Rect Bounds { get; }

    /// <summary>
    /// 静态画笔绘制光线
    /// </summary>
    private static readonly SKPaint LineGlowPaint = new()
    {
        IsAntialias = true,
        Style = SKPaintStyle.Stroke,
        StrokeCap = SKStrokeCap.Butt,
        BlendMode = SKBlendMode.SrcOver
    };

    /// <summary>
    /// 静态画笔绘制线的核心部分
    /// </summary>
    private static readonly SKPaint LineCorePaint = new()
    {
        IsAntialias = true,
        Style = SKPaintStyle.Stroke,
        StrokeCap = SKStrokeCap.Round,
        BlendMode = SKBlendMode.Plus,

    };

    /// <summary>
    /// 粒子画笔
    /// </summary>
    private static readonly SKPaint ParticlePaint = new()
    {
        IsAntialias = true,
        Style = SKPaintStyle.Fill
    };
    
    /// <summary>
    /// 爆炸粒子画笔
    /// </summary>
    private static readonly SKPaint ExplosionPaint = new()
    {
        IsAntialias = true,
        Style = SKPaintStyle.Fill,
        BlendMode = SKBlendMode.Plus 
    };

    /// <summary>
    /// 背景动态网格画笔
    /// </summary>
    private static readonly SKPaint BackGridPaint = new()
    {
        Style = SKPaintStyle.Stroke,
        StrokeWidth = 1f,
        IsAntialias = true,
        BlendMode = SKBlendMode.Plus
    };
        
    public NeonRenderOperation(IList<PhotonParticle> particles, IList<TraceSegment> segments, IList<ExplosionParticle> explosions, Rect bounds, double totalTime)
    {
        Bounds = bounds;
        // 快速快照
        _particles = particles.ToArray();
        _segments = segments.ToArray();
        _explosions = explosions.ToArray();
        _totalTime = totalTime;
    }

    /// <summary>
    /// 每一帧渲染调用函数
    /// </summary>
    /// <param name="context">获取Skia api</param>
    public void Render(ImmediateDrawingContext context)
    {
        var lease = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
        if (lease == null) return;

        using var skiaLease = lease.Lease();
        var canvas = skiaLease.SkCanvas;

        // 基础配置
        LineGlowPaint.StrokeJoin = SKStrokeJoin.Round;
        LineCorePaint.StrokeJoin = SKStrokeJoin.Round;
        DrawBackground(canvas);
        DrawStars(canvas);
        /*DrawCyberGrid(canvas);*/
        // 分层渲染
        DrawTraceSegments(canvas);
        DrawPhotonParticles(canvas);
        DrawExplosionParticles(canvas);
    }

    /// <summary>
    /// 绘制背景动态网格
    /// </summary>
    /// <param name="canvas"></param>
    private void DrawCyberGrid(SKCanvas canvas)
    {
        var paint = BackGridPaint;

        var w = (float)Bounds.Width;
        var h = (float)Bounds.Height;
        var gridSize = 20f;

        
        var xOffset = (float)(_totalTime * 0.02) % gridSize;
        var yOffset = (float)(_totalTime * 0.02) % gridSize;

        
        for (var x = xOffset; x < w; x += gridSize)
        {
            paint.Color = SKColors.Cyan.WithAlpha((byte)(15 + Math.Sin(_totalTime * 0.001) * 10)); 
            canvas.DrawLine(x, 0, x, h, paint);
        }
        for (var y = yOffset; y < h; y += gridSize)
        {
            paint.Color = SKColors.Cyan.WithAlpha((byte)(15 + Math.Cos(_totalTime * 0.001) * 10));
            canvas.DrawLine(0, y, w, y, paint);
        }
    }
    
    /// <summary>
    /// 绘制背景
    /// </summary>
    private void DrawBackground(SKCanvas canvas)
    {
        var w = (float)Bounds.Width;
        var h = (float)Bounds.Height;
        var cx = w / 2;
        var cy = h / 2;
        using (var paint = new SKPaint())
        {
            var colors = new SKColor[] 
            { 
                new (5, 5, 10),
                new(16, 36, 70),
                new (36, 49, 120), 
                new (5, 5, 10),    
                SKColors.Black            
            };
            
            paint.Shader = SKShader.CreateRadialGradient(
                new SKPoint(cx, cy),
                Math.Max(w, h) * 0.8f,
                colors,
                 [0.0f,0.1f, 0.3f, 0.6f, 1.0f ],
                SKShaderTileMode.Clamp);

            canvas.DrawRect(0, 0, w, h, paint);
        }
    }
    
    /// <summary>
    /// 绘制星星
    /// </summary>
    private void DrawStars(SKCanvas canvas)
    {
        var w = (float)Bounds.Width;
        var h = (float)Bounds.Height;
        using var paint = new SKPaint();
        paint.IsAntialias = true;
        paint.Style = SKPaintStyle.Fill;
        paint.Color = SKColors.White;
        
        var rng = new Random(1337); 

        int starCount = 150; // 星星数量

        for (int i = 0; i < starCount; i++)
        {
            float x = (float)rng.NextDouble() * w;
            float y = (float)rng.NextDouble() * h;
            float size = (float)rng.NextDouble() * 2.0f + 0.5f;


            double flicker = Math.Sin(_totalTime * 0.003 + x); 
            byte alpha = (byte)(150 + flicker * 100);

            paint.Color = SKColors.White.WithAlpha(alpha);
            canvas.DrawCircle(x, y, size, paint);
        }
    }

    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="canvas"></param>
    private void DrawTraceSegments(SKCanvas canvas)
    {
        foreach (var trace in _segments)
        {
            if (trace.Opacity <= 0) continue;
            
            float alpha = (float)trace.Opacity;
            var skColor = new SKColor(trace.Color.R, trace.Color.G, trace.Color.B);
            var p1x = (float)trace.StartPoint.X;
            var p1y = (float)trace.StartPoint.Y;
            var p2x = (float)trace.EndPoint.X;
            var p2y = (float)trace.EndPoint.Y;
           
            LineGlowPaint.Color = skColor.WithAlpha((byte)(alpha * 40));
            LineGlowPaint.StrokeWidth = (float)trace.StrokeThickness * 3.0f;
            canvas.DrawLine(p1x, p1y, p2x, p2y, LineGlowPaint);
            
            LineGlowPaint.Color = skColor.WithAlpha((byte)(alpha * 120));
            LineGlowPaint.StrokeWidth = (float)trace.StrokeThickness * 1.2f;
            canvas.DrawLine(p1x, p1y, p2x, p2y, LineGlowPaint);
            
            LineCorePaint.Color = skColor.WithAlpha((byte)(alpha * 255));
            LineCorePaint.StrokeWidth = (float)trace.StrokeThickness * 0.4f;
            canvas.DrawLine(p1x, p1y, p2x, p2y, LineCorePaint);
        }
    }

    private void DrawPhotonParticles(SKCanvas canvas)
    {
        foreach (var particle in _particles)
        {
            if (particle.Opacity <= 0) continue;

            var center = particle.GetCenterForBounds(Bounds);
            var skPoint = new SKPoint((float)center.X, (float)center.Y);
            float radius = (float)particle.Radius;
            byte alphaByte = (byte)(particle.Opacity * 255);

            var colorInner = new SKColor(particle.Color.R, particle.Color.G, particle.Color.B, alphaByte);
            
            // 使用径向渐变
            using (var shader = SKShader.CreateRadialGradient(
                skPoint, radius * 1.5f,
                new[] { colorInner, SKColors.Transparent }, 
                new[] { 0.2f, 1.0f },
                SKShaderTileMode.Clamp))
            {
                ParticlePaint.Shader = shader;
                canvas.DrawCircle(skPoint, radius * 1.5f, ParticlePaint);
            }
            
            ParticlePaint.Shader = null;
            ParticlePaint.Color = SKColors.White.WithAlpha(alphaByte);
            canvas.DrawCircle(skPoint, radius * 0.3f, ParticlePaint);
        }
    }

    private void DrawExplosionParticles(SKCanvas canvas)
    {
        ExplosionPaint.StrokeCap = SKStrokeCap.Round;

        foreach (var exp in _explosions)
        {
            if (exp.Opacity <= 0) continue;

            float cx = (float)(Bounds.Left + exp.Position.X * Bounds.Width);
            float cy = (float)(Bounds.Top + exp.Position.Y * Bounds.Height);
            float vx = (float)(exp.Velocity.X * Bounds.Width * 60);
            float vy = (float)(exp.Velocity.Y * Bounds.Height * 60);
            float speed = (float)Math.Sqrt(vx * vx + vy * vy);
            float alpha = (float)exp.Opacity;

            if (speed > 1.0f)
                DrawSpike(canvas, cx, cy, vx, vy, speed, exp.Radius, exp.Color, alpha);
            else
                DrawBreatheSpark(canvas, cx, cy, exp.Radius, exp.Color, alpha, exp.Age);
        }
    }

    private void DrawSpike(SKCanvas canvas, float x, float y, float vx, float vy, float speed, double r, Color color, float alpha)
    {
        canvas.Save();
        canvas.Translate(x, y);
        canvas.RotateRadians((float)Math.Atan2(vy, vx));

        ExplosionPaint.Style = SKPaintStyle.Fill;
        ExplosionPaint.Shader = null;
        

        ExplosionPaint.Color = new SKColor(color.R, color.G, color.B, (byte)(alpha * 80));
        canvas.DrawOval(0, 0, speed * 1.5f + (float)r, (float)r * 1.2f, ExplosionPaint);
        
        ExplosionPaint.Color = SKColors.White.WithAlpha((byte)(alpha * 255));
        canvas.DrawOval(0, 0, speed * 0.8f + (float)r * 0.5f, (float)r * 0.4f, ExplosionPaint);
        
        canvas.Restore();
    }

    private void DrawBreatheSpark(SKCanvas canvas, float x, float y, double r, Color color, float alpha, double age)
    {
        float breathe = 1.0f + (float)Math.Sin(age * 0.005f) * 0.2f;
        float currentR = (float)r * breathe;
        var skColor = new SKColor(color.R, color.G, color.B);

        using (var shader = SKShader.CreateRadialGradient(
            new SKPoint(x, y), currentR * 2.5f,
            new[] { skColor.WithAlpha((byte)(alpha * 150)), SKColors.Transparent },
            new[] { 0.0f, 1.0f }, SKShaderTileMode.Clamp))
        {
            ExplosionPaint.Style = SKPaintStyle.Fill;
            ExplosionPaint.Shader = shader;
            canvas.DrawCircle(x, y, currentR * 2.5f, ExplosionPaint);
        }

        ExplosionPaint.Shader = null;
        ExplosionPaint.Color = SKColors.White.WithAlpha((byte)(alpha * 255));
        canvas.DrawCircle(x, y, currentR * 0.5f, ExplosionPaint);
    }
    
    public void Dispose() { }
    public bool HitTest(Point p) => false;
    public bool Equals(ICustomDrawOperation? other) => false;
}