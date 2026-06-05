using UnityEditor;
using UnityEngine;

/// <summary>
/// TerrainDebug 的 Inspector 按钮工具。
/// </summary>
[CustomEditor(typeof(TerrainDebug))]
public class TerrainDebugEditor : Editor
{
    private SerializedProperty emptyTerrainColor;
    private SerializedProperty plateTerrainColor;
    private SerializedProperty waterTerrainColor;
    private SerializedProperty slopeTerrainColor;

    /// <summary>
    /// 缓存需要显示在 Inspector 中的颜色属性。
    /// </summary>
    private void OnEnable()
    {
        emptyTerrainColor = serializedObject.FindProperty("emptyTerrainColor");
        plateTerrainColor = serializedObject.FindProperty("plateTerrainColor");
        waterTerrainColor = serializedObject.FindProperty("waterTerrainColor");
        slopeTerrainColor = serializedObject.FindProperty("slopeTerrainColor");
    }

    /// <summary>
    /// 绘制地形 Debug 的独立控制按钮。
    /// </summary>
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(emptyTerrainColor);
        EditorGUILayout.PropertyField(plateTerrainColor);
        EditorGUILayout.PropertyField(waterTerrainColor);
        EditorGUILayout.PropertyField(slopeTerrainColor);
        serializedObject.ApplyModifiedProperties();

        TerrainDebug terrainDebug = (TerrainDebug)target;
        if(GUILayout.Button("Show Terrain Debug"))
        {
            terrainDebug.ToggleVisibility();
            SceneView.RepaintAll();
        }

        if(GUILayout.Button("Show Only Terrains"))
        {
            terrainDebug.ToggleOnlyTerrains();
            SceneView.RepaintAll();
        }
    }
}
