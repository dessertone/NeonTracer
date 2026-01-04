using System.Collections.Generic;

namespace NeonTracer.Core;

public class ObjectPool<T> where T : IPoolable, new()
{
    private readonly Stack<T> _pool = new ();
    private readonly int _maxSize;

    public ObjectPool(int maxSize, int initialSize)
    {
        _maxSize = maxSize;
        if(initialSize > _maxSize) initialSize = _maxSize;
        for (int i = 0; i < initialSize; i++)
        {
            _pool.Push(new T());
        }
    }

    public T Get() => _pool.Count > 0 ? _pool.Pop() : new T();

    public void Return(T obj)
    {
        if (_pool.Count < _maxSize)
        {
            
            _pool.Push(obj);
        }
    }

}