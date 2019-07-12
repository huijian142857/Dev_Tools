/*
* Author:  caoshanshan
* Email:   me@dreamyouxi.com

C# 内存池  byte[]

使用方法@see 

 */
#define DEBUG_POOL_INFO

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Base
{
    //这里面返回的byte[] 的数组大小 不可用 需要外部自行处理
    //ThreadSafe
    public class MemoryPool
    {
        static object _lock = new object();//弃用Spinlock 是因为GC
        public const int MAX_SIZE = 102400;//100kb is max
        public const int FatalErrorSize = 1024 * 1024 * 3;//3mb is limit
        public const int MaxQueueSize = 512;

        static List<Queue<byte[]>> _list1 = null;
        static List<Queue<byte[]>> _list2 = null;
        static List<Queue<byte[]>> _list3 = null;

        static Queue<byte[]> GetQueue(int size)
        {
            size--;
            if (size < 512)
            {
                if (_list1 == null)
                {
                    _list1 = new List<Queue<byte[]>>();
                    for (int i = 0; i < 512; i += 16)
                    {
                        _list1.Add(new Queue<byte[]>());
                    }
                }
                return _list1[size / 16];
            }
            else if (size < 2048)
            {
                if (_list2 == null)
                {
                    _list2 = new List<Queue<byte[]>>();
                    for (int i = 512; i < 2048; i += 64)
                    {
                        _list2.Add(new Queue<byte[]>());
                    }
                }
                return _list2[size / 64 - 8];
            }
            else
            {
                if (_list3 == null)
                {
                    _list3 = new List<Queue<byte[]>>();
                    for (int i = 2048; i < MAX_SIZE; i += 256)
                    {
                        _list3.Add(new Queue<byte[]>());
                    }
                }
                return _list3[size / 256 - 8];
            }
        }
        static int ToAllocSize(int request_size)
        {
            if (request_size <= 512)
            {
                return request_size % 16 == 0 ? (request_size / 16 * 16) : ((request_size / 16 + 1) * 16);
            }
            else if (request_size <= 2048)
            {
                return request_size % 64 == 0 ? (request_size / 64 * 64) : ((request_size / 64 + 1) * 64);
            }
            else
            {
                return request_size % 256 == 0 ? (request_size / 256 * 256) : ((request_size / 256 + 1) * 256);
            }
        }

        public static byte[] Alloc(int size)
        {
            if (size <= 0) return null;
            if (size > FatalErrorSize)
            {
                return null;
            }
            if (size > MAX_SIZE)
            {
                return new byte[size];
            }
            lock (_lock)
            {
                var queue = GetQueue(size);
                while (queue.Count > 0)
                {
                    var ret = queue.Dequeue();
                    if (ret != null && ret.Length >= size)
                    {
                        return ret;
                    }
                }
                return new byte[ToAllocSize(size)];
            }
        }
        public static void Recycle(ref byte[] bytes)
        {
            if (bytes == null) return;
            if (bytes.Length > FatalErrorSize)
            {
                bytes = null;
                return;
            }
            if (bytes.Length > MAX_SIZE)
            {
                bytes = null;
                return;
            }
            if (bytes.Length < 16) return;
            lock (_lock)
            {
                var queue = GetQueue(bytes.Length);
                if (queue.Count < MaxQueueSize)
                {
                    queue.Enqueue(bytes);
                }
                bytes = null;
            }
        }
        public static void Clear()
        {
            lock (_lock)
            {
                /*  for (int i = 0; i < _list1.Count; i++)
                  {
                      _list1[i] = new Queue<byte[]>();
                  }
                  for (int i = 0; i < _list2.Count; i++)
                  {
                      _list2[i] = new Queue<byte[]>();
                  }*/
                for (int i = 0; i < _list3.Count; i++)
                {
                    _list3[i] = new Queue<byte[]>();
                }
            }
        }
        public static void PrintStatus()
        {
#if UNITY_EDITOR
            lock (_lock)
            {
                if (_list1 != null)
                {
                    Debug.Log("MemoryPool 0x1 info [ total block count= " + _list1.Count + "]");
                    foreach (var p in _list1)
                    {
                        if (p.Count > 0)
                        {
                            Debug.Log("MemoryPool info [ size = " + p.Peek().Length + " reserve=" + p.Count + "] ");
                        }
                    }
                }
                if (_list2 != null)
                {
                    Debug.Log("MemoryPool 0x2 info [ total block count= " + _list2.Count + "]");
                    foreach (var p in _list2)
                    {
                        if (p.Count > 0)
                        {
                            Debug.Log("MemoryPool info [ size = " + p.Peek().Length + " reserve=" + p.Count + "] ");
                        }
                    }
                }
                if (_list3 != null)
                {
                    Debug.Log("MemoryPool 0x3 info [ total block count= " + _list3.Count + "]");
                    foreach (var p in _list3)
                    {
                        if (p.Count > 0)
                        {
                            Debug.Log("MemoryPool info [ size = " + p.Peek().Length + " reserve=" + p.Count + "] ");
                        }
                    }
                }
            }
#endif
        }
        public static void AutoTest()
        {
#if UNITY_EDITOR
            for (int i = 1; i < MemoryPool.MAX_SIZE * 2; i++)
            {
                var p = MemoryPool.Alloc(i);
                var pp = MemoryPool.Alloc(i);

                MemoryPool.Recycle(ref pp);
                MemoryPool.Recycle(ref p);
            }
            for (int i = 0; i < _list1.Count; i++)
            {
                var p = _list1[i];
                if (p.Count == 2)
                {

                }
                else
                {
                    //  Debug.LogError("fatal error " + p.Count + "    " + i);
                }
            }

            for (int i = 0; i < _list2.Count; i++)
            {
                var p = _list2[i];
                if (p.Count == 2)
                {

                }
                else
                {
                    Debug.LogError("fatal error " + p.Count + "    " + i);
                }
            }
            for (int i = 0; i < _list3.Count; i++)
            {
                var p = _list3[i];
                if (p.Count == 2)
                {

                }
                else
                {
                    Debug.LogError("fatal error " + p.Count + "    " + i);
                }
            }
#endif
        }
    }
}

