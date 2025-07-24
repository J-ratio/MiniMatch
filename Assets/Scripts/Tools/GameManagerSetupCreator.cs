#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class GameManagerSetupCreator : MonoBehaviour
{
    [MenuItem("Tools/Create GameManager Setup")]
    public static void CreateGameManagerSetup()
    {
        // Check if GameManager already exists
        GameManager existing = FindObjectOfType<GameManager>();
        if (existing != null)
        {
            Debug.LogWarning("A GameManager already exists in the scene.");
            Selection.activeGameObject = existing.gameObject;
            return;
        }

        // Create GameManager GameObject
        GameObject gmObj = new GameObject("GameManager");
        GameManager gm = gmObj.AddComponent<GameManager>();

        // Log instructions for user
        Debug.Log("GameManager created. Assign the Card Prefab, Grid Parent, and Card Icons in the Inspector.");
        Selection.activeGameObject = gmObj;
    }
}
#endif
