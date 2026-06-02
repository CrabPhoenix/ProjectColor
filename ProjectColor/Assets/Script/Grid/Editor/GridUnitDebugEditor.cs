using UnityEditor;
using UnityEngine;

/// <summary>
/// GridUnitDebug 的 Inspector 按钮工具。
/// </summary>
[CustomEditor(typeof(GridUnitDebug))]
public class GridUnitDebugEditor : Editor
{
    private SerializedProperty emptyCellColor;
    private SerializedProperty occupiedCellColor;

    /// <summary>
    /// 缓存需要显示在 Inspector 中的颜色属性。
    /// </summary>
    private void OnEnable()
    {
        emptyCellColor = serializedObject.FindProperty("emptyCellColor");
        occupiedCellColor = serializedObject.FindProperty("occupiedCellColor");
    }

    /// <summary>
    /// 绘制单位占用 Debug 的独立控制按钮。
    /// </summary>
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(emptyCellColor);
        EditorGUILayout.PropertyField(occupiedCellColor);
        serializedObject.ApplyModifiedProperties();

        GridUnitDebug gridUnitDebug = (GridUnitDebug)target;
        if(GUILayout.Button("Toggle Unit Debug"))
        {
            gridUnitDebug.ToggleVisibility();
            SceneView.RepaintAll();
        }

        if(GUILayout.Button("Toggle Only Occupied Cells"))
        {
            gridUnitDebug.ToggleOnlyOccupiedCells();
            SceneView.RepaintAll();
        }
    }
}
