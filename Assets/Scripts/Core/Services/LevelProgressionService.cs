using System.Collections.Generic;
using UnityEngine;
using MiniMatch.Core.Events;
using MiniMatch.Core.Services;

namespace MiniMatch.Core.Services
{
    /// <summary>
    /// Manages level progression and unlocking logic
    /// </summary>
    public class LevelProgressionService
    {
        public int CurrentLevelIndex { get; private set; }
        public int TotalLevels { get; private set; }
        public bool IsGameComplete => CurrentLevelIndex >= TotalLevels;
        
        private readonly GameEventBus _eventBus;
        private readonly IMetricsCollector _metricsCollector;
        private readonly LevelData[] _levelData;
        
        public LevelProgressionService(LevelData[] levelData, IMetricsCollector metricsCollector, GameEventBus eventBus = null)
        {
            _levelData = levelData ?? throw new System.ArgumentNullException(nameof(levelData));
            _metricsCollector = metricsCollector ?? throw new System.ArgumentNullException(nameof(metricsCollector));
            _eventBus = eventBus ?? GameEventBus.Instance;
            
            TotalLevels = _levelData.Length;
            CurrentLevelIndex = 0;
            
            SubscribeToEvents();
        }
        
        public LevelData GetCurrentLevel()
        {
            if (CurrentLevelIndex >= 0 && CurrentLevelIndex < _levelData.Length)
            {
                return _levelData[CurrentLevelIndex];
            }
            return null;
        }
        
        public LevelData GetLevel(int index)
        {
            if (index >= 0 && index < _levelData.Length)
            {
                return _levelData[index];
            }
            return null;
        }
        
        public bool CanAdvanceToNextLevel()
        {
            // Simple progression - just complete the current level
            // Could be enhanced with score requirements, etc.
            return CurrentLevelIndex < TotalLevels - 1;
        }
        
        public bool AdvanceToNextLevel()
        {
            if (!CanAdvanceToNextLevel())
            {
                Debug.Log("Cannot advance - already at final level or game complete");
                return false;
            }
            
            CurrentLevelIndex++;
            Debug.Log($"Advanced to level {CurrentLevelIndex + 1}/{TotalLevels}");
            
            return true;
        }
        
        public void RestartCurrentLevel()
        {
            Debug.Log($"Restarting level {CurrentLevelIndex + 1}");
            // Level restart logic would go here
        }
        
        public void ResetProgression()
        {
            CurrentLevelIndex = 0;
            Debug.Log("Level progression reset to beginning");
        }
        
        public float GetProgressPercentage()
        {
            if (TotalLevels == 0) return 0f;
            return (float)CurrentLevelIndex / TotalLevels * 100f;
        }
        
        public LevelProgressionInfo GetProgressionInfo()
        {
            return new LevelProgressionInfo
            {
                CurrentLevel = CurrentLevelIndex + 1,
                TotalLevels = TotalLevels,
                ProgressPercentage = GetProgressPercentage(),
                IsComplete = IsGameComplete,
                CanAdvance = CanAdvanceToNextLevel()
            };
        }
        
        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<AllPairsMatchedEvent>(OnAllPairsMatched);
        }
        
        private void OnAllPairsMatched(AllPairsMatchedEvent completionEvent)
        {
            Debug.Log($"Level {CurrentLevelIndex + 1} completed!");
            
            if (IsGameComplete)
            {
                Debug.Log("🎉 Game Complete! All levels finished!");
                // Could trigger game completion ceremony, final score calculation, etc.
            }
        }
        
        ~LevelProgressionService()
        {
            _eventBus?.Unsubscribe<AllPairsMatchedEvent>(OnAllPairsMatched);
        }
    }
    
    public class LevelProgressionInfo
    {
        public int CurrentLevel { get; set; }
        public int TotalLevels { get; set; }
        public float ProgressPercentage { get; set; }
        public bool IsComplete { get; set; }
        public bool CanAdvance { get; set; }
    }
}