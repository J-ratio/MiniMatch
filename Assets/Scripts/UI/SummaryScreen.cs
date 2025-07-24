using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class SummaryScreen : MonoBehaviour
{
    public GameObject panel;
    public TextMeshProUGUI summaryText;

    public void ShowSummary(List<LevelMetrics> metrics)
    {
        panel.SetActive(true);
        var analyzer = new CognitiveScoreAnalyzer();
        var assessment = analyzer.AnalyzeMetrics(metrics);
        
        string summary = "<b>Cognitive Assessment Summary</b>\n\n";
        
        // Basic metrics
        float totalTime = metrics.Sum(m => m.timeTaken);
        int totalFlips = metrics.Sum(m => m.flips);
        int totalMistakes = metrics.Sum(m => m.mistakes);
        
        // Level details
        for (int i = 0; i < metrics.Count; i++)
        {
            var m = metrics[i];
            summary += $"<b>Level {i+1}</b> - Time: {m.timeTaken:F1}s, Flips: {m.flips}, Mistakes: {m.mistakes}\n";
        }
        
        // Cognitive assessment results
        summary += $"\n<b>Cognitive Assessment</b>\n";
        summary += $"Memory Score: {assessment.MemoryScore:F1}%\n";
        summary += $"Response Speed: {assessment.ResponseSpeed:F2}s\n";
        summary += $"Error Rate: {assessment.ErrorRate:F1}%\n";
        summary += $"Learning Curve: {assessment.LearningCurve:F2}\n";
        summary += $"\n<b>Assessment Result:</b> {assessment.CognitiveStatus}";
        
        summaryText.text = summary;
    }
}

