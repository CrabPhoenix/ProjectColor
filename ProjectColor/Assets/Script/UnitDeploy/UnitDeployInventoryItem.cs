using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 表示部署仓库中的一个可点击单位格子。
/// </summary>
public class UnitDeployInventoryItem : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image iconImage;
    [SerializeField] private Text countText;

    private Unit unitPrefab;
    private Action<Unit> onClicked;

    public Unit UnitPrefab => unitPrefab;

    /// <summary>
    /// 设置仓库格子的 UI 引用。
    /// </summary>
    public void SetReferences(Button itemButton, Image icon, Text countLabel)
    {
        button = itemButton;
        iconImage = icon;
        countText = countLabel;
    }

    /// <summary>
    /// 初始化仓库格子显示与点击回调。
    /// </summary>
    public void Initialize(Unit prefab, int count, Action<Unit> clicked)
    {
        unitPrefab = prefab;
        onClicked = clicked;

        if(button == null) button = GetComponent<Button>();
        if(button != null)
        {
            button.onClick.RemoveListener(HandleClick);
            button.onClick.AddListener(HandleClick);
            button.interactable = count > 0;
        }

        if(iconImage != null)
        {
            UnitDeployVisualUtility.ApplyToImage(unitPrefab, iconImage);
        }

        if(countText != null)
        {
            countText.text = count.ToString();
        }
    }

    /// <summary>
    /// 处理仓库格子的点击输入。
    /// </summary>
    private void HandleClick()
    {
        if(unitPrefab == null) return;

        onClicked?.Invoke(unitPrefab);
    }
}
