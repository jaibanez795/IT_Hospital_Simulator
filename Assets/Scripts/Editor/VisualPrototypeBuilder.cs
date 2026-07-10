#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class VisualPrototypeBuilder
{
    const string MatFolder = "Assets/Materials/VisualPrototype";
    const string PrefabFolder = "Assets/Prefabs/VisualPrototype";
    const string TicketPrefabPath = "Assets/Prefabs/Ticket.prefab";

    static readonly Dictionary<string, Material> Materials = new Dictionary<string, Material>();

    [MenuItem("IT Hospital/Apply Visual Prototype Pass")]
    public static void ApplyVisualPrototypePass()
    {
        EnsureFolders();
        CreateAllMaterials();
        CreateAllPrefabs();
        ApplyToActiveScene();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        EditorUtility.DisplayDialog(
            "Visual Prototype aplicado",
            "Materiales, prefabs y decoración visual actualizados.\n\nGuarda la escena (Ctrl+S) y presiona Play para probar.",
            "OK");
    }

    [MenuItem("IT Hospital/Visual Prototype/Create Materials And Prefabs Only")]
    public static void CreateAssetsOnly()
    {
        EnsureFolders();
        CreateAllMaterials();
        CreateAllPrefabs();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Visual Prototype: materiales y prefabs creados.");
    }

    static void EnsureFolders()
    {
        CreateFolder("Assets/Materials");
        CreateFolder(MatFolder);
        CreateFolder("Assets/Prefabs");
        CreateFolder(PrefabFolder);
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

    static void CreateAllMaterials()
    {
        Materials.Clear();

        Materials["Floor_Hospital"] = CreateMat("Floor_Hospital", new Color(0.88f, 0.86f, 0.82f), 0.35f);
        Materials["Wall_Hospital"] = CreateMat("Wall_Hospital", new Color(0.78f, 0.84f, 0.92f), 0.2f);
        Materials["Zone_Reception"] = CreateMat("Zone_Reception", new Color(0.35f, 0.65f, 1f, 0.28f), 0.15f, transparent: true);
        Materials["Zone_Emergency"] = CreateMat("Zone_Emergency", new Color(1f, 0.42f, 0.42f, 0.28f), 0.15f, transparent: true);
        Materials["Zone_ServerRoom"] = CreateMat("Zone_ServerRoom", new Color(0.35f, 0.82f, 0.55f, 0.28f), 0.15f, transparent: true);
        Materials["Player_J1"] = CreateMat("Player_J1", new Color(0.2f, 0.72f, 1f), 0.55f);
        Materials["Player_J2"] = CreateMat("Player_J2", new Color(1f, 0.55f, 0.18f), 0.55f);
        Materials["Player_Incapacitated"] = CreateMat("Player_Incapacitated", new Color(0.55f, 0.55f, 0.58f, 0.55f), 0.1f, transparent: true);
        Materials["Ticket_Low"] = CreateMat("Ticket_Low", new Color(0.85f, 0.85f, 0.35f), 0.45f);
        Materials["Ticket_Medium"] = CreateMat("Ticket_Medium", new Color(1f, 0.9f, 0.2f), 0.45f);
        Materials["Ticket_High"] = CreateMat("Ticket_High", new Color(1f, 0.55f, 0.15f), 0.45f);
        Materials["Ticket_Critical"] = CreateMat("Ticket_Critical", new Color(1f, 0.2f, 0.2f), 0.45f);
        Materials["Prop_Computer"] = CreateMat("Prop_Computer", new Color(0.25f, 0.28f, 0.32f), 0.65f);
        Materials["Prop_Printer"] = CreateMat("Prop_Printer", new Color(0.92f, 0.92f, 0.95f), 0.35f);
        Materials["Prop_Router"] = CreateMat("Prop_Router", new Color(0.15f, 0.15f, 0.18f), 0.7f);
        Materials["Prop_Desk"] = CreateMat("Prop_Desk", new Color(0.72f, 0.55f, 0.38f), 0.25f);
        Materials["Prop_ServerRack"] = CreateMat("Prop_ServerRack", new Color(0.18f, 0.2f, 0.24f), 0.75f);
        Materials["Prop_Bed"] = CreateMat("Prop_Bed", new Color(0.75f, 0.88f, 0.95f), 0.2f);
        Materials["Prop_Sign"] = CreateMat("Prop_Sign", new Color(0.95f, 0.95f, 0.7f), 0.3f);
        Materials["Prop_CableRed"] = CreateMat("Prop_CableRed", new Color(0.95f, 0.25f, 0.25f), 0.6f);
        Materials["Prop_CableBlue"] = CreateMat("Prop_CableBlue", new Color(0.25f, 0.45f, 0.95f), 0.6f);
        Materials["Prop_CableYellow"] = CreateMat("Prop_CableYellow", new Color(0.95f, 0.85f, 0.2f), 0.6f);
        Materials["Prop_Light"] = CreateMat("Prop_Light", new Color(0.3f, 1f, 0.45f), 0.85f);
    }

    static Material CreateMat(string name, Color color, float smoothness, bool transparent = false)
    {
        string path = $"{MatFolder}/{name}.mat";
        Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");

        Material material = existing != null ? existing : new Material(shader);
        material.shader = shader;
        material.color = color;
        material.SetFloat("_Smoothness", smoothness);

        if (transparent)
        {
            ConfigureTransparent(material, color);
        }

        if (existing == null)
        {
            AssetDatabase.CreateAsset(material, path);
        }
        else
        {
            EditorUtility.SetDirty(material);
        }

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

    static Material GetMat(string key)
    {
        Materials.TryGetValue(key, out Material mat);
        return mat;
    }

    static void CreateAllPrefabs()
    {
        SavePrefab(CreatePropDesk(), $"{PrefabFolder}/Prop_Desk.prefab");
        SavePrefab(CreatePropComputer(), $"{PrefabFolder}/Prop_Computer.prefab");
        SavePrefab(CreatePropPrinter(), $"{PrefabFolder}/Prop_Printer.prefab");
        SavePrefab(CreatePropRouter(), $"{PrefabFolder}/Prop_Router.prefab");
        SavePrefab(CreatePropServerRack(), $"{PrefabFolder}/Prop_ServerRack.prefab");
        SavePrefab(CreatePropHospitalBed(), $"{PrefabFolder}/Prop_HospitalBed.prefab");
        SavePrefab(CreatePropSign("SIGN"), $"{PrefabFolder}/Prop_Sign.prefab");
        SavePrefab(CreatePlayerVisual(true), $"{PrefabFolder}/PlayerVisual_ITTech_J1.prefab");
        SavePrefab(CreatePlayerVisual(false), $"{PrefabFolder}/PlayerVisual_ITTech_J2.prefab");
        SavePrefab(CreateTicketVisualCable(), $"{PrefabFolder}/TicketVisual_Cable.prefab");
        SavePrefab(CreateTicketVisualRouter(), $"{PrefabFolder}/TicketVisual_Router.prefab");
        RebuildTicketPrefab();
    }

    static void SavePrefab(GameObject root, string path)
    {
        PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
    }

    static GameObject CreatePropDesk()
    {
        GameObject root = new GameObject("Prop_Desk");
        Material mat = GetMat("Prop_Desk");

        GameObject top = CreatePrimitiveChild(root.transform, PrimitiveType.Cube, "Top", mat, new Vector3(1.4f, 0.08f, 0.7f), new Vector3(0f, 0.75f, 0f));
        CreatePrimitiveChild(root.transform, PrimitiveType.Cube, "LegFL", mat, new Vector3(0.08f, 0.75f, 0.08f), new Vector3(-0.6f, 0.375f, 0.28f));
        CreatePrimitiveChild(root.transform, PrimitiveType.Cube, "LegFR", mat, new Vector3(0.08f, 0.75f, 0.08f), new Vector3(0.6f, 0.375f, 0.28f));
        CreatePrimitiveChild(root.transform, PrimitiveType.Cube, "LegBL", mat, new Vector3(0.08f, 0.75f, 0.08f), new Vector3(-0.6f, 0.375f, -0.28f));
        CreatePrimitiveChild(root.transform, PrimitiveType.Cube, "LegBR", mat, new Vector3(0.08f, 0.75f, 0.08f), new Vector3(0.6f, 0.375f, -0.28f));
        return root;
    }

    static GameObject CreatePropComputer()
    {
        GameObject root = new GameObject("Prop_Computer");
        Material mat = GetMat("Prop_Computer");
        Material light = GetMat("Prop_Light");

        CreatePrimitiveChild(root.transform, PrimitiveType.Cube, "Monitor", mat, new Vector3(0.55f, 0.42f, 0.06f), new Vector3(0f, 0.55f, 0f));
        CreatePrimitiveChild(root.transform, PrimitiveType.Cube, "Screen", light, new Vector3(0.42f, 0.28f, 0.02f), new Vector3(0f, 0.58f, 0.04f));
        CreatePrimitiveChild(root.transform, PrimitiveType.Cube, "Base", mat, new Vector3(0.18f, 0.04f, 0.18f), new Vector3(0f, 0.28f, 0f));
        return root;
    }

    static GameObject CreatePropPrinter()
    {
        GameObject root = new GameObject("Prop_Printer");
        Material mat = GetMat("Prop_Printer");

        CreatePrimitiveChild(root.transform, PrimitiveType.Cube, "Body", mat, new Vector3(0.55f, 0.28f, 0.45f), new Vector3(0f, 0.14f, 0f));
        CreatePrimitiveChild(root.transform, PrimitiveType.Cube, "Tray", mat, new Vector3(0.45f, 0.04f, 0.2f), new Vector3(0f, 0.02f, 0.28f));
        return root;
    }

    static GameObject CreatePropRouter()
    {
        GameObject root = new GameObject("Prop_Router");
        Material mat = GetMat("Prop_Router");
        Material light = GetMat("Prop_Light");

        CreatePrimitiveChild(root.transform, PrimitiveType.Cube, "Body", mat, new Vector3(0.7f, 0.12f, 0.45f), new Vector3(0f, 0.06f, 0f));
        CreatePrimitiveChild(root.transform, PrimitiveType.Cylinder, "AntennaL", mat, new Vector3(0.04f, 0.22f, 0.04f), new Vector3(-0.22f, 0.2f, 0f));
        CreatePrimitiveChild(root.transform, PrimitiveType.Cylinder, "AntennaR", mat, new Vector3(0.04f, 0.22f, 0.04f), new Vector3(0.22f, 0.2f, 0f));
        CreatePrimitiveChild(root.transform, PrimitiveType.Sphere, "Led", light, new Vector3(0.06f, 0.06f, 0.06f), new Vector3(0.28f, 0.1f, 0.12f));
        return root;
    }

    static GameObject CreatePropServerRack()
    {
        GameObject root = new GameObject("Prop_ServerRack");
        Material mat = GetMat("Prop_ServerRack");
        Material light = GetMat("Prop_Light");

        CreatePrimitiveChild(root.transform, PrimitiveType.Cube, "Frame", mat, new Vector3(0.65f, 1.4f, 0.55f), new Vector3(0f, 0.7f, 0f));
        for (int i = 0; i < 3; i++)
        {
            CreatePrimitiveChild(root.transform, PrimitiveType.Cube, $"Unit{i + 1}", light, new Vector3(0.55f, 0.12f, 0.45f), new Vector3(0f, 0.35f + i * 0.35f, 0f));
        }

        return root;
    }

    static GameObject CreatePropHospitalBed()
    {
        GameObject root = new GameObject("Prop_HospitalBed");
        Material mat = GetMat("Prop_Bed");

        CreatePrimitiveChild(root.transform, PrimitiveType.Cube, "Mattress", mat, new Vector3(1.2f, 0.18f, 0.55f), new Vector3(0f, 0.25f, 0f));
        CreatePrimitiveChild(root.transform, PrimitiveType.Cube, "Pillow", mat, new Vector3(0.35f, 0.1f, 0.35f), new Vector3(-0.35f, 0.38f, 0f));
        CreatePrimitiveChild(root.transform, PrimitiveType.Cube, "Frame", mat, new Vector3(1.25f, 0.08f, 0.6f), new Vector3(0f, 0.12f, 0f));
        return root;
    }

    static GameObject CreatePropSign(string text)
    {
        GameObject root = new GameObject("Prop_Sign");
        Material mat = GetMat("Prop_Sign");
        CreatePrimitiveChild(root.transform, PrimitiveType.Cube, "Board", mat, new Vector3(1.2f, 0.45f, 0.06f), new Vector3(0f, 0.9f, 0f));
        WorldLabelsSetup.CreateOrUpdateLabel(root.transform, "SignLabel", text, new Vector3(0f, 0.9f, -0.05f), 0.06f);
        return root;
    }

    static GameObject CreatePlayerVisual(bool isJ1)
    {
        GameObject root = new GameObject("PlayerVisual");
        Material bodyMat = GetMat(isJ1 ? "Player_J1" : "Player_J2");

        CreatePrimitiveChild(root.transform, PrimitiveType.Capsule, "Body", bodyMat, new Vector3(0.55f, 0.7f, 0.4f), new Vector3(0f, 0.55f, 0f));
        CreatePrimitiveChild(root.transform, PrimitiveType.Sphere, "Head", bodyMat, new Vector3(0.38f, 0.38f, 0.38f), new Vector3(0f, 1.05f, 0f));
        CreatePrimitiveChild(root.transform, PrimitiveType.Cube, "Badge", GetMat("Prop_Sign"), new Vector3(0.14f, 0.1f, 0.02f), new Vector3(0.18f, 0.82f, 0.18f));
        CreatePrimitiveChild(root.transform, PrimitiveType.Cube, "LaptopBag", GetMat("Prop_Computer"), new Vector3(0.22f, 0.28f, 0.12f), new Vector3(0f, 0.62f, -0.22f));
        CreatePrimitiveChild(root.transform, PrimitiveType.Cube, "ToolBelt", bodyMat, new Vector3(0.42f, 0.08f, 0.14f), new Vector3(0f, 0.38f, 0.16f));
        return root;
    }

    static GameObject CreateTicketVisualCable()
    {
        GameObject root = new GameObject("TicketVisual_Cable");
        Material body = GetMat("Ticket_Medium");

        CreatePrimitiveChild(root.transform, PrimitiveType.Cube, "Post", body, new Vector3(0.25f, 0.55f, 0.25f), new Vector3(0f, 0.28f, 0f));
        CreatePrimitiveChild(root.transform, PrimitiveType.Cylinder, "CableRed", GetMat("Prop_CableRed"), new Vector3(0.04f, 0.35f, 0.04f), new Vector3(-0.08f, 0.45f, 0.08f));
        CreatePrimitiveChild(root.transform, PrimitiveType.Cylinder, "CableBlue", GetMat("Prop_CableBlue"), new Vector3(0.04f, 0.35f, 0.04f), new Vector3(0.08f, 0.45f, -0.05f));
        CreatePrimitiveChild(root.transform, PrimitiveType.Cylinder, "CableYellow", GetMat("Prop_CableYellow"), new Vector3(0.04f, 0.35f, 0.04f), new Vector3(0f, 0.45f, 0.1f));
        WorldLabelsSetup.CreateOrUpdateLabel(root.transform, "PriorityLabel", "!", new Vector3(0f, 0.75f, 0f), 0.08f);
        return root;
    }

    static GameObject CreateTicketVisualRouter()
    {
        GameObject root = new GameObject("TicketVisual_Router");
        GameObject router = CreatePropRouter();
        router.transform.SetParent(root.transform);
        router.transform.localPosition = new Vector3(0f, 0.15f, 0f);
        router.transform.localScale = Vector3.one * 0.85f;
        WorldLabelsSetup.CreateOrUpdateLabel(root.transform, "PriorityLabel", "!", new Vector3(0f, 0.65f, 0f), 0.08f);
        return root;
    }

    static void RebuildTicketPrefab()
    {
        GameObject ticketRoot = new GameObject("Ticket");
        ticketRoot.transform.localScale = Vector3.one;

        BoxCollider trigger = ticketRoot.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = new Vector3(1.1f, 1.2f, 1.1f);
        trigger.center = new Vector3(0f, 0.5f, 0f);

        ticketRoot.AddComponent<Ticket>();

        GameObject cableVisual = CreateTicketVisualCable();
        cableVisual.name = "TicketVisual_Cable";
        cableVisual.transform.SetParent(ticketRoot.transform);
        cableVisual.transform.localPosition = Vector3.zero;

        GameObject routerVisual = CreateTicketVisualRouter();
        routerVisual.name = "TicketVisual_Router";
        routerVisual.transform.SetParent(ticketRoot.transform);
        routerVisual.transform.localPosition = Vector3.zero;
        routerVisual.SetActive(false);

        PrefabUtility.SaveAsPrefabAsset(ticketRoot, TicketPrefabPath);
        Object.DestroyImmediate(ticketRoot);
    }

    static GameObject CreatePrimitiveChild(Transform parent, PrimitiveType type, string name, Material mat, Vector3 scale, Vector3 localPos)
    {
        GameObject go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.SetParent(parent);
        go.transform.localScale = scale;
        go.transform.localPosition = localPos;
        go.transform.localRotation = Quaternion.identity;
        Object.DestroyImmediate(go.GetComponent<Collider>());
        go.GetComponent<MeshRenderer>().sharedMaterial = mat;
        return go;
    }

    static void ApplyToActiveScene()
    {
        ApplyEnvironment();
        ApplyZone("Zone_Recepcion", "Zone_Reception", "Recepción", BuildReceptionProps);
        ApplyZone("Zone_Urgencias", "Zone_Emergency", "Urgencias", BuildEmergencyProps);
        ApplyZone("Zone_CuartoServidores", "Zone_ServerRoom", "Cuarto Servidores", BuildServerRoomProps);
        ApplyPlayers();
        ApplyLighting();
    }

    static void ApplyEnvironment()
    {
        GameObject floor = GameObject.Find("Floor");
        if (floor != null)
        {
            MeshRenderer renderer = floor.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = GetMat("Floor_Hospital");
            }
        }

        Transform environment = GameObject.Find("Environment")?.transform;
        if (environment == null)
        {
            return;
        }

        Transform walls = environment.Find("Walls");
        if (walls == null)
        {
            GameObject wallsRoot = new GameObject("Walls");
            wallsRoot.transform.SetParent(environment);
            wallsRoot.transform.localPosition = Vector3.zero;
            walls = wallsRoot.transform;
        }

        ClearChildren(walls);
        Material wallMat = GetMat("Wall_Hospital");
        CreatePrimitiveChild(walls, PrimitiveType.Cube, "WallNorth", wallMat, new Vector3(26f, 2.5f, 0.3f), new Vector3(0f, 1.25f, 13f));
        CreatePrimitiveChild(walls, PrimitiveType.Cube, "WallSouth", wallMat, new Vector3(26f, 2.5f, 0.3f), new Vector3(0f, 1.25f, -13f));
        CreatePrimitiveChild(walls, PrimitiveType.Cube, "WallEast", wallMat, new Vector3(0.3f, 2.5f, 26f), new Vector3(13f, 1.25f, 0f));
        CreatePrimitiveChild(walls, PrimitiveType.Cube, "WallWest", wallMat, new Vector3(0.3f, 2.5f, 26f), new Vector3(-13f, 1.25f, 0f));
    }

    static void ApplyZone(string zoneName, string materialKey, string label, System.Action<Transform> buildProps)
    {
        GameObject zone = GameObject.Find(zoneName);
        if (zone == null)
        {
            return;
        }

        MeshRenderer renderer = zone.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = GetMat(materialKey);
        }

        WorldLabelsSetup.CreateOrUpdateLabel(zone.transform, "ZoneLabel", label, new Vector3(0f, 1.6f, 0f), 0.12f);

        Transform props = zone.transform.Find("ZoneProps");
        if (props == null)
        {
            GameObject propsRoot = new GameObject("ZoneProps");
            propsRoot.transform.SetParent(zone.transform);
            propsRoot.transform.localPosition = Vector3.zero;
            props = propsRoot.transform;
        }

        ClearChildren(props);
        buildProps(props);
    }

    static void BuildReceptionProps(Transform parent)
    {
        PlacePrefab($"{PrefabFolder}/Prop_Desk.prefab", parent, new Vector3(0f, 0f, -1.2f), 1f);
        PlacePrefab($"{PrefabFolder}/Prop_Computer.prefab", parent, new Vector3(0f, 0.78f, -1.2f), 1f);
        PlacePrefab($"{PrefabFolder}/Prop_Printer.prefab", parent, new Vector3(1.1f, 0.78f, -0.8f), 1f);
        PlacePropSign(parent, "Recepción", new Vector3(-1.8f, 0f, 1.8f));
    }

    static void BuildEmergencyProps(Transform parent)
    {
        PlacePrefab($"{PrefabFolder}/Prop_HospitalBed.prefab", parent, new Vector3(-1f, 0f, 0.5f), 1f);
        PlacePrefab($"{PrefabFolder}/Prop_Computer.prefab", parent, new Vector3(1.4f, 0f, -1.2f), 0.9f);
        PlacePropSign(parent, "Urgencias", new Vector3(1.8f, 0f, 1.6f));
    }

    static void BuildServerRoomProps(Transform parent)
    {
        PlacePrefab($"{PrefabFolder}/Prop_ServerRack.prefab", parent, new Vector3(-1.5f, 0f, -1f), 1f);
        PlacePrefab($"{PrefabFolder}/Prop_ServerRack.prefab", parent, new Vector3(1.5f, 0f, -1f), 1f);
        PlacePrefab($"{PrefabFolder}/Prop_Router.prefab", parent, new Vector3(0f, 0f, 1.4f), 1.2f);
        CreatePrimitiveChild(parent, PrimitiveType.Cube, "CableRun", GetMat("Prop_CableYellow"), new Vector3(2.2f, 0.03f, 0.08f), new Vector3(0f, 0.05f, 0.2f));
        PlacePropSign(parent, "Cuarto Servidores", new Vector3(0f, 0f, 2.2f));

        Transform hideZone = parent.parent.Find("HideZone");
        if (hideZone != null)
        {
            WorldLabelsSetup.CreateOrUpdateLabel(hideZone, "HideZoneLabel", "HideZone IT", new Vector3(0f, 1.8f, 0f), 0.08f);
        }
    }

    static void PlacePropSign(Transform parent, string text, Vector3 localPos)
    {
        GameObject sign = CreatePropSign(text);
        sign.transform.SetParent(parent);
        sign.transform.localPosition = localPos;
        sign.transform.localRotation = Quaternion.identity;
        sign.transform.localScale = Vector3.one;
    }

    static void PlacePrefab(string prefabPath, Transform parent, Vector3 localPos, float scale)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            return;
        }

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        instance.transform.localPosition = localPos;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one * scale;
    }

    static void ApplyPlayers()
    {
        ApplyPlayerVisual("Player1", 1, $"{PrefabFolder}/PlayerVisual_ITTech_J1.prefab");
        ApplyPlayerVisual("Player2", 2, $"{PrefabFolder}/PlayerVisual_ITTech_J2.prefab");
    }

    static void ApplyPlayerVisual(string objectName, int playerIndex, string visualPrefabPath)
    {
        GameObject player = GameObject.Find(objectName);
        if (player == null)
        {
            PlayerController[] players = Object.FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
            foreach (PlayerController controller in players)
            {
                if (controller.PlayerIndex == playerIndex)
                {
                    player = controller.gameObject;
                    break;
                }
            }
        }

        if (player == null)
        {
            return;
        }

        MeshRenderer rootRenderer = player.GetComponent<MeshRenderer>();
        if (rootRenderer != null)
        {
            rootRenderer.enabled = false;
        }

        Transform oldVisual = player.transform.Find("PlayerVisual");
        if (oldVisual != null)
        {
            Object.DestroyImmediate(oldVisual.gameObject);
        }

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(visualPrefabPath);
        if (prefab != null)
        {
            GameObject visual = (GameObject)PrefabUtility.InstantiatePrefab(prefab, player.transform);
            visual.name = "PlayerVisual";
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = Vector3.one;
        }

        Transform indicator = player.transform.Find("IncapacitatedIndicator");
        if (indicator == null)
        {
            GameObject indicatorObject = new GameObject("IncapacitatedIndicator");
            indicatorObject.transform.SetParent(player.transform);
            indicatorObject.transform.localPosition = new Vector3(0f, 1.55f, 0f);
            WorldLabelsSetup.CreateOrUpdateLabel(indicatorObject.transform, "IncapLabel", "ZZZ", Vector3.zero, 0.1f);
            indicatorObject.SetActive(false);
        }

        WorldLabelsSetup.CreateOrUpdateLabel(
            player.transform,
            "PlayerLabel",
            playerIndex == 1 ? "J1" : "J2",
            new Vector3(0f, 1.45f, 0f),
            0.1f);
    }

    static void ApplyLighting()
    {
        Light[] lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach (Light light in lights)
        {
            if (light.type != LightType.Directional)
            {
                continue;
            }

            light.intensity = 1.25f;
            light.color = new Color(1f, 0.98f, 0.92f);
            light.shadows = LightShadows.Soft;
            light.transform.rotation = Quaternion.Euler(52f, -35f, 0f);
        }

        Camera camera = Camera.main;
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.18f, 0.22f, 0.28f);
        }
    }

    static void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Object.DestroyImmediate(parent.GetChild(i).gameObject);
        }
    }
}
#endif
