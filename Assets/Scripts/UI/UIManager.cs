using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] Text operacionText;
    [SerializeField] Text estresText;
    [SerializeField] Text desempenoText;
    [SerializeField] Text timerText;
    [SerializeField] Text sospechaJ1Text;
    [SerializeField] Text sospechaJ2Text;

    [Header("Ticket Queue")]
    [SerializeField] Text ticketQueueTitleText;
    [SerializeField] Text ticketQueueText;

    [Header("Messages")]
    [SerializeField] Text temporaryMessageText;
    [SerializeField] Text globalEventBannerText;
    [SerializeField] GameObject endScreenPanel;
    [SerializeField] Text endScreenTitleText;
    [SerializeField] Text endScreenReasonText;

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
        RefreshTicketQueue();
    }

    void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying)
        {
            return;
        }

        RefreshStats(
            GameManager.Instance.Operacion,
            GameManager.Instance.Estres,
            GameManager.Instance.Desempeno,
            GameManager.Instance.TimeRemaining);

        RefreshSospecha();
        RefreshTicketQueue();
    }

    public void RefreshStats(float operacion, float estres, float desempeno, float timeRemaining)
    {
        SetText(operacionText, $"Operación: {operacion:0}");
        SetText(estresText, $"Estrés: {estres:0}");
        SetText(desempenoText, $"Desempeño: {desempeno:0}");
        SetText(timerText, $"Tiempo: {Mathf.CeilToInt(timeRemaining)}s");
    }

    void RefreshSospecha()
    {
        SetText(sospechaJ1Text, $"Sospecha J1: {(player1 != null ? player1.Sospecha : 0f):0}");
        SetText(sospechaJ2Text, $"Sospecha J2: {(player2 != null ? player2.Sospecha : 0f):0}");
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

        RefreshStats(
            GameManager.Instance.Operacion,
            GameManager.Instance.Estres,
            GameManager.Instance.Desempeno,
            GameManager.Instance.TimeRemaining);
        RefreshSospecha();
        RefreshTicketQueue();
    }

    static void SetText(Text label, string value)
    {
        if (label != null)
        {
            label.text = value;
        }
    }
}
