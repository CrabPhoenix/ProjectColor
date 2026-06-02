using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 处理游戏的UI变化
/// </summary>
public class UI_Controller : MonoBehaviour
{
    [SerializeField] private InGameUI inGameUI;
    [SerializeField] private PlayerAttribute playerAttribute;

    [SerializeField] private GameObject startPanel;
    [SerializeField] private Button startButton;
    
    [SerializeField] private GameObject winPanel;
    [SerializeField] private Button winButton;
    [SerializeField] private TMP_Text winScoreText;

    [SerializeField] private GameObject losePanel;
    [SerializeField] private Button loseButton;
    [SerializeField] private TMP_Text loseScoreText;

    [SerializeField] private TMP_Text InGameScoreText;

    [SerializeField] private List<Image> HpImages;


    void Start()
    {
        ShowStartPanel();
        HandleReset();
    }

    void OnEnable()
    {
        GameEvent.OnGameWin += ShowWinPanel;
        GameEvent.OnGameLose += ShowLosePanel;

        GameEvent.OnGameRestart += HandleReset;
        inGameUI.OnScoreUpdate += HandleScoreUpdate;

        playerAttribute.OnHpUpdate += HandleHpUpdate;

        startButton.onClick.AddListener(HandleStartButton);
        winButton.onClick.AddListener(HandleGameWinButton);
        loseButton.onClick.AddListener(HandleGameLoseButton);
    }


    void OnDisable()
    {
        GameEvent.OnGameWin -= ShowWinPanel;
        GameEvent.OnGameLose -= ShowLosePanel;

        GameEvent.OnGameRestart -= HandleReset;
        inGameUI.OnScoreUpdate -= HandleScoreUpdate;

        playerAttribute.OnHpUpdate -= HandleHpUpdate;

        startButton.onClick.RemoveListener(HandleStartButton);
        winButton.onClick.RemoveListener(HandleGameWinButton);
        loseButton.onClick.RemoveListener(HandleGameLoseButton);
    }

    #region 处理不同的页面显示与按钮

    private void HandleStartButton()
    {
        GameEvent.GameStart();
        startPanel.SetActive(false);
    }

    private void HandleGameWinButton()
    {
        GameEvent.GameRestart();
        winPanel.SetActive(false);
    }

    private void HandleGameLoseButton()
    {
        GameEvent.GameRestart();
        losePanel.SetActive(false);
    }

    private void ShowWinPanel()
    {
        winPanel.SetActive(true);
    }

    private void ShowLosePanel()
    {
        losePanel.SetActive(true);
    }

    private void ShowStartPanel()
    {
        startPanel.SetActive(true);
    }

    #endregion


    private void HandleScoreUpdate(int score)
    {
        InGameScoreText.text = "Score: " + score.ToString();
        winScoreText.text = "Your Score: " + score.ToString();
        loseScoreText.text = "Your Score: " + score.ToString();
    }


    private void HandleHpUpdate(int Hp)
    {
        for(int i = HpImages.Count - 1; i >= 0; i--)
        {
            if(i > Hp - 1) HpImages[i].enabled = false;   
            else HpImages[i].enabled = true;
        }
    }


    private void HandleReset()
    {
        HandleScoreUpdate(0);
        HandleHpUpdate(playerAttribute.maxHp);
    }
  
}
