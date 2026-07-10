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
    NPCData requesterNpc;
    PlayerController lastTouchedPlayer;
    bool isInProgress;
    bool isCompleted;
    bool expiryPaused;
    float timeLeft;
    bool timerStarted;
    Transform cableVisualRoot;
    Transform routerVisualRoot;
    readonly List<MeshRenderer> priorityRenderers = new List<MeshRenderer>();
    Vector3 baseLocalScale = Vector3.one;

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
    public NPCData RequesterNpc => requesterNpc;

    void Awake()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
        baseLocalScale = transform.localScale;
        CacheVisualRoots();
    }

    void CacheVisualRoots()
    {
        cableVisualRoot = transform.Find("TicketVisual_Cable");
        routerVisualRoot = transform.Find("TicketVisual_Router");
        priorityRenderers.Clear();

        MeshRenderer rootRenderer = GetComponent<MeshRenderer>();
        if (rootRenderer != null)
        {
            priorityRenderers.Add(rootRenderer);
        }

        if (cableVisualRoot != null)
        {
            priorityRenderers.AddRange(cableVisualRoot.GetComponentsInChildren<MeshRenderer>(true));
        }

        if (routerVisualRoot != null)
        {
            priorityRenderers.AddRange(routerVisualRoot.GetComponentsInChildren<MeshRenderer>(true));
        }
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

    public void Configure(TicketSpawnData data, float limitSeconds, NPCData requester = null)
    {
        ticketTitle = data.ticketTitle;
        zoneName = data.zoneName;
        priority = data.priority;
        isCritical = data.isCritical;
        ticketType = data.ticketType;
        ticketMode = data.ticketMode;
        requiredPlayers = data.requiredPlayers;
        requesterNpc = requester;
        tiempoLimite = limitSeconds;
        timeLeft = limitSeconds;
        timerStarted = true;

        ApplyVisualPriority();
        ApplyTicketTypeVisual();
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
        requesterNpc = null;

        ApplyVisualPriority();
        ApplyTicketTypeVisual();
    }

    void ApplyTicketTypeVisual()
    {
        if (cableVisualRoot == null && routerVisualRoot == null)
        {
            CacheVisualRoots();
        }

        if (cableVisualRoot != null)
        {
            cableVisualRoot.gameObject.SetActive(ticketType == TicketType.Cable);
        }

        if (routerVisualRoot != null)
        {
            routerVisualRoot.gameObject.SetActive(ticketType == TicketType.Router);
        }
    }

    void ApplyVisualPriority()
    {
        if (priorityRenderers.Count == 0)
        {
            CacheVisualRoots();
        }

        if (priorityRenderers.Count == 0)
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

        float scaleMultiplier = 1f;
        if (isCritical)
        {
            color = new Color(1f, 0.15f, 0.15f);
            scaleMultiplier = 1.15f;
        }
        else
        {
            scaleMultiplier = priority switch
            {
                TicketPriority.Low => 0.85f,
                TicketPriority.Medium => 1f,
                TicketPriority.High => 1.1f,
                TicketPriority.Critical => 1.15f,
                _ => 1f
            };
        }

        for (int i = 0; i < priorityRenderers.Count; i++)
        {
            MeshRenderer renderer = priorityRenderers[i];
            if (renderer == null)
            {
                continue;
            }

            if (IsAccentRenderer(renderer))
            {
                continue;
            }

            renderer.material.color = color;
        }

        transform.localScale = baseLocalScale * scaleMultiplier;

        Transform label = transform.Find("TicketVisual_Cable/PriorityLabel")
            ?? transform.Find("TicketVisual_Router/PriorityLabel");
        if (label != null)
        {
            TextMesh textMesh = label.GetComponent<TextMesh>();
            if (textMesh != null)
            {
                textMesh.text = isCritical || priority == TicketPriority.Critical
                    ? "!!"
                    : priority == TicketPriority.High
                        ? "!"
                        : priority == TicketPriority.Low
                            ? "·"
                            : "!";
                textMesh.characterSize = isCritical ? 0.1f : 0.08f;
            }
        }
    }

    static bool IsAccentRenderer(MeshRenderer renderer)
    {
        string name = renderer.gameObject.name;
        return name.StartsWith("Cable") || name == "Led" || name == "Screen";
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
        lastTouchedPlayer = player;
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
        ApplyResult(result, GetResolver());

        string npcMessage = ApplyNpcResolveEffects(result, GetResolver());
        string message = ResolveCompletionMessage(result, customMessage, npcMessage);
        GameManager.Instance?.ShowTemporaryMessage(message);

        Destroy(gameObject);
    }

    string ResolveCompletionMessage(TicketResult result, string customMessage, string npcMessage)
    {
        if (!string.IsNullOrEmpty(customMessage))
        {
            return customMessage;
        }

        if (isCritical && result != TicketResult.Fallo)
        {
            return "Ticket crítico resuelto";
        }

        if (!string.IsNullOrEmpty(npcMessage))
        {
            return npcMessage;
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
        ApplyPriorityExpireEffects();
        ApplyExpirePlayerEffects();

        string npcMessage = ApplyNpcExpireEffects(lastTouchedPlayer);
        string message = !string.IsNullOrEmpty(npcMessage)
            ? npcMessage
            : isCritical || priority == TicketPriority.Critical
                ? $"Ticket crítico expiró: {ticketTitle}"
                : $"Ticket expirado: {ticketTitle}";

        GameManager.Instance?.ShowTemporaryMessage(message);

        Destroy(gameObject);
    }

    void ApplyPriorityExpireEffects()
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
                break;
            case TicketPriority.Critical:
                GameManager.Instance?.AddOperacion(-25f);
                break;
        }
    }

    void ApplyExpirePlayerEffects()
    {
        if (lastTouchedPlayer != null && lastTouchedPlayer.IsActive)
        {
            lastTouchedPlayer.AddSospecha(10f);
        }
    }

    int GetPlayerId(PlayerController player)
    {
        if (player == null)
        {
            return 0;
        }

        return player.Stats != null ? player.Stats.PlayerId : player.PlayerIndex;
    }

    void ApplyNpcRelationship(int playerId, int amount)
    {
        if (playerId <= 0 || requesterNpc == null || NPCManager.Instance == null)
        {
            return;
        }

        NPCManager.Instance.AddRelationship(playerId, requesterNpc.NpcId, amount);
    }

    string ApplyNpcResolveEffects(TicketResult result, PlayerController resolver)
    {
        if (requesterNpc == null || NPCManager.Instance == null)
        {
            return null;
        }

        string npcName = requesterNpc.DisplayName;
        int playerId = GetPlayerId(resolver);

        switch (result)
        {
            case TicketResult.Perfecto:
                ApplyNpcRelationship(playerId, 15);
                if (requesterNpc.Personality == NPCPersonality.Important && resolver?.Stats != null)
                {
                    resolver.Stats.AddDesempeno(5f);
                }

                return $"{npcName} quedó impresionado";

            case TicketResult.Aceptable:
                ApplyNpcRelationship(playerId, 10);
                return $"{npcName} quedó conforme";

            case TicketResult.ParcheRapido:
                ApplyNpcRelationship(playerId, 5);
                return $"{npcName} no entendió qué hiciste, pero jaló";

            case TicketResult.Fallo:
                ApplyNpcRelationship(playerId, -10);
                return $"{npcName} quedó molesto";
        }

        return null;
    }

    string ApplyNpcExpireEffects(PlayerController touchedPlayer)
    {
        if (requesterNpc == null || NPCManager.Instance == null)
        {
            return null;
        }

        int playerId = GetPlayerId(touchedPlayer);
        if (playerId <= 0)
        {
            // TODO: definir penalización de relación por expiración sin jugador asignado.
            return null;
        }

        string npcName = requesterNpc.DisplayName;
        ApplyNpcRelationship(playerId, -10);

        switch (requesterNpc.Personality)
        {
            case NPCPersonality.Friendly:
                ApplyNpcRelationship(playerId, -5);
                return $"{npcName} se decepcionó";

            case NPCPersonality.Snitch:
                ApplyNpcRelationship(playerId, -15);
                AddSospechaToAllActivePlayers(10f);
                return $"{npcName} te puso dedo";

            case NPCPersonality.Important:
                ApplyNpcRelationship(playerId, -15);
                GameManager.Instance?.AddOperacion(-5f);
                touchedPlayer?.Stats?.AddDesempeno(-5f);
                return $"{npcName} reportó el problema a dirección";

            case NPCPersonality.Neutral:
            default:
                return $"{npcName} volvió a levantar el ticket";
        }
    }

    static void AddSospechaToAllActivePlayers(float amount)
    {
        PlayerController[] players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        foreach (PlayerController player in players)
        {
            if (player != null && player.IsActive)
            {
                player.AddSospecha(amount);
            }
        }
    }

    PlayerController GetResolver()
    {
        return participatingPlayers.Count > 0 ? participatingPlayers[0] : null;
    }

    string GetCompletionMessage(TicketResult result)
    {
        if (result == TicketResult.Fallo && ticketType == TicketType.Cable)
        {
            return "Fallaste el cableado";
        }

        return $"[{GetPriorityLabel()}] {GetRequesterLabel()}{ticketTitle}: {result}";
    }

    public string GetQueueLine()
    {
        int secondsLeft = Mathf.CeilToInt(Mathf.Max(0f, timeLeft));
        string prefix = isCritical ? "[CRIT]" : $"[{GetPriorityLabel()}]";
        return $"{prefix} {GetRequesterLabel()}{ticketTitle} — {zoneName} — {secondsLeft}s";
    }

    string GetRequesterLabel()
    {
        if (requesterNpc == null)
        {
            return string.Empty;
        }

        return $"{requesterNpc.DisplayName}: ";
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

    void ApplyResult(TicketResult result, PlayerController resolver)
    {
        PlayerStats stats = resolver != null ? resolver.Stats : null;

        switch (result)
        {
            case TicketResult.Perfecto:
                GameManager.Instance?.AddOperacion(15f);
                stats?.AddDesempeno(18f);
                stats?.AddEstres(8f);
                break;
            case TicketResult.Aceptable:
                GameManager.Instance?.AddOperacion(10f);
                stats?.AddDesempeno(5f);
                stats?.AddEstres(5f);
                break;
            case TicketResult.ParcheRapido:
                GameManager.Instance?.AddOperacion(5f);
                stats?.AddDesempeno(1f);
                stats?.AddEstres(3f);
                break;
            case TicketResult.Fallo:
                GameManager.Instance?.AddOperacion(-10f);
                stats?.AddEstres(8f);
                stats?.AddSospecha(10f);
                break;
        }
    }
}
