using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class CableMinigame : MinigameBase
{
    const float TimeLimit = 6f;
    const float PerfectThreshold = 2.5f;

    [Header("UI")]
    [SerializeField] GameObject panelRoot;
    [SerializeField] Text titleText;
    [SerializeField] Text instructionText;
    [SerializeField] Text timerText;
    [SerializeField] Text feedbackText;
    [SerializeField] Button cableAButton;
    [SerializeField] Button cableBButton;
    [SerializeField] Button cableCButton;

    int correctCableIndex = -1;
    float elapsedTime;
    bool awaitingInput;
    bool finished;

    void Awake()
    {
        EnsureEventSystem();
        WireButtonListeners();
        DisableTextRaycasts();
        HidePanel();
    }

    void Update()
    {
        if (!awaitingInput || finished)
        {
            return;
        }

        elapsedTime += Time.deltaTime;
        UpdateTimerDisplay();

        if (elapsedTime >= TimeLimit)
        {
            FinishWithResult(TicketResult.Fallo);
            return;
        }

        HandleKeyboardInput();
    }

    protected override void OnMinigameStarted()
    {
        finished = false;
        awaitingInput = true;
        elapsedTime = 0f;
        correctCableIndex = Random.Range(0, 3);

        SetText(titleText, "Seguir cable");
        SetText(instructionText, "Elige: teclas 1/2/3 o click en botones");
        SetText(feedbackText, string.Empty);
        UpdateTimerDisplay();

        WireButtonListeners();
        DisableTextRaycasts();
        EnsureEventSystem();
        ShowPanel();
    }

    public override void CloseMinigame()
    {
        awaitingInput = false;
        finished = false;
        HidePanel();
        base.CloseMinigame();
    }

    void WireButtonListeners()
    {
        if (cableAButton != null)
        {
            cableAButton.onClick.RemoveAllListeners();
            cableAButton.onClick.AddListener(() => OnCableSelected(0));
        }

        if (cableBButton != null)
        {
            cableBButton.onClick.RemoveAllListeners();
            cableBButton.onClick.AddListener(() => OnCableSelected(1));
        }

        if (cableCButton != null)
        {
            cableCButton.onClick.RemoveAllListeners();
            cableCButton.onClick.AddListener(() => OnCableSelected(2));
        }
    }

    void HandleKeyboardInput()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        if (keyboard.digit1Key.wasPressedThisFrame || keyboard.numpad1Key.wasPressedThisFrame)
        {
            OnCableSelected(0);
        }
        else if (keyboard.digit2Key.wasPressedThisFrame || keyboard.numpad2Key.wasPressedThisFrame)
        {
            OnCableSelected(1);
        }
        else if (keyboard.digit3Key.wasPressedThisFrame || keyboard.numpad3Key.wasPressedThisFrame)
        {
            OnCableSelected(2);
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
        SetRaycastTarget(timerText, false);
        SetRaycastTarget(feedbackText, false);

        DisableButtonLabelRaycasts(cableAButton);
        DisableButtonLabelRaycasts(cableBButton);
        DisableButtonLabelRaycasts(cableCButton);
    }

    static void DisableButtonLabelRaycasts(Button button)
    {
        if (button == null)
        {
            return;
        }

        Text label = button.GetComponentInChildren<Text>();
        SetRaycastTarget(label, false);
    }

    static void SetRaycastTarget(Text label, bool enabled)
    {
        if (label != null)
        {
            label.raycastTarget = enabled;
        }
    }

    void OnCableSelected(int selectedIndex)
    {
        if (!awaitingInput || finished)
        {
            return;
        }

        if (selectedIndex == correctCableIndex)
        {
            TicketResult result = elapsedTime < PerfectThreshold
                ? TicketResult.Perfecto
                : TicketResult.Aceptable;
            FinishWithResult(result);
            return;
        }

        FinishWithResult(TicketResult.ParcheRapido);
    }

    void FinishWithResult(TicketResult result)
    {
        if (finished)
        {
            return;
        }

        finished = true;
        awaitingInput = false;

        SetText(feedbackText, GetFeedbackMessage(result));
        CompleteMinigame(result);
    }

    static string GetFeedbackMessage(TicketResult result)
    {
        return result switch
        {
            TicketResult.Perfecto => "Cable correcto. Rápido y limpio.",
            TicketResult.Aceptable => "Cable correcto. Tarde, pero funciona.",
            TicketResult.ParcheRapido => "Cable equivocado. Parche improvisado.",
            TicketResult.Fallo => "Tiempo agotado. Caos en el cableado.",
            _ => string.Empty
        };
    }

    void UpdateTimerDisplay()
    {
        float remaining = Mathf.Max(0f, TimeLimit - elapsedTime);
        SetText(timerText, $"Tiempo: {remaining:0.0}s");
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
