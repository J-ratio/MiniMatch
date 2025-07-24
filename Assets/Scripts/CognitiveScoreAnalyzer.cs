using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CognitiveScoreAnalyzer
{
    public class CognitiveAssessment
    {
        public float MemoryScore { get; set; }
        public float ResponseSpeed { get; set; }
        public float ErrorRate { get; set; }
        public float LearningCurve { get; set; }
        public string CognitiveStatus { get; set; }
    }

    public CognitiveAssessment AnalyzeMetrics(List<LevelMetrics> metrics)
    {
        var assessment = new CognitiveAssessment();
        
        // Analyze memory performance
        assessment.MemoryScore = CalculateMemoryScore(metrics);
        
        // Analyze response speed
        assessment.ResponseSpeed = CalculateResponseSpeed(metrics);
        
        // Analyze error patterns
        assessment.ErrorRate = CalculateErrorRate(metrics);
        
        // Analyze learning progression
        assessment.LearningCurve = CalculateLearningCurve(metrics);
        
        // Determine cognitive status
        assessment.CognitiveStatus = DetermineCognitiveStatus(
            assessment.MemoryScore,
            assessment.ResponseSpeed,
            assessment.ErrorRate,
            assessment.LearningCurve
        );
        
        return assessment;
    }

    private float CalculateMemoryScore(List<LevelMetrics> metrics)
    {
        float score = 100;
        foreach (var metric in metrics)
        {
            // Deduct for perseverative errors
            score -= metric.perseverativeErrors * 5;
            
            // Deduct for repeated mistakes
            foreach (var mistake in metric.repeatedMistakes)
            {
                score -= mistake.Value * 2;
            }
        }
        return Mathf.Max(0, score);
    }

    private float CalculateResponseSpeed(List<LevelMetrics> metrics)
    {
        var allTimes = metrics.SelectMany(m => m.decisionTimes).ToList();
        return allTimes.Count > 0 ? allTimes.Average() : 0;
    }

    private float CalculateErrorRate(List<LevelMetrics> metrics)
    {
        float totalMistakes = metrics.Sum(m => m.mistakes);
        float totalFlips = metrics.Sum(m => m.flips);
        return totalFlips > 0 ? (totalMistakes / totalFlips) * 100 : 0;
    }

    private float CalculateLearningCurve(List<LevelMetrics> metrics)
    {
        if (metrics.Count < 2) return 0;
        
        float initialPerformance = metrics[0].mistakes;
        float finalPerformance = metrics[metrics.Count - 1].mistakes;
        return (initialPerformance - finalPerformance) / metrics.Count;
    }

    private string DetermineCognitiveStatus(float memoryScore, float responseSpeed, float errorRate, float learningCurve)
    {
        if (memoryScore < 50 && errorRate > 40 && responseSpeed > 5.0f)
            return "Significant Cognitive Concern";
        else if (memoryScore < 70 && errorRate > 30)
            return "Moderate Cognitive Concern";
        else if (memoryScore < 85 && errorRate > 20)
            return "Mild Cognitive Concern";
        else
            return "Normal Cognitive Function";
    }
}
