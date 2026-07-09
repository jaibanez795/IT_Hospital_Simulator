#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public static class MinigameSetup
{
    [MenuItem("IT Hospital/Setup Minigames UI")]
    public static void SetupAllMinigames()
    {
        SetupCableMinigameInternal(showDialog: false);
        SetupRouterMinigameInternal(showDialog: false);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog(
            "Minigames listos",
            "Cable y Router conectados.\n\nGuarda la escena (Ctrl+S) y presiona Play.",
            "OK");
    }

    [MenuItem("IT Hospital/Setup Cable Minigame UI")]
    public static void SetupCableMinigame()
    {
        SetupCableMinigameInternal(showDialog: true);
    }

    static void SetupCableMinigameInternal(bool showDialog)
    {
        MinigameManager manager = EnsureMinigameManager();
        CableMinigame cableMinigame = Object.FindFirstObjectByType<CableMinigame>();

        GameObject systems = GameObject.Find("Systems");
        if (systems == null)
        {
            systems = new GameObject("Systems");
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
            EditorUtility.DisplayDialog("Setup Minigames", "No se encontró un Canvas en la escena.", "OK");
            return;
        }

        EnsureEventSystem();

        Transform existingPanel = canvas.transform.Find("CableMinigamePanel");
        GameObject panelRoot = existingPanel != null ? existingPanel.gameObject : CreateCableMinigamePanel(canvas.transform);

        SerializedObject cableSO = new SerializedObject(cableMinigame);
        cableSO.FindProperty("panelRoot").objectReferenceValue = panelRoot;
        cableSO.FindProperty("titleText").objectReferenceValue = panelRoot.transform.Find("TitleText")?.GetComponent<Text>();
        cableSO.FindProperty("instructionText").objectReferenceValue = panelRoot.transform.Find("InstructionText")?.GetComponent<Text>();
        cableSO.FindProperty("timerText").objectReferenceValue = panelRoot.transform.Find("TimerText")?.GetComponent<Text>();
        cableSO.FindProperty("feedbackText").objectReferenceValue = panelRoot.transform.Find("FeedbackText")?.GetComponent<Text>();
        cableSO.FindProperty("cableAButton").objectReferenceValue = panelRoot.transform.Find("CableAButton")?.GetComponent<Button>();
        cableSO.FindProperty("cableBButton").objectReferenceValue = panelRoot.transform.Find("CableBButton")?.GetComponent<Button>();
        cableSO.FindProperty("cableCButton").objectReferenceValue = panelRoot.transform.Find("CableCButton")?.GetComponent<Button>();
        cableSO.ApplyModifiedPropertiesWithoutUndo();

        SerializedObject managerSO = new SerializedObject(manager);
        managerSO.FindProperty("cableMinigame").objectReferenceValue = cableMinigame;
        managerSO.ApplyModifiedPropertiesWithoutUndo();

        panelRoot.SetActive(false);

        if (showDialog)
        {
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog(
                "Cable Minigame listo",
                "CableMinigame y panel UI conectados.\n\nGuarda la escena (Ctrl+S) y presiona Play.",
                "OK");
        }
    }

    static void SetupRouterMinigameInternal(bool showDialog)
    {
        MinigameManager manager = EnsureMinigameManager();
        RouterMinigame routerMinigame = Object.FindFirstObjectByType<RouterMinigame>();

        GameObject systems = GameObject.Find("Systems");
        if (systems == null)
        {
            systems = new GameObject("Systems");
        }

        if (routerMinigame == null)
        {
            GameObject routerObject = new GameObject("RouterMinigame");
            routerObject.transform.SetParent(systems.transform);
            routerMinigame = routerObject.AddComponent<RouterMinigame>();
        }

        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            return;
        }

        EnsureEventSystem();

        Transform existingPanel = canvas.transform.Find("RouterMinigamePanel");
        GameObject panelRoot = existingPanel != null ? existingPanel.gameObject : CreateRouterMinigamePanel(canvas.transform);

        SerializedObject routerSO = new SerializedObject(routerMinigame);
        routerSO.FindProperty("panelRoot").objectReferenceValue = panelRoot;
        routerSO.FindProperty("titleText").objectReferenceValue = panelRoot.transform.Find("TitleText")?.GetComponent<Text>();
        routerSO.FindProperty("instructionText").objectReferenceValue = panelRoot.transform.Find("InstructionText")?.GetComponent<Text>();
        routerSO.FindProperty("holdTimeText").objectReferenceValue = panelRoot.transform.Find("HoldTimeText")?.GetComponent<Text>();
        routerSO.FindProperty("idealZoneText").objectReferenceValue = panelRoot.transform.Find("IdealZoneText")?.GetComponent<Text>();
        routerSO.FindProperty("feedbackText").objectReferenceValue = panelRoot.transform.Find("FeedbackText")?.GetComponent<Text>();
        routerSO.FindProperty("holdProgressBar").objectReferenceValue = panelRoot.transform.Find("HoldProgressBar")?.GetComponent<Image>();
        routerSO.FindProperty("holdButton").objectReferenceValue = panelRoot.transform.Find("HoldButton")?.GetComponent<Button>();
        routerSO.ApplyModifiedPropertiesWithoutUndo();

        SerializedObject managerSO = new SerializedObject(manager);
        managerSO.FindProperty("routerMinigame").objectReferenceValue = routerMinigame;
        managerSO.ApplyModifiedPropertiesWithoutUndo();

        panelRoot.SetActive(false);

        if (showDialog)
        {
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();
        }
    }

    static MinigameManager EnsureMinigameManager()
    {
        MinigameManager manager = Object.FindFirstObjectByType<MinigameManager>();
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

        return manager;
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
        Font font = GetDefaultFont();

        GameObject panel = CreatePanelRoot(canvasTransform, "CableMinigamePanel", new Vector2(420f, 300f));

        CreateCenteredLabel(panel.transform, "TitleText", "Seguir cable", font, 28, new Vector2(0f, 110f), 400f);
        CreateCenteredLabel(panel.transform, "InstructionText", "Elige el cable correcto", font, 18, new Vector2(0f, 70f), 400f);
        CreateCenteredLabel(panel.transform, "TimerText", "Tiempo: 6.0s", font, 20, new Vector2(0f, 35f), 400f);
        CreateCenteredLabel(panel.transform, "FeedbackText", string.Empty, font, 16, new Vector2(0f, -120f), 400f);

        CreateActionButton(panel.transform, "CableAButton", "Cable A", font, new Vector2(-120f, -30f), new Vector2(100f, 40f));
        CreateActionButton(panel.transform, "CableBButton", "Cable B", font, new Vector2(0f, -30f), new Vector2(100f, 40f));
        CreateActionButton(panel.transform, "CableCButton", "Cable C", font, new Vector2(120f, -30f), new Vector2(100f, 40f));

        return panel;
    }

    static GameObject CreateRouterMinigamePanel(Transform canvasTransform)
    {
        Font font = GetDefaultFont();

        GameObject panel = CreatePanelRoot(canvasTransform, "RouterMinigamePanel", new Vector2(460f, 340f));

        CreateCenteredLabel(panel.transform, "TitleText", "Reiniciar router", font, 28, new Vector2(0f, 130f), 420f);
        CreateCenteredLabel(panel.transform, "InstructionText", "Mantén presionado el botón. Suelta en la zona correcta.", font, 16, new Vector2(0f, 95f), 420f);
        CreateCenteredLabel(panel.transform, "IdealZoneText", "Zona ideal: 2.0s - 3.5s", font, 14, new Vector2(0f, 65f), 420f);
        CreateCenteredLabel(panel.transform, "HoldTimeText", "Tiempo presionado: 0.0s", font, 18, new Vector2(0f, 35f), 420f);
        CreateCenteredLabel(panel.transform, "FeedbackText", string.Empty, font, 16, new Vector2(0f, -140f), 420f);

        CreateProgressBar(panel.transform, "HoldProgressBar", new Vector2(0f, 0f), new Vector2(320f, 18f));
        CreateActionButton(panel.transform, "HoldButton", "Mantener reinicio", font, new Vector2(0f, -55f), new Vector2(220f, 48f));

        return panel;
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

    static GameObject CreatePanelRoot(Transform canvasTransform, string name, Vector2 size)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(canvasTransform, false);

        Image background = panel.AddComponent<Image>();
        background.color = new Color(0f, 0f, 0f, 0.82f);

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = size;

        return panel;
    }

    static void CreateProgressBar(Transform parent, string name, Vector2 position, Vector2 size)
    {
        GameObject barObject = new GameObject(name);
        barObject.transform.SetParent(parent, false);

        Image image = barObject.AddComponent<Image>();
        image.color = new Color(0.2f, 0.75f, 0.35f, 1f);
        image.type = Image.Type.Filled;
        image.fillMethod = Image.FillMethod.Horizontal;
        image.fillOrigin = (int)Image.OriginHorizontal.Left;
        image.fillAmount = 0f;

        RectTransform rect = barObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
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

    static Button CreateActionButton(Transform parent, string name, string label, Font font, Vector2 position, Vector2 size)
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
        rect.sizeDelta = size;

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
