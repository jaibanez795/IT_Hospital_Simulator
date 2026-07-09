using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class RouterMinigame : MinigameBase
{
    const float TimeLimit = 6f;
    const float MinIdeal = 2f;
    const float MaxIdeal = 3.5f;
    const float PerfectMin = 2.6f;
    const float PerfectMax = 3.2f;

    [Header("UI")]
    [SerializeField] GameObject panelRoot;
    [SerializeField] Text titleText;
    [SerializeField] Text instructionText;
    [SerializeField] Text holdTimeText;
    [SerializeField] Text idealZoneText;
    [SerializeField] Text feedbackText;
    [SerializeField] Image holdProgressBar;
    [SerializeField] Button holdButton;

    float minigameElapsed;
    float currentHoldDuration;
    bool isHolding;
    bool uiHoldActive;
    bool finished;
    bool active;

    void Awake()
    {
        EnsureEventSystem();
        WireHoldButton();
        DisableTextRaycasts();
        HidePanel();
    }

    void Update()
    {
        if (!active || finished)
        {
            return;
        }

        minigameElapsed += Time.deltaTime;
        bool holdInputActive = IsHoldInputActive();

        if (holdInputActive && !isHolding)
        {
            isHolding = true;
            currentHoldDuration = 0f;
        }

        if (isHolding)
        {
            currentHoldDuration += Time.deltaTime;
            UpdateHoldDisplay();

            if (minigameElapsed >= TimeLimit)
            {
                FinishWithResult(TicketResult.Fallo, "Reseteaste de fábrica");
                return;
            }
        }

        if (!holdInputActive && isHolding)
        {
            isHolding = false;
            EvaluateRelease(currentHoldDuration);
            return;
        }

        if (!isHolding && minigameElapsed >= TimeLimit)
        {
            FinishWithResult(TicketResult.Fallo, "No alcanzó a reiniciar");
        }
    }

    protected override void OnMinigameStarted()
    {
        active = true;
        finished = false;
        isHolding = false;
        uiHoldActive = false;
        minigameElapsed = 0f;
        currentHoldDuration = 0f;

        SetText(titleText, "Reiniciar router");
        SetText(instructionText, "Mantén presionado el botón. Suelta en la zona correcta.");
        SetText(idealZoneText, "Zona ideal: 2.0s - 3.5s (perfecto: 2.6s - 3.2s)");
        SetText(feedbackText, string.Empty);
        SetText(holdTimeText, "Tiempo presionado: 0.0s");
        SetProgress(0f);

        WireHoldButton();
        DisableTextRaycasts();
        EnsureEventSystem();
        ShowPanel();
    }

    public override void CloseMinigame()
    {
        active = false;
        finished = false;
        isHolding = false;
        uiHoldActive = false;
        HidePanel();
        base.CloseMinigame();
    }

    public void SetUiHold(bool held)
    {
        if (!active || finished)
        {
            return;
        }

        uiHoldActive = held;
    }

    bool IsHoldInputActive()
    {
        Keyboard keyboard = Keyboard.current;
        bool spaceHeld = keyboard != null && keyboard.spaceKey.isPressed;
        return uiHoldActive || spaceHeld;
    }

    void WireHoldButton()
    {
        if (holdButton == null)
        {
            return;
        }

        EventTrigger trigger = holdButton.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = holdButton.gameObject.AddComponent<EventTrigger>();
        }

        trigger.triggers.Clear();

        AddTriggerEntry(trigger, EventTriggerType.PointerDown, _ => SetUiHold(true));
        AddTriggerEntry(trigger, EventTriggerType.PointerUp, _ => SetUiHold(false));
        AddTriggerEntry(trigger, EventTriggerType.PointerExit, _ => SetUiHold(false));
    }

    static void AddTriggerEntry(EventTrigger trigger, EventTriggerType type, UnityEngine.Events.UnityAction<BaseEventData> action)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry { eventID = type };
        entry.callback.AddListener(action);
        trigger.triggers.Add(entry);
    }

    void EvaluateRelease(float holdDuration)
    {
        if (holdDuration < MinIdeal)
        {
            FinishWithResult(TicketResult.Fallo, "No alcanzó a reiniciar");
            return;
        }

        if (holdDuration > MaxIdeal)
        {
            FinishWithResult(TicketResult.ParcheRapido, "Lo reseteaste raro, pero jaló");
            return;
        }

        if (holdDuration >= PerfectMin && holdDuration <= PerfectMax)
        {
            FinishWithResult(TicketResult.Perfecto, "Reinicio impecable");
            return;
        }

        FinishWithResult(TicketResult.Aceptable, "Reinicio suficiente");
    }

    void FinishWithResult(TicketResult result, string message)
    {
        if (finished)
        {
            return;
        }

        finished = true;
        active = false;
        isHolding = false;
        uiHoldActive = false;

        SetText(feedbackText, message);
        CompleteMinigame(result, message);
    }

    void UpdateHoldDisplay()
    {
        SetText(holdTimeText, $"Tiempo presionado: {currentHoldDuration:0.0}s");
        SetProgress(Mathf.Clamp01(currentHoldDuration / MaxIdeal));
    }

    void SetProgress(float normalized)
    {
        if (holdProgressBar != null)
        {
            holdProgressBar.fillAmount = normalized;
        }
    }

    void EnsureEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<InputSystemUIInputModule>();
    }

    void DisableTextRaycasts()
    {
        SetRaycastTarget(titleText, false);
        SetRaycastTarget(instructionText, false);
        SetRaycastTarget(holdTimeText, false);
        SetRaycastTarget(idealZoneText, false);
        SetRaycastTarget(feedbackText, false);

        if (holdButton != null)
        {
            Text label = holdButton.GetComponentInChildren<Text>();
            SetRaycastTarget(label, false);
        }
    }

    static void SetRaycastTarget(Text label, bool enabled)
    {
        if (label != null)
        {
            label.raycastTarget = enabled;
        }
    }

    void ShowPanel()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }
    }

    void HidePanel()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }

    static void SetText(Text label, string value)
    {
        if (label != null)
        {
            label.text = value;
        }
    }
}
