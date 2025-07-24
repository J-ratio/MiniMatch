using System;
using System.Collections.Generic;

[Serializable]
public class LevelMetrics
{
    public float timeTaken;
    public int flips;
    public int mistakes;
    public List<float> flipTimes = new List<float>();
    public Dictionary<string, int> repeatedMistakes = new Dictionary<string, int>(); // Use string key for serialization
    public List<float> decisionTimes = new List<float>();
    public int perseverativeErrors; // Repeated same mistake
}

