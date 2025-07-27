using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MiniMatch.Domain.ValueObjects;

namespace MiniMatch.Domain.Services
{
    /// <summary>
    /// Calculates scores based on gameplay metrics
    /// </summary>
    public class ScoreCalculationService
    {
        private const int BASE_SCORE_PER_MATCH = 100;
        private const int TIME_BONUS_THRESHOLD = 30; // seconds
        private const int MISTAKE_PENALTY = 25;
        private const int PERFECT_LEVEL_BONUS = 200;
        
        public Score CalculateLevelScore(LevelMetrics metrics)
        {
            if (metrics == null) return new Score(0);
            
            int score = 0;
            
            // Base score for completion
            int estimatedPairs = metrics.flips / 2; // Rough estimate
            score += estimatedPairs * BASE_SCORE_PER_MATCH;
            
            // Time bonus (faster completion = higher bonus)
            if (metrics.timeTaken < TIME_BONUS_THRESHOLD)
            {
                int timeBonus = Mathf.RoundToInt((TIME_BONUS_THRESHOLD - metrics.timeTaken) * 10);
                score += timeBonus;
            }
            
            // Mistake penalty
            score -= metrics.mistakes * MISTAKE_PENALTY;
            
            // Perfect level bonus (no mistakes)
            if (metrics.mistakes == 0)
            {
                score += PERFECT_LEVEL_BONUS;
            }
            
            // Perseverative error penalty (extra penalty for repeated mistakes)
            score -= metrics.perseverativeErrors * (MISTAKE_PENALTY / 2);
            
            return new Score(Mathf.Max(0, score));
        }
        
        public Score CalculateTotalScore(List<LevelMetrics> allMetrics)
        {
            if (allMetrics == null || allMetrics.Count == 0) return new Score(0);
            
            int totalScore = 0;
            
            foreach (var metrics in allMetrics)
            {
                totalScore += CalculateLevelScore(metrics).Value;
            }
            
            // Completion bonus for finishing all levels
            int completionBonus = allMetrics.Count * 50;
            totalScore += completionBonus;
            
            // Consistency bonus (reward for improving over time)
            int consistencyBonus = CalculateConsistencyBonus(allMetrics);
            totalScore += consistencyBonus;
            
            return new Score(totalScore);
        }
        
        public ScoreBreakdown GetScoreBreakdown(LevelMetrics metrics)
        {
            var breakdown = new ScoreBreakdown();
            
            if (metrics == null) return breakdown;
            
            int estimatedPairs = metrics.flips / 2;
            breakdown.BaseScore = estimatedPairs * BASE_SCORE_PER_MATCH;
            
            if (metrics.timeTaken < TIME_BONUS_THRESHOLD)
            {
                breakdown.TimeBonus = Mathf.RoundToInt((TIME_BONUS_THRESHOLD - metrics.timeTaken) * 10);
            }
            
            breakdown.MistakePenalty = metrics.mistakes * MISTAKE_PENALTY;
            
            if (metrics.mistakes == 0)
            {
                breakdown.PerfectBonus = PERFECT_LEVEL_BONUS;
            }
            
            breakdown.PerseverativePenalty = metrics.perseverativeErrors * (MISTAKE_PENALTY / 2);
            
            return breakdown;
        }
        
        private int CalculateConsistencyBonus(List<LevelMetrics> allMetrics)
        {
            if (allMetrics.Count < 2) return 0;
            
            // Check if mistake count is decreasing over time (learning)
            var mistakeCounts = allMetrics.Select(m => m.mistakes).ToList();
            bool isImproving = true;
            
            for (int i = 1; i < mistakeCounts.Count; i++)
            {
                if (mistakeCounts[i] > mistakeCounts[i - 1])
                {
                    isImproving = false;
                    break;
                }
            }
            
            return isImproving ? 100 : 0;
        }
    }
    
    public class ScoreBreakdown
    {
        public int BaseScore { get; set; }
        public int TimeBonus { get; set; }
        public int MistakePenalty { get; set; }
        public int PerfectBonus { get; set; }
        public int PerseverativePenalty { get; set; }
        
        public int TotalScore => BaseScore + TimeBonus + PerfectBonus - MistakePenalty - PerseverativePenalty;
    }
}