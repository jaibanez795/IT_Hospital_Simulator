using UnityEngine;

public class GlobalEventManager : MonoBehaviour
{
    public static GlobalEventManager Instance { get; private set; }

    [Header("Director Visit")]
    [SerializeField] float eventInterval = 60f;
    [SerializeField] float eventDuration = 15f;
    [SerializeField] float suspicionCheckInterval = 3f;
    [SerializeField] float suspiciousSospecha = 15f;
    [SerializeField] float hideZoneSospecha = 25f;

    GlobalEventType? activeEvent;
    float nextEventTime;
    float eventEndTime;
    float nextCheckTime;

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
        ScheduleNextEvent();
    }

    void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying)
        {
            return;
        }

        if (activeEvent == null && Time.time >= nextEventTime)
        {
            StartDirectorVisit();
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
        if (player == null)
        {
            return;
        }

        if (player.IsInHideZone)
        {
            player.AddSospecha(hideZoneSospecha);
            GameManager.Instance?.ShowTemporaryMessage("Te cacharon escondido durante la visita");
            return;
        }

        if (player.AppearsOccupied())
        {
            return;
        }

        player.AddSospecha(suspiciousSospecha);
        string playerLabel = player.PlayerIndex == 1 ? "J1" : "J2";
        GameManager.Instance?.ShowTemporaryMessage($"{playerLabel} se vio sospechoso durante la visita");
    }

    void ScheduleNextEvent()
    {
        nextEventTime = Time.time + eventInterval;
    }
}
