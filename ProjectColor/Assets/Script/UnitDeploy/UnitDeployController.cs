using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// 控制部署阶段的仓库选择、单位预览、格子放置和开始战斗。
/// </summary>
public class UnitDeployController : MonoBehaviour
{
    [SerializeField] private GameStageManager stageManager;
    [SerializeField] private UnitDeployConfig deployConfig;
    [SerializeField] private UnitDeployAreaConfig deployAreaConfig;
    [SerializeField] private UnitDeployUI deployUI;
    [SerializeField] private UnitDeployAreaHighlighter deployAreaHighlighter;
    [SerializeField] private Camera targetCamera;

    private readonly Dictionary<Unit, int> inventory = new Dictionary<Unit, int>();
    private Unit selectedPrefab;
    private GameObject previewObject;
    private SpriteRenderer previewRenderer;
    private bool subscribedStageEvent;

    /// <summary>
    /// 初始化依赖引用和部署 UI。
    /// </summary>
    private void Awake()
    {
        ResolveReferences();
        ResolveConfigs();
        if(deployUI != null)
        {
            deployUI.SetCallbacks(HandleUnitClicked, HandleStartBattleClicked, HandleConfirmStartBattleClicked);
            if(stageManager == null || stageManager.CurrentStage != GameStage.Deployment)
            {
                deployUI.SetVisible(false);
            }
        }
    }

    /// <summary>
    /// 启用时订阅整体阶段变化。
    /// </summary>
    private void OnEnable()
    {
        TrySubscribeStageEvent();
        if(stageManager != null)
        {
            HandleStageChanged(stageManager.CurrentStage);
        }
    }

    /// <summary>
    /// 启动时同步当前阶段显示。
    /// </summary>
    private void Start()
    {
        TrySubscribeStageEvent();
        if(stageManager != null)
        {
            HandleStageChanged(stageManager.CurrentStage);
        }
    }

    /// <summary>
    /// 禁用时取消订阅整体阶段变化。
    /// </summary>
    private void OnDisable()
    {
        if(stageManager != null && subscribedStageEvent)
        {
            stageManager.OnStageChanged -= HandleStageChanged;
        }

        subscribedStageEvent = false;
    }

    /// <summary>
    /// 每帧处理部署阶段的鼠标输入和预览位置。
    /// </summary>
    private void Update()
    {
        if(stageManager == null || stageManager.CurrentStage != GameStage.Deployment) return;

        UpdatePreview();

        if(IsRightClickThisFrame())
        {
            CancelSelection();
            return;
        }

        if(!IsLeftClickThisFrame()) return;
        if(IsPointerOverUI()) return;

        if(selectedPrefab == null && TryPickUpPlacedUnit())
        {
            return;
        }

        TryPlaceSelectedUnit();
    }

    /// <summary>
    /// 由阶段管理器设置部署 UI 引用。
    /// </summary>
    public void SetReferences(GameStageManager manager, UnitDeployUI ui)
    {
        stageManager = manager;
        deployUI = ui;
        ResolveReferences();
        ResolveConfigs();
        if(deployUI != null)
        {
            deployUI.SetCallbacks(HandleUnitClicked, HandleStartBattleClicked, HandleConfirmStartBattleClicked);
        }

        TrySubscribeStageEvent();
        if(stageManager != null)
        {
            HandleStageChanged(stageManager.CurrentStage);
        }
    }

    /// <summary>
    /// 整体阶段变化时刷新部署 UI 状态。
    /// </summary>
    private void HandleStageChanged(GameStage stage)
    {
        bool isDeployment = stage == GameStage.Deployment;
        ResolveConfigs();
        if(deployUI != null)
        {
            deployUI.SetVisible(isDeployment);
        }

        if(isDeployment)
        {
            BuildInventory();
            RefreshUI();
            deployAreaHighlighter?.Show(deployAreaConfig);
        }
        else
        {
            deployAreaHighlighter?.Hide();
            CancelSelection();
        }
    }

    /// <summary>
    /// 根据部署配置重建本次部署库存。
    /// </summary>
    private void BuildInventory()
    {
        inventory.Clear();
        if(deployConfig == null) return;

        foreach(UnitDeployEntry entry in deployConfig.GetValidPlayerEntries())
        {
            if(inventory.ContainsKey(entry.UnitPrefab))
            {
                inventory[entry.UnitPrefab] += entry.Count;
            }
            else
            {
                inventory.Add(entry.UnitPrefab, entry.Count);
            }
        }
    }

    /// <summary>
    /// 刷新部署 UI 的库存显示。
    /// </summary>
    private void RefreshUI()
    {
        if(deployUI != null)
        {
            deployUI.RefreshInventory(inventory);
        }
    }

