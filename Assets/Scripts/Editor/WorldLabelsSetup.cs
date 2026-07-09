#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class WorldLabelsSetup
{
    [MenuItem("IT Hospital/Add World Labels")]
    public static void AddWorldLabels()
    {
        AddZoneLabel("Zone_Recepcion", "Recepción");
        AddZoneLabel("Zone_Urgencias", "Urgencias");
        AddZoneLabel("Zone_CuartoServidores", "Cuarto Servidores");
        AddPlayerLabel("Player1", "J1");
        AddPlayerLabel("Player2", "J2");

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorUtility.DisplayDialog(
            "Labels agregados",
            "Etiquetas de zonas y jugadores creadas.\n\nGuarda la escena (Ctrl+S).",
            "OK");
    }

    static void AddZoneLabel(string zoneName, string labelText)
    {
        GameObject zone = GameObject.Find(zoneName);
        if (zone == null)
        {
            return;
        }

        CreateOrUpdateLabel(zone.transform, "ZoneLabel", labelText, new Vector3(0f, 1.4f, 0f), 0.14f);
    }

    static void AddPlayerLabel(string playerName, string labelText)
    {
        GameObject player = GameObject.Find(playerName);
        if (player == null)
        {
            player = FindPlayerByIndex(playerName == "Player1" ? 1 : 2);
        }

        if (player == null)
        {
            return;
        }

        CreateOrUpdateLabel(player.transform, "PlayerLabel", labelText, new Vector3(0f, 1.3f, 0f), 0.12f);
    }

    static GameObject FindPlayerByIndex(int index)
    {
        PlayerController[] players = Object.FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        foreach (PlayerController player in players)
        {
            if (player.PlayerIndex == index)
            {
                return player.gameObject;
            }
        }

        return null;
    }

    public static void CreateOrUpdateLabel(Transform parent, string objectName, string labelText, Vector3 localPosition, float characterSize)
    {
        Transform existing = parent.Find(objectName);
        GameObject labelObject = existing != null ? existing.gameObject : new GameObject(objectName);
        if (existing == null)
        {
            labelObject.transform.SetParent(parent);
        }

        labelObject.transform.localPosition = localPosition;
        labelObject.transform.localRotation = Quaternion.identity;

        TextMesh textMesh = labelObject.GetComponent<TextMesh>();
        if (textMesh == null)
        {
            textMesh = labelObject.AddComponent<TextMesh>();
        }

        textMesh.text = labelText;
        textMesh.fontSize = 64;
        textMesh.characterSize = characterSize;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.color = Color.white;

        if (labelObject.GetComponent<WorldBillboardLabel>() == null)
        {
            labelObject.AddComponent<WorldBillboardLabel>();
        }
    }
}
#endif
