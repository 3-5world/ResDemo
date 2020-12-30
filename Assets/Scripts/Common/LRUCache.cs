using System;
using System.Collections.Generic;

public class DLinkNode<T> where T : class, new()
{
    public DLinkNode<T> Prev = null;
    public DLinkNode<T> Next = null;
    public T Value = null;
    public uint Key;

    public void Reset()
    {
        Prev = Next = null;
        Value = null;
        Key = 0;
    }
}

class DLinkList<T> where T : class, new()
{
    private DLinkNode<T> head = null;
    private DLinkNode<T> tail = null;
    private int size = 0;


    public DLinkNode<T> Tail
    {
        get { return tail; }
    }

    public int Size
    {
        get { return size; }
    }

    public void AddToHead(DLinkNode<T> node)
    {
        if (node == null)
            return;
        node.Prev = null;
        if (head == null)
        {
            head = tail = node;
        }
        else
        {
            node.Next = head;
            head.Prev = node;
            head = node;
        }
        size++;
    }

    public void AddToTail(DLinkNode<T> node)
    {
        if (node == null)
            return;
        node.Next = null;
        if (tail == null)
        {
            head = tail = node;
        }
        else
        {
            node.Prev = tail;
            tail.Next = node;
            tail = node;
        }
        size++;
    }

    public void MoveToHead(DLinkNode<T> node)
    {
        if (node == null || node == head)
            return;

        if (node.Prev == null && node.Next == null)
            return;

        if (node == tail)
            tail = node.Prev;

        if (node.Prev != null)
            node.Prev.Next = node.Next;

        if (node.Next != null)
            node.Next.Prev = node.Prev;
        node.Prev = null;
        node.Next = head;
        head.Prev = node;
        head = node;
        if (tail == null)
            tail = head;
    }

    public void RemoveNode(DLinkNode<T> node)
    {
        if (node == null)
            return;
        if (node == head)
            head = node.Next;
        if (node == tail)
            tail = node.Prev;

        if (node.Prev != null)
            node.Prev.Next = node.Next;
        if (node.Next != null)
            node.Next.Prev = node.Prev;
    }
}

public class LRUCache<T> where T : class, new()
{
    DLinkList<T> dLinkList = null;
    private int capacity = 0;
    Dictionary<uint, DLinkNode<T>> findDic = null;

    protected ClassObjectPool<DLinkNode<T>> nodePool = null;

    private static Dictionary<Type, object> classPoolDic = new Dictionary<Type, object>();

    public LRUCache(int capacity)
    {
        dLinkList = new DLinkList<T>();
        findDic = new Dictionary<uint, DLinkNode<T>>();

        nodePool = GetOrCreateClassPool(capacity);
    }

    public bool IsFull()
    {
        return dLinkList.Size == capacity;
    }

    public int Size
    {
        get
        {
            return dLinkList.Size;
        }
    }

    public ClassObjectPool<DLinkNode<T>> GetOrCreateClassPool(int maxcount)
    {
        var type = typeof(DLinkNode<T>);
        object outObj = null;
        if (classPoolDic.TryGetValue(type, out outObj) || outObj == null)
        {
            var pool = new ClassObjectPool<DLinkNode<T>>(maxcount);
            classPoolDic.Add(type, pool);
            return pool;
        }

        return outObj as ClassObjectPool<DLinkNode<T>>;
    }

    public void Clear()
    {
        while (dLinkList.Tail != null)
            RemoveLast();
    }

    public T Get(uint key)
    {
        DLinkNode<T> node = null;
        if (!findDic.TryGetValue(key, out node) || node == null)
            return null;
        T value = node.Value;
        dLinkList.RemoveNode(node);
        findDic.Remove(key);
        node.Reset();
        nodePool.Recycle(node);
        return value;
    }

    public bool IsExist(uint key)
    {
        DLinkNode<T> node = null;
        if (!findDic.TryGetValue(key, out node) || node == null)
            return true;
        return false;
    }

    public void Cache(uint key, T value)
    {
        DLinkNode<T> node = null;
        if (findDic.TryGetValue(key, out node) && node != null)
        {
            node.Value = value;
            dLinkList.MoveToHead(node);
        }
        else
        {
            if (dLinkList.Size == capacity)
            {
                //node = dLinkList.Tail;
                //findDic.Remove(node.Key);
                //node.Value = value;
                //node.Key = key;
                //dLinkList.MoveToHead(node);
                //findDic.Add(key, node);
                return;
            }
            else
            {
                node = nodePool.Spawn(true);
                node.Value = value;
                node.Key = key;
                dLinkList.AddToHead(node);
                findDic.Add(key, node);
            }
        }
    }

    public void Remove(uint key)
    {
        DLinkNode<T> node = null;
        if (findDic.TryGetValue(key, out node) && node != null)
        {
            dLinkList.RemoveNode(node);
            findDic.Remove(key);
            node.Reset();
            nodePool.Recycle(node);
        }
    }

    void RemoveLast()
    {
        var node = dLinkList.Tail;
        dLinkList.RemoveNode(dLinkList.Tail);
        findDic.Remove(node.Key);
        node.Reset();
        nodePool.Recycle(node);
    }

    public T Back()
    {
        if (dLinkList.Tail == null)
            return null;
        else
            return dLinkList.Tail.Value;
    }
}