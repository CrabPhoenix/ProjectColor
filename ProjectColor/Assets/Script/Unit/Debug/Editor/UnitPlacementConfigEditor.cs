using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// UnitPlacementConfig 的编辑器工具，用于配置格子单位并按配置生成单位。
/// </summary>
[CustomEditor(typeof(UnitPlacementConfig))]
public class UnitPlacementConfigEditor : Editor
{
    private static readonly Dictionary<string, bool> cellFoldoutStates = new Dictionary<string, bool>();
    private static readonly Dictionary<string, bool> cellHadUnitStates = new Dictionary<string, bool>();

    private SerializedProperty cellsProperty;
    private bool cellsFoldout;

    /// <summary>
    /// 缓存配置中的格子列表属性。
    /// </summary>
    private void OnEnable()
    {
        cellsProperty = serializedObject.FindProperty("cells");
        cellsFoldout = false;
    }

    /// <summary>
    /// 绘制单位摆放配置的 Inspector 和工具按钮。
    /// </summary>
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        UnitPlacementConfig config = (UnitPlacementConfig)target;
        if(config.HasDuplicateCells())
        {
            EditorGUILayout.HelpBox("配置中存在重复格子，请点击 Refresh Valid Cells From Scene Grid 重新整理。", MessageType.Warning);
        }

        DrawCells();

        EditorGUILayout.Space();
        if(GUILayout.Button("Refresh Valid Cells From Scene Grid"))
        {
            serializedObject.ApplyModifiedProperties();
            RefreshValidCells(config);
            serializedObject.Update();
        }

        if(GUILayout.Button("Spawn Units In Current Scene"))
        {
            serializedObject.ApplyModifiedProperties();
            SpawnUnits(config);
            serializedObject.Update();
        }

