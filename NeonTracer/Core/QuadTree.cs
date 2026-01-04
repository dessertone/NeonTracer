using System.Collections.Generic;
using Avalonia;
using NeonTracer.Models;

namespace NeonTracer.Core;

public class QuadTree
{
    private readonly Rect _bounds;
    private readonly int _capacity;
    private readonly List<PhotonParticle> _particles = new();
    private QuadTree? _topLeft, _topRight, _bottomLeft, _bottomRight;
    private bool _isDivided;

    public QuadTree(Rect bounds, int capacity = 4)
    {
        _bounds = bounds;
        _capacity = capacity;
    }

    public bool Insert(PhotonParticle particle)
    {
        if (!_bounds.Contains(particle.Position)) return false;

        if (_particles.Count < _capacity)
        {
            _particles.Add(particle);
            return true;
        }

        if (!_isDivided) Subdivide();

        if (_topLeft!.Insert(particle)) return true;
        if (_topRight!.Insert(particle)) return true;
        if (_bottomLeft!.Insert(particle)) return true;
        if (_bottomRight!.Insert(particle)) return true;

        return false;
    }

    private void Subdivide()
    {
        var x = _bounds.X;
        var y = _bounds.Y;
        var w = _bounds.Width / 2;
        var h = _bounds.Height / 2;

        _topLeft = new QuadTree(new Rect(x, y, w, h), _capacity);
        _topRight = new QuadTree(new Rect(x + w, y, w, h), _capacity);
        _bottomLeft = new QuadTree(new Rect(x, y + h, w, h), _capacity);
        _bottomRight = new QuadTree(new Rect(x + w, y + h, w, h), _capacity);
        _isDivided = true;
    }

    public PhotonParticle? QueryNearest(Point position, double searchRadius)
    {
        var range = new Rect(position.X - searchRadius, position.Y - searchRadius, searchRadius * 2, searchRadius * 2);
        var candidates = new List<PhotonParticle>();
        Query(range, candidates);

        PhotonParticle? nearest = null;
        double minDistanceSq = double.MaxValue;
        double rSq = searchRadius * searchRadius;

        foreach (var p in candidates)
        {
            if(!p.IsAlive) continue;
            double distSq = (p.Position.X - position.X) * (p.Position.X - position.X) + 
                            (p.Position.Y - position.Y) * (p.Position.Y - position.Y);
            if (distSq < rSq && distSq < minDistanceSq)
            {
                minDistanceSq = distSq;
                nearest = p;
            }
        }
        return nearest;
    }

    private void Query(Rect range, List<PhotonParticle> found)
    {
        if (!_bounds.Intersects(range)) return;

        foreach (var p in _particles)
        {
            if (range.Contains(p.Position)) found.Add(p);
        }

        if (_isDivided)
        {
            _topLeft!.Query(range, found);
            _topRight!.Query(range, found);
            _bottomLeft!.Query(range, found);
            _bottomRight!.Query(range, found);
        }
    }
}