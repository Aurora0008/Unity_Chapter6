using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// 编辑器场景生成器与运行时入口共用布局。
public static class ConnectionDemoUI
{
    public static ConnectionDemo Create()
    {
        var demo = new GameObject("ConnectionDemo").AddComponent<ConnectionDemo>();
        var canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas),
            typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(960, 540);
        scaler.matchWidthOrHeight = 0.5f;
        Button connect = CreateButton(canvasObject.transform, "ConnectButton", "连接服务器", new Vector2(-65, 0), new Vector2(200, 45));
        Button close = CreateButton(canvasObject.transform, "CloseButton", "断开", new Vector2(105, 0), new Vector2(100, 45));
#if UNITY_EDITOR
        // 保存为真正的Inspector On Click绑定，而不是仅运行时添加监听。
        if (!Application.isPlaying)
        {
            UnityEditor.Events.UnityEventTools.AddPersistentListener(connect.onClick, demo.OnConnectClick);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(close.onClick, demo.OnCloseClick);
        }
        else
#endif
        {
            connect.onClick.AddListener(demo.OnConnectClick);
            close.onClick.AddListener(demo.OnCloseClick);
        }
        if (Object.FindObjectOfType<EventSystem>() == null)
        {
            var events = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            events.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
            events.AddComponent<StandaloneInputModule>();
#endif
        }
        return demo;
    }

    static Button CreateButton(Transform parent, string name, string caption, Vector2 position, Vector2 size)
    {
        var obj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        obj.transform.SetParent(parent, false);
        var rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        var image = obj.GetComponent<Image>();
        image.color = new Color(0.95f, 0.95f, 0.95f);
        obj.GetComponent<Button>().targetGraphic = image;
        var label = new GameObject("Text", typeof(RectTransform), typeof(Text));
        label.transform.SetParent(obj.transform, false);
        var labelRect = label.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero; labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = labelRect.offsetMax = Vector2.zero;
        var text = label.GetComponent<Text>();
#if UNITY_2022_2_OR_NEWER
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
#else
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
#endif
        text.text = caption; text.fontSize = 24;
        text.color = Color.black; text.alignment = TextAnchor.MiddleCenter;
        text.raycastTarget = false;
        return obj.GetComponent<Button>();
    }
}
