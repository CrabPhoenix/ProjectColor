using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 控制部署阶段底部单位仓库和开始战斗按钮。
/// </summary>
public class UnitDeployUI : MonoBehaviour
{
    [SerializeField] private RectTransform warehousePanel;
    [SerializeField] private RectTransform itemRoot;
    [SerializeField] private Button startBattleButton;
    [SerializeField] private RectTransform remainingUnitsDialog;
    [SerializeField] private Text remainingUnitsText;
    [SerializeField] private Button confirmStartBattleButton;
    [SerializeField] private Button continueDeployButton;

    private readonly List<UnitDeployInventoryItem> items = new List<UnitDeployInventoryItem>();
    private Action<Unit> onUnitClicked;
    private Action onStartBattleClicked;
    private Action onConfirmStartBattleClicked;

    public Button StartBattleButton => startBattleButton;

    /// <summary>
    /// 设置部署 UI 的引用。
    /// </summary>
    public void SetReferences(RectTransform panel, RectTransform root, Button startButton)
    {
        warehousePanel = panel;
        itemRoot = root;
        startBattleButton = startButton;
    }

    /// <summary>
    /// 设置部署阶段未放完单位提示弹窗的引用。
    /// </summary>
    public void SetDialogReferences(RectTransform dialogPanel, Text messageText, Button confirmButton, Button continueButton)
    {
        remainingUnitsDialog = dialogPanel;
        remainingUnitsText = messageText;
        confirmStartBattleButton = confirmButton;
        continueDeployButton = continueButton;
        HideRemainingUnitsDialog();
    }

    /// <summary>
    /// 设置 UI 交互回调。
    /// </summary>
    public void SetCallbacks(Action<Unit> unitClicked, Action startBattleClicked, Action confirmStartBattleClicked)
    {
        onUnitClicked = unitClicked;
        onStartBattleClicked = startBattleClicked;
        onConfirmStartBattleClicked = confirmStartBattleClicked;
        if(startBattleButton != null)
        {
            startBattleButton.onClick.RemoveListener(HandleStartBattleClicked);
            startBattleButton.onClick.AddListener(HandleStartBattleClicked);
        }

        if(confirmStartBattleButton != null)
        {
            confirmStartBattleButton.onClick.RemoveListener(HandleConfirmStartBattleClicked);
            confirmStartBattleButton.onClick.AddListener(HandleConfirmStartBattleClicked);
        }

        if(continueDeployButton != null)
        {
            continueDeployButton.onClick.RemoveListener(HandleContinueDeployClicked);
            continueDeployButton.onClick.AddListener(HandleContinueDeployClicked);
        }
    }

    /// <summary>
    /// 显示或隐藏部署 UI。
    /// </summary>
    public void SetVisible(bool visible)
    {
        if(!visible)
        {
            HideRemainingUnitsDialog();
        }

        gameObject.SetActive(visible);
    }

    /// <summary>
    /// 显示还有单位未部署的确认弹窗。
    /// </summary>
    public void ShowRemainingUnitsDialog()
    {
        if(remainingUnitsText != null)
        {
            remainingUnitsText.text = "你还有单位未放入场地";
        }

        if(remainingUnitsDialog != null)
        {
            remainingUnitsDialog.gameObject.SetActive(true);
            remainingUnitsDialog.SetAsLastSibling();
        }
    }

    /// <summary>
    /// 隐藏还有单位未部署的确认弹窗。
    /// </summary>
    public void HideRemainingUnitsDialog()
    {
        if(remainingUnitsDialog != null)
        {
            remainingUnitsDialog.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 按当前库存刷新仓库格子。
    /// </summary>
    public void RefreshInventory(IReadOnlyDictionary<Unit, int> inventory)
    {
        ClearItems();
        if(itemRoot == null || inventory == null) return;

        foreach(KeyValuePair<Unit, int> pair in inventory)
        {
            if(pair.Key == null) continue;

            UnitDeployInventoryItem item = CreateInventoryItem(pair.Key.name);
            item.Initialize(pair.Key, pair.Value, onUnitClicked);
            items.Add(item);
        }
    }

    /// <summary>
    /// 清理全部仓库格子。
    /// </summary>
    private void ClearItems()
    {
        foreach(UnitDeployInventoryItem item in items)
        {
            if(item != null)
            {
                Destroy(item.gameObject);
            }
        }

        items.Clear();
    }

    /// <summary>
    /// 创建一个仓库单位格子。
    /// </summary>
    private UnitDeployInventoryItem CreateInventoryItem(string itemName)
    {
        GameObject itemObject = new GameObject(itemName);
        itemObject.transform.SetParent(itemRoot, false);

        RectTransform itemRect = itemObject.AddComponent<RectTransform>();
        itemRect.sizeDelta = new Vector2(72f, 72f);

        Image background = itemObject.AddComponent<Image>();
        background.color = new Color(0.16f, 0.16f, 0.16f, 0.85f);

        Button button = itemObject.AddComponent<Button>();
        button.targetGraphic = background;

        Image icon = CreateIcon(itemObject.transform);
        Text countText = CreateCountText(itemObject.transform);

        UnitDeployInventoryItem item = itemObject.AddComponent<UnitDeployInventoryItem>();
        item.SetReferences(button, icon, countText);
        return item;
    }

    /// <summary>
    /// 创建仓库格子中的单位图标。
    /// </summary>
    private Image CreateIcon(Transform parent)
    {
        GameObject iconObject = new GameObject("Icon");
        iconObject.transform.SetParent(parent, false);

        RectTransform iconRect = iconObject.AddComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.1f, 0.1f);
        iconRect.anchorMax = new Vector2(0.9f, 0.9f);
        iconRect.offsetMin = Vector2.zero;
        iconRect.offsetMax = Vector2.zero;

        Image icon = iconObject.AddComponent<Image>();
        icon.preserveAspect = true;
        return icon;
    }

    /// <summary>
    /// 创建仓库格子左上角的数量文本。
    /// </summary>
    private Text CreateCountText(Transform parent)
    {
        GameObject textObject = new GameObject("Count");
        textObject.transform.SetParent(parent, false);

        RectTransform textRect = textObject.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0f, 1f);
        textRect.anchorMax = new Vector2(0f, 1f);
        textRect.pivot = new Vector2(0f, 1f);
        textRect.anchoredPosition = new Vector2(4f, -2f);
        textRect.sizeDelta = new Vector2(42f, 24f);

        Text text = textObject.AddComponent<Text>();
        text.alignment = TextAnchor.MiddleLeft;
        text.color = Color.white;
        text.fontSize = 18;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return text;
    }

    /// <summary>
    /// 处理开始战斗按钮点击。
    /// </summary>
    private void HandleStartBattleClicked()
    {
        onStartBattleClicked?.Invoke();
    }

    /// <summary>
    /// 处理确认弹窗中的开始战斗按钮点击。
    /// </summary>
    private void HandleConfirmStartBattleClicked()
    {
        HideRemainingUnitsDialog();
        onConfirmStartBattleClicked?.Invoke();
    }

    /// <summary>
    /// 处理确认弹窗中的继续部署按钮点击。
    /// </summary>
    private void HandleContinueDeployClicked()
    {
        HideRemainingUnitsDialog();
    }
}
