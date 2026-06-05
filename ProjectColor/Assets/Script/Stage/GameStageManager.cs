using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 管理标题、游戏中和结算三个整体游戏阶段。
/// </summary>
[ExecuteAlways]
public class GameStageManager : MonoBehaviour
{
    private static GameStageManager instance;
    private static bool hasPendingStage;
    private static GameStage pendingStage;

    [SerializeField] private TurnManager turnManager;
    [SerializeField] private TurnPhaseCameraController cameraController;
    [SerializeField] private Canvas gameCanvas;
    [SerializeField] private TitleMenuUI titleMenuUI;
    [SerializeField] private GameRuleMenuUI gameRuleMenuUI;
    [SerializeField] private SettlementMenuUI settlementMenuUI;
    [SerializeField] private UnitDeployUI unitDeployUI;
    [SerializeField] private UnitDeployController unitDeployController;

    private GameStage currentStage = GameStage.Title;
    private bool subscribedTurnEvent;
#if UNITY_EDITOR
    private bool editorEnsureQueued;
#endif

    public static GameStageManager Instance => instance;
    public GameStage CurrentStage => currentStage;
    public bool IsGameplay => currentStage == GameStage.Gameplay;
    public event Action<GameStage> OnStageChanged;

    /// <summary>
    /// 初始化阶段管理器和场景 UI。
    /// </summary>
    private void Awake()
    {
        if(!Application.isPlaying)
        {
            ScheduleEditorEnsureSceneUI();
            return;
        }

        if(Application.isPlaying)
        {
            if(instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
        }

        ResolveTurnManager();
        ResolveCameraController();
        EnsureSingleEventSystem(Application.isPlaying);
        EnsureSceneUI();
        ResolveInitialStage();
        ApplyStage(currentStage, null);
        TrySubscribeTurnEvent();
    }

    /// <summary>
    /// 启动时根据初始阶段决定是否延后一帧开始玩家阶段。
    /// </summary>
    private void Start()
    {
        if(!Application.isPlaying) return;

        TrySubscribeTurnEvent();
        if(currentStage == GameStage.Gameplay)
        {
            StartCoroutine(StartGameplayNextFrame());
        }
    }

    /// <summary>
    /// 启用时订阅胜负事件。
    /// </summary>
    private void OnEnable()
    {
        if(!Application.isPlaying) return;

        TrySubscribeTurnEvent();
    }

    /// <summary>
    /// 禁用时取消胜负事件订阅。
    /// </summary>
    private void OnDisable()
    {
        if(turnManager != null && subscribedTurnEvent)
        {
            turnManager.OnGameResult -= HandleGameResult;
        }

        subscribedTurnEvent = false;
    }

    /// <summary>
    /// 在编辑器中保持默认 UI 结构存在。
    /// </summary>
    private void OnValidate()
    {
        if(Application.isPlaying) return;

        ScheduleEditorEnsureSceneUI();
    }

#if UNITY_EDITOR
    /// <summary>
    /// 在编辑器中延迟创建或刷新阶段 UI，避免在 OnValidate 中直接修改场景。
    /// </summary>
    private void ScheduleEditorEnsureSceneUI()
    {
        if(editorEnsureQueued) return;

        editorEnsureQueued = true;
        UnityEditor.EditorApplication.delayCall += HandleEditorEnsureSceneUIDelayed;
    }

    /// <summary>
    /// 延迟执行编辑器阶段 UI 创建和 EventSystem 清理。
    /// </summary>
    private void HandleEditorEnsureSceneUIDelayed()
    {
        editorEnsureQueued = false;
        if(this == null || Application.isPlaying) return;

        EnsureSingleEventSystem(false);
        EnsureSceneUI();
        if(gameObject.scene.IsValid())
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }
    }
#else
    /// <summary>
    /// 非编辑器环境不需要延迟创建编辑器 UI。
    /// </summary>
    private void ScheduleEditorEnsureSceneUI()
    {
    }
#endif

