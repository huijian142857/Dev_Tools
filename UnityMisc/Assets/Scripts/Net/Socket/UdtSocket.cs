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
using System.Runtime;
using System.Runtime.InteropServices;
//server code is in BattleServer/udt_edition

public class UdtSocket : SocketClient
{

   /* static bool StartUp = false;

    private ThreadSafeQueue _sendQueue;
    public ThreadSafeQueue _recvQueue;

    public override MsgStream PickOneMsg()
    {
        var x = _recvQueue.Dequeue();
        return x as MsgStream;
    }

    private ArrayList _recv_list;

    private Thread t_send;
    private Thread t_recv;

    private bool isThreadInit = false;
 

      public override void AddSendMsg(MsgStream msg)
    {
        //   msg += "\0";
        _sendQueue.Enqueue(msg);
    }

      public override bool Startup(string ip, int port)
    {
     int ret=   UdtImpl.StartUp();
        Debug.LogError("udt  Startup up:" + ret);
        this.InitSocket();
        if (this.Connect(ip, port))
        {
            this.InitThread();
            Debug.Log("[NetWork]:Socket Thread Open");
        }
        else
        {
            return false;
        }
        return true;

    }
      public override void Terminal()
    {
        if (isThreadInit)
        {
            t_send.Abort();
            t_recv.Abort();

            Debug.Log("[NetWork]:Socket Thread Close");
        }
        if (isConnected)
        {
            //             _inner_tcp_stream.Close();
            //             _inner_socket.Close();
        }
    }

    private bool Connect(string ip, int port)
    {
        try
        {
            Debug.LogError(" con " + ip + "   " + port);

         if(   UdtImpl.Connected(ip, "9000"))
         {
             Debug.LogError("conn ok");
         }
         else
         {
             Debug.LogError(" conn error");
         }
            //  _inner_socket.Connect(ip, port);
            //   _inner_tcp_stream = _inner_socket.GetStream();
            this.isConnected = true;
        }
        catch (Exception e)
        {
            Debug.Log(e);
            return false;
        }
        return true;
    }

    private void InitSocket()
    {
        UdtImpl.StartUp();
        //  _inner_socket = new TcpClient();
        _recvQueue = new ThreadSafeQueue();
        _recv_list = new ArrayList();

        _sendQueue = new ThreadSafeQueue();

    }
    private void InitThread()
    {

        t_send = new Thread(new ThreadStart(this.ThreadFunction_Send));
        t_recv = new Thread(new ThreadStart(this.ThreadFunction_Recv));

        t_send.Start();
        t_recv.Start();

        isThreadInit = true;
    }
    private void ThreadFunction_Send()
    {

        while (true)
        {
            Thread.Sleep(1);

            while (_sendQueue.Empty() == false)
            {
                var msg = ((MsgStream)_sendQueue.Dequeue()).Msg;
                byte[] buffer = System.Text.Encoding.Default.GetBytes(msg);
                int len = buffer.Length;

                byte[] buf = System.BitConverter.GetBytes(len);
                byte[] send = new byte[buf.Length + len];

                buf.CopyTo(send, 0);
                buffer.CopyTo(send, buf.Length);
                Debug.LogWarning("send len=" + send.Length + " content=" + msg);
                //
                //  _inner_tcp_stream.Write(send, 0, send.Length);
                //   UdtImpl.Send(msg, msg.Length);
                var str = System.Text.Encoding.Default.GetString(send, 0, send.Length);
                UdtImpl.Send(msg, msg.Length);

            }
        }

    }

    private void ThreadFunction_Recv()
    {

        while (true)
        {
            Thread.Sleep(1);
            if (UdtImpl.Recv() > 0)
            {
                string str = Marshal.PtrToStringAnsi(UdtImpl.GetBuffer());
                var msg = MsgStream.Create(str);

                Debug.LogError("recv:" + str);
                MsgType type = msg.Type;
                if (type == MsgType.CUSTOM_CMD)
                {
                    var cmd = (CustomMsgType)msg.Int;
                    if (cmd == CustomMsgType.ping)
                    {
                        TimeSpan ts = DateTime.Now - mLastPingTime;//mLastPingTimes[idx];
                        mPing = (int)(ts.TotalMilliseconds);
                        mLastPingTime = DateTime.Now;
                        continue;
                    }
                }
                msg.ResetReader();
                _recvQueue.Enqueue(msg);
            }
        }
    }*/
}

