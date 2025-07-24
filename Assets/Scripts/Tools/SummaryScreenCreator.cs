#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.UI;

public class SummaryScreenCreator : MonoBehaviour
{
    [MenuItem("Tools/Create Summary Screen")] 
    public static void CreateSummaryScreen()
    {
        // Find or create Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasObj.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        // Create panel
        GameObject panel = new GameObject("SummaryPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panel.transform.SetParent(canvas.transform);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(500, 400);
        panelRect.anchoredPosition = Vector2.zero;
        Image panelImage = panel.GetComponent<Image>();
        panelImage.color = new Color(1, 1, 1, 0.95f);

        // Create TMP text
        GameObject textObj = new GameObject("SummaryText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(panel.transform);
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(20, 20);
        textRect.offsetMax = new Vector2(-20, -20);
        TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
        tmp.text = "Summary will appear here.";
        tmp.fontSize = 28;
        tmp.alignment = TextAlignmentOptions.TopLeft;
        tmp.color = Color.black;

        // Add SummaryScreen script
        SummaryScreen summaryScreen = panel.AddComponent<SummaryScreen>();
        summaryScreen.panel = panel;
        summaryScreen.summaryText = tmp;

        Debug.Log("SummaryScreen created and set up under Canvas.");
        Selection.activeGameObject = panel;
    }
}
#endif
