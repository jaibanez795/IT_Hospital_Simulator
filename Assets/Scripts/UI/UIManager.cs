using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Global")]
    [SerializeField] Text operacionText;
    [SerializeField] Text timerText;

    [Header("Players")]
    [SerializeField] Text j1StatsText;
    [SerializeField] Text j2StatsText;

    [Header("Ticket Queue")]
    [SerializeField] Text ticketQueueTitleText;
    [SerializeField] Text ticketQueueText;

    [Header("Relationships")]
    [SerializeField] Text relationshipsText;

    [Header("Messages")]
    [SerializeField] Text temporaryMessageText;
    [SerializeField] Text globalEventBannerText;
    [SerializeField] GameObject endScreenPanel;
    [SerializeField] Text endScreenTitleText;
    [SerializeField] Text endScreenReasonText;

    [Header("Legacy Fields (optional fallback)")]
    [SerializeField] Text estresText;
    [SerializeField] Text desempenoText;
    [SerializeField] Text sospechaJ1Text;
    [SerializeField] Text sospechaJ2Text;

    PlayerController player1;
    PlayerController player2;

    void Start()
    {
        PlayerController[] players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        foreach (PlayerController player in players)
        {
            if (player.PlayerIndex == 1)
            {
                player1 = player;
            }
            else if (player.PlayerIndex == 2)
            {
                player2 = player;
            }
        }

        if (endScreenPanel != null)
        {
            endScreenPanel.SetActive(false);
        }

        ClearTemporaryMessage();
        SetText(ticketQueueTitleText, "COLA DE TICKETS");
        ValidateReferences();
        RefreshAll();
    }

    void ValidateReferences()
    {
        if (operacionText == null)
        {
            Debug.LogWarning("UIManager: falta operacionText. Las métricas globales no se mostrarán.");
        }

        if (timerText == null)
        {
            Debug.LogWarning("UIManager: falta timerText. El temporizador no se mostrará.");
        }

        if (j1StatsText == null)
        {
            Debug.LogWarning("UIManager: falta j1StatsText. Las stats de J1 no se mostrarán.");
        }

        if (j2StatsText == null)
        {
            Debug.LogWarning("UIManager: falta j2StatsText. Las stats de J2 no se mostrarán.");
        }

        if (ticketQueueText == null)
        {
            Debug.LogWarning("UIManager: falta ticketQueueText. La cola de tickets no se mostrará.");
        }

        if (relationshipsText == null)
        {
            Debug.LogWarning("UIManager: falta relationshipsText. Las relaciones NPC no se mostrarán.");
        }

        if (temporaryMessageText == null)
        {
            Debug.LogWarning("UIManager: falta temporaryMessageText. Los mensajes temporales no se mostrarán.");
        }

        if (globalEventBannerText == null)
        {
            Debug.LogWarning("UIManager: falta globalEventBannerText. El banner de eventos globales no se mostrará.");
        }

        if (endScreenPanel == null)
        {
            Debug.LogWarning("UIManager: falta endScreenPanel. La pantalla final no se mostrará.");
        }

        if (endScreenTitleText == null || endScreenReasonText == null)
        {
            Debug.LogWarning("UIManager: faltan textos de pantalla final (endScreenTitleText o endScreenReasonText).");
        }

        if (player1 == null)
        {
            Debug.LogWarning("UIManager: no se encontró Player1 (PlayerIndex=1) en la escena.");
        }

        if (player2 == null)
        {
            Debug.LogWarning("UIManager: no se encontró Player2 (PlayerIndex=2) en la escena.");
        }
    }

    void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying)
        {
            return;
        }

        RefreshAll();
    }

    public void RefreshAll()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        RefreshGlobalStats(GameManager.Instance.Operacion, GameManager.Instance.TimeRemaining);
        RefreshPlayerPanels();
        RefreshTicketQueue();
        RefreshRelationships();
    }

    public void RefreshGlobalStats(float operacion, float timeRemaining)
    {
        SetText(operacionText, $"Operación: {operacion:0}");
        SetText(timerText, $"Tiempo: {Mathf.CeilToInt(timeRemaining)}s");
    }

    void RefreshPlayerPanels()
    {
        RefreshPlayerPanel(player1, j1StatsText, estresText, sospechaJ1Text);
        RefreshPlayerPanel(player2, j2StatsText, desempenoText, sospechaJ2Text);
    }

    static void RefreshPlayerPanel(PlayerController player, Text primaryText, Text legacyEstresText, Text legacySospechaText)
    {
        if (player == null || player.Stats == null)
        {
            SetText(primaryText, string.Empty);
            return;
        }

        PlayerStats stats = player.Stats;
        string panelText =
            $"{stats.GetLabel()} [{stats.GetStatusLabel()}]\n" +
            $"Estrés: {stats.Estres:0}  Sospecha: {stats.Sospecha:0}\n" +
            $"Desempeño: {stats.DesempenoVisible:0}  Actas: {stats.Actas}";

        SetText(primaryText, panelText);

        if (legacyEstresText != null && primaryText != legacyEstresText)
        {
            SetText(legacyEstresText, string.Empty);
        }

        if (legacySospechaText != null && primaryText != legacySospechaText)
        {
            SetText(legacySospechaText, string.Empty);
        }
    }

    public void RefreshTicketQueue()
    {
        if (ticketQueueText == null)
        {
            return;
        }

        Ticket[] tickets = FindObjectsByType<Ticket>(FindObjectsSortMode.None);
        if (tickets.Length == 0)
        {
            SetText(ticketQueueText, "(sin tickets activos)");
            return;
        }

        List<Ticket> sortedTickets = new List<Ticket>(tickets);
        sortedTickets.Sort(CompareTicketsForQueue);

        System.Text.StringBuilder builder = new System.Text.StringBuilder();
        for (int i = 0; i < sortedTickets.Count; i++)
        {
            builder.AppendLine(sortedTickets[i].GetQueueLine());
        }

        SetText(ticketQueueText, builder.ToString().TrimEnd());
    }

    public void RefreshRelationships()
    {
        if (relationshipsText == null)
        {
            return;
        }

        if (NPCManager.Instance == null)
        {
            SetText(relationshipsText, "Relaciones:\n(sin NPCManager)");
            return;
        }

        SetText(relationshipsText, NPCManager.Instance.GetAllRelationshipsSummary());
    }

    static int CompareTicketsForQueue(Ticket a, Ticket b)
    {
        int priorityCompare = GetPrioritySortValue(b.Priority).CompareTo(GetPrioritySortValue(a.Priority));
        if (priorityCompare != 0)
        {
            return priorityCompare;
        }

        return a.TimeLeft.CompareTo(b.TimeLeft);
    }

    static int GetPrioritySortValue(TicketPriority priority)
    {
        return priority switch
        {
            TicketPriority.Critical => 4,
            TicketPriority.High => 3,
            TicketPriority.Medium => 2,
            TicketPriority.Low => 1,
            _ => 0
        };
    }

    public void SetTemporaryMessage(string message)
    {
        SetText(temporaryMessageText, message);
    }

    public void ClearTemporaryMessage()
    {
        SetText(temporaryMessageText, string.Empty);
    }

    public void SetGlobalEventBanner(string message)
    {
        SetText(globalEventBannerText, message);
    }

    public void ClearGlobalEventBanner()
    {
        SetText(globalEventBannerText, string.Empty);
    }

    public void ShowEndScreen(GameState state, string reason)
    {
        if (endScreenPanel != null)
        {
            endScreenPanel.SetActive(true);
        }

        string title = state == GameState.Won ? "VICTORIA" : "DERROTA";
        SetText(endScreenTitleText, title);
        SetText(endScreenReasonText, reason);
        RefreshAll();
    }

    static void SetText(Text label, string value)
    {
        if (label != null)
        {
            label.text = value;
        }
    }
}