    /// <summary>
    /// 点击开始游戏时进入游戏阶段。
    /// </summary>
    public void StartGame()
    {
        if(!Application.isPlaying) return;

        SetStage(GameStage.Deployment, null);
    }

    /// <summary>
    /// 点击重启游戏时重载当前关卡并直接进入游戏阶段。
    /// </summary>
    public void RestartGame()
    {
        if(!Application.isPlaying) return;

        LoadCurrentSceneAs(GameStage.Deployment);
    }

    /// <summary>
    /// 部署完成后进入现有战斗阶段。
    /// </summary>
    public void StartBattle()
    {
        if(!Application.isPlaying) return;

        SetStage(GameStage.Gameplay, null);
        StartCoroutine(StartGameplayNextFrame());
    }

    /// <summary>
    /// 点击回到标题界面时重载当前关卡并进入标题阶段。
    /// </summary>
    public void BackToTitle()
    {
        if(!Application.isPlaying) return;

        LoadCurrentSceneAs(GameStage.Title);
    }

    /// <summary>
    /// 点击退出游戏时输出日志并保留真正退出游戏的代码。
    /// </summary>
    public void ExitGame()
    {
        Debug.Log("已退出游戏");
        // Application.Quit();
    }

    /// <summary>
    /// 在标题阶段显示游戏规则界面。
    /// </summary>
    public void ShowGameRules()
    {
        if(currentStage != GameStage.Title) return;

        if(titleMenuUI != null)
        {
            titleMenuUI.SetVisible(false);
        }

        if(gameRuleMenuUI != null)
        {
            gameRuleMenuUI.SetVisible(true);
        }
    }

    /// <summary>
    /// 隐藏游戏规则界面并回到标题界面。
    /// </summary>
    public void HideGameRules()
    {
        if(currentStage != GameStage.Title) return;

        if(gameRuleMenuUI != null)
        {
            gameRuleMenuUI.SetVisible(false);
        }

        if(titleMenuUI != null)
        {
            titleMenuUI.SetVisible(true);
        }
    }

    /// <summary>
    /// 判断当前是否处于游戏阶段。
    /// </summary>
    public static bool IsGameplayActive()
    {
        return instance != null && instance.currentStage == GameStage.Gameplay;
    }

    /// <summary>
    /// 判断当前是否允许玩家手动移动相机。
    /// </summary>
    public static bool IsCameraManualControlStage()
    {
        return instance != null && (instance.currentStage == GameStage.Gameplay || instance.currentStage == GameStage.Deployment);
    }

    /// <summary>
    /// 胜负出现时切换到结算阶段。
    /// </summary>
    private void HandleGameResult(GameResult result)
    {
        SetStage(GameStage.Settlement, result);
    }

    /// <summary>
    /// 设置当前整体阶段并刷新 UI。
    /// </summary>
    private void SetStage(GameStage stage, GameResult? result)
    {
        if(currentStage == stage && stage != GameStage.Settlement) return;

        GameStage previousStage = currentStage;
        currentStage = stage;
        if(previousStage == GameStage.Deployment && stage != GameStage.Deployment)
        {
            UnitDeployAreaHighlighter.HideAll();
        }

        if(stage == GameStage.Deployment)
        {
            EnsureSceneUI();
        }

        ApplyStage(stage, result);
        OnStageChanged?.Invoke(currentStage);
    }

    /// <summary>
    /// 按阶段显示或隐藏标题和结算 UI。
    /// </summary>
    private void ApplyStage(GameStage stage, GameResult? result)
    {
        ApplyCameraControl(stage);

        if(titleMenuUI != null)
        {
            titleMenuUI.SetVisible(stage == GameStage.Title);
        }

        if(gameRuleMenuUI != null)
        {
            gameRuleMenuUI.SetVisible(false);
        }

        if(settlementMenuUI != null)
        {
            settlementMenuUI.SetVisible(stage == GameStage.Settlement);
            if(stage == GameStage.Settlement && result.HasValue)
            {
                settlementMenuUI.SetResult(result.Value);
            }
        }

        if(unitDeployUI != null)
        {
            if(stage == GameStage.Deployment)
            {
                unitDeployUI.transform.SetAsLastSibling();
            }

            unitDeployUI.SetVisible(stage == GameStage.Deployment);
        }
    }

