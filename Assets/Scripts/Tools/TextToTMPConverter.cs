#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class TextToTMPConverter : MonoBehaviour
{
    [MenuItem("Tools/Convert All Text To TMP")]
    public static void ConvertAllTextToTMP()
    {
        Text[] texts = GameObject.FindObjectsOfType<Text>(true);
        int converted = 0;
        foreach (var text in texts)
        {
            GameObject go = text.gameObject;
            string oldText = text.text;
            int fontSize = text.fontSize;
            Color color = text.color;
            TextAnchor alignment = text.alignment;
            RectTransform rect = go.GetComponent<RectTransform>();
            Vector2 anchorMin = rect.anchorMin;
            Vector2 anchorMax = rect.anchorMax;
            Vector2 pivot = rect.pivot;
            Vector2 anchoredPosition = rect.anchoredPosition;
            Vector2 sizeDelta = rect.sizeDelta;

            DestroyImmediate(text);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = oldText;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = (TextAlignmentOptions)alignment;
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
            converted++;
        }
        Debug.Log($"Converted {converted} Text components to TextMeshProUGUI.");
    }
}
#endif
