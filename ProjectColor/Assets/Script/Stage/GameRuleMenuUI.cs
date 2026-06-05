using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// 控制标题阶段的游戏规则界面，并允许玩家通过按钮或右键返回标题界面。
/// </summary>
public class GameRuleMenuUI : MonoBehaviour
{
    private const string DefaultRuleContent = "这是一个规则";
    private const string OldDefaultRuleContent = "在这里填写游戏规则。";

    [SerializeField] private GameStageManager stageManager;
    [SerializeField] private Text ruleText;
    [SerializeField] private Button understandButton;
    [SerializeField, TextArea(6, 18)] private string ruleContent = DefaultRuleContent;

    /// <summary>
    /// 设置游戏规则界面所需引用。
    /// </summary>
    public void SetReferences(GameStageManager manager, Text contentText, Button closeButton)
    {
        stageManager = manager;
        ruleText = contentText;
        understandButton = closeButton;
        NormalizeDefaultRuleContent();
        RefreshRuleText();
        BindButtons();
    }

    /// <summary>
    /// 启用时绑定按钮事件并刷新规则文本。
    /// </summary>
    private void OnEnable()
    {
        RefreshRuleText();
        BindButtons();
    }

    /// <summary>
    /// 每帧检测右键返回标题界面。
    /// </summary>
    private void Update()
    {
        if(Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
        {
            HideRules();
        }
    }

    /// <summary>
    /// 禁用时解绑按钮事件。
    /// </summary>
    private void OnDisable()
    {
        if(understandButton != null)
        {
            understandButton.onClick.RemoveListener(HideRules);
        }
    }

    /// <summary>
    /// Inspector 内容改变时刷新规则文本。
    /// </summary>
    private void OnValidate()
    {
        NormalizeDefaultRuleContent();
        RefreshRuleText();
    }

    /// <summary>
    /// 显示或隐藏游戏规则界面。
    /// </summary>
    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
        if(visible)
        {
            RefreshRuleText();
        }
    }

    /// <summary>
    /// 绑定了解按钮点击事件。
    /// </summary>
    private void BindButtons()
    {
        if(understandButton == null) return;

        understandButton.onClick.RemoveListener(HideRules);
        understandButton.onClick.AddListener(HideRules);
    }

    /// <summary>
    /// 将 Inspector 中填写的规则内容同步到界面文本。
    /// </summary>
    private void RefreshRuleText()
    {
        if(ruleText != null)
        {
            ruleText.text = ruleContent;
        }
    }

    /// <summary>
    /// 将旧默认规则文本迁移为当前默认文本，保留用户手动填写的内容。
    /// </summary>
    private void NormalizeDefaultRuleContent()
    {
        if(string.IsNullOrWhiteSpace(ruleContent) || ruleContent == OldDefaultRuleContent)
        {
            ruleContent = DefaultRuleContent;
        }
    }

    /// <summary>
    /// 隐藏规则界面并回到标题界面。
    /// </summary>
    private void HideRules()
    {
        if(stageManager != null)
        {
            stageManager.HideGameRules();
            return;
        }

        SetVisible(false);
    }
}