        serializedObject.ApplyModifiedProperties();
    }

    /// <summary>
    /// 绘制可折叠的所有有效格子。
    /// </summary>
    private void DrawCells()
    {
        cellsFoldout = EditorGUILayout.Foldout(cellsFoldout, $"Cells ({cellsProperty.arraySize})", true);
        if(!cellsFoldout) return;

        EditorGUI.indentLevel++;
        for(int i = 0; i < cellsProperty.arraySize; i++)
        {
            DrawCell(cellsProperty.GetArrayElementAtIndex(i));
        }
        EditorGUI.indentLevel--;
    }

    /// <summary>
    /// 绘制单个格子的折叠项。
    /// </summary>
    private void DrawCell(SerializedProperty cellProperty)
    {
        SerializedProperty cellPositionProperty = cellProperty.FindPropertyRelative("cellPosition");
        SerializedProperty unitPrefabProperty = cellProperty.FindPropertyRelative("unitPrefab");
        SerializedProperty unitTeamProperty = cellProperty.FindPropertyRelative("unitTeam");

        Vector2Int cellPosition = cellPositionProperty.vector2IntValue;
        Unit unitPrefab = unitPrefabProperty.objectReferenceValue as Unit;
        UnitTeam unitTeam = GetTeamFromPrefab(unitPrefab);
        unitTeamProperty.enumValueIndex = GetEnumIndex(unitTeamProperty, unitTeam);

        string key = $"{target.GetInstanceID()}_{cellPosition.x}_{cellPosition.y}";
        bool hasUnit = unitPrefab != null;
        if(!cellFoldoutStates.ContainsKey(key))
        {
            cellFoldoutStates[key] = hasUnit;
        }

        bool hadUnitBeforeDraw = cellHadUnitStates.TryGetValue(key, out bool hadUnit) && hadUnit;
        if(hadUnitBeforeDraw && !hasUnit)
        {
            cellFoldoutStates[key] = false;
            cellHadUnitStates[key] = false;
        }

        if(hasUnit)
        {
            cellFoldoutStates[key] = true;
            cellHadUnitStates[key] = true;
        }

        string title = $"Grid({cellPosition.x}, {cellPosition.y})";
        cellFoldoutStates[key] = EditorGUILayout.Foldout(cellFoldoutStates[key], title, true);
        if(!cellFoldoutStates[key]) return;

        EditorGUI.indentLevel++;
        DrawUnitPrefabSelector(unitPrefabProperty, unitPrefab, hadUnitBeforeDraw, key);

        unitPrefab = unitPrefabProperty.objectReferenceValue as Unit;
        if(unitPrefab == null)
        {
            cellHadUnitStates[key] = false;
            if(hadUnitBeforeDraw)
            {
                cellFoldoutStates[key] = false;
            }
        }
        else
        {
            cellHadUnitStates[key] = true;
        }

        unitTeam = GetTeamFromPrefab(unitPrefab);
        unitTeamProperty.enumValueIndex = GetEnumIndex(unitTeamProperty, unitTeam);

        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.EnumPopup("Unit Team", unitTeam);
        EditorGUI.EndDisabledGroup();
        EditorGUI.indentLevel--;
    }

    /// <summary>
    /// 绘制只允许选择 Unit 派生 Prefab 的选择控件。
    /// </summary>
    private void DrawUnitPrefabSelector(SerializedProperty unitPrefabProperty, Unit unitPrefab, bool hadUnitBeforeDraw, string key)
    {
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.ObjectField("Unit Prefab", unitPrefab != null ? unitPrefab.gameObject : null, typeof(GameObject), false);
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.BeginHorizontal();
        if(GUILayout.Button("Select Unit Prefab"))
        {
            UnitPrefabPickerWindow.Open(selectedUnit =>
            {
                Undo.RecordObject(target, "Select Unit Prefab");
                serializedObject.Update();
                unitPrefabProperty.objectReferenceValue = selectedUnit;
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
                cellHadUnitStates[key] = selectedUnit != null;
                if(selectedUnit != null)
                {
                    cellFoldoutStates[key] = true;
                }
                Repaint();
            });
        }

        if(GUILayout.Button("Clear"))
        {
            Undo.RecordObject(target, "Clear Unit Prefab");
            unitPrefabProperty.objectReferenceValue = null;
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
            cellHadUnitStates[key] = false;
            if(hadUnitBeforeDraw)
            {
                cellFoldoutStates[key] = false;
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// 从当前场景的 GridManager 刷新所有有效格子，并保留场上已有单位。
    /// </summary>
    private void RefreshValidCells(UnitPlacementConfig config)
    {
        GridManager gridManager = FindFirstObjectByType<GridManager>();
        if(gridManager == null || !gridManager.IsGridReady())
        {
            EditorUtility.DisplayDialog("Unit Placement", "当前场景没有可用的 GridManager。", "OK");
            return;
        }

        Dictionary<Vector2Int, UnitPlacementCell> unitCells = CollectSceneUnitCells(gridManager);
        Dictionary<Vector2Int, UnitPlacementCell> existingCells = CollectExistingCells(config);
        List<UnitPlacementCell> newCells = new List<UnitPlacementCell>();

        GridObject[,] gridObjects = gridManager.GetGridObject();
        foreach(GridObject gridObject in gridObjects)
        {
            if(gridObject == null) continue;
            GridCell gridCell = gridObject.GetCellPosion();
            if(!gridManager.IsCellWalkable(gridCell)) continue;

            Vector2Int cellPosition = new Vector2Int(gridCell.X, gridCell.Y);

            if(unitCells.TryGetValue(cellPosition, out UnitPlacementCell sceneUnitCell))
            {
                sceneUnitCell.SyncUnitTeamFromPrefab();
                newCells.Add(sceneUnitCell);
                continue;
            }

            if(existingCells.TryGetValue(cellPosition, out UnitPlacementCell existingCell))
            {
                existingCell.SyncUnitTeamFromPrefab();
                newCells.Add(existingCell);
                continue;
            }

            newCells.Add(new UnitPlacementCell(cellPosition));
        }

        Undo.RecordObject(config, "Refresh Unit Placement Config");
        config.SetCells(newCells);
        EditorUtility.SetDirty(config);
        AssetDatabase.SaveAssets();
    }

    /// <summary>
    /// 在当前场景中按配置生成单位，并删除配置管理格子上的旧单位。
    /// </summary>
    private void SpawnUnits(UnitPlacementConfig config)
    {
        GridManager gridManager = FindFirstObjectByType<GridManager>();
        if(gridManager == null || !gridManager.IsGridReady())
        {
            EditorUtility.DisplayDialog("Unit Placement", "当前场景没有可用的 GridManager。", "OK");
            return;
        }

        ClearUnitsInConfigCells(config, gridManager);

        foreach(UnitPlacementCell cell in config.Cells)
        {
            cell.SyncUnitTeamFromPrefab();
            if(!cell.HasUnit || cell.UnitPrefab == null) continue;

            GridCell gridCell = new GridCell(cell.CellPosition.x, cell.CellPosition.y);
            if(!gridManager.IsCellWalkable(gridCell)) continue;

            Vector3 position = gridManager.GetWorldInGrid(gridCell);
            Unit unit = (Unit)PrefabUtility.InstantiatePrefab(cell.UnitPrefab);
            Undo.RegisterCreatedObjectUndo(unit.gameObject, "Spawn Unit From Placement Config");
            unit.transform.position = position;
            unit.transform.SetParent(GetOrCreateTeamParent(cell.UnitTeam));
            unit.name = cell.UnitPrefab.name;
            if(unit.GetComponent<UnitPlacementSpawned>() == null)
            {
                unit.gameObject.AddComponent<UnitPlacementSpawned>();
            }
        }

        EditorUtility.SetDirty(config);
        EditorSceneManager.MarkSceneDirty(gridManager.gameObject.scene);
    }

    /// <summary>
    /// 删除配置包含的格子上已有的单位，使配置成为场景单位摆放的来源。
    /// </summary>
    private void ClearUnitsInConfigCells(UnitPlacementConfig config, GridManager gridManager)
    {
        HashSet<Vector2Int> configCells = new HashSet<Vector2Int>();
        foreach(UnitPlacementCell cell in config.Cells)
        {
            configCells.Add(cell.CellPosition);
        }

        Unit[] sceneUnits = FindObjectsByType<Unit>(FindObjectsSortMode.None);
        foreach(Unit unit in sceneUnits)
        {
            if(unit == null) continue;

            GridCell gridCell = gridManager.GetCellFromWorldPosition(unit.transform.position);
            Vector2Int cellPosition = new Vector2Int(gridCell.X, gridCell.Y);
            if(configCells.Contains(cellPosition))
            {
                Undo.DestroyObjectImmediate(unit.gameObject);
            }
        }
    }

    /// <summary>
    /// 收集当前配置中已有的格子信息。
    /// </summary>
    private Dictionary<Vector2Int, UnitPlacementCell> CollectExistingCells(UnitPlacementConfig config)
    {
        Dictionary<Vector2Int, UnitPlacementCell> cells = new Dictionary<Vector2Int, UnitPlacementCell>();
        foreach(UnitPlacementCell cell in config.Cells)
        {
            if(!cells.ContainsKey(cell.CellPosition))
            {
                cells.Add(cell.CellPosition, cell);
            }
        }

        return cells;
    }

    /// <summary>
    /// 收集场景中单位所在的格子和对应 Prefab。
    /// </summary>
    private Dictionary<Vector2Int, UnitPlacementCell> CollectSceneUnitCells(GridManager gridManager)
    {
        Dictionary<Vector2Int, UnitPlacementCell> unitCells = new Dictionary<Vector2Int, UnitPlacementCell>();
        Unit[] sceneUnits = FindObjectsByType<Unit>(FindObjectsSortMode.None);

        foreach(Unit unit in sceneUnits)
        {
            if(unit == null || !unit.IsAlive) continue;

            GridCell gridCell = gridManager.GetCellFromWorldPosition(unit.transform.position);
            if(!gridManager.IsCellWalkable(gridCell)) continue;

            Vector2Int cellPosition = new Vector2Int(gridCell.X, gridCell.Y);
            if(unitCells.ContainsKey(cellPosition)) continue;

            Unit prefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(unit);
            UnitPlacementCell placementCell = new UnitPlacementCell(cellPosition);
            placementCell.SetUnit(prefab);
            unitCells.Add(cellPosition, placementCell);
        }

        return unitCells;
    }

    /// <summary>
    /// 根据单位 Prefab 读取阵营，没有单位时返回 None。
    /// </summary>
    private UnitTeam GetTeamFromPrefab(Unit unitPrefab)
    {
        if(unitPrefab == null) return UnitTeam.None;
        return unitPrefab.Team;
    }

    /// <summary>
    /// 获得枚举值在序列化枚举中的索引。
    /// </summary>
    private int GetEnumIndex(SerializedProperty enumProperty, UnitTeam unitTeam)
    {
        string valueName = unitTeam.ToString();
        for(int i = 0; i < enumProperty.enumNames.Length; i++)
        {
            if(enumProperty.enumNames[i] == valueName) return i;
        }

        return 0;
    }

    /// <summary>
    /// 获得或创建 Unit/阵营 父物体。
    /// </summary>
    private Transform GetOrCreateTeamParent(UnitTeam team)
    {
        GameObject unitRoot = GameObject.Find("Unit");
        if(unitRoot == null)
        {
            unitRoot = new GameObject("Unit");
            Undo.RegisterCreatedObjectUndo(unitRoot, "Create Unit Root");
        }

        string teamName = team.ToString();
        Transform teamParent = unitRoot.transform.Find(teamName);
        if(teamParent != null) return teamParent;

        GameObject teamObject = new GameObject(teamName);
        Undo.RegisterCreatedObjectUndo(teamObject, "Create Unit Team Parent");
        teamObject.transform.SetParent(unitRoot.transform);
        return teamObject.transform;
    }
}

/// <summary>
/// 只显示拥有 Unit 派生组件的 Prefab 的选择窗口。
/// </summary>
public class UnitPrefabPickerWindow : EditorWindow
{
    private readonly List<Unit> unitPrefabs = new List<Unit>();
    private Action<Unit> onSelected;
    private Vector2 scrollPosition;
    private string searchText = string.Empty;

    /// <summary>
    /// 打开单位 Prefab 选择窗口。
    /// </summary>
    public static void Open(Action<Unit> onSelected)
    {
        UnitPrefabPickerWindow window = CreateInstance<UnitPrefabPickerWindow>();
        window.titleContent = new GUIContent("Unit Prefabs");
        window.minSize = new Vector2(320f, 420f);
        window.onSelected = onSelected;
        window.RefreshUnitPrefabs();
        window.ShowUtility();
    }

    /// <summary>
    /// 绘制可选单位 Prefab 列表。
    /// </summary>
    private void OnGUI()
    {
        EditorGUILayout.LabelField("Select Unit Prefab", EditorStyles.boldLabel);
        searchText = EditorGUILayout.TextField("Search", searchText);

        EditorGUILayout.Space();
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        foreach(Unit unitPrefab in unitPrefabs)
        {
            if(unitPrefab == null) continue;
            if(!string.IsNullOrEmpty(searchText) && !unitPrefab.name.ToLowerInvariant().Contains(searchText.ToLowerInvariant())) continue;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.ObjectField(unitPrefab.gameObject, typeof(GameObject), false);
            EditorGUILayout.LabelField(unitPrefab.Team.ToString(), GUILayout.Width(70f));
            if(GUILayout.Button("Select", GUILayout.Width(70f)))
            {
                onSelected?.Invoke(unitPrefab);
                Close();
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();
        if(GUILayout.Button("Cancel"))
        {
            Close();
        }
    }

    /// <summary>
    /// 从项目中收集所有带 Unit 派生组件的 Prefab。
    /// </summary>
    private void RefreshUnitPrefabs()
    {
        unitPrefabs.Clear();
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        foreach(string prefabGuid in prefabGuids)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if(prefab == null) continue;

            Unit unitPrefab = prefab.GetComponent<Unit>();
            if(unitPrefab == null) continue;

            unitPrefabs.Add(unitPrefab);
        }

        unitPrefabs.Sort((left, right) => string.Compare(left.name, right.name, StringComparison.OrdinalIgnoreCase));
    }
}
