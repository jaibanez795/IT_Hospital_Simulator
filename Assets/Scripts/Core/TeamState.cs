using UnityEngine;

public class TeamState : MonoBehaviour
{
    [SerializeField] float operacion = 70f;
    [SerializeField] float duracionTurno = 300f;

    float timeRemaining;
    GameState gameState = GameState.Playing;
    string endReason = string.Empty;

    public float Operacion => operacion;
    public float TimeRemaining => timeRemaining;
    public GameState State => gameState;
    public string EndReason => endReason;
    public bool IsPlaying => gameState == GameState.Playing;

    public void Initialize()
    {
        timeRemaining = duracionTurno;
        gameState = GameState.Playing;
        endReason = string.Empty;
        operacion = Mathf.Clamp(operacion, 0f, 100f);
    }

    public void TickTime(float deltaTime)
    {
        if (gameState != GameState.Playing)
        {
            return;
        }

        timeRemaining -= deltaTime;
        if (timeRemaining < 0f)
        {
            timeRemaining = 0f;
        }
    }

    public void AddOperacion(float amount)
    {
        operacion = Mathf.Clamp(operacion + amount, 0f, 100f);
    }

    public void SetWon(string reason)
    {
        gameState = GameState.Won;
        endReason = reason;
    }

    public void SetLost(string reason)
    {
        gameState = GameState.Lost;
        endReason = reason;
    }

    public bool IsOperacionCollapsed()
    {
        return operacion <= 0f;
    }

    public bool IsTurnTimeUp()
    {
        return timeRemaining <= 0f;
    }
}