    /// <summary>
    /// 根据整体阶段控制玩家是否可以移动相机。
    /// </summary>
    private void ApplyCameraControl(GameStage stage)
    {
        ResolveCameraController();
        if(cameraController == null) return;

        if(stage == GameStage.Gameplay || stage == GameStage.Deployment)
        {
            cameraController.StopFollowing();
            cameraController.SetManualControlEnabled(true);
            return;
        }

        cameraController.StopFollowing();
        cameraController.SetManualControlEnabled(false);
    }

    /// <summary>
    /// 延后一帧开始现有玩家阶段，等待单位生成脚本完成 Start。
    /// </summary>
    private IEnumerator StartGameplayNextFrame()
    {
        yield return null;

        ResolveTurnManager();
        ResolveCameraController();
        if(turnManager != null && currentStage == GameStage.Gameplay)
        {
            turnManager.BeginGame();
        }
    }

    /// <summary>
    /// 设置下次加载场景后的阶段并重载当前场景。
    /// </summary>
    private void LoadCurrentSceneAs(GameStage stage)
    {
        hasPendingStage = true;
        pendingStage = stage;
        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.name);
    }

    /// <summary>
    /// 根据静态标记决定本次加载后的初始阶段。
    /// </summary>
    private void ResolveInitialStage()
    {
        if(hasPendingStage)
        {
            currentStage = pendingStage;
            hasPendingStage = false;
            return;
        }

        currentStage = GameStage.Title;
    }

    /// <summary>
    /// 查找当前场景中的回合管理器。
    /// </summary>
    private void ResolveTurnManager()
    {
        if(turnManager != null) return;

        turnManager = GetComponent<TurnManager>();
        if(turnManager == null)
        {
            turnManager = FindFirstObjectByType<TurnManager>();
        }
    }

    /// <summary>
    /// 查找当前场景中的回合相机控制器。
    /// </summary>
    private void ResolveCameraController()
    {
        if(cameraController != null) return;

        cameraController = FindFirstObjectByType<TurnPhaseCameraController>();
        if(cameraController != null) return;

        Camera mainCamera = Camera.main;
        if(mainCamera == null) return;

        cameraController = mainCamera.GetComponent<TurnPhaseCameraController>();
        if(cameraController == null)
        {
            cameraController = mainCamera.gameObject.AddComponent<TurnPhaseCameraController>();
        }
    }

    /// <summary>
    /// 订阅回合管理器的胜负事件。
    /// </summary>
    private void TrySubscribeTurnEvent()
    {
        if(subscribedTurnEvent) return;

        ResolveTurnManager();
        if(turnManager == null) return;

        turnManager.OnGameResult += HandleGameResult;
        subscribedTurnEvent = true;
    }

    /// <summary>
    /// 确保场景中存在可被 UI 点击的事件系统。
    /// </summary>
    private void EnsureSingleEventSystem(bool createIfMissing)
    {
        EventSystem[] eventSystems = FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
        EventSystem eventSystem = eventSystems.Length > 0 ? eventSystems[0] : null;

        if(eventSystem == null)
        {
            if(!createIfMissing) return;

            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystem = eventSystemObject.AddComponent<EventSystem>();
        }
        else
        {
            eventSystem.gameObject.name = "EventSystem";
        }

        for(int i = 1; i < eventSystems.Length; i++)
        {
            if(eventSystems[i] == null) continue;

            if(Application.isPlaying)
            {
                Destroy(eventSystems[i].gameObject);
            }
            else
            {
                DestroyImmediate(eventSystems[i].gameObject);
            }
        }

        StandaloneInputModule standaloneInputModule = eventSystem.GetComponent<StandaloneInputModule>();
        if(standaloneInputModule != null)
        {
            if(Application.isPlaying)
            {
                Destroy(standaloneInputModule);
            }
            else
            {
                DestroyImmediate(standaloneInputModule);
            }
        }

        if(eventSystem.GetComponent<InputSystemUIInputModule>() == null)
        {
            eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
        }
    }

    /// <summary>
    /// 确保标题和结算 UI 以可编辑对象存在于场景中。
    /// </summary>
    private void EnsureSceneUI()
    {
        if(gameCanvas == null)
        {
            GameObject canvasObject = GameObject.Find("GameUIRoot");
            if(canvasObject == null)
            {
                canvasObject = new GameObject("GameUIRoot");
            }

            gameCanvas = canvasObject.GetComponent<Canvas>();
            if(gameCanvas == null) gameCanvas = canvasObject.AddComponent<Canvas>();
            gameCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            gameCanvas.sortingOrder = 1500;

            if(canvasObject.GetComponent<CanvasScaler>() == null) canvasObject.AddComponent<CanvasScaler>();
            if(canvasObject.GetComponent<GraphicRaycaster>() == null) canvasObject.AddComponent<GraphicRaycaster>();
        }

        titleMenuUI = CreateTitleMenu();

        gameRuleMenuUI = CreateGameRuleMenu();

        settlementMenuUI = CreateSettlementMenu();

        unitDeployUI = CreateDeployMenu();
        if(unitDeployUI == null) return;

        unitDeployUI.enabled = true;
        unitDeployController = unitDeployUI.GetComponent<UnitDeployController>();
        if(unitDeployController == null)
        {
            unitDeployController = unitDeployUI.gameObject.AddComponent<UnitDeployController>();
        }

        unitDeployController.enabled = true;
        unitDeployController.SetReferences(this, unitDeployUI);
    }

    /// <summary>
    /// 创建标题界面。
    /// </summary>
    private TitleMenuUI CreateTitleMenu()
    {
        Transform panel = FindOrCreatePanel("TitlePanel");
        if(!IsUnityObjectAlive(panel)) return null;

        Button startButton = CreateButton(panel, "StartGameButton", "开始游戏", new Vector2(0f, 120f));
        Button ruleButton = CreateButton(panel, "GameRuleButton", "游戏规则", new Vector2(0f, 0f));
        Button exitButton = CreateButton(panel, "ExitGameButton", "退出游戏", new Vector2(0f, -120f));

        TitleMenuUI ui = panel.GetComponent<TitleMenuUI>();
        if(ui == null) ui = panel.gameObject.AddComponent<TitleMenuUI>();
        ui.SetReferences(this, startButton, ruleButton, exitButton);
        return ui;
    }

    /// <summary>
    /// 创建游戏规则界面。
    /// </summary>
    private GameRuleMenuUI CreateGameRuleMenu()
    {
        Transform panel = FindOrCreatePanel("GameRulePanel");
        if(!IsUnityObjectAlive(panel)) return null;

        RectTransform background = FindOrCreateChildRect(panel, "RuleBackground", out bool createdBackground);
        if(!IsUnityObjectAlive(background)) return null;

        ApplyDefaultRect(background, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(600f, 400f), createdBackground || background.sizeDelta == new Vector2(960f, 620f));

        Image backgroundImage = background.GetComponent<Image>();
        if(backgroundImage == null) backgroundImage = background.gameObject.AddComponent<Image>();
        backgroundImage.color = new Color(0.22f, 0.22f, 0.22f, 0.96f);

        CreateText(background, "TitleText", "游戏规则", 52, Color.white, new Vector2(0f, 250f), new Vector2(860f, 70f));
        Text ruleText = CreateText(background, "RuleText", "在这里填写游戏规则。", 30, Color.white, new Vector2(0f, 35f), new Vector2(820f, 350f));
        Button understandButton = CreateDialogButton(background, "UnderstandButton", "了解", new Vector2(0f, -245f));

        GameRuleMenuUI ui = panel.GetComponent<GameRuleMenuUI>();
        if(ui == null) ui = panel.gameObject.AddComponent<GameRuleMenuUI>();
        ui.SetReferences(this, ruleText, understandButton);
        panel.gameObject.SetActive(false);
        return ui;
    }

    /// <summary>
    /// 创建结算界面。
    /// </summary>
    private SettlementMenuUI CreateSettlementMenu()
    {
        Transform panel = FindOrCreatePanel("SettlementPanel");
        if(!IsUnityObjectAlive(panel)) return null;

        Text resultText = CreateText(panel, "ResultText", "Victory", 80, Color.white, new Vector2(0f, 160f), new Vector2(560f, 100f));
        Button restartButton = CreateButton(panel, "RestartGameButton", "重启游戏", new Vector2(0f, 20f));
        Button titleButton = CreateButton(panel, "BackToTitleButton", "回到标题界面", new Vector2(0f, -120f));

        SettlementMenuUI ui = panel.GetComponent<SettlementMenuUI>();
        if(ui == null) ui = panel.gameObject.AddComponent<SettlementMenuUI>();
        ui.SetReferences(this, resultText, restartButton, titleButton);
        return ui;
    }

    /// <summary>
    /// 创建部署阶段界面。
    /// </summary>
    private UnitDeployUI CreateDeployMenu()
    {
        Transform panel = FindOrCreatePanel("DeploymentPanel");
        if(!IsUnityObjectAlive(panel)) return null;

        RectTransform warehousePanel = FindOrCreateChildRect(panel, "WarehousePanel", out bool createdWarehousePanel);
        if(!IsUnityObjectAlive(warehousePanel)) return null;

        ApplyDefaultRect(warehousePanel, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), Vector2.zero, new Vector2(0f, 120f), createdWarehousePanel);

        Image warehouseImage = warehousePanel.GetComponent<Image>();
        if(warehouseImage == null) warehouseImage = warehousePanel.gameObject.AddComponent<Image>();
        warehouseImage.color = new Color(0.25f, 0.25f, 0.25f, 0.62f);

        RectTransform itemRoot = FindOrCreateChildRect(warehousePanel, "ItemRoot", out bool createdItemRoot);
        if(!IsUnityObjectAlive(itemRoot)) return null;

        ApplyDefaultRect(itemRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(760f, 88f), createdItemRoot);

        HorizontalLayoutGroup layoutGroup = itemRoot.GetComponent<HorizontalLayoutGroup>();
        if(layoutGroup == null) layoutGroup = itemRoot.gameObject.AddComponent<HorizontalLayoutGroup>();
        layoutGroup.childAlignment = TextAnchor.MiddleCenter;
        layoutGroup.spacing = 12f;
        layoutGroup.childControlWidth = false;
        layoutGroup.childControlHeight = false;
        layoutGroup.childForceExpandWidth = false;
        layoutGroup.childForceExpandHeight = false;

        Button startBattleButton = CreateSmallButton(panel, "StartBattleButton", "开始战斗", new Vector2(0f, 142f));
        RectTransform remainingUnitsDialog = CreateRemainingUnitsDialog(panel, out Text remainingUnitsText, out Button confirmButton, out Button continueButton);

        UnitDeployUI ui = panel.GetComponent<UnitDeployUI>();
        if(ui == null) ui = panel.gameObject.AddComponent<UnitDeployUI>();
        ui.SetReferences(warehousePanel, itemRoot, startBattleButton);
        ui.SetDialogReferences(remainingUnitsDialog, remainingUnitsText, confirmButton, continueButton);
        return ui;
    }

    /// <summary>
    /// 创建未放完单位时的开始战斗确认弹窗。
    /// </summary>
    private RectTransform CreateRemainingUnitsDialog(Transform parent, out Text messageText, out Button confirmButton, out Button continueButton)
    {
        messageText = null;
        confirmButton = null;
        continueButton = null;

        RectTransform dialog = FindOrCreateChildRect(parent, "RemainingUnitsDialog", out bool createdDialog);
        if(!IsUnityObjectAlive(dialog)) return null;

        ApplyDefaultRect(dialog, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(460f, 180f), createdDialog);

        Image dialogImage = dialog.GetComponent<Image>();
        if(dialogImage == null) dialogImage = dialog.gameObject.AddComponent<Image>();
        dialogImage.color = new Color(0.35f, 0.35f, 0.35f, 0.95f);

        messageText = CreateText(dialog, "MessageText", "你还有单位未放入场地", 24, Color.white, new Vector2(0f, 42f), new Vector2(420f, 52f));
        confirmButton = CreateDialogButton(dialog, "ConfirmStartBattleButton", "开始战斗", new Vector2(-100f, -42f));
        continueButton = CreateDialogButton(dialog, "ContinueDeployButton", "继续部署", new Vector2(100f, -42f));
        dialog.gameObject.SetActive(false);
        return dialog;
    }

    /// <summary>
    /// 查找或创建全屏 UI 面板。
    /// </summary>
    private Transform FindOrCreatePanel(string panelName)
    {
        if(!IsUnityObjectAlive(gameCanvas))
        {
            gameCanvas = null;
            return null;
        }

        Transform canvasTransform = gameCanvas.transform;
        if(!IsUnityObjectAlive(canvasTransform)) return null;

        Transform panel = canvasTransform.Find(panelName);
        bool createdPanel = panel == null;
        if(panel == null)
        {
            GameObject panelObject = new GameObject(panelName);
            panelObject.transform.SetParent(canvasTransform, false);
            panel = panelObject.transform;
        }

        if(!IsUnityObjectAlive(panel)) return null;

        RectTransform rectTransform = panel.GetComponent<RectTransform>();
        bool createdRect = rectTransform == null;
        if(rectTransform == null) rectTransform = panel.gameObject.AddComponent<RectTransform>();
        if(createdPanel || createdRect)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        return panel;
    }

    /// <summary>
    /// 查找或创建指定父物体下的 RectTransform 子物体。
    /// </summary>
    private RectTransform FindOrCreateChildRect(Transform parent, string childName)
    {
        return FindOrCreateChildRect(parent, childName, out _);
    }

    /// <summary>
    /// 查找或创建指定父物体下的 RectTransform 子物体，并返回是否为新创建对象。
    /// </summary>
    private RectTransform FindOrCreateChildRect(Transform parent, string childName, out bool created)
    {
        created = false;
        if(!IsUnityObjectAlive(parent)) return null;

        Transform child = parent.Find(childName);
        if(child == null)
        {
            GameObject childObject = new GameObject(childName);
            childObject.transform.SetParent(parent, false);
            child = childObject.transform;
            created = true;
        }

        if(!IsUnityObjectAlive(child)) return null;

        RectTransform rectTransform = child.GetComponent<RectTransform>();
        if(rectTransform == null) rectTransform = child.gameObject.AddComponent<RectTransform>();
        return rectTransform;
    }

    /// <summary>
    /// 只在需要初始化时设置默认 UI 布局，避免覆盖用户手动调整的 RectTransform。
    /// </summary>
    private void ApplyDefaultRect(RectTransform rectTransform, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta, bool shouldApply)
    {
        if(!shouldApply || !IsUnityObjectAlive(rectTransform)) return;

        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.pivot = pivot;
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = sizeDelta;
    }

    /// <summary>
    /// 判断 Unity 对象是否仍然有效，避免编辑器延迟回调访问已销毁对象。
    /// </summary>
    private bool IsUnityObjectAlive(UnityEngine.Object unityObject)
    {
        try
        {
            return unityObject != null;
        }
        catch(MissingReferenceException)
        {
            return false;
        }
    }

    /// <summary>
    /// 创建灰底白字按钮。
    /// </summary>
    private Button CreateButton(Transform parent, string objectName, string label, Vector2 anchoredPosition)
    {
        Transform existingButton = parent.Find(objectName);
        bool createdButton = existingButton == null;
        GameObject buttonObject = existingButton != null ? existingButton.gameObject : new GameObject(objectName);
        buttonObject.transform.SetParent(parent, false);

        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        if(buttonRect == null) buttonRect = buttonObject.AddComponent<RectTransform>();
        ApplyDefaultRect(buttonRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), anchoredPosition, new Vector2(520f, 96f), createdButton);

        Image image = buttonObject.GetComponent<Image>();
        if(image == null) image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.35f, 0.35f, 0.35f, 1f);

        Button button = buttonObject.GetComponent<Button>();
        if(button == null) button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;

        bool createdText = buttonObject.transform.Find("Text") == null;
        Text text = CreateText(buttonObject.transform, "Text", label, 60, Color.white, Vector2.zero, Vector2.zero);
        RectTransform textRect = text.GetComponent<RectTransform>();
        if(createdText)
        {
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
        }
        return button;
    }

    /// <summary>
    /// 创建部署阶段使用的小按钮。
    /// </summary>
    private Button CreateSmallButton(Transform parent, string objectName, string label, Vector2 anchoredPosition)
    {
        Transform existingButton = parent.Find(objectName);
        bool createdButton = existingButton == null;
        GameObject buttonObject = existingButton != null ? existingButton.gameObject : new GameObject(objectName);
        buttonObject.transform.SetParent(parent, false);

        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        if(buttonRect == null) buttonRect = buttonObject.AddComponent<RectTransform>();
        ApplyDefaultRect(buttonRect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), anchoredPosition, new Vector2(180f, 44f), createdButton);

        Image image = buttonObject.GetComponent<Image>();
        if(image == null) image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.35f, 0.35f, 0.35f, 1f);

        Button button = buttonObject.GetComponent<Button>();
        if(button == null) button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;

        bool createdText = buttonObject.transform.Find("Text") == null;
        Text text = CreateText(buttonObject.transform, "Text", label, 24, Color.white, Vector2.zero, Vector2.zero);
        RectTransform textRect = text.GetComponent<RectTransform>();
        if(createdText)
        {
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
        }
        return button;
    }

    /// <summary>
    /// 创建确认弹窗中的灰底白字按钮。
    /// </summary>
    private Button CreateDialogButton(Transform parent, string objectName, string label, Vector2 anchoredPosition)
    {
        Transform existingButton = parent.Find(objectName);
        bool createdButton = existingButton == null;
        GameObject buttonObject = existingButton != null ? existingButton.gameObject : new GameObject(objectName);
        buttonObject.transform.SetParent(parent, false);

        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        if(buttonRect == null) buttonRect = buttonObject.AddComponent<RectTransform>();
        ApplyDefaultRect(buttonRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), anchoredPosition, new Vector2(150f, 44f), createdButton);

        Image image = buttonObject.GetComponent<Image>();
        if(image == null) image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.25f, 0.25f, 0.25f, 1f);

        Button button = buttonObject.GetComponent<Button>();
        if(button == null) button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;

        bool createdText = buttonObject.transform.Find("Text") == null;
        Text text = CreateText(buttonObject.transform, "Text", label, 24, Color.white, Vector2.zero, Vector2.zero);
        RectTransform textRect = text.GetComponent<RectTransform>();
        if(createdText)
        {
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
        }
        return button;
    }

    /// <summary>
    /// 创建或更新 UI 文本。
    /// </summary>
    private Text CreateText(Transform parent, string objectName, string content, int fontSize, Color color, Vector2 anchoredPosition, Vector2 size)
    {
        Transform existingText = parent.Find(objectName);
        bool createdTextObject = existingText == null;
        GameObject textObject = existingText != null ? existingText.gameObject : new GameObject(objectName);
        textObject.transform.SetParent(parent, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        bool createdRect = textRect == null;
        if(textRect == null) textRect = textObject.AddComponent<RectTransform>();
        ApplyDefaultRect(textRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), anchoredPosition, size, createdTextObject || createdRect);

        Text text = textObject.GetComponent<Text>();
        if(text == null)
        {
            text = textObject.AddComponent<Text>();
            text.alignment = TextAnchor.MiddleCenter;
        }

        text.text = content;
        text.color = color;
        text.fontSize = fontSize;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return text;
    }
}
