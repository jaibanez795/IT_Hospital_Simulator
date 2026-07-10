using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TicketSpawner : MonoBehaviour
{
    const string DefaultTicketPrefabPath = "Assets/Prefabs/Ticket.prefab";

    [SerializeField] Ticket ticketPrefab;
    [SerializeField] Transform[] spawnPoints;
    [SerializeField] float minSpawnInterval = 8f;
    [SerializeField] float maxSpawnInterval = 14f;
    [SerializeField] float ticketTimeLimit = 30f;
    [SerializeField] int maxActiveTickets = 5;
    [SerializeField] float spawnHeightOffset = 0.6f;
    [SerializeField] float firstSpawnDelay = 2f;

    [Header("Debug")]
    [SerializeField] bool useDebugNpc;
    [SerializeField] string debugNpcId = "dr_ramirez";

    float nextSpawnTime;

    void Awake()
    {
        ResolveMissingPrefabReference();
    }

    void Start()
    {
        ValidateReferences();
        nextSpawnTime = Time.time + firstSpawnDelay;
    }

    void ValidateReferences()
    {
        if (ticketPrefab == null)
        {
            Debug.LogError("TicketSpawner: falta Ticket Prefab. Arrastra Assets/Prefabs/Ticket.prefab al campo Ticket Prefab.");
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("TicketSpawner: no hay Spawn Points asignados. Los tickets no pueden aparecer.");
            return;
        }

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawnPoints[i] == null)
            {
                Debug.LogWarning($"TicketSpawner: spawnPoints[{i}] es null. Ese punto será ignorado al spawnear.");
            }
        }

        if (NPCManager.Instance == null)
        {
            Debug.LogWarning("TicketSpawner: no se encontró NPCManager. Los tickets aparecerán sin requester NPC.");
        }
    }

    void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
        {
            return;
        }

        if (ticketPrefab == null || spawnPoints == null || spawnPoints.Length == 0)
        {
            return;
        }

        if (Time.time < nextSpawnTime)
        {
            return;
        }

        if (CountActiveTickets() >= maxActiveTickets)
        {
            ScheduleNextSpawn();
            return;
        }

        SpawnTicket();
        ScheduleNextSpawn();
    }

    void ResolveMissingPrefabReference()
    {
        if (ticketPrefab != null)
        {
            return;
        }

#if UNITY_EDITOR
        ticketPrefab = AssetDatabase.LoadAssetAtPath<Ticket>(DefaultTicketPrefabPath);
#endif
    }

    void ScheduleNextSpawn()
    {
        nextSpawnTime = Time.time + Random.Range(minSpawnInterval, maxSpawnInterval);
    }

    int CountActiveTickets()
    {
        return FindObjectsByType<Ticket>(FindObjectsSortMode.None).Length;
    }

    void SpawnTicket()
    {
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        string zoneName = TicketDefinitionLibrary.GetZoneNameFromSpawnPoint(spawnPoint);
        TicketSpawnData definition = TicketDefinitionLibrary.GetRandomForZone(zoneName);
        Vector3 spawnPosition = spawnPoint.position + Vector3.up * spawnHeightOffset;

        Ticket ticket = Instantiate(ticketPrefab, spawnPosition, Quaternion.identity);

        NPCData requester = ResolveRequesterNpc(zoneName, definition.ticketTitle);
        ticket.Configure(definition, ticketTimeLimit, requester);
        ticket.name = $"Ticket_{definition.priority}_{spawnPoint.name}";
    }

    NPCData ResolveRequesterNpc(string zoneName, string ticketTitle)
    {
        if (NPCManager.Instance == null)
        {
            return null;
        }

        if (useDebugNpc)
        {
            NPCData debugNpc = NPCManager.Instance.GetNpcById(debugNpcId);
            if (debugNpc != null)
            {
                return debugNpc;
            }
        }

        return NPCManager.Instance.GetNpcForZone(zoneName, ticketTitle);
    }
}