    /// <summary>
    /// 处理仓库单位点击。
    /// </summary>
    private void HandleUnitClicked(Unit prefab)
    {
        if(prefab == null) return;
        if(!inventory.TryGetValue(prefab, out int count) || count <= 0) return;

        selectedPrefab = prefab;
        EnsurePreviewObject();
        ApplyPreviewSprite(prefab);
        UpdatePreview();
    }

    /// <summary>
    /// 处理开始战斗按钮点击。
    /// </summary>
    private void HandleStartBattleClicked()
    {
        CancelSelection();
        if(HasRemainingUnits())
        {
            deployUI?.ShowRemainingUnitsDialog();
            return;
        }

        StartBattle();
    }

    /// <summary>
    /// 处理确认弹窗中的强制开始战斗。
    /// </summary>
    private void HandleConfirmStartBattleClicked()
    {
        CancelSelection();
        StartBattle();
    }

    /// <summary>
    /// 进入战斗阶段。
    /// </summary>
    private void StartBattle()
    {
        if(stageManager != null)
        {
            stageManager.StartBattle();
        }
    }

    /// <summary>
    /// 尝试在当前鼠标指向的格子放置所选单位。
    /// </summary>
    private void TryPlaceSelectedUnit()
    {
        if(selectedPrefab == null || GridManager.Instance == null) return;
        if(!TryGetMouseCell(out GridCell targetCell)) return;
        if(!CanDeployToCell(targetCell)) return;
        if(!inventory.TryGetValue(selectedPrefab, out int count) || count <= 0) return;

        Vector3 spawnPosition = GridManager.Instance.GetWorldInGrid(targetCell);
        Unit unit = Instantiate(selectedPrefab, spawnPosition, Quaternion.identity, GetPlayerUnitParent());
        unit.Facing?.FaceTeamDefault(UnitTeam.Player);
        unit.name = selectedPrefab.name;
        inventory[selectedPrefab] = count - 1;
        UnitGridOccupancy.RegisterUnit(unit, targetCell);
        deployAreaHighlighter?.RefreshCells();
        RefreshUI();

        if(inventory[selectedPrefab] <= 0)
        {
            CancelSelection();
        }
        else
        {
            UpdatePreview();
        }
    }

    /// <summary>
    /// 尝试拾起已经部署到场景中的玩家单位。
    /// </summary>
    private bool TryPickUpPlacedUnit()
    {
        if(!TryGetMouseCell(out GridCell targetCell)) return false;
        if(!UnitGridOccupancy.TryGetUnit(targetCell, out Unit placedUnit)) return false;
        if(placedUnit == null || !placedUnit.CanPlayerControl) return false;
        if(!TryGetInventoryPrefabForUnit(placedUnit, out Unit prefab)) return false;

        UnitGridOccupancy.UnregisterUnit(placedUnit);
        Destroy(placedUnit.gameObject);
        deployAreaHighlighter?.RefreshCells();
        inventory[prefab] = inventory.TryGetValue(prefab, out int count) ? count + 1 : 1;
        selectedPrefab = prefab;
        EnsurePreviewObject();
        ApplyPreviewSprite(selectedPrefab);
        RefreshUI();
        UpdatePreview();
        return true;
    }

    /// <summary>
    /// 根据场景单位匹配部署库存中的 Prefab。
    /// </summary>
    private bool TryGetInventoryPrefabForUnit(Unit placedUnit, out Unit prefab)
    {
        prefab = null;
        if(placedUnit == null) return false;

        string placedUnitName = NormalizeUnitName(placedUnit.name);
        foreach(Unit unitPrefab in inventory.Keys)
        {
            if(unitPrefab == null) continue;
            if(unitPrefab.name != placedUnitName) continue;

            prefab = unitPrefab;
            return true;
        }

        return false;
    }

    /// <summary>
    /// 判断指定格子是否可以部署单位。
    /// </summary>
    private bool CanDeployToCell(GridCell cell)
    {
        if(GridManager.Instance == null) return false;
        if(deployAreaConfig == null || !deployAreaConfig.Contains(cell)) return false;
        if(!GridManager.Instance.IsCellWalkable(cell)) return false;
        if(UnitGridOccupancy.IsCellOccupied(cell)) return false;

        return true;
    }

    /// <summary>
    /// 更新部署预览对象的位置和显示状态。
    /// </summary>
    private void UpdatePreview()
    {
        if(selectedPrefab == null || previewObject == null || GridManager.Instance == null)
        {
            HidePreview();
            return;
        }

        if(!TryGetMouseCell(out GridCell targetCell) || !CanDeployToCell(targetCell))
        {
            HidePreview();
            return;
        }

        previewObject.transform.position = GridManager.Instance.GetWorldInGrid(targetCell);
        previewObject.SetActive(true);
    }

