using UnityEngine;

public class MinigameManager : MonoBehaviour
{
    public static MinigameManager Instance { get; private set; }

    [SerializeField] CableMinigame cableMinigame;

    MinigameBase activeMinigame;
    Ticket activeTicket;
    PlayerController activePlayer;

    public bool IsMinigameActive => activeMinigame != null;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (cableMinigame != null)
        {
            cableMinigame.Initialize(this);
        }
    }

    public bool TryStartMinigame(Ticket ticket, PlayerController player)
    {
        if (IsMinigameActive || ticket == null || player == null)
        {
            return false;
        }

        MinigameBase minigame = GetMinigameForTicket(ticket);
        if (minigame == null)
        {
            return false;
        }

        activeMinigame = minigame;
        activeTicket = ticket;
        activePlayer = player;

        activeTicket.SetExpiryPaused(true);
        activePlayer.SetMinigameLocked(true);

        activeMinigame.StartMinigame(player, ticket);
        return true;
    }

    public void FinishMinigame(TicketResult result)
    {
        Ticket ticket = activeTicket;
        CloseActiveMinigame();
        ClearActiveState();

        if (ticket != null)
        {
            ticket.CompleteTicket(result);
        }
    }

    public void CancelMinigame()
    {
        Ticket ticket = activeTicket;
        CloseActiveMinigame();

        if (ticket != null)
        {
            ticket.ResetInProgress();
        }

        ClearActiveState();
    }

    void CloseActiveMinigame()
    {
        if (activeMinigame != null)
        {
            activeMinigame.CloseMinigame();
        }

        if (activePlayer != null)
        {
            activePlayer.SetMinigameLocked(false);
        }

        if (activeTicket != null)
        {
            activeTicket.SetExpiryPaused(false);
        }
    }

    void ClearActiveState()
    {
        activeMinigame = null;
        activeTicket = null;
        activePlayer = null;
    }

    MinigameBase GetMinigameForTicket(Ticket ticket)
    {
        switch (ticket.TicketType)
        {
            case TicketType.Cable:
                return cableMinigame;
            default:
                return null;
        }
    }
}
