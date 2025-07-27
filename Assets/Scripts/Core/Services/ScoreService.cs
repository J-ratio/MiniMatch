using UnityEngine;
using MiniMatch.Domain.ValueObjects;
using MiniMatch.Domain.Services;
using MiniMatch.Core.Events;
using MiniMatch.Core.Services;

namespace MiniMatch.Core.Services
{
    /// <summary>
    /// Manages scoring throughout the game using domain services
    /// </summary>
    public class ScoreService
    {
        public Score CurrentLevelScore { get; private set; }
        public Score TotalScore { get; private set; }
        
        private readonly ScoreCalculationService _scoreCalculator;
        private readonly IMetricsCollector _metricsCollector;
        private readonly GameEventBus _eventBus;
        
        public ScoreService(IMetricsCollector metricsCollector, GameEventBus eventBus = null)
        {
            _metricsCollector = metricsCollector ?? throw new System.ArgumentNullException(nameof(metricsCollector));
            _eventBus = eventBus ?? GameEventBus.Instance;
            _scoreCalculator = new ScoreCalculationService();
            
            CurrentLevelScore = new Score(0);
            TotalScore = new Score(0);
            
            SubscribeToEvents();
        }
        
        public void UpdateCurrentLevelScore()
        {
            var currentMetrics = _metricsCollector.GetCurrentLevelMetrics();
            if (currentMetrics != null)
            {
                var previousScore = CurrentLevelScore;
                CurrentLevelScore = _scoreCalculator.CalculateLevelScore(currentMetrics);
                
                // Publish score update event
                _eventBus.Publish(new ScoreUpdatedEvent(CurrentLevelScore, previousScore));
            }
        }
        
        public void CalculateTotalScore()
        {
            var allMetrics = _metricsCollector.GetAllMetrics();
            var previousTotal = TotalScore;
            TotalScore = _scoreCalculator.CalculateTotalScore(allMetrics);
            
            Debug.Log($"Total score calculated: {TotalScore.Value} points");
            
            // Publish total score update
            _eventBus.Publish(new ScoreUpdatedEvent(TotalScore, previousTotal));
        }
        
        public ScoreBreakdown GetCurrentLevelBreakdown()
        {
            var currentMetrics = _metricsCollector.GetCurrentLevelMetrics();
            return _scoreCalculator.GetScoreBreakdown(currentMetrics);
        }
        
        public void ResetScores()
        {
            CurrentLevelScore = new Score(0);
            TotalScore = new Score(0);
            
            _eventBus.Publish(new ScoreUpdatedEvent(CurrentLevelScore, new Score(0)));
        }
        
        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<CardsMatchedEvent>(OnCardsMatched);
            _eventBus.Subscribe<CardsMismatchedEvent>(OnCardsMismatched);
            _eventBus.Subscribe<LevelCompletedEvent>(OnLevelCompleted);
        }
        
        private void OnCardsMatched(CardsMatchedEvent matchEvent)
        {
            // Update score after each match
            UpdateCurrentLevelScore();
        }
        
        private void OnCardsMismatched(CardsMismatchedEvent mismatchEvent)
        {
            // Update score after each mistake
            UpdateCurrentLevelScore();
        }
        
        private void OnLevelCompleted(LevelCompletedEvent completedEvent)
        {
            // Final score calculation for the level
            UpdateCurrentLevelScore();
            CalculateTotalScore();
            
            Debug.Log($"Level {completedEvent.LevelNumber} final score: {CurrentLevelScore.Value}");
        }
        
        ~ScoreService()
        {
            _eventBus?.Unsubscribe<CardsMatchedEvent>(OnCardsMatched);
            _eventBus?.Unsubscribe<CardsMismatchedEvent>(OnCardsMismatched);
            _eventBus?.Unsubscribe<LevelCompletedEvent>(OnLevelCompleted);
        }
    }
}