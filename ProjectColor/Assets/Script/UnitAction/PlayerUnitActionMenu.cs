using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 显示在玩家单位所在格左侧的行动选择菜单和攻击二级菜单。
/// </summary>
public class PlayerUnitActionMenu : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Vector2 screenOffset = new Vector2(-4f, 0f);

    private Canvas actionCanvas;
    private RectTransform panelRect;
    private RectTransform attackSubmenuRect;
    private Button attackButton;
    private Button swordButton;
    private Button interactionButton;
    private Button waitButton;
    private Text attackText;
    private Text swordText;
    private Text interactionText;
    private Text waitText;
    private Outline attackOutline;
    private Outline swordOutline;
    private Outline interactionOutline;
    private Outline waitOutline;
    private Unit targetUnit;
    private Action<PlayerUnitActionType> onActionSelected;

    /// <summary>
    /// 初始化菜单 UI。
    /// </summary>
    private void Awake()
    {
        if(targetCamera == null) targetCamera = Camera.main;
        CreateMenu();
        Hide();
    }

    /// <summary>
    /// 每帧让菜单贴在目标单位所在格左侧。
    /// </summary>
    private void LateUpdate()
    {
        if(targetUnit == null || panelRect == null || !panelRect.gameObject.activeSelf) return;
        if(targetCamera == null) targetCamera = Camera.main;
        if(targetCamera == null) return;

        Vector2 screenPosition = targetCamera.WorldToScreenPoint(GetMenuAnchorWorldPosition());
        panelRect.anchoredPosition = screenPosition + screenOffset;
    }

    /// <summary>
    /// 显示指定单位的行动菜单。
    /// </summary>
    public void Show(Unit unit, PlayerUnitActionType selectedAction, Action<PlayerUnitActionType> actionSelected)
    {
        targetUnit = unit;
        onActionSelected = actionSelected;
        if(panelRect != null)
        {
            panelRect.gameObject.SetActive(unit != null);
        }

        SetSelectedAction(selectedAction);
    }

    /// <summary>
    /// 隐藏行动菜单。
    /// </summary>
    public void Hide()
    {
        targetUnit = null;
        onActionSelected = null;
        if(panelRect != null)
        {
            panelRect.gameObject.SetActive(false);
        }

        if(attackSubmenuRect != null)
        {
            attackSubmenuRect.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 更新当前选中行动的高亮显示。
    /// </summary>
    public void SetSelectedAction(PlayerUnitActionType selectedAction)
    {
        bool isAttackSelected = selectedAction == PlayerUnitActionType.Attack || selectedAction == PlayerUnitActionType.Sword;
        bool isSwordSelected = selectedAction == PlayerUnitActionType.Sword;
        bool isInteractionSelected = selectedAction == PlayerUnitActionType.Interaction || selectedAction == PlayerUnitActionType.ConvertNeutral;
        bool isWaitSelected = selectedAction == PlayerUnitActionType.Wait;

        if(attackText != null) attackText.color = isAttackSelected ? Color.yellow : Color.white;
        if(swordText != null) swordText.color = isSwordSelected ? Color.yellow : Color.white;
        if(interactionText != null) interactionText.color = isInteractionSelected ? Color.yellow : Color.white;
        if(waitText != null) waitText.color = isWaitSelected ? Color.yellow : Color.white;
        if(attackOutline != null) attackOutline.enabled = isAttackSelected;
        if(swordOutline != null) swordOutline.enabled = isSwordSelected;
        if(interactionOutline != null) interactionOutline.enabled = isInteractionSelected;
        if(waitOutline != null) waitOutline.enabled = isWaitSelected;
        if(attackSubmenuRect != null) attackSubmenuRect.gameObject.SetActive(isAttackSelected);
    }

    /// <summary>
    /// 创建菜单 UI。
    /// </summary>
    private void CreateMenu()
    {
        actionCanvas = FindFirstObjectByType<Canvas>();
        if(actionCanvas == null)
        {
            GameObject canvasObject = new GameObject("PlayerActionCanvas");
            actionCanvas = canvasObject.AddComponent<Canvas>();
            actionCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            actionCanvas.sortingOrder = 1100;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
        }

        GameObject panelObject = new GameObject("PlayerActionMenu");
        panelObject.transform.SetParent(actionCanvas.transform, false);
        panelRect = panelObject.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.zero;
        panelRect.pivot = new Vector2(1f, 0.5f);
        panelRect.sizeDelta = new Vector2(150f, 128f);

        Image panelImage = panelObject.AddComponent<Image>();
        panelImage.color = new Color(0.03f, 0.08f, 0.1f, 0.86f);

        attackButton = CreateButton(panelRect, "Attack", "攻击", new Vector2(0f, 36f), new Vector2(120f, 32f), out attackText);
        attackOutline = CreateSelectedOutline(attackButton.gameObject);
        attackButton.onClick.AddListener(() => onActionSelected?.Invoke(PlayerUnitActionType.Attack));

        interactionButton = CreateButton(panelRect, "Interaction", "互动", new Vector2(0f, 0f), new Vector2(120f, 32f), out interactionText);
        interactionOutline = CreateSelectedOutline(interactionButton.gameObject);
        interactionButton.onClick.AddListener(() => onActionSelected?.Invoke(PlayerUnitActionType.Interaction));

        waitButton = CreateButton(panelRect, "Wait", "待命", new Vector2(0f, -36f), new Vector2(120f, 32f), out waitText);
        waitOutline = CreateSelectedOutline(waitButton.gameObject);
        waitButton.onClick.AddListener(() => onActionSelected?.Invoke(PlayerUnitActionType.Wait));

        CreateAttackSubmenu();
    }

    /// <summary>
    /// 创建攻击技能二级菜单。
    /// </summary>
    private void CreateAttackSubmenu()
    {
        GameObject submenuObject = new GameObject("AttackSubmenu");
        submenuObject.transform.SetParent(panelRect, false);
        attackSubmenuRect = submenuObject.AddComponent<RectTransform>();
        attackSubmenuRect.anchorMin = new Vector2(0f, 0.5f);
        attackSubmenuRect.anchorMax = new Vector2(0f, 0.5f);
        attackSubmenuRect.pivot = new Vector2(1f, 0.5f);
        attackSubmenuRect.anchoredPosition = new Vector2(-4f, 36f);
        attackSubmenuRect.sizeDelta = new Vector2(96f, 40f);

        Image submenuImage = submenuObject.AddComponent<Image>();
        submenuImage.color = new Color(0.03f, 0.08f, 0.1f, 0.86f);

        swordButton = CreateButton(attackSubmenuRect, "Sword", "Sword", Vector2.zero, new Vector2(76f, 28f), out swordText);
        swordOutline = CreateSelectedOutline(swordButton.gameObject);
        swordButton.onClick.AddListener(() => onActionSelected?.Invoke(PlayerUnitActionType.Sword));
        attackSubmenuRect.gameObject.SetActive(false);
    }

    /// <summary>
    /// 创建菜单按钮。
    /// </summary>
    private Button CreateButton(Transform parent, string objectName, string label, Vector2 anchoredPosition, Vector2 size, out Text labelText)
    {
        GameObject buttonObject = new GameObject(objectName);
        buttonObject.transform.SetParent(parent, false);

        RectTransform buttonRect = buttonObject.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = anchoredPosition;
        buttonRect.sizeDelta = size;

        Image buttonImage = buttonObject.AddComponent<Image>();
        buttonImage.color = new Color(0.05f, 0.13f, 0.16f, 0.72f);

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = buttonImage;

        GameObject textObject = new GameObject("Text");
        textObject.transform.SetParent(buttonObject.transform, false);
        RectTransform textRect = textObject.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(8f, 0f);
        textRect.offsetMax = new Vector2(-8f, 0f);

        labelText = textObject.AddComponent<Text>();
        labelText.text = label;
        labelText.color = Color.white;
        labelText.fontSize = 16;
        labelText.alignment = TextAnchor.MiddleLeft;
        labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        return button;
    }

    /// <summary>
    /// 创建选中状态使用的外框。
    /// </summary>
    private Outline CreateSelectedOutline(GameObject targetObject)
    {
        Outline outline = targetObject.AddComponent<Outline>();
        outline.effectColor = new Color(0.45f, 1f, 0.82f, 1f);
        outline.effectDistance = new Vector2(2f, 2f);
        outline.enabled = false;
        return outline;
    }

    /// <summary>
    /// 获得贴在单位所在格左侧的菜单锚点。
    /// </summary>
    private Vector3 GetMenuAnchorWorldPosition()
    {
        if(targetUnit == null) return Vector3.zero;
        if(GridManager.Instance == null) return targetUnit.transform.position + new Vector3(-0.5f, 0f, 0f);

        targetUnit.UnitMover.RefreshCurrentCell();
        return GridManager.Instance.GetWorldInGrid(targetUnit.CurrentCell) + new Vector3(-0.5f, 0f, 0f);
    }
}
