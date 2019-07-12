/*
* Author:  caoshanshan
* Email:   me@dreamyouxi.com

 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;

public sealed class ThreadSafeQueue : object
{
    public void Enqueue(object obj)
    {
        lock (locker)
        {
            _queue.Enqueue(obj);
        }
    }
    public int Count()
    {
        lock (locker)
        {
            int c = _queue.Count;
            return c;
        }
    }
    public object Dequeue()
    {
        lock (locker)
        {
            if (_queue.Count == 0)
            {
                return null;

            }
            object obj = _queue.Dequeue();
            return obj;
        }
    }

    public bool Empty()
    {
        lock (locker)
        {
            bool ret = (0 == _queue.Count);
            return ret;
        }
    }
    public void Clear()
    {
        lock (locker)
        {
            _queue.Clear();
        }
    }
    
    public ThreadSafeQueue()
    {
        _queue = new Queue();
    }
    private Queue _queue;
 //   public SpinLock locker;
    public object locker = new object();

};


public sealed class ThreadSafeQueue<T>
{
    public void Enqueue(T obj)
    {
        lock (locker)
        {
            _queue.Enqueue(obj);
        }
    }
    public int Count()
    {
        lock (locker)
        {
            int c = _queue.Count;
            return c;
        }
    }
    public T Dequeue()
    {
        lock (locker)
        {
            if (_queue.Count == 0)
            {
                return default(T);
            }
            T obj = _queue.Dequeue();
            return obj;
        }
    }

    public bool Empty()
    {
        lock (locker)
        {
            bool ret = (0 == _queue.Count);
            return ret;
        }

    }
    public void Clear()
    {
        lock (locker)
        {
            _queue.Clear();
        }
    }
    private Queue<T> _queue = new Queue<T>();
    //    private SpinLock locker = new SpinLock();//弃用spinlock 是因为 GC
    private object locker = new object();
};

