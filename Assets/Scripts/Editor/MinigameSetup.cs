#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public static class MinigameSetup
{
    [MenuItem("IT Hospital/Setup Cable Minigame UI")]
    public static void SetupCableMinigame()
    {
        MinigameManager manager = Object.FindFirstObjectByType<MinigameManager>();
        CableMinigame cableMinigame = Object.FindFirstObjectByType<CableMinigame>();

        GameObject systems = GameObject.Find("Systems");
        if (systems == null)
        {
            systems = new GameObject("Systems");
        }

        if (manager == null)
        {
            GameObject managerObject = new GameObject("MinigameManager");
            managerObject.transform.SetParent(systems.transform);
            manager = managerObject.AddComponent<MinigameManager>();
        }

        if (cableMinigame == null)
        {
            GameObject cableObject = new GameObject("CableMinigame");
            cableObject.transform.SetParent(systems.transform);
            cableMinigame = cableObject.AddComponent<CableMinigame>();
        }

        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("Setup Cable Minigame", "No se encontró un Canvas en la escena.", "OK");
            return;
        }

        EnsureEventSystem();

        Transform existingPanel = canvas.transform.Find("CableMinigamePanel");
        GameObject panelRoot = existingPanel != null ? existingPanel.gameObject : CreateCableMinigamePanel(canvas.transform);

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null)
        {
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        Text titleText = panelRoot.transform.Find("TitleText")?.GetComponent<Text>();
        Text instructionText = panelRoot.transform.Find("InstructionText")?.GetComponent<Text>();
        Text timerText = panelRoot.transform.Find("TimerText")?.GetComponent<Text>();
        Text feedbackText = panelRoot.transform.Find("FeedbackText")?.GetComponent<Text>();
        Button cableA = panelRoot.transform.Find("CableAButton")?.GetComponent<Button>();
        Button cableB = panelRoot.transform.Find("CableBButton")?.GetComponent<Button>();
        Button cableC = panelRoot.transform.Find("CableCButton")?.GetComponent<Button>();

        SerializedObject cableSO = new SerializedObject(cableMinigame);
        cableSO.FindProperty("panelRoot").objectReferenceValue = panelRoot;
        cableSO.FindProperty("titleText").objectReferenceValue = titleText;
        cableSO.FindProperty("instructionText").objectReferenceValue = instructionText;
        cableSO.FindProperty("timerText").objectReferenceValue = timerText;
        cableSO.FindProperty("feedbackText").objectReferenceValue = feedbackText;
        cableSO.FindProperty("cableAButton").objectReferenceValue = cableA;
        cableSO.FindProperty("cableBButton").objectReferenceValue = cableB;
        cableSO.FindProperty("cableCButton").objectReferenceValue = cableC;
        cableSO.ApplyModifiedPropertiesWithoutUndo();

        SerializedObject managerSO = new SerializedObject(manager);
        managerSO.FindProperty("cableMinigame").objectReferenceValue = cableMinigame;
        managerSO.ApplyModifiedPropertiesWithoutUndo();

        panelRoot.SetActive(false);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog(
            "Cable Minigame listo",
            "MinigameManager, CableMinigame y panel UI conectados.\n\nGuarda la escena (Ctrl+S) y presiona Play.",
            "OK");
    }

    static void EnsureEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<InputSystemUIInputModule>();
    }

    static GameObject CreateCableMinigamePanel(Transform canvasTransform)
    {
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null)
        {
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        GameObject panel = new GameObject("CableMinigamePanel");
        panel.transform.SetParent(canvasTransform, false);

        Image background = panel.AddComponent<Image>();
        background.color = new Color(0f, 0f, 0f, 0.82f);

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(420f, 300f);

        CreateCenteredLabel(panel.transform, "TitleText", "Seguir cable", font, 28, new Vector2(0f, 110f), 400f);
        CreateCenteredLabel(panel.transform, "InstructionText", "Elige el cable correcto", font, 18, new Vector2(0f, 70f), 400f);
        CreateCenteredLabel(panel.transform, "TimerText", "Tiempo: 6.0s", font, 20, new Vector2(0f, 35f), 400f);
        CreateCenteredLabel(panel.transform, "FeedbackText", string.Empty, font, 16, new Vector2(0f, -120f), 400f);

        CreateCableButton(panel.transform, "CableAButton", "Cable A", font, new Vector2(-120f, -30f));
        CreateCableButton(panel.transform, "CableBButton", "Cable B", font, new Vector2(0f, -30f));
        CreateCableButton(panel.transform, "CableCButton", "Cable C", font, new Vector2(120f, -30f));

        return panel;
    }

    static Text CreateCenteredLabel(Transform parent, string name, string content, Font font, int size, Vector2 position, float width)
    {
        GameObject textObject = new GameObject(name);
        textObject.transform.SetParent(parent, false);

        Text text = textObject.AddComponent<Text>();
        text.font = font;
        text.fontSize = size;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.text = content;
        text.raycastTarget = false;

        RectTransform rect = text.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(width, 36f);

        return text;
    }

    static Button CreateCableButton(Transform parent, string name, string label, Font font, Vector2 position)
    {
        GameObject buttonObject = new GameObject(name);
        buttonObject.transform.SetParent(parent, false);

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.25f, 0.45f, 0.75f, 1f);

        Button button = buttonObject.AddComponent<Button>();

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(100f, 40f);

        GameObject labelObject = new GameObject("Text");
        labelObject.transform.SetParent(buttonObject.transform, false);

        Text text = labelObject.AddComponent<Text>();
        text.font = font;
        text.fontSize = 16;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.text = label;
        text.raycastTarget = false;

        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        return button;
    }
}
#endif
