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
using System.IO;
using System.Net;
using System.Runtime.InteropServices;


public class TcpSocket : SocketClient
{
    System.Net.Sockets.Socket _inner_socket = null;

    private ThreadSafeQueue<MsgStream> _sendQueue;
    public ThreadSafeQueue<MsgStream> _recvQueue;
    private bool _thread_running = true;

    public override MsgStream PickOneMsg()
    {
        var x = _recvQueue.Dequeue();
        return x as MsgStream;
    }
    private Thread t_send;
    private Thread t_recv;
    private bool isThreadInit = false;

    public override void AddSendMsg(MsgStream msg)
    {
        if (!_thread_running || !isConnected)
        {
            msg.Dispose();
            return;
        }
        //   msg += "\0";
        _sendQueue.Enqueue(msg);
    }

    public override bool Startup(string ip, int port)
    {
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
    bool hasTerminal = false;
    ~TcpSocket()
    {
        this.Terminal();
    }
    private void TryCloseSocket()
    {
        if (_inner_socket != null)
        {
            try
            {
                _inner_socket.Shutdown(SocketShutdown.Both);
            }
            catch (Exception e) { }
            try
            {
                _inner_socket.Disconnect(false);
            }
            catch (Exception e) { }
            try
            {
                _inner_socket.Close();
            }
            catch (Exception e) { }
            /*
#if !UNITY_EDITOR
            try
            {
                    _inner_socket.Dispose();
            }
            catch (Exception e) { }
#endif
            */
            _inner_socket = null;
        }
    }
    public override void Terminal()
    {
        if (hasTerminal) return;
        hasTerminal = true;
        if (isThreadInit)
        {
            _thread_running = false;
            // t_send.Abort();
            // t_recv.Abort();
            Debug.Log("[NetWork]:Socket Thread Terminal");
        }
        this.TryCloseSocket();
        this.Disconnected();
    }

    private bool Connect(string ip, int port)
    {
        try
        {
            if (port == 0 || ip == "") return false;
            Debug.Log("connect battle server " + ip + ":" + port);

            IPAddress ipAddress;
            AddressFamily af;
            bool ok = Misc.Utils.IPV4ToIPV6(ip, out ipAddress, out af);
            this.TryCloseSocket();
            _inner_socket = new Socket(af, System.Net.Sockets.SocketType.Stream, ProtocolType.Tcp);
            _inner_socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
            _inner_socket.Connect(new IPEndPoint(ipAddress, port));
            _inner_socket.Blocking = true;
            _inner_socket.NoDelay = true;
            _inner_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, 1024 * 16);
            _inner_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, 1024 * 16);
            this.isConnected = true;
        }
        catch (Exception e)
        {
            this.isConnected = false;
            _inner_socket = null;
            Debug.Log(e.Message);
            return false;
        }
        return true;
    }

    private void InitSocket()
    {
        _recvQueue = new ThreadSafeQueue<MsgStream>();
        _sendQueue = new ThreadSafeQueue<MsgStream>();
    }
    private void InitThread()
    {
        t_send = new Thread(new ThreadStart(this.ThreadFunction_Send));
        t_recv = new Thread(new ThreadStart(this.ThreadFunction_Recv));
        _thread_running = true;
        isThreadInit = true;
        t_send.Start();
        t_recv.Start();

    }
    public const int MAX_VALID_BUFFER_LEN = (4000);//max valid buffer lenth   20kb-100 byte
    private static byte[] _buffer_send = new byte[4096];// 4kb buffer cache ,avoid memory-alloc
    private void ThreadFunction_Send()
    {
        MsgStream ms = null;
        try
        {
            _inner_socket.NoDelay = true;
            while (_thread_running && isConnected)
            {
                Thread.Sleep(1);
                if (ms != null)
                {
                    ms.Dispose();
                    ms = null;
                }
                while (_sendQueue.Empty() == false && isConnected && _thread_running)
                {
                    ms = _sendQueue.Dequeue();
                    if (ms == null) continue;
                    //减小 缓冲区 异常 的概率
                    //TODO 考虑改为 异步IO
                    bool ok = false;
                    using (ms)
                    {
                        byte[] buffer = null;
                        buffer = ms.stream.buffer;
                        XOREncrypt.Encrypt(buffer, buffer.Length);
                        int len = ms.stream.Length;
                        if (len < MAX_VALID_BUFFER_LEN)
                        {
                            try
                            {
                                byte[] buf = System.BitConverter.GetBytes(len);
                                len = buf.Length + len;
                                buf.CopyTo(_buffer_send, 0);//write data len

                                buffer.CopyTo(_buffer_send, buf.Length);//write data
                                                                        // Debug.LogError("send cus " + ms.Msg + "len " + send.Length);
                                                                        //    Debug.LogWarning("send buffer len=" + len);
                                int send_len = 0;
                                while (send_len < len)
                                {
                                    int l = _inner_socket.Send(_buffer_send, send_len, len - send_len, SocketFlags.None);
                                    if (l >= 0)
                                    {
                                        send_len += l;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }

                                if (send_len == len) ok = true;
                            }
                            catch (Exception e)
                            {
                                ok = false;
                            }
                        }
                    }
                    ms = null;
                    if (!ok)
                    {
                        Debug.LogError("TcpSocket error send buffer overfollow 0x1  ");
                        this.Disconnected();
                        break;
                    }
                }
            }
            this.Disconnected();
        }
        catch (Exception e)
        {
            Debug.Log(e);
            this.Disconnected();
        }
        if (ms != null)
        {
            ms.Dispose();
            ms = null;
        }
        while (_sendQueue.Empty() == false)
        {
            var x = _sendQueue.Dequeue();
            if (x != null) x.Dispose();
        }
        Debug.Log("[NetWork]:Socket Send Thread Close");
    }
    private void Disconnected()
    {
        isConnected = false;
    }
    byte[] _buffer_head = new byte[4];

    private void ThreadFunction_Recv()
    {
        try
        {
            _inner_socket.NoDelay = true;
            while (_thread_running && isConnected)
            {
                //      Thread.Sleep(1);//能减少socket error 10054 出现的 概率
                byte[] buffer = _buffer_head;
                int c = 0;
                bool will_break = false;
                while (c < 4)
                {
                    int l = _inner_socket.Receive(buffer, c, 4 - c, SocketFlags.None);  // _inner_tcp_stream.Read(buffer, c, 4 - c);
                    if (l > 0)
                    {
                        c += l;
                    }
                    else if (l == 0)
                    {
                        this.Disconnected();
                        Debug.Log("socket read faild 4 server disconnected");
                        will_break = true;
                        break;
                    }
                    else
                    {
                        this.Disconnected();
                        Debug.Log("socket read faild 1");
                        will_break = true;
                        break;
                    }
                }
                if (will_break) break;
                if (!isConnected || !_thread_running) break;
                int len = (int)System.BitConverter.ToInt32(buffer, 0);
                if (len < MAX_VALID_BUFFER_LEN)
                {
                    //byte[] buf = new byte[len];
                    byte[] buf = Base.MemoryPool.Alloc(len);
                    try
                    {
                        int read_len = 0;
                        while (read_len < len)
                        {
                            int l = _inner_socket.Receive(buf, read_len, len - read_len, SocketFlags.None);// _inner_tcp_stream.Read(buf, read_len, len - read_len);
                            if (l > 0)
                            {
                                read_len += l;
                            }
                            else if (l == 0)
                            {

                                /*                       
               //https://msdn.microsoft.com/zh-cn/library/8s4y8aff(v=vs.90).aspx
                       如果当前使用的是面向连接的 Socket，那么 Receive 方法将会读取所有可用的数据，直到达到缓冲区的大小为止。如果远程主机使用 Shutdown 方法关闭了 Socket 连接，并且所有可用数据均已收到，则 Receive 方法将立即完成并返回零字节。*/
                                break;
                            }
                            else
                            {
                                break;
                            }
                        }
                        if (read_len != len)
                        {
                            this.Disconnected();
                            Debug.Log("socket read faild 2");
                            break;
                        }
                        XOREncrypt.Decrypt(buf, len);
                        byte by = buf[0];
                        if (by == 1) // 占位符
                        {
                            MsgStream msg = MsgStream.Create(buf, len);

                            if (msg.IsCustomCmd)
                            {
                                if (msg.CustomType == CustomMsgType.ping)
                                {
                                    TimeSpan ts = DateTime.Now - mLastPingTime;//mLastPingTimes[idx];
                                    mPing = (int)(ts.TotalMilliseconds);
                                    mLastPingTime = DateTime.Now;
                                    //    this.LastSendPingTimestamp = Utils.GetTimestampSecondsInt();
                                    msg.Dispose();
                                    continue;
                                }
                            }
                            _recvQueue.Enqueue(msg);
                        }
                        else
                        {
                            //maybe dicconnected by server or net error
                            Debug.LogError("****************** unexp bytes headdrer=" + (int)by + " len=" + len + " read_len=" + read_len);
                            this.Disconnected();
                        }
                    }
                    catch (Exception e)
                    {
                        //不回收  让他GC掉好了
                    }
                }
                else
                {
                    this.Disconnected();
                    Debug.LogError("TcpSocket error send buffer overfollow 0x2  ");
                }
            }
        }
        catch (SocketException e)
        {
            Debug.Log("TcpSocket:" + e.Message + "   " + e.ErrorCode + "  " + e.NativeErrorCode + "  " + e.SocketErrorCode);
        }
        catch (Exception e)
        {
            Debug.Log("TcpSocket:" + e.Message);
        }
        this.Disconnected();
        while (_recvQueue.Empty() == false)
        {
            var x = _recvQueue.Dequeue();
            if (x != null) x.Dispose();
        }
        Debug.Log("[NetWork]:Socket Recv Thread Close");
    }
}


//本地虚拟服务器  可用于局域网对战
//BattleServer的 客户端实现 只有消息转发功能
//TODO BattleServer.cs 使用 内部直接转接到该class
public class LocalServer
{

}