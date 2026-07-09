using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Ticket : MonoBehaviour, IInteractable
{
    [SerializeField] TicketType ticketType = TicketType.Cable;
    [SerializeField] TicketMode ticketMode = TicketMode.Solo;
    [SerializeField] int requiredPlayers = 1;
    [SerializeField] float tiempoLimite = 30f;

    readonly List<PlayerController> participatingPlayers = new List<PlayerController>();
    bool isInProgress;
    bool isCompleted;
    float timeLeft;
    bool timerStarted;

    public TicketType TicketType => ticketType;
    public TicketMode TicketMode => ticketMode;
    public int RequiredPlayers => requiredPlayers;
    public bool IsInProgress => isInProgress;
    public bool IsCompleted => isCompleted;

    void Awake()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    void Update()
    {
        if (isCompleted || !timerStarted)
        {
            return;
        }

        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f)
        {
            ExpireTicket();
        }
    }

    public void Configure(TicketType type, TicketMode mode, int playersRequired, float limitSeconds)
    {
        ticketType = type;
        ticketMode = mode;
        requiredPlayers = playersRequired;
        tiempoLimite = limitSeconds;
        timeLeft = limitSeconds;
        timerStarted = true;
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

        if (!CanJoin(player))
        {
            return;
        }

        JoinTicket(player);

        if (CanStart())
        {
            StartTicket();
            CompleteTicket(RollRandomResult());
        }
    }

    public void CompleteTicket(TicketResult result)
    {
        if (isCompleted)
        {
            return;
        }

        isCompleted = true;
        isInProgress = false;
        ApplyResult(result);

        string typeLabel = ticketType == TicketType.Cable ? "Cable" : "Router";
        GameManager.Instance?.ShowTemporaryMessage($"Ticket {typeLabel}: {result}");

        Destroy(gameObject);
    }

    public void ExpireTicket()
    {
        if (isCompleted)
        {
            return;
        }

        isCompleted = true;
        isInProgress = false;
        GameManager.Instance?.AddOperacion(-15f);
        GameManager.Instance?.ShowTemporaryMessage("Ticket expirado: -15 Operación");

        Destroy(gameObject);
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

    static void ApplyResult(TicketResult result)
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
        }
    }
}
