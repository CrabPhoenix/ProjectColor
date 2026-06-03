using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 控制结算界面的胜负显示、重启和回到标题按钮。
/// </summary>
public class SettlementMenuUI : MonoBehaviour
{
    [SerializeField] private GameStageManager stageManager;
    [SerializeField] private Text resultText;
    [SerializeField] private Button restartGameButton;
    [SerializeField] private Button backToTitleButton;

    /// <summary>
    /// 设置结算界面所需引用。
    /// </summary>
    public void SetReferences(GameStageManager manager, Text resultLabel, Button restartButton, Button titleButton)
    {
        stageManager = manager;
        resultText = resultLabel;
        restartGameButton = restartButton;
        backToTitleButton = titleButton;
        BindButtons();
    }

    /// <summary>
    /// 启用时绑定按钮事件。
    /// </summary>
    private void OnEnable()
    {
        BindButtons();
    }

    /// <summary>
    /// 禁用时解绑按钮事件。
    /// </summary>
    private void OnDisable()
    {
        if(restartGameButton != null) restartGameButton.onClick.RemoveListener(HandleRestartGame);
        if(backToTitleButton != null) backToTitleButton.onClick.RemoveListener(HandleBackToTitle);
    }

    /// <summary>
    /// 显示或隐藏结算界面。
    /// </summary>
    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }

    /// <summary>
    /// 设置胜负显示内容。
    /// </summary>
    public void SetResult(GameResult result)
    {
        if(resultText == null) return;

        if(result == GameResult.Victory)
        {
            resultText.text = "Victory";
            resultText.color = Color.green;
        }
        else
        {
            resultText.text = "Defeat";
            resultText.color = Color.red;
        }
    }

    /// <summary>
    /// 绑定按钮点击事件。
    /// </summary>
    private void BindButtons()
    {
        if(restartGameButton != null)
        {
            restartGameButton.onClick.RemoveListener(HandleRestartGame);
            restartGameButton.onClick.AddListener(HandleRestartGame);
        }

        if(backToTitleButton != null)
        {
            backToTitleButton.onClick.RemoveListener(HandleBackToTitle);
            backToTitleButton.onClick.AddListener(HandleBackToTitle);
        }
    }

    /// <summary>
    /// 处理重启游戏按钮。
    /// </summary>
    private void HandleRestartGame()
    {
        if(stageManager != null)
        {
            stageManager.RestartGame();
        }
    }

    /// <summary>
    /// 处理回到标题界面按钮。
    /// </summary>
    private void HandleBackToTitle()
    {
        if(stageManager != null)
        {
            stageManager.BackToTitle();
        }
    }
}
