#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class PrototypeSceneBuilder
{
    const string ScenePath = "Assets/Scenes/Prototype_01_LocalCoop.unity";
    const string TicketPrefabPath = "Assets/Prefabs/Ticket.prefab";

    [MenuItem("IT Hospital/Build Prototype 01 Scene")]
    public static void BuildScene()
    {
        EnsureFolders();

        Material floorMat = CreateMaterial("Floor", new Color(0.35f, 0.35f, 0.38f));
        Material recepcionMat = CreateMaterial("Zone_Recepcion", new Color(0.3f, 0.55f, 0.85f, 0.35f));
        Material urgenciasMat = CreateMaterial("Zone_Urgencias", new Color(0.85f, 0.35f, 0.35f, 0.35f));
        Material servidoresMat = CreateMaterial("Zone_Servidores", new Color(0.35f, 0.75f, 0.45f, 0.35f));
        Material player1Mat = CreateMaterial("Player1", new Color(0.2f, 0.6f, 1f));
        Material player2Mat = CreateMaterial("Player2", new Color(1f, 0.55f, 0.2f));
        Material ticketMat = CreateMaterial("Ticket", new Color(1f, 0.9f, 0.2f));

        Ticket ticketPrefab = CreateTicketPrefab(ticketMat);

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        CreateDirectionalLight();
        CreateCamera();

        GameObject environment = new GameObject("Environment");
        CreateFloor(environment.transform, floorMat);

        Transform recepcion = CreateZoneMarker(environment.transform, "Zone_Recepcion", recepcionMat, new Vector3(-8f, 0.5f, 0f), new Vector3(6f, 1f, 6f));
        Transform urgencias = CreateZoneMarker(environment.transform, "Zone_Urgencias", urgenciasMat, new Vector3(0f, 0.5f, 8f), new Vector3(6f, 1f, 6f));
        Transform servidores = CreateZoneMarker(environment.transform, "Zone_CuartoServidores", servidoresMat, new Vector3(8f, 0.5f, 0f), new Vector3(6f, 1f, 6f));

        CreateSpawnPoint(recepcion, "SpawnPoint", Vector3.zero);
        CreateSpawnPoint(urgencias, "SpawnPoint", Vector3.zero);
        CreateSpawnPoint(servidores, "SpawnPoint", Vector3.zero);

        GameObject hideZoneObject = new GameObject("HideZone");
        hideZoneObject.transform.SetParent(servidores);
        hideZoneObject.transform.localPosition = Vector3.zero;
        BoxCollider hideCollider = hideZoneObject.AddComponent<BoxCollider>();
        hideCollider.isTrigger = true;
        hideCollider.size = new Vector3(5f, 2f, 5f);
        hideCollider.center = new Vector3(0f, 1f, 0f);
        hideZoneObject.AddComponent<HideZone>();

        GameObject playersRoot = new GameObject("Players");
        PlayerController player1 = CreatePlayer(playersRoot.transform, "Player1", player1Mat, new Vector3(-3f, 1f, -3f), 1);
        PlayerController player2 = CreatePlayer(playersRoot.transform, "Player2", player2Mat, new Vector3(3f, 1f, -3f), 2);

        GameObject systemsRoot = new GameObject("Systems");
        GameObject gameManagerObject = new GameObject("GameManager");
        gameManagerObject.transform.SetParent(systemsRoot.transform);
        gameManagerObject.AddComponent<GameManager>();

        GameObject spawnerObject = new GameObject("TicketSpawner");
        spawnerObject.transform.SetParent(systemsRoot.transform);
        TicketSpawner spawner = spawnerObject.AddComponent<TicketSpawner>();

        Transform[] spawnPoints =
        {
            recepcion.Find("SpawnPoint"),
            urgencias.Find("SpawnPoint"),
            servidores.Find("SpawnPoint")
        };

        SerializedObject spawnerSO = new SerializedObject(spawner);
        spawnerSO.FindProperty("ticketPrefab").objectReferenceValue = ticketPrefab;
        spawnerSO.FindProperty("spawnPoints").arraySize = spawnPoints.Length;
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            spawnerSO.FindProperty("spawnPoints").GetArrayElementAtIndex(i).objectReferenceValue = spawnPoints[i];
        }
        spawnerSO.ApplyModifiedPropertiesWithoutUndo();

        if (spawnerSO.FindProperty("ticketPrefab").objectReferenceValue == null)
        {
            Debug.LogError("TicketSpawner quedó sin prefab. Revisa Assets/Prefabs/Ticket.prefab.");
        }

        UIManager uiManager = CreateCanvasUI();

        EditorSceneManager.SaveScene(scene, ScenePath);
        AddSceneToBuildSettings(ScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Escena creada en {ScenePath}. Abre la escena y presiona Play para probar.");
        EditorUtility.DisplayDialog(
            "Prototype 01 listo",
            "Escena creada correctamente.\n\n1. Abre Assets/Scenes/Prototype_01_LocalCoop.unity\n2. Presiona Play\n\nJ1: WASD + E\nJ2: Flechas + Ctrl derecho o Enter",
            "OK");
    }

    static void EnsureFolders()
    {
        CreateFolder("Assets/Scripts");
        CreateFolder("Assets/Scripts/Core");
        CreateFolder("Assets/Scripts/Players");
        CreateFolder("Assets/Scripts/Tickets");
        CreateFolder("Assets/Scripts/Zones");
        CreateFolder("Assets/Scripts/UI");
        CreateFolder("Assets/Scripts/Editor");
        CreateFolder("Assets/Prefabs");
        CreateFolder("Assets/Materials");
        CreateFolder("Assets/Scenes");
    }

    static void CreateFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
        {
            return;
        }

        string parent = Path.GetDirectoryName(path)?.Replace("\\", "/");
        string folderName = Path.GetFileName(path);
        if (!string.IsNullOrEmpty(parent) && !string.IsNullOrEmpty(folderName))
        {
            AssetDatabase.CreateFolder(parent, folderName);
        }
    }

    static Material CreateMaterial(string name, Color color)
    {
        string path = $"Assets/Materials/{name}.mat";
        Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null)
        {
            existing.color = color;
            EditorUtility.SetDirty(existing);
            return existing;
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        Material material = new Material(shader);
        material.color = color;

        if (color.a < 1f)
        {
            ConfigureTransparent(material, color);
        }

        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    static void ConfigureTransparent(Material material, Color color)
    {
        material.SetFloat("_Surface", 1f);
        material.SetFloat("_Blend", 0f);
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.renderQueue = 3000;
        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.color = color;
    }

    static Ticket CreateTicketPrefab(Material ticketMat)
    {
        GameObject ticketObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ticketObject.name = "Ticket";
        ticketObject.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);

        Object.DestroyImmediate(ticketObject.GetComponent<BoxCollider>());
        BoxCollider trigger = ticketObject.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = Vector3.one * 1.2f;

        MeshRenderer renderer = ticketObject.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = ticketMat;

        ticketObject.AddComponent<Ticket>();

        PrefabUtility.SaveAsPrefabAsset(ticketObject, TicketPrefabPath);
        Object.DestroyImmediate(ticketObject);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return AssetDatabase.LoadAssetAtPath<Ticket>(TicketPrefabPath);
    }

    static void CreateDirectionalLight()
    {
        GameObject lightObject = new GameObject("Directional Light");
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.1f;
        lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
    }

    static void CreateCamera()
    {
        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.AddComponent<Camera>();
        cameraObject.AddComponent<AudioListener>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.12f, 0.14f, 0.16f);
        cameraObject.transform.position = new Vector3(0f, 18f, -10f);
        cameraObject.transform.rotation = Quaternion.Euler(55f, 0f, 0f);
    }

    static void CreateFloor(Transform parent, Material material)
    {
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Floor";
        floor.transform.SetParent(parent);
        floor.transform.localScale = new Vector3(2.5f, 1f, 2.5f);
        floor.GetComponent<MeshRenderer>().sharedMaterial = material;
    }

    static Transform CreateZoneMarker(Transform parent, string name, Material material, Vector3 position, Vector3 scale)
    {
        GameObject zone = GameObject.CreatePrimitive(PrimitiveType.Cube);
        zone.name = name;
        zone.transform.SetParent(parent);
        zone.transform.position = position;
        zone.transform.localScale = scale;

        Collider collider = zone.GetComponent<Collider>();
        Object.DestroyImmediate(collider);

        MeshRenderer renderer = zone.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = material;

        return zone.transform;
    }

    static void CreateSpawnPoint(Transform parent, string name, Vector3 localPosition)
    {
        GameObject spawnPoint = new GameObject(name);
        spawnPoint.transform.SetParent(parent);
        spawnPoint.transform.localPosition = localPosition;
    }

    static PlayerController CreatePlayer(Transform parent, string name, Material material, Vector3 position, int playerIndex)
    {
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = name;
        player.transform.SetParent(parent);
        player.transform.position = position;

        CapsuleCollider capsule = player.GetComponent<CapsuleCollider>();
        capsule.height = 2f;
        capsule.radius = 0.4f;

        Rigidbody rb = player.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        player.GetComponent<MeshRenderer>().sharedMaterial = material;

        PlayerController controller = player.AddComponent<PlayerController>();
        SerializedObject playerSO = new SerializedObject(controller);
        playerSO.FindProperty("playerIndex").intValue = playerIndex;
        playerSO.FindProperty("moveSpeed").floatValue = 18f;
        playerSO.ApplyModifiedPropertiesWithoutUndo();

        return controller;
    }

    static UIManager CreateCanvasUI()
    {
        GameObject canvasObject = new GameObject("Canvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();

        UIManager uiManager = canvasObject.AddComponent<UIManager>();

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null)
        {
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        Text operacionText = CreateLabel(canvasObject.transform, "OperacionText", new Vector2(10f, -10f), new Vector2(0f, 1f), font, 18);
        Text estresText = CreateLabel(canvasObject.transform, "EstresText", new Vector2(10f, -35f), new Vector2(0f, 1f), font, 18);
        Text desempenoText = CreateLabel(canvasObject.transform, "DesempenoText", new Vector2(10f, -60f), new Vector2(0f, 1f), font, 18);
        Text timerText = CreateLabel(canvasObject.transform, "TimerText", new Vector2(-10f, -10f), new Vector2(1f, 1f), font, 22);
        Text sospechaJ1Text = CreateLabel(canvasObject.transform, "SospechaJ1Text", new Vector2(10f, -90f), new Vector2(0f, 1f), font, 16);
        Text sospechaJ2Text = CreateLabel(canvasObject.transform, "SospechaJ2Text", new Vector2(10f, -112f), new Vector2(0f, 1f), font, 16);
        Text temporaryMessageText = CreateLabel(canvasObject.transform, "TemporaryMessageText", new Vector2(0f, -180f), new Vector2(0.5f, 1f), font, 20);
        temporaryMessageText.alignment = TextAnchor.MiddleCenter;

        GameObject endPanel = CreatePanel(canvasObject.transform, "EndScreenPanel");
        Text endTitle = CreateLabel(endPanel.transform, "EndTitleText", Vector2.zero, new Vector2(0.5f, 0.5f), font, 36);
        endTitle.rectTransform.anchoredPosition = new Vector2(0f, 40f);
        endTitle.alignment = TextAnchor.MiddleCenter;

        Text endReason = CreateLabel(endPanel.transform, "EndReasonText", Vector2.zero, new Vector2(0.5f, 0.5f), font, 22);
        endReason.rectTransform.anchoredPosition = new Vector2(0f, -20f);
        endReason.alignment = TextAnchor.MiddleCenter;

        SerializedObject uiSO = new SerializedObject(uiManager);
        uiSO.FindProperty("operacionText").objectReferenceValue = operacionText;
        uiSO.FindProperty("estresText").objectReferenceValue = estresText;
        uiSO.FindProperty("desempenoText").objectReferenceValue = desempenoText;
        uiSO.FindProperty("timerText").objectReferenceValue = timerText;
        uiSO.FindProperty("sospechaJ1Text").objectReferenceValue = sospechaJ1Text;
        uiSO.FindProperty("sospechaJ2Text").objectReferenceValue = sospechaJ2Text;
        uiSO.FindProperty("temporaryMessageText").objectReferenceValue = temporaryMessageText;
        uiSO.FindProperty("endScreenPanel").objectReferenceValue = endPanel;
        uiSO.FindProperty("endScreenTitleText").objectReferenceValue = endTitle;
        uiSO.FindProperty("endScreenReasonText").objectReferenceValue = endReason;
        uiSO.ApplyModifiedPropertiesWithoutUndo();

        return uiManager;
    }

    static Text CreateLabel(Transform parent, string name, Vector2 anchoredPosition, Vector2 anchor, Font font, int fontSize)
    {
        GameObject textObject = new GameObject(name);
        textObject.transform.SetParent(parent, false);

        Text text = textObject.AddComponent<Text>();
        text.font = font;
        text.fontSize = fontSize;
        text.color = Color.white;
        text.text = name;

        RectTransform rect = text.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = anchor;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(420f, 30f);

        return text;
    }

    static void AddSceneToBuildSettings(string scenePath)
    {
        EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
        foreach (EditorBuildSettingsScene scene in scenes)
        {
            if (scene.path == scenePath)
            {
                return;
            }
        }

        EditorBuildSettingsScene[] updatedScenes = new EditorBuildSettingsScene[scenes.Length + 1];
        for (int i = 0; i < scenes.Length; i++)
        {
            updatedScenes[i] = scenes[i];
        }

        updatedScenes[scenes.Length] = new EditorBuildSettingsScene(scenePath, true);
        EditorBuildSettings.scenes = updatedScenes;
    }

    static GameObject CreatePanel(Transform parent, string name)
    {
        GameObject panelObject = new GameObject(name);
        panelObject.transform.SetParent(parent, false);

        Image image = panelObject.AddComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0.75f);

        RectTransform rect = panelObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        panelObject.SetActive(false);
        return panelObject;
    }
}
#endif
