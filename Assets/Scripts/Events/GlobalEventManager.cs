using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

public class GlobalEventManager : MonoBehaviour
{
    public static GlobalEventManager Instance { get; private set; }

    [Header("Director Visit")]
    [SerializeField] float eventInterval = 60f;
    [SerializeField] float eventDuration = 15f;
    [SerializeField] float suspicionCheckInterval = 3f;
    [SerializeField] float suspiciousSospecha = 15f;
    [SerializeField] float hideZoneSospecha = 25f;

    [Header("Friendly Tip")]
    [SerializeField] int friendlyTipThreshold = 25;
    [SerializeField] float directorVisitWarningSeconds = 5f;

    [Header("Debug — activar antes de Play")]
    [Tooltip("F7=J1+Paty | F8=J2+Carlos | F9=Visita en 8s | F10=Limpiar relaciones Friendly")]
    [SerializeField] bool enableDebugKeys;

    GlobalEventType? activeEvent;
    float nextEventTime;
    float eventEndTime;
    float nextCheckTime;
    bool friendlyTipsSentForCurrentEvent;

    public bool IsDirectorVisitActive => activeEvent == GlobalEventType.DirectorVisit;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        ValidateReferences();
        ScheduleNextEvent();
    }

    void ValidateReferences()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GlobalEventManager: no se encontró GameManager. Los eventos globales no se evaluarán.");
        }

        if (FindFirstObjectByType<UIManager>() == null)
        {
            Debug.LogWarning("GlobalEventManager: no se encontró UIManager. El banner de eventos no se mostrará.");
        }

        if (NPCManager.Instance == null)
        {
            Debug.LogWarning("GlobalEventManager: no se encontró NPCManager. Los pitazos Friendly no funcionarán.");
        }

        PlayerController[] players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        if (players.Length == 0)
        {
            Debug.LogWarning("GlobalEventManager: no hay jugadores en la escena. DirectorVisit no podrá penalizar sospecha.");
        }
        else if (players.Length < 2)
        {
            Debug.LogWarning("GlobalEventManager: se encontró menos de 2 jugadores. El prototipo local co-op espera J1 y J2.");
        }
    }

    void Update()
    {
        if (enableDebugKeys)
        {
            HandleDebugKeys();
        }

        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying)
        {
            return;
        }

        if (activeEvent == null)
        {
            TrySendFriendlyDirectorTips();

            if (Time.time >= nextEventTime)
            {
                StartDirectorVisit();
            }
        }

        if (activeEvent == GlobalEventType.DirectorVisit)
        {
            if (Time.time >= eventEndTime)
            {
                EndDirectorVisit();
                return;
            }

            if (Time.time >= nextCheckTime)
            {
                CheckDirectorVisitSuspicion();
                nextCheckTime = Time.time + suspicionCheckInterval;
            }
        }
    }

    void TrySendFriendlyDirectorTips()
    {
        if (friendlyTipsSentForCurrentEvent)
        {
            return;
        }

        float warningTime = nextEventTime - directorVisitWarningSeconds;
        if (Time.time < warningTime || Time.time >= nextEventTime)
        {
            return;
        }

        SendFriendlyDirectorTips();
        friendlyTipsSentForCurrentEvent = true;
    }

    void SendFriendlyDirectorTips()
    {
        if (NPCManager.Instance == null)
        {
            return;
        }

        PlayerController[] players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        StringBuilder tipsBuilder = new StringBuilder();

        foreach (PlayerController player in players)
        {
            if (player == null || player.Stats == null)
            {
                continue;
            }

            int playerId = player.Stats.PlayerId;
            NPCData tipNpc = NPCManager.Instance.GetFriendlyTipNpcForPlayer(playerId, friendlyTipThreshold);
            if (tipNpc == null)
            {
                continue;
            }

            string playerLabel = player.Stats.GetLabel();
            string tipMessage = NPCManager.Instance.GetDirectorVisitTipMessage(tipNpc, playerLabel);
            if (string.IsNullOrEmpty(tipMessage))
            {
                continue;
            }

            if (tipsBuilder.Length > 0)
            {
                tipsBuilder.AppendLine();
            }

            tipsBuilder.Append(tipMessage);
        }

        if (tipsBuilder.Length == 0)
        {
            return;
        }

        float messageDuration = Mathf.Max(directorVisitWarningSeconds + 1f, 6f);
        GameManager.Instance?.ShowTemporaryMessage($"PITAZO: {tipsBuilder}", messageDuration);
    }

    void StartDirectorVisit()
    {
        activeEvent = GlobalEventType.DirectorVisit;
        eventEndTime = Time.time + eventDuration;
        nextCheckTime = Time.time + suspicionCheckInterval;

        UIManager uiManager = FindFirstObjectByType<UIManager>();
        uiManager?.SetGlobalEventBanner("EVENTO: VISITA DEL DIRECTOR");
        GameManager.Instance?.ShowTemporaryMessage("Visita del director: parezcan ocupados");
    }

    void EndDirectorVisit()
    {
        activeEvent = null;
        ScheduleNextEvent();

        UIManager uiManager = FindFirstObjectByType<UIManager>();
        uiManager?.ClearGlobalEventBanner();
        GameManager.Instance?.ShowTemporaryMessage("El director se fue");
    }

    void CheckDirectorVisitSuspicion()
    {
        PlayerController[] players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        foreach (PlayerController player in players)
        {
            EvaluatePlayerDuringVisit(player);
        }
    }

    void EvaluatePlayerDuringVisit(PlayerController player)
    {
        if (player == null || !player.IsActive)
        {
            return;
        }

        string playerLabel = player.Stats != null ? player.Stats.GetLabel() : $"J{player.PlayerIndex}";

        if (player.IsInHideZone)
        {
            player.AddSospecha(hideZoneSospecha);
            GameManager.Instance?.ShowTemporaryMessage($"{playerLabel} fue cachado escondido durante la visita");
            return;
        }

        if (player.AppearsOccupied())
        {
            return;
        }

        player.AddSospecha(suspiciousSospecha);
        GameManager.Instance?.ShowTemporaryMessage($"{playerLabel} se vio sospechoso durante la visita");
    }

    void ScheduleNextEvent()
    {
        nextEventTime = Time.time + eventInterval;
        friendlyTipsSentForCurrentEvent = false;
    }

    void HandleDebugKeys()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        if (NPCManager.Instance == null)
        {
            return;
        }

        if (keyboard.f7Key.wasPressedThisFrame)
        {
            NPCManager.Instance.SetRelationship(1, "paty", 25);
            GameManager.Instance?.ShowTemporaryMessage("DEBUG: J1 ahora tiene +25 con Paty");
        }
        else if (keyboard.f8Key.wasPressedThisFrame)
        {
            NPCManager.Instance.SetRelationship(2, "carlos", 25);
            GameManager.Instance?.ShowTemporaryMessage("DEBUG: J2 ahora tiene +25 con Carlos");
        }
        else if (keyboard.f9Key.wasPressedThisFrame)
        {
            ScheduleDirectorVisitIn(8f);
            GameManager.Instance?.ShowTemporaryMessage("DEBUG: Visita del director en 8s (pitazo en ~3s)");
        }
        else if (keyboard.f10Key.wasPressedThisFrame)
        {
            NPCManager.Instance.SetRelationship(1, "paty", 0);
            NPCManager.Instance.SetRelationship(1, "carlos", 0);
            NPCManager.Instance.SetRelationship(2, "paty", 0);
            NPCManager.Instance.SetRelationship(2, "carlos", 0);
            GameManager.Instance?.ShowTemporaryMessage("DEBUG: Relaciones Friendly limpiadas");
        }
    }

    public void ScheduleDirectorVisitIn(float secondsFromNow)
    {
        nextEventTime = Time.time + Mathf.Max(1f, secondsFromNow);
        friendlyTipsSentForCurrentEvent = false;
    }

    [ContextMenu("Debug/Schedule Director Visit In 8s")]
    void DebugScheduleDirectorVisitSoon()
    {
        ScheduleDirectorVisitIn(8f);
    }
}
