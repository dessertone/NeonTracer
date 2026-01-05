using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;
using NeonTracer.Core;
using NeonTracer.Models;

namespace NeonTracer.Logic;

public class GameEngine
{
    public List<ExplosionParticle> ActiveExplosions { get; } = new(1000);
    public List<PhotonParticle> ActivePhotons { get; } = new(1000);
    public List<TraceSegment> ActiveSegments { get; } = new(100);
    public List<AutoTracker> AutoTrackers { get; } = new();

    private readonly ObjectPool<ExplosionParticle> _explosionParticlePool = new(10000, 100);
    private readonly ObjectPool<PhotonParticle> _photonParticlePool = new(10000, 100);
    private readonly ObjectPool<TraceSegment> _segmentPool = new(10000, 100);
    private readonly Random _random = new();
    private readonly IEnumerator<Color> _colorEnumerator;
    
    public event Action<double>? ScoreChanged;
    
    private double _score;
    public double Score
    {
        get => _score;
        set 
        {
            if (value != _score)
            {
                _score = value; ScoreChanged?.Invoke(value);
            } 
        }
    }
    public Rect Bounds { get; set; }

    public GameEngine()
    {
        _colorEnumerator = ColorGradientGenerator().GetEnumerator();
        
    }
    /// <summary>
    /// 添加一个指定颜色的自动追踪器
    /// </summary>
    public void AddAutoTracker(Color color)
    {
        var startPos = new Point(_random.NextDouble() * 0.5 + 0.25, _random.NextDouble() * 0.5 + 0.25); 
        var tracker = new AutoTracker(startPos, color) 
        { 
            Speed = 0.4 + (_random.NextDouble() * 0.1) 
        };
        AutoTrackers.Add(tracker);
    }
    
    public void RemoveLastAutoTracker()
    {
        if (AutoTrackers.Count > 0)
        {
            AutoTrackers.RemoveAt(AutoTrackers.Count - 1);
        }
    }
    public void Reset()
    {
        Score = 0;
        ActiveExplosions.Clear();
        ActivePhotons.Clear();
        ActiveSegments.Clear();
        AutoTrackers.Clear();
    }
    private IEnumerable<Color> ColorGradientGenerator()
    {
        double hue = 0;
        while (true)
        {
            hue += 2.0; 
            if (hue >= 360) hue = 0;
            yield return HsvToRgb(hue, 1.0, 1.0);
        }
    }
    private Color HsvToRgb(double h, double s, double v) 
    {
        int hi = (int)(Math.Floor(h / 60.0)) % 6;
        double f = h / 60.0 - Math.Floor(h / 60.0);
        byte vByte = (byte)(v * 255);
        byte p = (byte)(v * (1 - s) * 255);
        byte q = (byte)(v * (1 - f * s) * 255);
        byte t = (byte)(v * (1 - (1 - f) * s) * 255);
        return hi switch { 0 => Color.FromRgb(vByte, t, p), 1 => Color.FromRgb(q, vByte, p), 2 => Color.FromRgb(p, vByte, t), 3 => Color.FromRgb(p, q, vByte), 4 => Color.FromRgb(t, p, vByte), _ => Color.FromRgb(vByte, p, q), };
    }

    public void Update(double millisecondsElapsed, Queue<Point> inputPoints)
    {
        if(ActivePhotons.Count < 20) GeneratePhotonParticlesBatch(20);
        
       
        UpdateParticles(millisecondsElapsed, ActiveExplosions, _explosionParticlePool);
        UpdateParticles(millisecondsElapsed, ActivePhotons, _photonParticlePool);
        UpdateSegments(millisecondsElapsed);
        
        ProcessMouseInput(inputPoints);
        
        if (Bounds.Width > 0 && Bounds.Height > 0)
        {
            ProcessAutoTrackers(millisecondsElapsed / 1000.0);
        }
    }

    /// <summary>
    /// 新增：处理自动追踪器
    /// </summary>
    private void ProcessAutoTrackers(double elapsedSeconds)
    {
        var quadTree = new QuadTree(new Rect(0, 0, 1, 1), capacity: 4);
        foreach (var p in ActivePhotons)
        {
            if(p.IsAlive) quadTree.Insert(p);
        }
        
        foreach (var tracker in AutoTrackers)
        {
            var movement = tracker.Update(elapsedSeconds, quadTree);
            if (movement.HasValue)
            {
                
                Point screenStart = new Point(movement.Value.Start.X * Bounds.Width, movement.Value.Start.Y * Bounds.Height);
                Point screenEnd = new Point(movement.Value.End.X * Bounds.Width, movement.Value.End.Y * Bounds.Height);

                
                CreateTrailAndCheckCollision(screenStart, screenEnd, tracker.Color,1000);
            }
        }
    }

