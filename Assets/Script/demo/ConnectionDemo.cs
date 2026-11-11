using UnityEngine;

// 无需手工绑定按钮：自动创建测试入口，或手工挂到空GameObject。
public sealed class ConnectionDemo : MonoBehaviour
{
    string host = "127.0.0.1";
    string port = "8888";
    string status = "Ready";
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (FindObjectOfType<ConnectionDemo>() == null)
            new GameObject("ConnectionDemo").AddComponent<ConnectionDemo>();
    }
    void OnEnable()
    {
        NetManager.AddEventListener(NetManager.NetEvent.ConnectSucc, OnConnectSucc);
        NetManager.AddEventListener(NetManager.NetEvent.ConnectFail, OnConnectFail);
        NetManager.AddEventListener(NetManager.NetEvent.Close, OnConnectClose);
    }
    void Update() { NetManager.Update(); }
    public void OnConnectClick()
    {
        int value;
        if (!int.TryParse(port, out value) || value < 1 || value > 65535) { status = "Invalid port"; return; }
        if (NetManager.IsConnected || NetManager.IsConnecting) return;
        status = "Connecting...";
        NetManager.Connect(host, value);
    }
    public void OnCloseClick() { NetManager.Close(); }
    void OnConnectSucc(string err) { status = "Connected"; Debug.Log("OnConnectSucc"); }
    void OnConnectFail(string err) { status = "Connect failed: " + err; Debug.Log("OnConnectFail " + err); }
    void OnConnectClose(string err) { status = "Closed"; Debug.Log("OnConnectClose"); }
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(20, 20, 520, 260), GUI.skin.box);
        GUILayout.Label("Chapter 6 - TCP Connection Test");
        host = GUILayout.TextField(host);
        port = GUILayout.TextField(port);
        if (GUILayout.Button("Connect Server")) OnConnectClick();
        if (GUILayout.Button("Disconnect")) OnCloseClick();
        GUILayout.Label(status);
        GUILayout.EndArea();
    }
    void OnDisable()
    {
        NetManager.RemoveEventListener(NetManager.NetEvent.ConnectSucc, OnConnectSucc);
        NetManager.RemoveEventListener(NetManager.NetEvent.ConnectFail, OnConnectFail);
        NetManager.RemoveEventListener(NetManager.NetEvent.Close, OnConnectClose);
        NetManager.Close();
    }
}
