using System;
using UnityEngine;
using MiniMatch.Core.Interfaces;
using MiniMatch.Core.Events;

namespace MiniMatch.Core.Services
{
    public class GameStateService : IGameState
    {
        public GamePhase CurrentPhase { get; private set; }
        public int CurrentLevel { get; private set; }
        public int MatchedPairs { get; private set; }
        public int TotalPairs { get; private set; }
        public float ElapsedTime { get; private set; }
        public bool CanInteract => CurrentPhase == GamePhase.Playing;
        
        public event Action<GamePhase> OnPhaseChanged;
        public event Action<int> OnLevelChanged;
        public event Action<float> OnTimeUpdated;
        
        private readonly GameEventBus _eventBus;
        
        public GameStateService(GameEventBus eventBus = null)
        {
            _eventBus = eventBus ?? GameEventBus.Instance;
            Reset();
            
            // Subscribe to relevant events
            _eventBus.Subscribe<CardsMatchedEvent>(OnCardsMatched);
            _eventBus.Subscribe<AllPairsMatchedEvent>(OnAllPairsMatched);
        }
        
        public void SetPhase(GamePhase phase)
        {
            if (CurrentPhase == phase) return;
            
            var previousPhase = CurrentPhase;
            CurrentPhase = phase;
            
            OnPhaseChanged?.Invoke(phase);
            _eventBus.Publish(new GamePhaseChangedEvent(previousPhase, phase));
            
            Debug.Log($"Game phase changed: {previousPhase} -> {phase}");
        }
        
        public void NextLevel()
        {
            CurrentLevel++;
            MatchedPairs = 0;
            ElapsedTime = 0f;
            
            OnLevelChanged?.Invoke(CurrentLevel);
            Debug.Log($"Advanced to level {CurrentLevel}");
        }
        
        public void UpdateTime(float deltaTime)
        {
            if (CurrentPhase != GamePhase.Playing) return;
            
            ElapsedTime += deltaTime;
            OnTimeUpdated?.Invoke(ElapsedTime);
        }
        
        public void Reset()
        {
            CurrentPhase = GamePhase.Loading;
            CurrentLevel = 0;
            MatchedPairs = 0;
            TotalPairs = 0;
            ElapsedTime = 0f;
        }
        
        public void SetTotalPairs(int totalPairs)
        {
            if (totalPairs < 0)
                throw new ArgumentException("Total pairs cannot be negative", nameof(totalPairs));
                
            TotalPairs = totalPairs;
        }
        
        private void OnCardsMatched(CardsMatchedEvent matchEvent)
        {
            MatchedPairs++;
            Debug.Log($"Pair matched! {MatchedPairs}/{TotalPairs}");
        }
        
        private void OnAllPairsMatched(AllPairsMatchedEvent completionEvent)
        {
            SetPhase(GamePhase.LevelComplete);
            
            // Only publish level completed event if we haven't already
            if (CurrentPhase == GamePhase.LevelComplete)
            {
                _eventBus.Publish(new LevelCompletedEvent(CurrentLevel, ElapsedTime, 0)); // TODO: Track mistakes
            }
        }
    }
}