#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class NPCSetup
{
    [MenuItem("IT Hospital/Setup NPC System")]
    public static void SetupNpcSystem()
    {
        GameObject systems = GameObject.Find("Systems");
        if (systems == null)
        {
            systems = new GameObject("Systems");
        }

        if (Object.FindFirstObjectByType<NPCManager>() == null)
        {
            GameObject npcManagerObject = new GameObject("NPCManager");
            npcManagerObject.transform.SetParent(systems.transform);
            npcManagerObject.AddComponent<NPCManager>();
        }

        UIManager uiManager = Object.FindFirstObjectByType<UIManager>();
        if (uiManager == null)
        {
            EditorUtility.DisplayDialog("Setup NPC System", "No se encontró UIManager en la escena.", "OK");
            return;
        }

        Canvas canvas = uiManager.GetComponent<Canvas>() ?? uiManager.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("Setup NPC System", "No se encontró Canvas en la escena.", "OK");
            return;
        }

        Font font = GetDefaultFont();
        Transform existing = canvas.transform.Find("RelationshipsText");
        Text relationshipsText = existing != null ? existing.GetComponent<Text>() : CreateRelationshipsLabel(canvas.transform, font);

        SerializedObject uiSO = new SerializedObject(uiManager);
        uiSO.FindProperty("relationshipsText").objectReferenceValue = relationshipsText;
        uiSO.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog(
            "NPC System listo",
            "NPCManager y panel de relaciones conectados.\n\nGuarda la escena (Ctrl+S) y presiona Play.",
            "OK");
    }

    static Text CreateRelationshipsLabel(Transform canvasTransform, Font font)
    {
        GameObject textObject = new GameObject("RelationshipsText");
        textObject.transform.SetParent(canvasTransform, false);

        Text text = textObject.AddComponent<Text>();
        text.font = font;
        text.fontSize = 13;
        text.alignment = TextAnchor.UpperLeft;
        text.color = Color.white;
        text.text = "Relaciones:";
        text.raycastTarget = false;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(10f, -135f);
        rect.sizeDelta = new Vector2(220f, 130f);

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
