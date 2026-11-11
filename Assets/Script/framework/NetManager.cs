using System;
using System.Collections.Generic;
using System.Net.Sockets;

// 6.1～6.4：Socket回调只入队，Unity主线程在Update中分发事件。
public static class NetManager
{
    public enum NetEvent { ConnectSucc = 1, ConnectFail = 2, Close = 3 }
    public delegate void EventListener(string err);
    static readonly object gate = new object();
    static readonly Dictionary<NetEvent, EventListener> eventListeners = new Dictionary<NetEvent, EventListener>();
    static readonly Queue<Action> pending = new Queue<Action>();
    static Socket socket;
    static ByteArray readBuff;
    static Queue<ByteArray> writeQueue;
    static bool isConnecting;
    static bool isClosing;
    public static bool IsConnected { get { lock (gate) return socket != null && socket.Connected && !isConnecting; } }
    public static bool IsConnecting { get { lock (gate) return isConnecting; } }

    public static void AddEventListener(NetEvent type, EventListener listener)
    {
        if (listener == null) throw new ArgumentNullException("listener");
        lock (gate) { EventListener old; eventListeners.TryGetValue(type, out old); eventListeners[type] = old + listener; }
    }
    public static void RemoveEventListener(NetEvent type, EventListener listener)
    {
        lock (gate)
        {
            EventListener old;
            if (!eventListeners.TryGetValue(type, out old)) return;
            old -= listener;
            if (old == null) eventListeners.Remove(type); else eventListeners[type] = old;
        }
    }
    static void FireEvent(NetEvent type, string err)
    {
        // 调用快照，允许回调增删监听器。某个监听器异常不阻止其他监听器。
        pending.Enqueue(delegate {
            EventListener handlers;
            lock (gate) eventListeners.TryGetValue(type, out handlers);
            if (handlers == null) return;
            foreach (EventListener handler in handlers.GetInvocationList())
            {
                try { handler(err); } catch (Exception ex) { System.Diagnostics.Trace.TraceError(ex.ToString()); }
            }
        });
    }
    public static void Update()
    {
        int count;
        lock (gate) count = pending.Count;
        for (int i = 0; i < count; i++)
        {
            Action action;
            lock (gate) { if (pending.Count == 0) break; action = pending.Dequeue(); }
            action();
        }
    }
    static void InitState()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        readBuff = new ByteArray();
        writeQueue = new Queue<ByteArray>();
        isConnecting = false; isClosing = false;
    }
    public static void Connect(string ip, int port)
    {
        lock (gate)
        {
            // 与教材一致：已连接或连接中，重复点击直接返回。
            if (socket != null && (isConnecting || socket.Connected)) return;
            if (socket != null) socket.Close();
            InitState();
            socket.NoDelay = true;
            isConnecting = true;
            try { socket.BeginConnect(ip, port, ConnectCallback, socket); }
            catch (Exception ex) { FailConnect(socket, ex); }
        }
    }
    static void FailConnect(Socket target, Exception ex)
    {
        if (target != socket) return;
        target.Close(); socket = null; isConnecting = false;
        FireEvent(NetEvent.ConnectFail, ex.Message);
    }
    static void ConnectCallback(IAsyncResult ar)
    {
        Socket target = (Socket)ar.AsyncState;
        lock (gate)
        {
            try
            {
                target.EndConnect(ar);
                if (target != socket) { target.Close(); return; }
                isConnecting = false;
                FireEvent(NetEvent.ConnectSucc, "");
            }
            catch (Exception ex) { FailConnect(target, ex); }
        }
    }
    public static void Close()
    {
        lock (gate)
        {
            if (socket == null || isConnecting || !socket.Connected || isClosing) return;
            if (writeQueue.Count > 0) { isClosing = true; return; }
            socket.Close(); socket = null; isClosing = false;
            FireEvent(NetEvent.Close, "");
        }
    }
}
