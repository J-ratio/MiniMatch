#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public class CardDataBulkCreator : MonoBehaviour
{
    [MenuItem("Tools/Bulk Create CardData from Sprites/Cards")] 
    public static void BulkCreateCardData()
    {
        string spritesPath = "Assets/Sprites/Cards";
        string soPath = "Assets/ScriptableObjects";
        if (!Directory.Exists(soPath))
            Directory.CreateDirectory(soPath);
        var guids = AssetDatabase.FindAssets("t:Sprite", new[] { spritesPath });
        int created = 0;
        foreach (var guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (sprite == null) continue;
            string name = Path.GetFileNameWithoutExtension(assetPath);
            CardData cardData = ScriptableObject.CreateInstance<CardData>();
            cardData.id = created;
            cardData.image = sprite;
            string assetName = $"CardData_{name}.asset";
            AssetDatabase.CreateAsset(cardData, Path.Combine(soPath, assetName));
            created++;
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Created {created} CardData assets in {soPath}");
    }
}
#endif
