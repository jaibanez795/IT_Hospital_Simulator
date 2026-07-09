#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class EventSetup
{
    [MenuItem("IT Hospital/Setup Global Events")]
    public static void SetupGlobalEvents()
    {
        GameObject systems = GameObject.Find("Systems");
        if (systems == null)
        {
            systems = new GameObject("Systems");
        }

        GlobalEventManager manager = Object.FindFirstObjectByType<GlobalEventManager>();
        if (manager == null)
        {
            GameObject managerObject = new GameObject("GlobalEventManager");
            managerObject.transform.SetParent(systems.transform);
            manager = managerObject.AddComponent<GlobalEventManager>();
        }

        UIManager uiManager = Object.FindFirstObjectByType<UIManager>();
        if (uiManager == null)
        {
            EditorUtility.DisplayDialog("Setup Global Events", "No se encontró UIManager en la escena.", "OK");
            return;
        }

        Canvas canvas = uiManager.GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = uiManager.GetComponentInParent<Canvas>();
        }

        Transform bannerTransform = canvas != null ? canvas.transform.Find("GlobalEventBannerText") : null;
        Text bannerText = bannerTransform != null ? bannerTransform.GetComponent<Text>() : null;

        if (bannerText == null && canvas != null)
        {
            Font font = GetDefaultFont();
            bannerText = CreateBannerLabel(canvas.transform, font);
        }

        SerializedObject uiSO = new SerializedObject(uiManager);
        uiSO.FindProperty("globalEventBannerText").objectReferenceValue = bannerText;
        uiSO.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog(
            "Global Events listo",
            "GlobalEventManager y banner UI conectados.\n\nGuarda la escena (Ctrl+S).\n\nPara probar rápido, baja Event Interval a 15s en GlobalEventManager.",
            "OK");
    }

    static Text CreateBannerLabel(Transform canvasTransform, Font font)
    {
        GameObject textObject = new GameObject("GlobalEventBannerText");
        textObject.transform.SetParent(canvasTransform, false);

        Text text = textObject.AddComponent<Text>();
        text.font = font;
        text.fontSize = 20;
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = new Color(1f, 0.85f, 0.2f, 1f);
        text.text = string.Empty;
        text.raycastTarget = false;

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -145f);
        rect.sizeDelta = new Vector2(520f, 30f);

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
