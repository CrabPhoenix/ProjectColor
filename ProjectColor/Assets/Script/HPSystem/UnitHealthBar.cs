using UnityEngine;

/// <summary>
/// 在单位头顶显示当前生命值点数。
/// </summary>
[RequireComponent(typeof(UnitHealth))]
public class UnitHealthBar : MonoBehaviour
{
    [SerializeField] private Vector3 localOffset = new Vector3(0f, 0.75f, 0f);
    [SerializeField] private int fontSize = 56;
    [SerializeField] private Color fullHealthColor = Color.white;
    [SerializeField] private Color lowHealthColor = Color.red;
    [SerializeField] private int sortingOrder = 220;

    private UnitHealth unitHealth;
    private TextMesh healthText;
    private MeshRenderer textRenderer;

    /// <summary>
    /// 创建并绑定生命值数字显示。
    /// </summary>
    private void Awake()
    {
        unitHealth = GetComponent<UnitHealth>();
        CreateHealthText();
        RefreshHealthText(unitHealth);
    }

    /// <summary>
    /// 订阅生命值变化。
    /// </summary>
    private void OnEnable()
    {
        if(unitHealth == null) unitHealth = GetComponent<UnitHealth>();
        if(unitHealth != null)
        {
            unitHealth.OnHealthChanged += RefreshHealthText;
            unitHealth.OnDeath += HideHealthText;
            RefreshHealthText(unitHealth);
        }
    }

    /// <summary>
    /// 取消订阅生命值变化。
    /// </summary>
    private void OnDisable()
    {
        if(unitHealth != null)
        {
            unitHealth.OnHealthChanged -= RefreshHealthText;
            unitHealth.OnDeath -= HideHealthText;
        }
    }

    /// <summary>
    /// 保持生命值文本始终位于单位世界坐标上方，避免受单位朝向旋转影响。
    /// </summary>
    private void LateUpdate()
    {
        if(healthText == null) return;

        healthText.transform.position = transform.position + localOffset;
        healthText.transform.rotation = Quaternion.identity;
    }

    /// <summary>
    /// 创建头顶生命值文本。
    /// </summary>
    private void CreateHealthText()
    {
        if(healthText != null) return;

        GameObject textObject = new GameObject("HealthText");
        textObject.transform.SetParent(transform, false);
        textObject.transform.localPosition = localOffset;

        healthText = textObject.AddComponent<TextMesh>();
        healthText.anchor = TextAnchor.MiddleCenter;
        healthText.alignment = TextAlignment.Center;
        healthText.fontSize = fontSize;
        healthText.characterSize = 0.05f;
        healthText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        textRenderer = textObject.GetComponent<MeshRenderer>();
        if(textRenderer != null)
        {
            textRenderer.sortingOrder = sortingOrder;
        }
    }

    /// <summary>
    /// 根据当前生命值刷新显示文本。
    /// </summary>
    private void RefreshHealthText(UnitHealth health)
    {
        if(health == null || healthText == null) return;

        healthText.text = health.CurrentHealth.ToString();
        healthText.color = health.HealthPercent > 0.5f ? fullHealthColor : lowHealthColor;
        healthText.gameObject.SetActive(health.CurrentHealth > 0);
    }

    /// <summary>
    /// 单位死亡时隐藏生命值文本。
    /// </summary>
    private void HideHealthText(UnitHealth health)
    {
        if(healthText != null)
        {
            healthText.gameObject.SetActive(false);
        }
    }
}
