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

    [Header("Medidores")]
    [SerializeField] float operacion = 70f;
    [SerializeField] float estres = 20f;
    [SerializeField] float desempeno = 30f;

    [Header("Turno")]
    [SerializeField] float duracionTurno = 180f;

    float timeRemaining;
    GameState gameState = GameState.Playing;
    string endReason = string.Empty;

    UIManager uiManager;
    Coroutine messageCoroutine;

    public float Operacion => operacion;
    public float Estres => estres;
    public float Desempeno => desempeno;
    public float TimeRemaining => timeRemaining;
    public GameState State => gameState;
    public string EndReason => endReason;
    public bool IsPlaying => gameState == GameState.Playing;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        timeRemaining = duracionTurno;
    }

    void Start()
    {
        uiManager = FindFirstObjectByType<UIManager>();
        RefreshUI();
    }

    void Update()
    {
        if (gameState != GameState.Playing)
        {
            return;
        }

        timeRemaining -= Time.deltaTime;
        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            Win("Turno sobrevivido");
            return;
        }

        CheckDefeatConditions();
        RefreshUI();
    }

    public void AddOperacion(float amount)
    {
        operacion = Mathf.Clamp(operacion + amount, 0f, 100f);
        CheckDefeatConditions();
        RefreshUI();
    }

    public void AddEstres(float amount)
    {
        estres = Mathf.Clamp(estres + amount, 0f, 100f);
        CheckDefeatConditions();
        RefreshUI();
    }

    public void AddDesempeno(float amount)
    {
        desempeno = Mathf.Clamp(desempeno + amount, 0f, 100f);
        CheckDefeatConditions();
        RefreshUI();
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

    void CheckDefeatConditions()
    {
        if (gameState != GameState.Playing)
        {
            return;
        }

        if (operacion <= 0f)
        {
            Lose("Colapso hospitalario");
        }
        else if (estres >= 100f)
        {
            Lose("Burnout");
        }
        else if (desempeno >= 100f)
        {
            Lose("Ascenso forzado");
        }
    }

    void Win(string reason)
    {
        if (gameState != GameState.Playing)
        {
            return;
        }

        gameState = GameState.Won;
        endReason = reason;
        ShowEndScreen();
    }

    void Lose(string reason)
    {
        if (gameState != GameState.Playing)
        {
            return;
        }

        gameState = GameState.Lost;
        endReason = reason;
        ShowEndScreen();
    }

    void ShowEndScreen()
    {
        RefreshUI();
        uiManager?.ShowEndScreen(gameState, endReason);
    }

    void RefreshUI()
    {
        uiManager?.RefreshStats(operacion, estres, desempeno, timeRemaining);
    }
}
