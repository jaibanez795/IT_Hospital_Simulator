using UnityEngine;

public abstract class MinigameBase : MonoBehaviour
{
    protected MinigameManager Manager;
    protected PlayerController ActivePlayer;
    protected Ticket ActiveTicket;

    public void Initialize(MinigameManager manager)
    {
        Manager = manager;
    }

    public void StartMinigame(PlayerController player, Ticket ticket)
    {
        ActivePlayer = player;
        ActiveTicket = ticket;
        OnMinigameStarted();
    }

    protected abstract void OnMinigameStarted();

    protected void CompleteMinigame(TicketResult result)
    {
        Manager?.FinishMinigame(result);
    }

    protected void CancelMinigame()
    {
        Manager?.CancelMinigame();
    }

    public virtual void CloseMinigame()
    {
        ActivePlayer = null;
        ActiveTicket = null;
    }
}
