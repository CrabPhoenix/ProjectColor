using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// UnitDeployConfig 的编辑器工具，用于配置可部署玩家单位和数量。
/// </summary>
[CustomEditor(typeof(UnitDeployConfig))]
public class UnitDeployConfigEditor : Editor
{
    private SerializedProperty unitsProperty;

    /// <summary>
    /// 缓存部署单位列表属性。
    /// </summary>
    private void OnEnable()
    {
        unitsProperty = serializedObject.FindProperty("units");
    }

    /// <summary>
    /// 绘制部署配置 Inspector。
    /// </summary>
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Deploy Units", EditorStyles.boldLabel);
        for(int i = 0; i < unitsProperty.arraySize; i++)
        {
            DrawDeployEntry(i);
        }

        EditorGUILayout.Space();
        if(GUILayout.Button("Add Deploy Unit"))
        {
            unitsProperty.InsertArrayElementAtIndex(unitsProperty.arraySize);
            SerializedProperty entryProperty = unitsProperty.GetArrayElementAtIndex(unitsProperty.arraySize - 1);
            entryProperty.FindPropertyRelative("unitPrefab").objectReferenceValue = null;
            entryProperty.FindPropertyRelative("count").intValue = 1;
        }

        serializedObject.ApplyModifiedProperties();
    }

    /// <summary>
    /// 绘制单个部署单位条目。
    /// </summary>
    private void DrawDeployEntry(int index)
    {
        SerializedProperty entryProperty = unitsProperty.GetArrayElementAtIndex(index);
        SerializedProperty unitPrefabProperty = entryProperty.FindPropertyRelative("unitPrefab");
        SerializedProperty countProperty = entryProperty.FindPropertyRelative("count");
        Unit unitPrefab = unitPrefabProperty.objectReferenceValue as Unit;

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField($"Element {index}", EditorStyles.boldLabel);

        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.ObjectField("Unit Prefab", unitPrefab != null ? unitPrefab.gameObject : null, typeof(GameObject), false);
        EditorGUI.EndDisabledGroup();

        countProperty.intValue = Mathf.Max(0, EditorGUILayout.IntField("Count", countProperty.intValue));

        EditorGUILayout.BeginHorizontal();
        if(GUILayout.Button("Select Player Unit Prefab"))
        {
            PlayerUnitPrefabPickerWindow.Open(selectedUnit =>
            {
                Undo.RecordObject(target, "Select Deploy Unit Prefab");
                serializedObject.Update();
                unitPrefabProperty.objectReferenceValue = selectedUnit;
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
                Repaint();
            });
        }

        if(GUILayout.Button("Clear"))
        {
            Undo.RecordObject(target, "Clear Deploy Unit Prefab");
            unitPrefabProperty.objectReferenceValue = null;
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }

        if(GUILayout.Button("Remove"))
        {
            Undo.RecordObject(target, "Remove Deploy Unit");
            unitsProperty.DeleteArrayElementAtIndex(index);
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            return;
        }

        EditorGUILayout.EndHorizontal();

        if(unitPrefab != null && unitPrefab.Team != UnitTeam.Player)
        {
            EditorGUILayout.HelpBox("部署配置只会使用 Player 阵营单位。", MessageType.Warning);
        }

        EditorGUILayout.EndVertical();
    }
}

/// <summary>
/// 只显示玩家阵营 Unit Prefab 的选择窗口。
/// </summary>
public class PlayerUnitPrefabPickerWindow : EditorWindow
{
    private readonly List<Unit> unitPrefabs = new List<Unit>();
    private Action<Unit> onSelected;
    private Vector2 scrollPosition;
    private string searchText = string.Empty;

    /// <summary>
    /// 打开玩家单位 Prefab 选择窗口。
    /// </summary>
    public static void Open(Action<Unit> onSelected)
    {
        PlayerUnitPrefabPickerWindow window = CreateInstance<PlayerUnitPrefabPickerWindow>();
        window.titleContent = new GUIContent("Player Unit Prefabs");
        window.minSize = new Vector2(340f, 420f);
        window.onSelected = onSelected;
        window.RefreshPlayerUnitPrefabs();
        window.ShowUtility();
    }

    /// <summary>
    /// 绘制可选玩家单位 Prefab 列表。
    /// </summary>
    private void OnGUI()
    {
        EditorGUILayout.LabelField("Select Player Unit Prefab", EditorStyles.boldLabel);
        searchText = EditorGUILayout.TextField("Search", searchText);

        EditorGUILayout.Space();
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        foreach(Unit unitPrefab in unitPrefabs)
        {
            if(unitPrefab == null) continue;
            if(!string.IsNullOrEmpty(searchText) && !unitPrefab.name.ToLowerInvariant().Contains(searchText.ToLowerInvariant())) continue;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.ObjectField(unitPrefab.gameObject, typeof(GameObject), false);
            if(GUILayout.Button("Select", GUILayout.Width(80f)))
            {
                onSelected?.Invoke(unitPrefab);
                Close();
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();

        if(unitPrefabs.Count == 0)
        {
            EditorGUILayout.HelpBox("项目中没有找到 Player 阵营的 Unit Prefab。", MessageType.Info);
        }

        EditorGUILayout.Space();
        if(GUILayout.Button("Cancel"))
        {
            Close();
        }
    }

    /// <summary>
    /// 从项目中收集所有玩家阵营 Unit Prefab。
    /// </summary>
    private void RefreshPlayerUnitPrefabs()
    {
        unitPrefabs.Clear();
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        foreach(string prefabGuid in prefabGuids)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if(prefab == null) continue;

            Unit unitPrefab = prefab.GetComponent<Unit>();
            if(unitPrefab == null || unitPrefab.Team != UnitTeam.Player) continue;

            unitPrefabs.Add(unitPrefab);
        }

        unitPrefabs.Sort((left, right) => string.Compare(left.name, right.name, StringComparison.OrdinalIgnoreCase));
    }
}
