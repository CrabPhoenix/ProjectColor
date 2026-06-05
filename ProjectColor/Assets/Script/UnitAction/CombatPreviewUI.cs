using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 显示玩家攻击确认前的双方伤害预览。
/// </summary>
public class CombatPreviewUI : MonoBehaviour
{
    [SerializeField] private Canvas previewCanvas;
    [SerializeField] private RectTransform panelRect;
    [SerializeField] private Text attackerDamageText;
    [SerializeField] private Text counterDamageText;
    [SerializeField] private Button attackButton;

    private Action onConfirm;

    public bool IsVisible => panelRect != null && panelRect.gameObject.activeSelf;

    /// <summary>
    /// 初始化预览 UI。
    /// </summary>
    private void Awake()
    {
        EnsureUI();
        BindAttackButton();
        Hide();
    }

    /// <summary>
    /// 显示本次战斗预览。
    /// </summary>
    public void Show(CombatPreviewData previewData, Action confirmAction)
    {
        EnsureUI();
        onConfirm = confirmAction;

        if(attackerDamageText != null)
        {
            attackerDamageText.text = $"主动伤害\n{previewData.AttackDamage}";
        }

        if(counterDamageText != null)
        {
            counterDamageText.text = $"回击伤害\n{previewData.CounterDamage}";
        }

        if(panelRect != null)
        {
            panelRect.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 隐藏伤害预览。
    /// </summary>
    public void Hide()
    {
        onConfirm = null;
        if(panelRect != null)
        {
            panelRect.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 确保场景中存在可编辑的预览 UI 对象。
    /// </summary>
    private void EnsureUI()
    {
        if(panelRect != null) return;

        if(previewCanvas == null)
        {
            Canvas existingCanvas = FindFirstObjectByType<Canvas>();
            if(existingCanvas != null)
            {
                previewCanvas = existingCanvas;
            }
            else
            {
                GameObject canvasObject = new GameObject("CombatPreviewCanvas");
                previewCanvas = canvasObject.AddComponent<Canvas>();
                previewCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                previewCanvas.sortingOrder = 1200;
                canvasObject.AddComponent<CanvasScaler>();
                canvasObject.AddComponent<GraphicRaycaster>();
            }
        }

        Transform existingPanel = previewCanvas.transform.Find("CombatPreviewPanel");
        if(existingPanel != null)
        {
            panelRect = existingPanel.GetComponent<RectTransform>();
            ResolveExistingChildren();
            return;
        }

        CreateDefaultUI();
    }

    /// <summary>
    /// 创建首次使用时的默认预览布局。
    /// </summary>
    private void CreateDefaultUI()
    {
        GameObject panelObject = new GameObject("CombatPreviewPanel");
        panelObject.transform.SetParent(previewCanvas.transform, false);

        panelRect = panelObject.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(420f, 160f);

        Image panelImage = panelObject.AddComponent<Image>();
        panelImage.color = new Color(0.08f, 0.08f, 0.08f, 0.86f);

        attackerDamageText = CreateText(panelRect, "AttackerDamageText", new Vector2(-120f, 20f), new Vector2(130f, 90f), TextAnchor.MiddleCenter);
        counterDamageText = CreateText(panelRect, "CounterDamageText", new Vector2(120f, 20f), new Vector2(130f, 90f), TextAnchor.MiddleCenter);
        attackButton = CreateButton(panelRect);
    }

    /// <summary>
    /// 解析已有预览 UI 子对象，避免覆盖用户手动调整过的布局。
    /// </summary>
    private void ResolveExistingChildren()
    {
        if(panelRect == null) return;

        if(attackerDamageText == null)
        {
            Transform child = panelRect.Find("AttackerDamageText");
            if(child != null) attackerDamageText = child.GetComponent<Text>();
        }

        if(counterDamageText == null)
        {
            Transform child = panelRect.Find("CounterDamageText");
            if(child != null) counterDamageText = child.GetComponent<Text>();
        }

        if(attackButton == null)
        {
            Transform child = panelRect.Find("AttackButton");
            if(child != null) attackButton = child.GetComponent<Button>();
        }

        if(attackButton != null)
        {
            BindAttackButton();
        }
    }

    /// <summary>
    /// 创建伤害文本。
    /// </summary>
    private Text CreateText(Transform parent, string objectName, Vector2 anchoredPosition, Vector2 size, TextAnchor alignment)
    {
        GameObject textObject = new GameObject(objectName);
        textObject.transform.SetParent(parent, false);

        RectTransform textRect = textObject.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = anchoredPosition;
        textRect.sizeDelta = size;

        Text text = textObject.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 24;
        text.color = Color.white;
        text.alignment = alignment;
        return text;
    }

    /// <summary>
    /// 创建攻击确认按钮。
    /// </summary>
    private Button CreateButton(Transform parent)
    {
        GameObject buttonObject = new GameObject("AttackButton");
        buttonObject.transform.SetParent(parent, false);

        RectTransform buttonRect = buttonObject.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = new Vector2(0f, -45f);
        buttonRect.sizeDelta = new Vector2(110f, 44f);

        Image buttonImage = buttonObject.AddComponent<Image>();
        buttonImage.color = new Color(0.45f, 0.45f, 0.45f, 0.95f);

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = buttonImage;
        button.onClick.AddListener(ConfirmAttack);

        Text labelText = CreateText(buttonObject.transform, "Text", Vector2.zero, new Vector2(110f, 44f), TextAnchor.MiddleCenter);
        labelText.text = "攻击";
        return button;
    }

    /// <summary>
    /// 绑定攻击确认按钮事件。
    /// </summary>
    private void BindAttackButton()
    {
        if(attackButton == null) return;

        attackButton.onClick.RemoveListener(ConfirmAttack);
        attackButton.onClick.AddListener(ConfirmAttack);
    }

    /// <summary>
    /// 点击确认按钮时执行当前预览的攻击。
    /// </summary>
    private void ConfirmAttack()
    {
        onConfirm?.Invoke();
    }
}
