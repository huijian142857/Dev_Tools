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


public class UdpSocket : SocketClient
{
    System.Net.Sockets.UdpClient _inner_socket = null;

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
    public bool _IsConnected = false;
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
        _IsConnected = false;
        this.InitSocket();
        if (this.Connect(ip, port))
        {
            this.InitThread();
            Debug.Log("[NetWork]:UdpSocket Thread Open");
        }
        else
        {
            return false;
        }
        return true;

    }
    bool hasTerminal = false;
    ~UdpSocket()
    {
        this.Terminal();
    }
    private void TryCloseSocket()
    {
        if (_inner_socket != null)
        {
            try
            {
                _inner_socket.Close();
            }
            catch (Exception e) { }
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
            Debug.Log("[NetWork]:UdpSocket Thread Terminal");
        }
        this.TryCloseSocket();
        this.Disconnected();
    }
    private IPEndPoint _server_endpoint = null;
    private bool Connect(string ip, int port)
    {
        try
        {
            if (port == 0 || ip == "") return false;
            Debug.Log("connect battle udp-server " + ip + ":" + port);
            this.TryCloseSocket();
            IPAddress ipAddress;
            AddressFamily af;
            bool ok = Misc.Utils.IPV4ToIPV6(ip, out ipAddress, out af);
            _server_endpoint = new IPEndPoint(ipAddress, port);
            _inner_socket = new UdpClient();
            _inner_socket.Connect(_server_endpoint);
            _inner_socket.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, 1024 * 16);
            _inner_socket.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, 1024 * 32);
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
    public const int MAX_VALID_BUFFER_LEN = (1400 - 100);//max valid buffer lenth   20kb-100 byte
    private static byte[] _buffer_send = new byte[1400];// 20kb buffer cache ,avoid memory-alloc
    private void ThreadFunction_Send()
    {
        MsgStream ms = null;
        try
        {
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
                    bool ok = false;
                    using (ms)
                    {
                        //减小 缓冲区 异常 的概率
                        //TODO 考虑改为 异步IO
                        Thread.Sleep(1);
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
                                int send_len = _inner_socket.Send(_buffer_send, len);

                                if (send_len == len)
                                {
                                    ok = true;
                                }
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
        Debug.Log("[NetWork]:UdpSocket Send Thread Close");
    }
    private void Disconnected()
    {
        isConnected = false;
    }
    private void ThreadFunction_Recv()
    {
        try
        {
            while (_thread_running && isConnected)
            {
                var bufraw = _inner_socket.Receive(ref _server_endpoint);
                if (bufraw.Length > 4)
                {
                    byte[] buf = Base.MemoryPool.Alloc(bufraw.Length - 4);    //new byte[bufraw.Length - 4];
                    Array.Copy(bufraw, 4, buf, 0, bufraw.Length - 4);
                    XOREncrypt.Decrypt(buf, bufraw.Length - 4);
                    MsgStream msg = MsgStream.Create(buf, bufraw.Length - 4);

                    if (msg.IsCustomCmd)
                    {
                        if (msg.CustomType == CustomMsgType.ping)
                        {
                            //服务器返回了 证明连接成功了
                            _IsConnected = true;
                            TimeSpan ts = DateTime.Now - mLastPingTime;//mLastPingTimes[idx];
                            mPing = (int)(ts.TotalMilliseconds);
                            mLastPingTime = DateTime.Now;
                            //    this.LastSendPingTimestamp = Utils.GetTimestampSecondsInt();
                            msg.Dispose();
                            continue;
                        }
                    }
                    //服务器返回了 证明连接成功了
                    _IsConnected = true;
                    _recvQueue.Enqueue(msg);
                }
                else
                {
                    //<=4
                }
            }
        }
        catch (SocketException e)
        {
            Debug.Log("UdpSocket:" + e.Message + "   " + e.ErrorCode + "  " + e.NativeErrorCode + "  " + e.SocketErrorCode);
        }
        catch (Exception e)
        {
            Debug.Log("UdpSocket:" + e.Message);
        }
        this.Disconnected();
        while (_recvQueue.Empty() == false)
        {
            var x = _recvQueue.Dequeue();
            if (x != null) x.Dispose();
        }
        Debug.Log("[NetWork]:UdpSocket Recv Thread Close");
    }

    public void Tick(float deltaTime)
    {

    }
}