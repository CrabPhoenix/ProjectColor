using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// 在编辑器中确保当前场景存在整体游戏阶段管理器和可编辑 UI。
/// </summary>
[InitializeOnLoad]
public static class StageSceneBootstrap
{
    /// <summary>
    /// 注册延迟检查，避免脚本刷新时立即修改场景。
    /// </summary>
    static StageSceneBootstrap()
    {
        EditorApplication.delayCall += EnsureStageManagerInScene;
    }

    /// <summary>
    /// 如果当前场景缺少阶段管理器，则自动创建一个场景对象。
    /// </summary>
    private static void EnsureStageManagerInScene()
    {
        if(Application.isPlaying) return;
        if(PrefabStageUtility.GetCurrentPrefabStage() != null) return;

        Scene activeScene = SceneManager.GetActiveScene();
        if(!activeScene.IsValid() || string.IsNullOrEmpty(activeScene.path)) return;
        bool sceneChanged = CleanupDuplicateEventSystems();
        if(Object.FindFirstObjectByType<GameStageManager>() != null)
        {
            if(sceneChanged)
            {
                EditorSceneManager.MarkSceneDirty(activeScene);
            }

            return;
        }

        GameObject managerObject = new GameObject("GameStageManager");
        managerObject.AddComponent<GameStageManager>();
        sceneChanged = true;

        if(sceneChanged)
        {
            EditorSceneManager.MarkSceneDirty(activeScene);
        }
    }

    /// <summary>
    /// 在编辑器中只保留一个 EventSystem，并确保其使用 Input System UI 模块。
    /// </summary>
    private static bool CleanupDuplicateEventSystems()
    {
        EventSystem[] eventSystems = Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
        if(eventSystems.Length == 0) return false;

        bool changed = false;
        EventSystem mainEventSystem = eventSystems[0];
        mainEventSystem.gameObject.name = "EventSystem";

        StandaloneInputModule standaloneInputModule = mainEventSystem.GetComponent<StandaloneInputModule>();
        if(standaloneInputModule != null)
        {
            Object.DestroyImmediate(standaloneInputModule);
            changed = true;
        }

        if(mainEventSystem.GetComponent<InputSystemUIInputModule>() == null)
        {
            mainEventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
            changed = true;
        }

        for(int i = 1; i < eventSystems.Length; i++)
        {
            if(eventSystems[i] == null) continue;

            Object.DestroyImmediate(eventSystems[i].gameObject);
            changed = true;
        }

        return changed;
    }
}
