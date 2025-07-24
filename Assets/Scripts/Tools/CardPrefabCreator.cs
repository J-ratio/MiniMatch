#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class CardPrefabCreator : MonoBehaviour
{
    [MenuItem("Tools/Create Card Prefab")]
    public static void CreateCardPrefab()
    {
        // Create root GameObject (Button)
        GameObject card = new GameObject("Card", typeof(RectTransform), typeof(CanvasRenderer), typeof(Button));
        Button button = card.GetComponent<Button>();

        // Add Card script
        Card cardScript = card.AddComponent<Card>();

        // Create Front child
        GameObject front = new GameObject("Front", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        front.transform.SetParent(card.transform);
        Image frontImage = front.GetComponent<Image>();

        // Create Back child
        GameObject back = new GameObject("Back", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        back.transform.SetParent(card.transform);
        Image backImage = back.GetComponent<Image>();

        // Set RectTransform anchors and size
        RectTransform cardRect = card.GetComponent<RectTransform>();
        cardRect.sizeDelta = new Vector2(100, 140);
        RectTransform frontRect = front.GetComponent<RectTransform>();
        frontRect.anchorMin = Vector2.zero;
        frontRect.anchorMax = Vector2.one;
        frontRect.offsetMin = Vector2.zero;
        frontRect.offsetMax = Vector2.zero;
        RectTransform backRect = back.GetComponent<RectTransform>();
        backRect.anchorMin = Vector2.zero;
        backRect.anchorMax = Vector2.one;
        backRect.offsetMin = Vector2.zero;
        backRect.offsetMax = Vector2.zero;

        // Assign Card script fields
        cardScript.frontImage = frontImage;
        cardScript.front = front;
        cardScript.back = back;

        // Save as prefab
        string prefabPath = "Assets/Prefabs/Card.prefab";
        PrefabUtility.SaveAsPrefabAsset(card, prefabPath);
        GameObject.DestroyImmediate(card);
        Debug.Log("Card prefab created at " + prefabPath);
    }
}
#endif
