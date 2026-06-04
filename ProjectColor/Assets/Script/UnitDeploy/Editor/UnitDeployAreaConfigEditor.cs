using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// UnitDeployAreaConfig 的编辑器工具，用于校验部署范围坐标。
/// </summary>
[CustomEditor(typeof(UnitDeployAreaConfig))]
public class UnitDeployAreaConfigEditor : Editor
{
    private const string PlacementConfigPath = "Assets/Config/DefaultUnitPlacementConfig.asset";

    private SerializedProperty rectangleStartCellProperty;
    private SerializedProperty rectangleEndCellProperty;
    private SerializedProperty extraDeployableCellsProperty;

    /// <summary>
    /// 缓存部署范围配置字段。
    /// </summary>
    private void OnEnable()
    {
        rectangleStartCellProperty = serializedObject.FindProperty("rectangleStartCell");
        rectangleEndCellProperty = serializedObject.FindProperty("rectangleEndCell");
        extraDeployableCellsProperty = serializedObject.FindProperty("extraDeployableCells");
    }

    /// <summary>
    /// 绘制部署范围配置 Inspector。
    /// </summary>
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        HashSet<Vector2Int> validCells = LoadValidCells();
        UnitDeployAreaConfig areaConfig = (UnitDeployAreaConfig)target;

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.LabelField("Deploy Rectangle", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(rectangleStartCellProperty, new GUIContent("Start Cell"));
        EditorGUILayout.PropertyField(rectangleEndCellProperty, new GUIContent("End Cell"));
        bool rectangleChanged = EditorGUI.EndChangeCheck();

        EditorGUILayout.Space();
        bool extraChanged = DrawExtraDeployableCells(areaConfig, validCells);

        serializedObject.ApplyModifiedProperties();

        if(rectangleChanged || extraChanged)
        {
            Undo.RecordObject(areaConfig, "Validate Unit Deploy Area");
            areaConfig.Normalize(validCells);
            EditorUtility.SetDirty(areaConfig);
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("所有格子坐标都会被修正为非负整数。额外坐标如果无效、重复或落在矩形范围内，会被自动移除。", MessageType.Info);

        if(validCells.Count == 0)
        {
            EditorGUILayout.HelpBox("没有从 DefaultUnitPlacementConfig.asset 读取到有效格子，当前只会执行非负坐标与重复坐标校验。", MessageType.Warning);
        }
    }

    /// <summary>
    /// 绘制额外部署格列表，并在新增时自动选择第一个可添加格子。
    /// </summary>
    private bool DrawExtraDeployableCells(UnitDeployAreaConfig areaConfig, HashSet<Vector2Int> validCells)
    {
        bool changed = false;
        extraDeployableCellsProperty.isExpanded = EditorGUILayout.Foldout(extraDeployableCellsProperty.isExpanded, "Extra Deployable Cells", true);
        if(!extraDeployableCellsProperty.isExpanded) return false;

        EditorGUI.indentLevel++;
        for(int i = 0; i < extraDeployableCellsProperty.arraySize; i++)
        {
            EditorGUILayout.BeginHorizontal();

            SerializedProperty cellProperty = extraDeployableCellsProperty.GetArrayElementAtIndex(i);
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(cellProperty, new GUIContent($"Element {i}"));
            if(EditorGUI.EndChangeCheck())
            {
                changed = true;
            }

            if(GUILayout.Button("Remove", GUILayout.Width(70f)))
            {
                extraDeployableCellsProperty.DeleteArrayElementAtIndex(i);
                changed = true;
                EditorGUILayout.EndHorizontal();
                break;
            }

            EditorGUILayout.EndHorizontal();
        }

        if(GUILayout.Button("Add Extra Cell"))
        {
            if(TryFindFirstAvailableExtraCell(areaConfig, validCells, out Vector2Int newCell))
            {
                int index = extraDeployableCellsProperty.arraySize;
                extraDeployableCellsProperty.InsertArrayElementAtIndex(index);
                extraDeployableCellsProperty.GetArrayElementAtIndex(index).vector2IntValue = newCell;
                changed = true;
            }
            else
            {
                EditorUtility.DisplayDialog("Extra Deployable Cells", "没有可添加到 Extra Deployable Cells 的有效格子。", "OK");
            }
        }

        EditorGUI.indentLevel--;
        return changed;
    }

    /// <summary>
    /// 按 x、y 的顺序查找第一个可以添加到额外部署格列表的有效格子。
    /// </summary>
    private bool TryFindFirstAvailableExtraCell(UnitDeployAreaConfig areaConfig, HashSet<Vector2Int> validCells, out Vector2Int result)
    {
        result = Vector2Int.zero;
        if(validCells.Count == 0) return false;

        List<Vector2Int> sortedCells = new List<Vector2Int>(validCells);
        sortedCells.Sort((left, right) =>
        {
            int xCompare = left.x.CompareTo(right.x);
            if(xCompare != 0) return xCompare;

            return left.y.CompareTo(right.y);
        });

        HashSet<Vector2Int> existingExtraCells = new HashSet<Vector2Int>();
        for(int i = 0; i < extraDeployableCellsProperty.arraySize; i++)
        {
            existingExtraCells.Add(extraDeployableCellsProperty.GetArrayElementAtIndex(i).vector2IntValue);
        }

        foreach(Vector2Int cell in sortedCells)
        {
            if(cell.x < 0 || cell.y < 0) continue;
            if(areaConfig.IsInsideRectangle(cell)) continue;
            if(existingExtraCells.Contains(cell)) continue;

            result = cell;
            return true;
        }

        return false;
    }

    /// <summary>
    /// 从当前关卡单位摆放配置中读取所有非负有效格子坐标。
    /// </summary>
    private HashSet<Vector2Int> LoadValidCells()
    {
        HashSet<Vector2Int> validCells = new HashSet<Vector2Int>();
        UnitPlacementConfig placementConfig = AssetDatabase.LoadAssetAtPath<UnitPlacementConfig>(PlacementConfigPath);
        if(placementConfig == null) return validCells;

        foreach(UnitPlacementCell cell in placementConfig.Cells)
        {
            Vector2Int cellPosition = cell.CellPosition;
            if(cellPosition.x < 0 || cellPosition.y < 0) continue;

            validCells.Add(cellPosition);
        }

        return validCells;
    }
}
