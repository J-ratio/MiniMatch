#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class GridSetupCreator : MonoBehaviour
{
    [MenuItem("Tools/Create Card Grid Parent")] 
    public static void CreateGridParent()
    {
        // Find or create Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasObj.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        // Create Panel
        GameObject gridPanel = new GameObject("CardGrid", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(GridLayoutGroup));
        gridPanel.transform.SetParent(canvas.transform);
        RectTransform rect = gridPanel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(500, 400);
        rect.anchoredPosition = Vector2.zero;

        // Configure GridLayoutGroup
        GridLayoutGroup grid = gridPanel.GetComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(100, 140);
        grid.spacing = new Vector2(10, 10);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 4;
        grid.childAlignment = TextAnchor.MiddleCenter;

        // Save as prefab
        string prefabPath = "Assets/Prefabs/CardGrid.prefab";
        PrefabUtility.SaveAsPrefabAsset(gridPanel, prefabPath);
        Debug.Log("Card grid parent prefab created at " + prefabPath);
        GameObject.DestroyImmediate(gridPanel);
    }
}
#endif
