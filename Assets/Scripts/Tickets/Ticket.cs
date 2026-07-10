using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Ticket : MonoBehaviour, IInteractable
{
    [SerializeField] string ticketTitle = "Ticket sin título";
    [SerializeField] string zoneName = "Desconocida";
    [SerializeField] TicketPriority priority = TicketPriority.Medium;
    [SerializeField] bool isCritical;
    [SerializeField] TicketType ticketType = TicketType.Cable;
    [SerializeField] TicketMode ticketMode = TicketMode.Solo;
    [SerializeField] int requiredPlayers = 1;
    [SerializeField] float tiempoLimite = 30f;

    readonly List<PlayerController> participatingPlayers = new List<PlayerController>();
    bool isInProgress;
    bool isCompleted;
    bool expiryPaused;
    float timeLeft;
    bool timerStarted;
    MeshRenderer meshRenderer;

    public string TicketTitle => ticketTitle;
    public string ZoneName => zoneName;
    public TicketPriority Priority => priority;
    public bool IsCritical => isCritical;
    public float TimeLeft => timeLeft;
    public TicketType TicketType => ticketType;
    public TicketMode TicketMode => ticketMode;
    public int RequiredPlayers => requiredPlayers;
    public bool IsInProgress => isInProgress;
    public bool IsCompleted => isCompleted;

    void Awake()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
        meshRenderer = GetComponent<MeshRenderer>();
    }

    void Update()
    {
        if (isCompleted || !timerStarted || expiryPaused)
        {
            return;
        }

        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f)
        {
            ExpireTicket();
        }
    }

    public void Configure(TicketSpawnData data, float limitSeconds)
    {
        ticketTitle = data.ticketTitle;
        zoneName = data.zoneName;
        priority = data.priority;
        isCritical = data.isCritical;
        ticketType = data.ticketType;
        ticketMode = data.ticketMode;
        requiredPlayers = data.requiredPlayers;
        tiempoLimite = limitSeconds;
        timeLeft = limitSeconds;
        timerStarted = true;

        ApplyVisualPriority();
        NotifySpawned();
    }

    public void Configure(TicketType type, TicketMode mode, int playersRequired, float limitSeconds)
    {
        ticketType = type;
        ticketMode = mode;
        requiredPlayers = playersRequired;
        tiempoLimite = limitSeconds;
        timeLeft = limitSeconds;
        timerStarted = true;
        ticketTitle = type == TicketType.Cable ? "Ticket de cable" : "Ticket de router";
        zoneName = "Desconocida";
        priority = TicketPriority.Medium;
        isCritical = false;

        ApplyVisualPriority();
    }

    void ApplyVisualPriority()
    {
        if (meshRenderer == null)
        {
            return;
        }

        Color color = priority switch
        {
            TicketPriority.Low => new Color(0.85f, 0.85f, 0.35f),
            TicketPriority.Medium => new Color(1f, 0.9f, 0.2f),
            TicketPriority.High => new Color(1f, 0.55f, 0.15f),
            TicketPriority.Critical => new Color(1f, 0.2f, 0.2f),
            _ => new Color(1f, 0.9f, 0.2f)
        };

        if (isCritical)
        {
            color = new Color(1f, 0.15f, 0.15f);
            transform.localScale = Vector3.one * 1.05f;
        }

        meshRenderer.material.color = color;
    }

    void NotifySpawned()
    {
        if (isCritical)
        {
            GameManager.Instance?.ShowTemporaryMessage($"Nuevo ticket crítico: {ticketTitle}");
        }
    }

    public void SetExpiryPaused(bool paused)
    {
        expiryPaused = paused;
    }

    public void ResetInProgress()
    {
        isInProgress = false;
        participatingPlayers.Clear();
    }

    public bool CanJoin(PlayerController player)
    {
        if (isCompleted || player == null)
        {
            return false;
        }

        if (participatingPlayers.Contains(player))
        {
            return false;
        }

        if (participatingPlayers.Count >= requiredPlayers)
        {
            return false;
        }

        return true;
    }

    public void JoinTicket(PlayerController player)
    {
        if (!CanJoin(player))
        {
            return;
        }

        participatingPlayers.Add(player);
    }

    public void LeaveTicket(PlayerController player)
    {
        participatingPlayers.Remove(player);

        if (isInProgress && participatingPlayers.Count == 0)
        {
            isInProgress = false;
        }
    }

    public bool CanStart()
    {
        return !isCompleted && !isInProgress && participatingPlayers.Count >= requiredPlayers;
    }

    public void StartTicket()
    {
        if (!CanStart())
        {
            return;
        }

        isInProgress = true;
    }

    public void Interact(PlayerController player)
    {
        if (isCompleted)
        {
            return;
        }

        if (MinigameManager.Instance != null && MinigameManager.Instance.IsMinigameActive)
        {
            return;
        }

        if (!CanJoin(player))
        {
            return;
        }

        JoinTicket(player);

        if (!CanStart())
        {
            return;
        }

        StartTicket();

        if (!TryStartMinigame(player))
        {
            ResolveWithRandomFallback();
        }
    }

    bool TryStartMinigame(PlayerController player)
    {
        if (MinigameManager.Instance == null)
        {
            return false;
        }

        return MinigameManager.Instance.TryStartMinigame(this, player);
    }

    void ResolveWithRandomFallback()
    {
        CompleteTicket(RollRandomResult());
    }

    public void CompleteTicket(TicketResult result, string customMessage = null)
    {
        if (isCompleted)
        {
            return;
        }

        isCompleted = true;
        isInProgress = false;
        ApplyResult(result);

        string message = ResolveCompletionMessage(result, customMessage);
        GameManager.Instance?.ShowTemporaryMessage(message);

        Destroy(gameObject);
    }

    string ResolveCompletionMessage(TicketResult result, string customMessage)
    {
        if (!string.IsNullOrEmpty(customMessage))
        {
            return customMessage;
        }

        if (isCritical && result != TicketResult.Fallo)
        {
            return "Ticket crítico resuelto";
        }

        return GetCompletionMessage(result);
    }

    public void ExpireTicket()
    {
        if (isCompleted)
        {
            return;
        }

        isCompleted = true;
        isInProgress = false;
        ApplyExpireEffects();

        string message = isCritical || priority == TicketPriority.Critical
            ? $"Ticket crítico expiró: {ticketTitle}"
            : $"Ticket expirado: {ticketTitle}";

        GameManager.Instance?.ShowTemporaryMessage(message);

        Destroy(gameObject);
    }

    void ApplyExpireEffects()
    {
        switch (priority)
        {
            case TicketPriority.Low:
                GameManager.Instance?.AddOperacion(-5f);
                break;
            case TicketPriority.Medium:
                GameManager.Instance?.AddOperacion(-10f);
                break;
            case TicketPriority.High:
                GameManager.Instance?.AddOperacion(-18f);
                GameManager.Instance?.AddEstres(5f);
                break;
            case TicketPriority.Critical:
                GameManager.Instance?.AddOperacion(-25f);
                GameManager.Instance?.AddEstres(10f);
                break;
        }
    }

    string GetCompletionMessage(TicketResult result)
    {
        if (result == TicketResult.Fallo && ticketType == TicketType.Cable)
        {
            return "Fallaste el cableado";
        }

        return $"[{GetPriorityLabel()}] {ticketTitle}: {result}";
    }

    public string GetQueueLine()
    {
        int secondsLeft = Mathf.CeilToInt(Mathf.Max(0f, timeLeft));
        string prefix = isCritical ? "[CRIT]" : $"[{GetPriorityLabel()}]";
        return $"{prefix} {ticketTitle} — {zoneName} — {secondsLeft}s";
    }

    public string GetPriorityLabel()
    {
        return priority switch
        {
            TicketPriority.Low => "LOW",
            TicketPriority.Medium => "MED",
            TicketPriority.High => "HIGH",
            TicketPriority.Critical => "CRIT",
            _ => priority.ToString().ToUpper()
        };
    }

    static TicketResult RollRandomResult()
    {
        int roll = Random.Range(0, 3);
        return roll switch
        {
            0 => TicketResult.Perfecto,
            1 => TicketResult.Aceptable,
            _ => TicketResult.ParcheRapido
        };
    }

    void ApplyResult(TicketResult result)
    {
        switch (result)
        {
            case TicketResult.Perfecto:
                GameManager.Instance?.AddOperacion(15f);
                GameManager.Instance?.AddDesempeno(18f);
                GameManager.Instance?.AddEstres(8f);
                break;
            case TicketResult.Aceptable:
                GameManager.Instance?.AddOperacion(10f);
                GameManager.Instance?.AddDesempeno(5f);
                GameManager.Instance?.AddEstres(5f);
                break;
            case TicketResult.ParcheRapido:
                GameManager.Instance?.AddOperacion(5f);
                GameManager.Instance?.AddDesempeno(1f);
                GameManager.Instance?.AddEstres(3f);
                break;
            case TicketResult.Fallo:
                GameManager.Instance?.AddOperacion(-10f);
                GameManager.Instance?.AddEstres(8f);
                break;
        }
    }
}
