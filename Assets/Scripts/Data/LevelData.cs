using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "MemoryMatch/LevelData", order = 2)]
public class LevelData : ScriptableObject
{
    public int rows = 3;
    public int cols = 4;
    public int numPairs = 6;
    public float previewTime = 2f;
}

