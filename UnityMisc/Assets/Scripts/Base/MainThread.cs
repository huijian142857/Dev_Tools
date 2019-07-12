/*
* Author:  caoshanshan
* Email:   me@dreamyouxi.com

 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//提交给主线程执行的 由于异步 下一帧执行 因此 慎用
public class MainThread : MonoBehaviour
{
    public static MainThread ins = null;
    void Awake()
    {
        ins = this;
    }
    void OnDestroy()
    {
        if (ins == this)
        {
            ins = null;
        }
    }
    //post event to main thread
    public void Post(VoidFuncVoid task)
    {
        if (task == null) return;
        lock (this.tasks)
        {
            this.tasks.Enqueue(task);
        }
    }

    public void PostPerFrameTask(VoidFuncVoid task)
    {
        if (perFrameTasks == null) return;
        this.perFrameTasks.Enqueue(task);
    }

    public void StopHeartBeat()
    {
        enable_heartbeat = false;
    }
    //interval is second
    public void StartHeartBeat(int interval)
    {
        this.interval = interval;
        time = Time.time;
        enable_heartbeat = true;
    }
    public void ResumeHeartBeat()
    {
        this.enable_heartbeat = true;
    }
    bool enable_heartbeat = false;
    float time = 0f;
    //1 min
    float interval = 60f;
    void Update()
    {
        lock (this.tasks)
        {
            if (this.tasks.Count > 0)
            {
                var tmp = this.task_swap;
                this.task_swap = this.tasks;
                this.tasks = this.task_swap;
            }
        }
        while (task_swap.Count > 0)
        {
            var cb = task_swap.Dequeue();
            try
            {
                cb();
            }
            catch (Exception e)
            {

            }
        }

        if (this.perFrameTasks.Count > 0)
        {
            var cb = perFrameTasks.Dequeue();
            try
            {
                cb();
            }
            catch (Exception e)
            {

            }
        }

        if (enable_heartbeat)
        {
            float now = Time.time;
            if (now - time > interval)
            {
                //if (LuaInterface.LuaMgr.ins != null)
                //{
                //    LuaInterface.LuaMgr.ins.CallNONE_BATTLE_HEART_BEAT(interval);
                //}
                time = now;
            }
        }
    }
    public void Clear()
    {
        this.tasks.Clear();
    }

    public void ClearPerFrameTask()
    {
        this.perFrameTasks.Clear();
    }

    Queue<VoidFuncVoid> tasks = new Queue<VoidFuncVoid>();
    Queue<VoidFuncVoid> perFrameTasks = new Queue<VoidFuncVoid>();
    Queue<VoidFuncVoid> task_swap = new Queue<VoidFuncVoid>();

}
