#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class TicketQueueSetup
{
    [MenuItem("IT Hospital/Setup Ticket Queue UI")]
    public static void SetupTicketQueueUI()
    {
        UIManager uiManager = Object.FindFirstObjectByType<UIManager>();
        if (uiManager == null)
        {
            EditorUtility.DisplayDialog("Setup Ticket Queue", "No se encontró UIManager en la escena.", "OK");
            return;
        }

        Canvas canvas = uiManager.GetComponent<Canvas>() ?? uiManager.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("Setup Ticket Queue", "No se encontró Canvas en la escena.", "OK");
            return;
        }

        Font font = GetDefaultFont();

        Transform titleTransform = canvas.transform.Find("TicketQueueTitleText");
        Text titleText = titleTransform != null ? titleTransform.GetComponent<Text>() : CreateQueueLabel(
            canvas.transform,
            "TicketQueueTitleText",
            "COLA DE TICKETS",
            font,
            16,
            FontStyle.Bold,
            new Vector2(-10f, -145f),
            new Vector2(1f, 1f),
            new Vector2(360f, 24f),
            TextAnchor.UpperRight);

        Transform queueTransform = canvas.transform.Find("TicketQueueText");
        Text queueText = queueTransform != null ? queueTransform.GetComponent<Text>() : CreateQueueLabel(
            canvas.transform,
            "TicketQueueText",
            "(sin tickets activos)",
            font,
            13,
            FontStyle.Normal,
            new Vector2(-10f, -170f),
            new Vector2(1f, 1f),
            new Vector2(360f, 180f),
            TextAnchor.UpperRight);

        queueText.alignment = TextAnchor.UpperRight;
        queueText.horizontalOverflow = HorizontalWrapMode.Wrap;
        queueText.verticalOverflow = VerticalWrapMode.Overflow;

        SerializedObject uiSO = new SerializedObject(uiManager);
        uiSO.FindProperty("ticketQueueTitleText").objectReferenceValue = titleText;
        uiSO.FindProperty("ticketQueueText").objectReferenceValue = queueText;
        uiSO.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog(
            "Ticket Queue listo",
            "Cola de tickets conectada al UIManager.\n\nGuarda la escena (Ctrl+S) y presiona Play.",
            "OK");
    }

    static Text CreateQueueLabel(
        Transform parent,
        string objectName,
        string content,
        Font font,
        int fontSize,
        FontStyle fontStyle,
        Vector2 anchoredPosition,
        Vector2 anchor,
        Vector2 size,
        TextAnchor alignment)
    {
        GameObject textObject = new GameObject(objectName);
        textObject.transform.SetParent(parent, false);

        Text text = textObject.AddComponent<Text>();
        text.font = font;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.alignment = alignment;
        text.color = Color.white;
        text.text = content;
        text.raycastTarget = false;

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = anchor;
        rect.anchoredPosition = anchoredPosition;
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
