using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 控制标题界面的开始游戏和退出游戏按钮。
/// </summary>
public class TitleMenuUI : MonoBehaviour
{
    [SerializeField] private GameStageManager stageManager;
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button exitGameButton;

    /// <summary>
    /// 设置标题界面所需引用。
    /// </summary>
    public void SetReferences(GameStageManager manager, Button startButton, Button exitButton)
    {
        stageManager = manager;
        startGameButton = startButton;
        exitGameButton = exitButton;
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
        if(startGameButton != null) startGameButton.onClick.RemoveListener(HandleStartGame);
        if(exitGameButton != null) exitGameButton.onClick.RemoveListener(HandleExitGame);
    }

    /// <summary>
    /// 显示或隐藏标题界面。
    /// </summary>
    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }

    /// <summary>
    /// 绑定按钮点击事件。
    /// </summary>
    private void BindButtons()
    {
        if(startGameButton != null)
        {
            startGameButton.onClick.RemoveListener(HandleStartGame);
            startGameButton.onClick.AddListener(HandleStartGame);
        }

        if(exitGameButton != null)
        {
            exitGameButton.onClick.RemoveListener(HandleExitGame);
            exitGameButton.onClick.AddListener(HandleExitGame);
        }
    }

    /// <summary>
    /// 处理开始游戏按钮。
    /// </summary>
    private void HandleStartGame()
    {
        if(stageManager != null)
        {
            stageManager.StartGame();
        }
    }

    /// <summary>
    /// 处理退出游戏按钮。
    /// </summary>
    private void HandleExitGame()
    {
        if(stageManager != null)
        {
            stageManager.ExitGame();
        }
    }
}
