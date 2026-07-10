using UnityEngine;

[System.Serializable]
public struct TicketSpawnData
{
    public string ticketTitle;
    public string zoneName;
    public TicketPriority priority;
    public bool isCritical;
    public TicketType ticketType;
    public TicketMode ticketMode;
    public int requiredPlayers;

    public static TicketSpawnData Create(
        string title,
        string zone,
        TicketPriority priority,
        TicketType type,
        bool critical = false,
        TicketMode mode = TicketMode.Solo,
        int players = 1)
    {
        return new TicketSpawnData
        {
            ticketTitle = title,
            zoneName = zone,
            priority = priority,
            isCritical = critical,
            ticketType = type,
            ticketMode = mode,
            requiredPlayers = players
        };
    }
}

public static class TicketDefinitionLibrary
{
    static readonly TicketSpawnData[] Definitions =
    {
        TicketSpawnData.Create("Recepción no imprime recetas", "Recepción", TicketPriority.Medium, TicketType.Cable),
        TicketSpawnData.Create("Consultorio 3 no detecta teclado", "Recepción", TicketPriority.Low, TicketType.Cable),
        TicketSpawnData.Create("Dirección no tiene WiFi", "Recepción", TicketPriority.Critical, TicketType.Router, critical: true),
        TicketSpawnData.Create("Urgencias perdió conexión", "Urgencias", TicketPriority.Critical, TicketType.Router, critical: true),
        TicketSpawnData.Create("Farmacia dice que el sistema está lento", "Urgencias", TicketPriority.High, TicketType.Cable),
        TicketSpawnData.Create("Router del site parpadea raro", "Cuarto Servidores", TicketPriority.High, TicketType.Router),
        TicketSpawnData.Create("Servidor de respaldo no responde", "Cuarto Servidores", TicketPriority.Medium, TicketType.Cable),
        TicketSpawnData.Create("Switch principal hace ruido raro", "Cuarto Servidores", TicketPriority.Critical, TicketType.Cable, critical: true)
    };

    public static TicketSpawnData GetRandomForZone(string zoneName)
    {
        int matchCount = 0;
        for (int i = 0; i < Definitions.Length; i++)
        {
            if (Definitions[i].zoneName == zoneName)
            {
                matchCount++;
            }
        }

        if (matchCount == 0)
        {
            return Definitions[Random.Range(0, Definitions.Length)];
        }

        int pick = Random.Range(0, matchCount);
        for (int i = 0; i < Definitions.Length; i++)
        {
            if (Definitions[i].zoneName != zoneName)
            {
                continue;
            }

            if (pick == 0)
            {
                return Definitions[i];
            }

            pick--;
        }

        return Definitions[0];
    }

    public static string GetZoneNameFromSpawnPoint(Transform spawnPoint)
    {
        if (spawnPoint == null)
        {
            return "Desconocida";
        }

        Transform zoneTransform = spawnPoint.parent;
        if (zoneTransform == null)
        {
            return spawnPoint.name;
        }

        return zoneTransform.name switch
        {
            "Zone_Recepcion" => "Recepción",
            "Zone_Urgencias" => "Urgencias",
            "Zone_CuartoServidores" => "Cuarto Servidores",
            _ => zoneTransform.name
        };
    }
}
