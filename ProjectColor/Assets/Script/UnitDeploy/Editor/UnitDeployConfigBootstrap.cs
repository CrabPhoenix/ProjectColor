using UnityEditor;
using UnityEngine;

/// <summary>
/// 在编辑器中创建默认部署单位配置和部署区域配置。
/// </summary>
[InitializeOnLoad]
public static class UnitDeployConfigBootstrap
{
    private const string DeployConfigPath = "Assets/Config/DefaultUnitDeployConfig.asset";
    private const string DeployAreaConfigPath = "Assets/Config/DefaultUnitDeployAreaConfig.asset";
    private const string PlacementConfigPath = "Assets/Config/DefaultUnitPlacementConfig.asset";

    /// <summary>
    /// 注册延迟创建，等待 Unity 完成脚本导入。
    /// </summary>
    static UnitDeployConfigBootstrap()
    {
        EditorApplication.delayCall += EnsureDefaultConfigs;
    }

    /// <summary>
    /// 确保默认部署配置资产存在。
    /// </summary>
    private static void EnsureDefaultConfigs()
    {
        EnsureDeployConfig();
        EnsureDeployAreaConfig();
        AssetDatabase.SaveAssets();
    }

    /// <summary>
    /// 创建默认部署仓库配置。
    /// </summary>
    private static void EnsureDeployConfig()
    {
        UnitDeployConfig deployConfig = AssetDatabase.LoadAssetAtPath<UnitDeployConfig>(DeployConfigPath);
        if(deployConfig != null) return;

        deployConfig = ScriptableObject.CreateInstance<UnitDeployConfig>();
        AssetDatabase.CreateAsset(deployConfig, DeployConfigPath);

        SerializedObject serializedObject = new SerializedObject(deployConfig);
        SerializedProperty unitsProperty = serializedObject.FindProperty("units");
        unitsProperty.ClearArray();

        UnitPlacementConfig placementConfig = AssetDatabase.LoadAssetAtPath<UnitPlacementConfig>(PlacementConfigPath);
        if(placementConfig != null)
        {
            foreach(UnitPlacementCell cell in placementConfig.Cells)
            {
                if(!cell.HasUnit || cell.UnitPrefab == null) continue;
                if(cell.UnitTeam != UnitTeam.Player) continue;

                AddOrIncreaseDeployEntry(unitsProperty, cell.UnitPrefab);
            }
        }

        if(unitsProperty.arraySize == 0)
        {
            Unit playerUnit = AssetDatabase.LoadAssetAtPath<Unit>("Assets/Prefab/PlayerUnit.prefab");
            if(playerUnit != null)
            {
                AddOrIncreaseDeployEntry(unitsProperty, playerUnit);
            }
        }

        serializedObject.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(deployConfig);
    }

    /// <summary>
    /// 创建默认部署区域配置。
    /// </summary>
    private static void EnsureDeployAreaConfig()
    {
        UnitDeployAreaConfig areaConfig = AssetDatabase.LoadAssetAtPath<UnitDeployAreaConfig>(DeployAreaConfigPath);
        if(areaConfig != null) return;

        areaConfig = ScriptableObject.CreateInstance<UnitDeployAreaConfig>();
        AssetDatabase.CreateAsset(areaConfig, DeployAreaConfigPath);

        SerializedObject serializedObject = new SerializedObject(areaConfig);
        SerializedProperty cellsProperty = serializedObject.FindProperty("deployableCells");
        cellsProperty.ClearArray();

        UnitPlacementConfig placementConfig = AssetDatabase.LoadAssetAtPath<UnitPlacementConfig>(PlacementConfigPath);
        if(placementConfig != null)
        {
            foreach(UnitPlacementCell cell in placementConfig.Cells)
            {
                int index = cellsProperty.arraySize;
                cellsProperty.InsertArrayElementAtIndex(index);
                SerializedProperty cellProperty = cellsProperty.GetArrayElementAtIndex(index);
                cellProperty.vector2IntValue = cell.CellPosition;
            }
        }

        serializedObject.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(areaConfig);
    }

    /// <summary>
    /// 增加或累加同类单位的可部署数量。
    /// </summary>
    private static void AddOrIncreaseDeployEntry(SerializedProperty unitsProperty, Unit unitPrefab)
    {
        for(int i = 0; i < unitsProperty.arraySize; i++)
        {
            SerializedProperty entryProperty = unitsProperty.GetArrayElementAtIndex(i);
            SerializedProperty prefabProperty = entryProperty.FindPropertyRelative("unitPrefab");
            if(prefabProperty.objectReferenceValue != unitPrefab) continue;

            SerializedProperty countProperty = entryProperty.FindPropertyRelative("count");
            countProperty.intValue += 1;
            return;
        }

        int index = unitsProperty.arraySize;
        unitsProperty.InsertArrayElementAtIndex(index);
        SerializedProperty newEntryProperty = unitsProperty.GetArrayElementAtIndex(index);
        newEntryProperty.FindPropertyRelative("unitPrefab").objectReferenceValue = unitPrefab;
        newEntryProperty.FindPropertyRelative("count").intValue = 1;
    }
}
