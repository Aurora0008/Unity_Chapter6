using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class CreateConnectionScene
{
    [MenuItem("Chapter 6/Create Textbook Test Scene")]
    public static void Create()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning("请先停止Play，再生成教材测试场景。");
            return;
        }
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        ConnectionDemoUI.Create();
        if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");
        string path = AssetDatabase.GenerateUniqueAssetPath("Assets/Scenes/test.unity");
        EditorSceneManager.SaveScene(scene, path);
        Selection.activeGameObject = GameObject.Find("Canvas");
        Debug.Log("教材测试场景已保存：" + path + "。点击Play后可连接/断开，结果见Console。");
    }
}
