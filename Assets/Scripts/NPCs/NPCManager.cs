using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
    public static NPCManager Instance { get; private set; }

    readonly List<NPCData> npcs = new List<NPCData>();
    readonly Dictionary<int, Dictionary<string, int>> playerRelationships = new Dictionary<int, Dictionary<string, int>>();

    public IReadOnlyList<NPCData> AllNpcs => npcs;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        InitializeDefaultNpcs();
    }

    void InitializeDefaultNpcs()
    {
        if (npcs.Count > 0)
        {
            return;
        }

        npcs.Add(new NPCData("paty", "Paty", "Recepción", NPCPersonality.Friendly));
        npcs.Add(new NPCData("dr_ramirez", "Dr. Ramírez", "Urgencias", NPCPersonality.Important));
        npcs.Add(new NPCData("mendoza", "Mendoza", "Dirección", NPCPersonality.Snitch));
        npcs.Add(new NPCData("carlos", "Carlos", "Almacén", NPCPersonality.Friendly));
        npcs.Add(new NPCData("mariana", "Mariana", "RH", NPCPersonality.Neutral));
    }

    public NPCData GetRandomNpc()
    {
        if (npcs.Count == 0)
        {
            return null;
        }

        return npcs[Random.Range(0, npcs.Count)];
    }

    public NPCData GetNpcById(string npcId)
    {
        if (string.IsNullOrEmpty(npcId))
        {
            return null;
        }

        for (int i = 0; i < npcs.Count; i++)
        {
            if (npcs[i].NpcId == npcId)
            {
                return npcs[i];
            }
        }

        return null;
    }

    public NPCData GetNpcForZone(string zoneName, string ticketTitle = null)
    {
        if (!string.IsNullOrEmpty(ticketTitle) && ticketTitle.Contains("Dirección"))
        {
            NPCData mendoza = GetNpcById("mendoza");
            if (mendoza != null)
            {
                return mendoza;
            }
        }

        List<NPCData> matches = GetNpcsMatchingZone(zoneName);
        if (matches.Count > 0)
        {
            return matches[Random.Range(0, matches.Count)];
        }

        return GetRandomNpc();
    }

    List<NPCData> GetNpcsMatchingZone(string zoneName)
    {
        List<NPCData> matches = new List<NPCData>();

        for (int i = 0; i < npcs.Count; i++)
        {
            if (NpcMatchesZone(npcs[i], zoneName))
            {
                matches.Add(npcs[i]);
            }
        }

        return matches;
    }

    static bool NpcMatchesZone(NPCData npc, string zoneName)
    {
        if (npc == null || string.IsNullOrEmpty(zoneName))
        {
            return false;
        }

        return zoneName switch
        {
            "Recepción" => npc.Department == "Recepción" || npc.Department == "Dirección",
            "Urgencias" => npc.Department == "Urgencias" || npc.Department == "RH",
            "Cuarto Servidores" => npc.Department == "Almacén" || npc.Department == "RH",
            _ => npc.Department == zoneName
        };
    }

    public void AddRelationship(int playerId, string npcId, int amount)
    {
        if (string.IsNullOrEmpty(npcId))
        {
            return;
        }

        EnsurePlayerRelationships(playerId);
        int current = playerRelationships[playerId][npcId];
        playerRelationships[playerId][npcId] = Mathf.Clamp(current + amount, -100, 100);
    }

    public int GetRelationship(int playerId, string npcId)
    {
        if (string.IsNullOrEmpty(npcId))
        {
            return 0;
        }

        EnsurePlayerRelationships(playerId);
        return playerRelationships[playerId][npcId];
    }

    void EnsurePlayerRelationships(int playerId)
    {
        if (!playerRelationships.ContainsKey(playerId))
        {
            playerRelationships[playerId] = new Dictionary<string, int>();
        }

        for (int i = 0; i < npcs.Count; i++)
        {
            string npcId = npcs[i].NpcId;
            if (!playerRelationships[playerId].ContainsKey(npcId))
            {
                playerRelationships[playerId][npcId] = 0;
            }
        }
    }

    public string GetRelationshipsSummaryForPlayer(int playerId)
    {
        EnsurePlayerRelationships(playerId);

        StringBuilder builder = new StringBuilder();
        builder.AppendLine($"Relaciones J{playerId}:");

        for (int i = 0; i < npcs.Count; i++)
        {
            NPCData npc = npcs[i];
            int score = playerRelationships[playerId][npc.NpcId];
            string sign = score > 0 ? "+" : string.Empty;
            builder.AppendLine($"{npc.DisplayName}: {sign}{score}");
        }

        return builder.ToString().TrimEnd();
    }

    public string GetAllRelationshipsSummary()
    {
        if (npcs.Count == 0)
        {
            return "Relaciones:\n(sin NPCs)";
        }

        StringBuilder builder = new StringBuilder();
        builder.AppendLine(GetRelationshipsSummaryForPlayer(1));
        builder.AppendLine();
        builder.AppendLine(GetRelationshipsSummaryForPlayer(2));
        return builder.ToString().TrimEnd();
    }
}
