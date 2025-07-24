#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class UISetupCreator : MonoBehaviour
{
    [MenuItem("Tools/Create UI Elements")]
    public static void CreateUIElements()
    {
        // Find or create Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasObj.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        // Create Score Text
        GameObject scoreTextObj = new GameObject("ScoreText", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        scoreTextObj.transform.SetParent(canvas.transform);
        RectTransform scoreRect = scoreTextObj.GetComponent<RectTransform>();
        scoreRect.anchorMin = new Vector2(0, 1);
        scoreRect.anchorMax = new Vector2(0, 1);
        scoreRect.pivot = new Vector2(0, 1);
        scoreRect.anchoredPosition = new Vector2(20, -20);
        scoreRect.sizeDelta = new Vector2(200, 50);
        Text scoreText = scoreTextObj.GetComponent<Text>();
        scoreText.text = "Score: 0";
        scoreText.fontSize = 32;
        scoreText.alignment = TextAnchor.UpperLeft;
        scoreText.color = Color.black;
        scoreText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Create Restart Button
        GameObject restartBtnObj = new GameObject("RestartButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Button), typeof(Image));
        restartBtnObj.transform.SetParent(canvas.transform);
        RectTransform btnRect = restartBtnObj.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(1, 1);
        btnRect.anchorMax = new Vector2(1, 1);
        btnRect.pivot = new Vector2(1, 1);
        btnRect.anchoredPosition = new Vector2(-20, -20);
        btnRect.sizeDelta = new Vector2(160, 50);
        Image btnImage = restartBtnObj.GetComponent<Image>();
        btnImage.color = new Color(0.8f, 0.8f, 0.8f, 1f);
        Button btn = restartBtnObj.GetComponent<Button>();

        // Add button text
        GameObject btnTextObj = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        btnTextObj.transform.SetParent(restartBtnObj.transform);
        RectTransform btnTextRect = btnTextObj.GetComponent<RectTransform>();
        btnTextRect.anchorMin = Vector2.zero;
        btnTextRect.anchorMax = Vector2.one;
        btnTextRect.offsetMin = Vector2.zero;
        btnTextRect.offsetMax = Vector2.zero;
        Text btnText = btnTextObj.GetComponent<Text>();
        btnText.text = "Restart";
        btnText.fontSize = 28;
        btnText.alignment = TextAnchor.MiddleCenter;
        btnText.color = Color.black;
        btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        Debug.Log("ScoreText and RestartButton created under Canvas.");
        Selection.activeGameObject = scoreTextObj;
    }
}
#endif
