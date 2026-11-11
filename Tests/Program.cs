using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

class Program
{
    static void Check(bool condition, string message) { if (!condition) throw new Exception(message); Console.WriteLine("PASS " + message); }
    static void Pump(Func<bool> condition)
    {
        DateTime limit = DateTime.UtcNow.AddSeconds(5);
        while (!condition() && DateTime.UtcNow < limit) { NetManager.Update(); Thread.Sleep(5); }
        Check(condition(), "event completed within timeout");
    }
    static void Main()
    {
        int success = 0, second = 0, failed = 0, closed = 0;
        int mainThread = Thread.CurrentThread.ManagedThreadId;
        bool onMain = true;
        NetManager.EventListener first = e => { success++; onMain &= mainThread == Thread.CurrentThread.ManagedThreadId; };
        NetManager.EventListener other = e => second++;
        NetManager.AddEventListener(NetManager.NetEvent.ConnectSucc, first);
        NetManager.AddEventListener(NetManager.NetEvent.ConnectSucc, other);
        NetManager.AddEventListener(NetManager.NetEvent.ConnectFail, e => { failed++; Check(e.Length > 0, "failure has diagnostic"); });
        NetManager.AddEventListener(NetManager.NetEvent.Close, e => closed++);
        NetManager.Close(); NetManager.Update(); Check(closed == 0, "close before connecting is safe");
        var listener = new TcpListener(IPAddress.Loopback, 0); listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;
        var accept = listener.AcceptSocketAsync();
        NetManager.Connect("127.0.0.1", port);
        NetManager.Connect("127.0.0.1", port);
        using (Socket peer = accept.GetAwaiter().GetResult())
        {
            Pump(() => success == 1);
            Check(second == 1 && onMain, "multiple listeners execute on caller thread");
            NetManager.Connect("127.0.0.1", port); NetManager.Update();
            Check(success == 1, "duplicate connect does not emit success");
            NetManager.Close(); NetManager.Close(); Pump(() => closed == 1);
            Check(peer.Receive(new byte[1]) == 0, "close releases TCP connection");
        }
        NetManager.RemoveEventListener(NetManager.NetEvent.ConnectSucc, other);
        accept = listener.AcceptSocketAsync(); NetManager.Connect("127.0.0.1", port);
        using (Socket peer = accept.GetAwaiter().GetResult())
        {
            Pump(() => success == 2); Check(second == 1, "removed listener is not invoked on reconnect");
            NetManager.Close(); Pump(() => closed == 2);
        }
        listener.Stop();
        NetManager.Connect("127.0.0.1", port); Pump(() => failed == 1);
        Check(!NetManager.IsConnecting && !NetManager.IsConnected, "failed connection resets state");
        NetManager.Connect("127.0.0.1", -1); Pump(() => failed == 2);
        listener.Start(); accept = listener.AcceptSocketAsync();
        port = ((IPEndPoint)listener.LocalEndpoint).Port;
        NetManager.Connect("127.0.0.1", port);
        using (Socket peer = accept.GetAwaiter().GetResult()) { Pump(() => success == 3); NetManager.Close(); Pump(() => closed == 3); }
        listener.Stop();
        var buffer = new ByteArray(4); buffer.bytes[0] = 1; buffer.bytes[1] = 2; buffer.writeIdx = 2; buffer.readIdx = 1;
        buffer.MoveBytes(); Check(buffer.length == 1 && buffer.bytes[0] == 2, "buffer compaction preserves unread bytes");
        buffer.ReSize(8); Check(buffer.length == 1 && buffer.remain == 7 && buffer.bytes[0] == 2, "buffer resize preserves unread bytes");
        Console.WriteLine("ALL CHECKS PASSED");
    }
}
