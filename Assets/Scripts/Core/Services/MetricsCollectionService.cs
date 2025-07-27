using System.Collections.Generic;
using UnityEngine;
using MiniMatch.Core.Services;
using MiniMatch.Core.Events;

namespace MiniMatch.Core.Services
{
    /// <summary>
    /// Collects and tracks gameplay metrics for cognitive analysis
    /// </summary>
    public class MetricsCollectionService : IMetricsCollector
    {
        private readonly GameEventBus _eventBus;
        private readonly List<LevelMetrics> _allMetrics = new();
        private LevelMetrics _currentLevelMetrics;
        private float _lastCardFlipTime = 0f;
        
        public MetricsCollectionService(GameEventBus eventBus = null)
        {
            _eventBus = eventBus ?? GameEventBus.Instance;
            SubscribeToEvents();
        }
        
        public void StartLevel(int levelIndex)
        {
            _currentLevelMetrics = new LevelMetrics();
            _lastCardFlipTime = 0f;
            
            Debug.Log($"Started metrics collection for level {levelIndex + 1}");
        }
        
        public void RecordCardFlip(float responseTime)
        {
            if (_currentLevelMetrics == null) return;
            
            _currentLevelMetrics.flips++;
            
            // Record decision time (time between card flips)
            if (_lastCardFlipTime > 0f)
            {
                float decisionTime = responseTime - _lastCardFlipTime;
                _currentLevelMetrics.decisionTimes.Add(decisionTime);
            }
            
            // Record flip time
            _currentLevelMetrics.flipTimes.Add(responseTime);
            _lastCardFlipTime = responseTime;
        }
        
        public void RecordMatch(int cardId1, int cardId2)
        {
            if (_currentLevelMetrics == null) return;
            
            Debug.Log($"Match recorded: {cardId1} & {cardId2}");
        }
        
        public void RecordMismatch(int cardId1, int cardId2)
        {
            if (_currentLevelMetrics == null) return;
            
            _currentLevelMetrics.mistakes++;
            
            // Create key for tracking repeated mistakes
            string mistakeKey = cardId1 < cardId2 ? $"{cardId1}-{cardId2}" : $"{cardId2}-{cardId1}";
            
            if (!_currentLevelMetrics.repeatedMistakes.ContainsKey(mistakeKey))
            {
                _currentLevelMetrics.repeatedMistakes[mistakeKey] = 0;
            }
            
            _currentLevelMetrics.repeatedMistakes[mistakeKey]++;
            
            // Track perseverative errors (same mistake repeated)
            if (_currentLevelMetrics.repeatedMistakes[mistakeKey] > 1)
            {
                _currentLevelMetrics.perseverativeErrors++;
            }
            
            Debug.Log($"Mismatch recorded: {cardId1} vs {cardId2} (Total mistakes: {_currentLevelMetrics.mistakes})");
        }
        
        public void CompleteLevel(float totalTime)
        {
            if (_currentLevelMetrics == null) return;
            
            _currentLevelMetrics.timeTaken = totalTime;
            _allMetrics.Add(_currentLevelMetrics);
            
            Debug.Log($"Level completed in {totalTime:F2}s with {_currentLevelMetrics.mistakes} mistakes");
            
            _currentLevelMetrics = null;
        }
        
        public LevelMetrics GetCurrentLevelMetrics()
        {
            return _currentLevelMetrics;
        }
        
        public List<LevelMetrics> GetAllMetrics()
        {
            return new List<LevelMetrics>(_allMetrics);
        }
        
        public void Reset()
        {
            _allMetrics.Clear();
            _currentLevelMetrics = null;
            _lastCardFlipTime = 0f;
            
            Debug.Log("Metrics collection reset");
        }
        
        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<LevelStartedEvent>(OnLevelStarted);
            _eventBus.Subscribe<CardFlippedEvent>(OnCardFlipped);
            _eventBus.Subscribe<CardsMatchedEvent>(OnCardsMatched);
            _eventBus.Subscribe<CardsMismatchedEvent>(OnCardsMismatched);
            _eventBus.Subscribe<LevelCompletedEvent>(OnLevelCompleted);
        }
        
        private void OnLevelStarted(LevelStartedEvent levelEvent)
        {
            StartLevel(levelEvent.LevelNumber - 1); // Convert to 0-based index
        }
        
        private void OnCardFlipped(CardFlippedEvent flipEvent)
        {
            RecordCardFlip(flipEvent.FlipTime);
        }
        
        private void OnCardsMatched(CardsMatchedEvent matchEvent)
        {
            RecordMatch(matchEvent.FirstCard.Id.Value, matchEvent.SecondCard.Id.Value);
        }
        
        private void OnCardsMismatched(CardsMismatchedEvent mismatchEvent)
        {
            RecordMismatch(mismatchEvent.FirstCard.Id.Value, mismatchEvent.SecondCard.Id.Value);
        }
        
        private void OnLevelCompleted(LevelCompletedEvent completedEvent)
        {
            CompleteLevel(completedEvent.CompletionTime);
        }
        
        ~MetricsCollectionService()
        {
            // Cleanup subscriptions
            _eventBus?.Unsubscribe<LevelStartedEvent>(OnLevelStarted);
            _eventBus?.Unsubscribe<CardFlippedEvent>(OnCardFlipped);
            _eventBus?.Unsubscribe<CardsMatchedEvent>(OnCardsMatched);
            _eventBus?.Unsubscribe<CardsMismatchedEvent>(OnCardsMismatched);
            _eventBus?.Unsubscribe<LevelCompletedEvent>(OnLevelCompleted);
        }
    }
}