    /// <summary>
    /// 取消当前拾取状态并隐藏预览。
    /// </summary>
    private void CancelSelection()
    {
        selectedPrefab = null;
        HidePreview();
    }

    /// <summary>
    /// 隐藏部署预览对象。
    /// </summary>
    private void HidePreview()
    {
        if(previewObject != null)
        {
            previewObject.SetActive(false);
        }
    }

    /// <summary>
    /// 创建部署预览对象。
    /// </summary>
    private void EnsurePreviewObject()
    {
        if(previewObject != null) return;

        previewObject = new GameObject("UnitDeployPreview");
        previewRenderer = previewObject.AddComponent<SpriteRenderer>();
        previewRenderer.color = new Color(1f, 1f, 1f, 0.5f);
        previewObject.SetActive(false);
    }

    /// <summary>
    /// 根据单位 Prefab 设置预览图像。
    /// </summary>
    private void ApplyPreviewSprite(Unit prefab)
    {
        EnsurePreviewObject();
        if(previewRenderer != null)
        {
            UnitDeployVisualUtility.ApplyToPreview(prefab, previewRenderer, 0.5f);
        }
    }

    /// <summary>
    /// 判断仓库中是否还有未部署单位。
    /// </summary>
    private bool HasRemainingUnits()
    {
        foreach(KeyValuePair<Unit, int> pair in inventory)
        {
            if(pair.Value > 0) return true;
        }

        return false;
    }

    /// <summary>
    /// 去除运行时实例名称中的 Clone 后缀。
    /// </summary>
    private string NormalizeUnitName(string unitName)
    {
        return unitName.Replace("(Clone)", string.Empty).Trim();
    }

    /// <summary>
    /// 获得鼠标当前指向的格子。
    /// </summary>
    private bool TryGetMouseCell(out GridCell cell)
    {
        cell = default;
        if(GridManager.Instance == null || targetCamera == null || Mouse.current == null) return false;

        Vector2 screenPosition = Mouse.current.position.ReadValue();
        Vector3 worldPosition = targetCamera.ScreenToWorldPoint(screenPosition);
        worldPosition.z = 0f;
        cell = GridManager.Instance.GetCellFromWorldPosition(worldPosition);
        return true;
    }

    /// <summary>
    /// 获得或创建玩家单位父物体。
    /// </summary>
    private Transform GetPlayerUnitParent()
    {
        GameObject unitRoot = GameObject.Find("Unit");
        if(unitRoot == null)
        {
            unitRoot = new GameObject("Unit");
        }

        Transform playerParent = unitRoot.transform.Find(UnitTeam.Player.ToString());
        if(playerParent != null) return playerParent;

        GameObject playerObject = new GameObject(UnitTeam.Player.ToString());
        playerObject.transform.SetParent(unitRoot.transform);
        return playerObject.transform;
    }

    /// <summary>
    /// 查找部署控制器所需引用。
    /// </summary>
    private void ResolveReferences()
    {
        if(stageManager == null) stageManager = FindFirstObjectByType<GameStageManager>();
        if(deployUI == null) deployUI = GetComponent<UnitDeployUI>();
        if(deployAreaHighlighter == null) deployAreaHighlighter = GetComponent<UnitDeployAreaHighlighter>();
        if(deployAreaHighlighter == null) deployAreaHighlighter = gameObject.AddComponent<UnitDeployAreaHighlighter>();
        if(targetCamera == null) targetCamera = Camera.main;
    }

    /// <summary>
    /// 按默认路径加载部署配置资产。
    /// </summary>
    private void ResolveConfigs()
    {
#if UNITY_EDITOR
        if(deployConfig == null)
        {
            deployConfig = UnityEditor.AssetDatabase.LoadAssetAtPath<UnitDeployConfig>("Assets/Config/DefaultUnitDeployConfig.asset");
        }

        if(deployAreaConfig == null)
        {
            deployAreaConfig = UnityEditor.AssetDatabase.LoadAssetAtPath<UnitDeployAreaConfig>("Assets/Config/DefaultUnitDeployAreaConfig.asset");
        }
#endif
    }

    /// <summary>
    /// 尝试订阅整体阶段事件。
    /// </summary>
    private void TrySubscribeStageEvent()
    {
        if(subscribedStageEvent) return;

        ResolveReferences();
        if(stageManager == null) return;

        stageManager.OnStageChanged += HandleStageChanged;
        subscribedStageEvent = true;
    }

    /// <summary>
    /// 检查鼠标左键是否在本帧按下。
    /// </summary>
    private bool IsLeftClickThisFrame()
    {
        return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
    }

    /// <summary>
    /// 检查鼠标右键是否在本帧按下。
    /// </summary>
    private bool IsRightClickThisFrame()
    {
        return Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame;
    }

    /// <summary>
    /// 检查鼠标是否正在指向 UI。
    /// </summary>
    private bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
}
