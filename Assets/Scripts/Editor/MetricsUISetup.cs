#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class MetricsUISetup
{
    [MenuItem("IT Hospital/Setup Individual Metrics UI")]
    public static void SetupIndividualMetricsUI()
    {
        GameObject gameManagerObject = GameObject.Find("GameManager");
        if (gameManagerObject == null)
        {
            EditorUtility.DisplayDialog("Setup Metrics UI", "No se encontró GameManager en la escena.", "OK");
            return;
        }

        if (gameManagerObject.GetComponent<TeamState>() == null)
        {
            gameManagerObject.AddComponent<TeamState>();
        }

        PlayerController[] players = Object.FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        foreach (PlayerController player in players)
        {
            if (player.GetComponent<PlayerStats>() == null)
            {
                PlayerStats stats = player.gameObject.AddComponent<PlayerStats>();
                SerializedObject statsSO = new SerializedObject(stats);
                statsSO.FindProperty("playerId").intValue = player.PlayerIndex;
                statsSO.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        UIManager uiManager = Object.FindFirstObjectByType<UIManager>();
        if (uiManager == null)
        {
            EditorUtility.DisplayDialog("Setup Metrics UI", "No se encontró UIManager.", "OK");
            return;
        }

        Canvas canvas = uiManager.GetComponent<Canvas>() ?? uiManager.GetComponentInParent<Canvas>();
        Font font = GetDefaultFont();

        Text j1Stats = FindOrCreateStatsLabel(canvas.transform, "J1StatsText", font, new Vector2(10f, -85f), new Vector2(280f, 70f));
        Text j2Stats = FindOrCreateStatsLabel(canvas.transform, "J2StatsText", font, new Vector2(10f, -160f), new Vector2(280f, 70f));

        SerializedObject uiSO = new SerializedObject(uiManager);
        uiSO.FindProperty("j1StatsText").objectReferenceValue = j1Stats;
        uiSO.FindProperty("j2StatsText").objectReferenceValue = j2Stats;
        uiSO.FindProperty("estresText").objectReferenceValue = null;
        uiSO.FindProperty("desempenoText").objectReferenceValue = null;
        uiSO.FindProperty("sospechaJ1Text").objectReferenceValue = null;
        uiSO.FindProperty("sospechaJ2Text").objectReferenceValue = null;
        uiSO.ApplyModifiedPropertiesWithoutUndo();

        SerializedObject gmSO = new SerializedObject(gameManagerObject.GetComponent<GameManager>());
        gmSO.FindProperty("teamState").objectReferenceValue = gameManagerObject.GetComponent<TeamState>();
        gmSO.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog(
            "Metrics UI listo",
            "TeamState, PlayerStats y paneles J1/J2 conectados.\n\nGuarda la escena (Ctrl+S).",
            "OK");
    }

    static Text FindOrCreateStatsLabel(Transform canvas, string name, Font font, Vector2 position, Vector2 size)
    {
        Transform existing = canvas.Find(name);
        if (existing != null)
        {
            return existing.GetComponent<Text>();
        }

        GameObject textObject = new GameObject(name);
        textObject.transform.SetParent(canvas, false);

        Text text = textObject.AddComponent<Text>();
        text.font = font;
        text.fontSize = 14;
        text.alignment = TextAnchor.UpperLeft;
        text.color = Color.white;
        text.raycastTarget = false;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        return text;
    }

    static Font GetDefaultFont()
    {
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null)
        {
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        return font;
    }
}
#endif
