/*
* Author:  caoshanshan
* Email:   me@dreamyouxi.com

 */
using UnityEngine;
using System.Collections;
using System.Threading;
using System.Net.Sockets;
using System.Timers;
using System;
using System.Net.NetworkInformation;
using System.Collections.Generic;
// socket interface for battleserver  can easy change socket type such tcp  udt or others
public abstract class SocketClient
{
    protected bool isConnected = false;
    protected int mPing = 0;//(latency time between server and client)
    //   private double mLastPingTimeStamp = 0;
    public bool IsConnected
    {
        get
        {
            return isConnected;
        }
    }
    protected DateTime mLastPingTime = DateTime.Now;
    //  List<DateTime> mLastPingTimes = new List<DateTime>();
    //int mPingTimeIndex = 0;
    public int pingMsgNum = 0;

    public void SendPingMsg(int ping)
    {
        var msg = MsgStream.Create(MsgType.CUSTOM_CMD, CustomMsgType.ping);
        msg.Write(mPing);
        mLastPingTime = DateTime.Now;
        //    mLastPingTimes.Add(DateTime.Now);
        this.AddSendMsg(msg);
       // if (pingMsgNum >= 2)
        {
            //已经发送过了 但是服务器未响应 认为断开连接了
        }
        ++pingMsgNum;
        //  this.LastSendPingTimestamp = Utils.GetTimestampSecondsInt();
    }
    public int LastSendPingTimestamp = 0;
    virtual public void AddSendMsg(MsgStream msg)
    {
        throw new NullReferenceException();
    }
    public int Ping
    {
        get
        {
            return mPing;
        }
    }
    virtual public void Terminal()
    {
    }

    virtual public bool Startup(string ip, int port)
    {
        return false;
    }
    virtual public MsgStream PickOneMsg()
    {
        return null;
    }
}
