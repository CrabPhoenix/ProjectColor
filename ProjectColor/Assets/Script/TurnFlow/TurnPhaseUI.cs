using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

/// <summary>
/// 创建并控制回合相关 UI，包括 End Turn 按钮和阶段提示字幕。
/// </summary>
public class TurnPhaseUI : MonoBehaviour
{
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private Button endPhaseButton;

    private Canvas turnCanvas;
    private GameObject phaseNoticePanel;
    private Text phaseNoticeText;
    private Coroutine phaseNoticeCoroutine;

    /// <summary>
    /// 初始化按钮、提示字幕、事件系统并绑定点击事件。
    /// </summary>
    private void Awake()
    {
        if(turnManager == null) turnManager = GetComponent<TurnManager>();
        if(turnManager == null) turnManager = FindFirstObjectByType<TurnManager>();

        EnsureEventSystem();
        if(endPhaseButton == null) endPhaseButton = CreateEndPhaseButton();
        CreatePhaseNoticePanel();

        endPhaseButton.onClick.AddListener(HandleEndPhaseButton);
    }

    /// <summary>
    /// 订阅回合阶段变化和阶段提示事件。
    /// </summary>
    private void OnEnable()
    {
        if(turnManager != null)
        {
            turnManager.OnPhaseChanged += HandlePhaseChanged;
            turnManager.OnPhaseNoticeRequested += HandlePhaseNoticeRequested;
        }
    }

    /// <summary>
    /// 取消订阅回合阶段变化和阶段提示事件。
    /// </summary>
    private void OnDisable()
    {
        if(turnManager != null)
        {
            turnManager.OnPhaseChanged -= HandlePhaseChanged;
            turnManager.OnPhaseNoticeRequested -= HandlePhaseNoticeRequested;
        }

        if(endPhaseButton != null)
        {
            endPhaseButton.onClick.RemoveListener(HandleEndPhaseButton);
        }
    }

    /// <summary>
    /// 点击按钮时结束玩家阶段。
    /// </summary>
    private void HandleEndPhaseButton()
    {
        if(turnManager != null)
        {
            turnManager.EndPlayerPhase();
        }
    }

    /// <summary>
    /// 根据当前阶段显示或隐藏按钮。
    /// </summary>
    private void HandlePhaseChanged(TurnPhase phase)
    {
        if(endPhaseButton != null)
        {
            endPhaseButton.gameObject.SetActive(phase == TurnPhase.Player);
        }
    }

    /// <summary>
    /// 显示敌方或中立阶段的中央字幕。
    /// </summary>
    private void HandlePhaseNoticeRequested(TurnPhase phase, float duration)
    {
        if(phaseNoticeText == null || phaseNoticePanel == null) return;

        if(phaseNoticeCoroutine != null)
        {
            StopCoroutine(phaseNoticeCoroutine);
        }

        phaseNoticeCoroutine = StartCoroutine(ShowPhaseNotice(phase, duration));
    }

    /// <summary>
    /// 显示阶段提示并在指定时间后隐藏。
    /// </summary>
    private IEnumerator ShowPhaseNotice(TurnPhase phase, float duration)
    {
        if(phase == TurnPhase.Ally)
        {
            phaseNoticeText.text = "Ally Phase";
            phaseNoticeText.color = Color.blue;
        }
        else if(phase == TurnPhase.Enemy)
        {
            phaseNoticeText.text = "Enemy Phase";
            phaseNoticeText.color = Color.red;
        }
        else if(phase == TurnPhase.Neutral)
        {
            phaseNoticeText.text = "Neutral Phase";
            phaseNoticeText.color = Color.yellow;
        }
        else
        {
            phaseNoticePanel.SetActive(false);
            yield break;
        }

        phaseNoticePanel.SetActive(true);
        yield return new WaitForSeconds(duration);
        phaseNoticePanel.SetActive(false);
    }

    /// <summary>
    /// 确保场景中存在可支持 Input System 的 UI 事件系统。
    /// </summary>
    private void EnsureEventSystem()
    {
        if(EventSystem.current != null)
        {
            StandaloneInputModule standaloneInputModule = EventSystem.current.GetComponent<StandaloneInputModule>();
            if(standaloneInputModule != null)
            {
                standaloneInputModule.enabled = false;
                Destroy(standaloneInputModule);
            }

            if(EventSystem.current.GetComponent<InputSystemUIInputModule>() == null)
            {
                EventSystem.current.gameObject.AddComponent<InputSystemUIInputModule>();
            }

            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<InputSystemUIInputModule>();
    }

    /// <summary>
    /// 自动创建右上角灰底黄字的 End Turn 按钮。
    /// </summary>
    private Button CreateEndPhaseButton()
    {
        turnCanvas = CreateTurnCanvas();

        GameObject buttonObject = new GameObject("End Turn");
        buttonObject.transform.SetParent(turnCanvas.transform, false);

        RectTransform buttonRect = buttonObject.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(1, 1);
        buttonRect.anchorMax = new Vector2(1, 1);
        buttonRect.pivot = new Vector2(1, 1);
        buttonRect.anchoredPosition = new Vector2(-20, -20);
        buttonRect.sizeDelta = new Vector2(150, 40);

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.35f, 0.35f, 0.35f, 1f);

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;

        Text text = CreateText(buttonObject.transform, "Text", "End Turn", 20, Color.yellow);
        RectTransform textRect = text.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        return button;
    }

    /// <summary>
    /// 创建中央阶段提示面板。
    /// </summary>
    private void CreatePhaseNoticePanel()
    {
        if(turnCanvas == null)
        {
            turnCanvas = endPhaseButton != null ? endPhaseButton.GetComponentInParent<Canvas>() : CreateTurnCanvas();
        }

        phaseNoticePanel = new GameObject("Phase Notice");
        phaseNoticePanel.transform.SetParent(turnCanvas.transform, false);

        RectTransform panelRect = phaseNoticePanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(460, 110);

        Image panelImage = phaseNoticePanel.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.75f);

        phaseNoticeText = CreateText(phaseNoticePanel.transform, "Text", string.Empty, 40, Color.white);
        RectTransform textRect = phaseNoticeText.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        phaseNoticePanel.SetActive(false);
    }

    /// <summary>
    /// 创建回合 UI 使用的画布。
    /// </summary>
    private Canvas CreateTurnCanvas()
    {
        GameObject canvasObject = new GameObject("TurnPhaseCanvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    /// <summary>
    /// 创建指定父物体下的 UI 文本。
    /// </summary>
    private Text CreateText(Transform parent, string objectName, string content, int fontSize, Color color)
    {
        GameObject textObject = new GameObject(objectName);
        textObject.transform.SetParent(parent, false);

        RectTransform textRect = textObject.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        Text text = textObject.AddComponent<Text>();
        text.text = content;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = color;
        text.fontSize = fontSize;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return text;
    }
}
