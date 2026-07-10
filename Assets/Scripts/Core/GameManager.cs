using System.Collections;
using UnityEngine;

public enum GameState
{
    Playing,
    Won,
    Lost
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] TeamState teamState;

    UIManager uiManager;
    Coroutine messageCoroutine;

    public float Operacion => teamState != null ? teamState.Operacion : 0f;
    public float TimeRemaining => teamState != null ? teamState.TimeRemaining : 0f;
    public GameState State => teamState != null ? teamState.State : GameState.Playing;
    public string EndReason => teamState != null ? teamState.EndReason : string.Empty;
    public bool IsPlaying => teamState != null && teamState.IsPlaying;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (teamState == null)
        {
            teamState = GetComponent<TeamState>();
        }

        if (teamState == null)
        {
            teamState = gameObject.AddComponent<TeamState>();
        }

        teamState.Initialize();
    }

    void Start()
    {
        uiManager = FindFirstObjectByType<UIManager>();
        RefreshUI();
    }

    void Update()
    {
        if (teamState == null || !teamState.IsPlaying)
        {
            return;
        }

        teamState.TickTime(Time.deltaTime);

        if (teamState.IsTurnTimeUp())
        {
            if (teamState.IsOperacionCollapsed())
            {
                Lose("Colapso hospitalario");
            }
            else if (HasAnyActivePlayer())
            {
                Win("Turno sobrevivido");
            }
            else
            {
                Lose("El equipo de IT desapareció");
            }

            return;
        }

        CheckTeamDefeatConditions();
        RefreshUI();
    }

    public void AddOperacion(float amount)
    {
        if (teamState == null)
        {
            return;
        }

        teamState.AddOperacion(amount);
        CheckTeamDefeatConditions();
        RefreshUI();
    }

    public void CheckTeamElimination()
    {
        CheckTeamDefeatConditions();
    }

    void CheckTeamDefeatConditions()
    {
        if (teamState == null || !teamState.IsPlaying)
        {
            return;
        }

        if (teamState.IsOperacionCollapsed())
        {
            Lose("Colapso hospitalario");
            return;
        }

        if (!HasAnyActivePlayer())
        {
            Lose("El equipo de IT desapareció");
        }
    }

    public static bool HasAnyActivePlayer()
    {
        PlayerStats[] players = FindObjectsByType<PlayerStats>(FindObjectsSortMode.None);
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].IsActive)
            {
                return true;
            }
        }

        return false;
    }

    public void ShowTemporaryMessage(string message, float duration = 3f)
    {
        if (uiManager == null)
        {
            uiManager = FindFirstObjectByType<UIManager>();
        }

        if (uiManager == null)
        {
            return;
        }

        if (messageCoroutine != null)
        {
            StopCoroutine(messageCoroutine);
        }

        uiManager.SetTemporaryMessage(message);
        messageCoroutine = StartCoroutine(ClearMessageAfterDelay(duration));
    }

    IEnumerator ClearMessageAfterDelay(float duration)
    {
        yield return new WaitForSeconds(duration);
        uiManager?.ClearTemporaryMessage();
        messageCoroutine = null;
    }

    void Win(string reason)
    {
        if (teamState == null || !teamState.IsPlaying)
        {
            return;
        }

        teamState.SetWon(reason);
        ShowEndScreen();
    }

    void Lose(string reason)
    {
        if (teamState == null || !teamState.IsPlaying)
        {
            return;
        }

        teamState.SetLost(reason);
        ShowEndScreen();
    }

    void ShowEndScreen()
    {
        RefreshUI();
        uiManager?.ShowEndScreen(teamState.State, teamState.EndReason);
    }

    public void RefreshUI()
    {
        uiManager?.RefreshAll();
    }
}
