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

    float nextSpawnTime;
    bool warnedMissingPrefab;

    void Awake()
    {
        ResolveMissingPrefabReference();
    }

    void Start()
    {
        nextSpawnTime = Time.time + firstSpawnDelay;
    }

    void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
        {
            return;
        }

        if (ticketPrefab == null)
        {
            if (!warnedMissingPrefab)
            {
                Debug.LogWarning("TicketSpawner: falta el Ticket Prefab. Arrastra Assets/Prefabs/Ticket.prefab al campo Ticket Prefab.");
                warnedMissingPrefab = true;
            }

            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            if (!warnedMissingPrefab)
            {
                Debug.LogWarning("TicketSpawner: no hay Spawn Points asignados.");
                warnedMissingPrefab = true;
            }

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
        ticket.Configure(definition, ticketTimeLimit);
        ticket.name = $"Ticket_{definition.priority}_{spawnPoint.name}";
    }
}