    /// <summary>
    /// 重构：仅处理鼠标队列的取值逻辑
    /// </summary>
    private void ProcessMouseInput(Queue<Point> moveQueue)
    {
        while (moveQueue.Count > 1)
        {
            var start = moveQueue.Dequeue();
            var end = moveQueue.Peek();
            
            _colorEnumerator.MoveNext();
            
            CreateTrailAndCheckCollision(start, end, _colorEnumerator.Current, 500);
        }
    }

    /// <summary>
    /// 创建光迹线段并检测碰撞
    /// </summary>
    private void CreateTrailAndCheckCollision(Point start, Point end, Color color, double maxAge)
    {
        var distance = Point.Distance(start, end);
        var stepSize = 5.0; 
        var thickness = 8;
        
        if (distance > stepSize)
        {
            var vector = end - start;
            var steps = (int)(distance / stepSize);
            var stepVector = vector / steps;
            var currentPos = start;
            for (int i = 0; i < steps; i++)
            {
                var nextPos = currentPos + stepVector;
                
                // 添加线段
                var segment = _segmentPool.Get();
                segment.Initialize(currentPos, nextPos, color, thickness, maxAge);
                ActiveSegments.Add(segment);
                
                // 碰撞检测
                CheckCollisionsInternal(currentPos, nextPos);

                currentPos = nextPos;
            }
        }
        else
        { 
            var segment = _segmentPool.Get();
            segment.Initialize(start, end, color, thickness, maxAge);
            ActiveSegments.Add(segment);
            
            CheckCollisionsInternal(start, end);
        }
    }
    
    private void CheckCollisionsInternal(Point start, Point end)
    {

        for (int i = ActivePhotons.Count - 1; i >= 0; i--)
        {
            var p = ActivePhotons[i];
            if(!p.IsAlive) continue;
            
            if (p.IntersectsLine(start, end, Bounds))
            {
                p.IsAlive = false; 
                /*var relativePoint = new Point(p.Position.X, p.Position.Y); */
                CreateSplatter(p.Position, p.Color);
                Score += 10;
            }
        }
    }

    private void CreateSplatter(Point position, Color color)
    {
        const double minSpeed = 0.0007; 
        const double maxSpeed = 0.0007;
        
        int count = _random.Next(7,10);
        for (int i = 0; i < count; i++)
        {
            var radius = _random.NextDouble() * 3 + 4;
            double angle = _random.NextDouble() * Math.PI * 2;
            double speed = _random.NextDouble() * (maxSpeed - minSpeed) + minSpeed;
            
            var velocity = new Vector(Math.Cos(angle) * speed, Math.Sin(angle) * speed);
            var maxAge = 2.5 * 1000;
            
            var p = _explosionParticlePool.Get();
            p.Initialize(position, velocity, radius, color, maxAge);
            ActiveExplosions.Add(p);
        }
    }
    
    private void UpdateSegments(double millisecondsElapsed)
    {
        for (var i = ActiveSegments.Count - 1; i >= 0; i--)
        {
            var segment = ActiveSegments[i];
            segment.Update(millisecondsElapsed);
            if (segment.Opacity <= 0)
            {
                ActiveSegments.RemoveAt(i);
                _segmentPool.Return(segment);
            }
        }
    }
    private void UpdateParticles<T>(double elapsedMilliseconds, IList<T> Elements, ObjectPool<T> pool) where T : ParticleBase, new()
    {
        for (var i = Elements.Count - 1; i >= 0; i--)
        {
            var particle = Elements[i];
            particle.Update(elapsedMilliseconds, Bounds);
            if (!particle.IsAlive)
            {
                Elements.RemoveAt(i);
                pool.Return(particle);
            }
        }
    }
    private void GeneratePhotonParticlesBatch(int Size)
    {
        for (int i = 0; i < Size; i++) GeneratePhotonParticles();
    }
    
    private void GeneratePhotonParticles()
    {
        var radius = _random.NextDouble() * 9 + 13;
        var position = new Point(_random.NextDouble(), _random.NextDouble()); 
        var velocity = new Vector((_random.NextDouble() - 0.5) * 0.0007,  (_random.NextDouble() - 0.5) * 0.0007);
        var Color = GetRandomColors();
        var maxAge = (_random.NextDouble() * 10 + 5) * 1000;
        
        var particle = _photonParticlePool.Get();
        particle.Initialize(position, velocity, radius, Color, maxAge);
        ActivePhotons.Add(particle);
    }

    private Color GetRandomColors() => Color.FromRgb((byte)_random.Next(0,255), (byte)_random.Next(0,255), (byte)_random.Next(0,255));
}