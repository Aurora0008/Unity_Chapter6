using UnityEngine;
using UnityEngine.UI;

// 教材6.4.3：Canvas下两个按钮，网络事件写入Console。
public sealed class ConnectionDemo : MonoBehaviour
{
    [SerializeField] string host = "127.0.0.1";
    [SerializeField] int port = 8888;
    public string Status { get; private set; }
    Font runtimeFont;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (FindObjectOfType<ConnectionDemo>() == null)
            ConnectionDemoUI.Create();
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
        if (port < 1 || port > 65535) { Debug.LogError("端口必须在1～65535之间"); return; }
        if (NetManager.IsConnected || NetManager.IsConnecting) return;
        Status = "连接中…";
        NetManager.Connect(host, port);
    }
    public void OnCloseClick() { NetManager.Close(); }
    void OnConnectSucc(string err) { Status = "连接成功"; Debug.Log("OnConnectSucc"); }
    void OnConnectFail(string err) { Status = "连接失败：" + err; Debug.Log("OnConnectFail " + err); }
    void OnConnectClose(string err) { Status = "连接已关闭"; Debug.Log("OnConnectClose"); }
    void Start()
    {
        // 使用本机中文字体；编辑器保存的场景仍引用Unity内置字体。
        string[] preferred = { "Microsoft YaHei", "SimHei", "PingFang SC", "Noto Sans CJK SC", "WenQuanYi Zen Hei" };
        var installed = new System.Collections.Generic.HashSet<string>(Font.GetOSInstalledFontNames());
        foreach (string name in preferred)
        {
            if (!installed.Contains(name)) continue;
            runtimeFont = Font.CreateDynamicFontFromOSFont(name, 24);
            foreach (Text text in FindObjectsOfType<Text>())
                if (text.transform.root.name == "Canvas") text.font = runtimeFont;
            break;
        }
    }
    void OnDisable()
    {
        NetManager.RemoveEventListener(NetManager.NetEvent.ConnectSucc, OnConnectSucc);
        NetManager.RemoveEventListener(NetManager.NetEvent.ConnectFail, OnConnectFail);
        NetManager.RemoveEventListener(NetManager.NetEvent.Close, OnConnectClose);
        NetManager.Close();
    }
    void OnDestroy() { if (runtimeFont != null) Destroy(runtimeFont); }
}
