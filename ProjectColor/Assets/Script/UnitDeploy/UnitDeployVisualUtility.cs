using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 提供部署阶段从单位 Prefab 读取显示外观的工具方法。
/// </summary>
public static class UnitDeployVisualUtility
{
    /// <summary>
    /// 将单位 Prefab 的 SpriteRenderer 外观应用到仓库图标。
    /// </summary>
    public static void ApplyToImage(Unit unitPrefab, Image iconImage)
    {
        if(iconImage == null) return;

        SpriteRenderer spriteRenderer = GetSpriteRenderer(unitPrefab);
        if(spriteRenderer == null || spriteRenderer.sprite == null)
        {
            iconImage.sprite = null;
            iconImage.color = Color.clear;
            iconImage.enabled = false;
            return;
        }

        iconImage.sprite = spriteRenderer.sprite;
        iconImage.color = spriteRenderer.color;
        iconImage.enabled = true;
        iconImage.preserveAspect = true;
    }

    /// <summary>
    /// 将单位 Prefab 的 SpriteRenderer 外观应用到部署预览。
    /// </summary>
    public static void ApplyToPreview(Unit unitPrefab, SpriteRenderer previewRenderer, float alpha)
    {
        if(previewRenderer == null) return;

        SpriteRenderer sourceRenderer = GetSpriteRenderer(unitPrefab);
        if(sourceRenderer == null || sourceRenderer.sprite == null)
        {
            previewRenderer.sprite = null;
            previewRenderer.color = Color.clear;
            return;
        }

        Color previewColor = sourceRenderer.color;
        previewColor.a = alpha;
        previewRenderer.sprite = sourceRenderer.sprite;
        previewRenderer.color = previewColor;
        previewRenderer.transform.localScale = sourceRenderer.transform.lossyScale;
        previewRenderer.sortingLayerID = sourceRenderer.sortingLayerID;
        previewRenderer.sortingOrder = sourceRenderer.sortingOrder + 1;
    }

    /// <summary>
    /// 获取单位 Prefab 上用于显示的 SpriteRenderer。
    /// </summary>
    private static SpriteRenderer GetSpriteRenderer(Unit unitPrefab)
    {
        return unitPrefab != null ? unitPrefab.GetComponentInChildren<SpriteRenderer>() : null;
    }
}
