using System.Collections.Generic;
using UnityEngine;

public class ClassObjectPool<T> where T : class, new()
{
    public Stack<T> _pool = new Stack<T>();
    protected int _maxCount = 0;
    protected int _noRecyleCount = 0;

    public int MaxCount
    {
        get { return _maxCount; }
    }
    public ClassObjectPool(int maxCount)
    {
        _maxCount = maxCount;
        for (int i = 0; i < maxCount; ++i)
        {
            _pool.Push(new T());
        }
    }

    public T Spawn(bool createIfPoolEmpty)
    {
        if (_pool.Count > 0)
        {
            var obj = _pool.Pop();
            if (obj == null)
            {
                if (createIfPoolEmpty)
                {
                    obj = new T();
                }
            }
            _noRecyleCount++;
            return obj;
        }
        else
        {
            if (createIfPoolEmpty)
            {
                var obj = new T();
                _noRecyleCount++;
                return obj;
            }
        }
        return null;
    }

    public bool Recycle(T obj)
    {
        if (obj == null)
            return false;
        _noRecyleCount--;
        if (_pool.Count >= _maxCount && _maxCount > 0)
        {
            obj = null;
            return false;
        }

        _pool.Push(obj);
        return true;
    }
}