using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MiniMatch.Core.Interfaces;
using MiniMatch.Core.Services;
using MiniMatch.Core.Events;
using MiniMatch.Core.Matching;
using MiniMatch.Domain.Models;
using MiniMatch.Domain.ValueObjects;
using MiniMatch.Presentation.Views;
using MiniMatch.Presentation.Adapters;


namespace MiniMatch.Presentation.Controllers
{
    /// <summary>
    /// Main game coordinator that orchestrates between different systems.
    /// Follows the Single Responsibility Principle by delegating specific concerns to specialized services.
    /// </summary>
    public class GameController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private LevelData[] levels;
        [SerializeField] private CardData[] allCardData;
        
        [Header("UI References")]
        [SerializeField] private Transform gridParent;
        [SerializeField] private GameObject cardPrefab;
        [SerializeField] private MiniMatch.Presentation.UI.GameUIController gameUIController;
        [SerializeField] private MiniMatch.Presentation.UI.GameSummaryScreen gameSummaryScreen;
        
        [Header("Audio")]
        [SerializeField] private MiniMatch.Infrastructure.Audio.AudioService audioService;
        
        // Core Services (Dependency Injection)
        private IGameState _gameState;
        private IMatchingEngine _matchingEngine;
        private ITimerService _timerService;
        private IMetricsCollector _metricsCollector;
        private ScoreService _scoreService;
        private LevelProgressionService _levelProgressionService;
        private GameEventBus _eventBus;
        
        // Current level data
        private List<CardModel> _currentCards = new();
        private int _currentLevelIndex = 0;
        
        private void Awake()
        {
            InitializeServices();
            SubscribeToEvents();
        }
        
        private void Start()
        {
            StartGame();
        }
        
        private void Update()
        {
            // Update timer service instead of game state directly
            _timerService?.UpdateTimer(Time.deltaTime);
        }
        
        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }
        
        private void InitializeServices()
        {
            try
            {
                _eventBus = GameEventBus.Instance;
                
                if (_eventBus == null)
                {
                    Debug.LogError("Failed to initialize GameEventBus");
                    return;
                }
                
                _gameState = new GameStateService(_eventBus);
                _matchingEngine = new MatchingEngine(_eventBus);
                _timerService = new TimerService(_eventBus);
                _metricsCollector = new MetricsCollectionService(_eventBus);
                _scoreService = new ScoreService(_metricsCollector, _eventBus);
                
                // Check if levels array is assigned
                if (levels == null || levels.Length == 0)
                {
                    Debug.LogError("No levels assigned to GameController! Please assign LevelData array in inspector.");
                    return;
                }
                
                _levelProgressionService = new LevelProgressionService(levels, _metricsCollector, _eventBus);
                
                // Initialize UI components
                if (gameSummaryScreen != null)
                {
                    Debug.Log("[GameController] Initializing GameSummaryScreen");
                    gameSummaryScreen.Initialize(_metricsCollector, _scoreService);
                }
                else
                {
                    Debug.LogWarning("[GameController] GameSummaryScreen is null! Please assign it in the inspector.");
                }
                
                // Audio service is already initialized via MonoBehaviour
                if (audioService == null)
                {
                    audioService = FindObjectOfType<MiniMatch.Infrastructure.Audio.AudioService>();
                }
                
                Debug.Log("All services initialized successfully");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to initialize services: {ex.Message}");
                Debug.LogException(ex);
            }
        }
        
        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<LevelCompletedEvent>(OnLevelCompleted);
            _eventBus.Subscribe<CardFlippedEvent>(OnCardFlipped);
            _eventBus.Subscribe<GamePhaseChangedEvent>(OnGamePhaseChanged);
            _eventBus.Subscribe<CardsMismatchedEvent>(OnCardsMismatched);
            _eventBus.Subscribe<CardsMatchedEvent>(OnCardsMatched);
            _eventBus.Subscribe<ScoreUpdatedEvent>(OnScoreUpdated);
            
            // Subscribe to timer events
            if (_timerService != null)
            {
                _timerService.OnTimeUpdated += OnTimerUpdated;
            }
        }
        
        private void UnsubscribeFromEvents()
        {
            _eventBus.Unsubscribe<LevelCompletedEvent>(OnLevelCompleted);
            _eventBus.Unsubscribe<CardFlippedEvent>(OnCardFlipped);
            _eventBus.Unsubscribe<GamePhaseChangedEvent>(OnGamePhaseChanged);
            _eventBus.Unsubscribe<CardsMismatchedEvent>(OnCardsMismatched);
            _eventBus.Unsubscribe<CardsMatchedEvent>(OnCardsMatched);
            _eventBus.Unsubscribe<ScoreUpdatedEvent>(OnScoreUpdated);
            
            // Unsubscribe from timer events
            if (_timerService != null)
            {
                _timerService.OnTimeUpdated -= OnTimerUpdated;
            }
        }
        
        private void StartGame()
        {
            _levelProgressionService.ResetProgression();
            StartCurrentLevel();
        }
        
        private void StartCurrentLevel()
        {
            try
            {
                if (_levelProgressionService == null)
                {
                    Debug.LogError("LevelProgressionService is null!");
                    return;
                }
                
                if (_levelProgressionService.IsGameComplete)
                {
                    if (_gameState != null)
                    {
                        _gameState.SetPhase(GamePhase.GameComplete);
                    }
                    Debug.Log("🎉 All levels completed!");
                    return;
                }
                
                var levelData = _levelProgressionService.GetCurrentLevel();
                if (levelData == null)
                {
                    Debug.LogError("Failed to get current level data");
                    return;
                }
                
                SetupLevel(levelData);
                
                if (_gameState != null)
                {
                    _gameState.NextLevel();
                    _gameState.SetTotalPairs(levelData.numPairs);
                }
                
                if (_matchingEngine != null)
                {
                    _matchingEngine.SetTotalPairs(levelData.numPairs);
                }
                
                var progressInfo = _levelProgressionService.GetProgressionInfo();
                _eventBus?.Publish(new LevelStartedEvent(progressInfo.CurrentLevel, levelData.numPairs));
                
                // Start timer when level begins
                if (_timerService != null)
                {
                    _timerService.ResetTimer();
                    _timerService.StartTimer();
                }
                
                if (_gameState != null)
                {
                    _gameState.SetPhase(GamePhase.Playing);
                }
                
                Debug.Log($"Started level {progressInfo.CurrentLevel}/{progressInfo.TotalLevels} ({progressInfo.ProgressPercentage:F1}% complete)");
                
                // Update UI with progress
                if (gameUIController != null)
                {
                    gameUIController.UpdateProgress(progressInfo.ProgressPercentage);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error starting current level: {ex.Message}");
                Debug.LogException(ex);
            }
        }
        
        private void SetupLevel(LevelData levelData)
        {
            ClearGrid();
            _currentCards = GenerateCards(levelData);
            CreateCardViews(_currentCards);
            
            _matchingEngine.Reset();
        }
        
        private List<CardModel> GenerateCards(LevelData levelData)
        {
            var cards = new List<CardModel>();
            var selectedCardData = SelectRandomCardData(levelData.numPairs);
            
            // Create pairs
            var cardDataPairs = new List<CardData>();
            foreach (var data in selectedCardData)
            {
                cardDataPairs.Add(data);
                cardDataPairs.Add(data);
            }
            
            // Shuffle
            ShuffleList(cardDataPairs);
            
            // Create card models
            for (int i = 0; i < cardDataPairs.Count; i++)
            {
                var data = cardDataPairs[i];
                var position = new Vector2Int(i % levelData.cols, i / levelData.cols);
                var card = new CardModel(new CardId(data.id), data.image, position);
                cards.Add(card);
            }
            
            return cards;
        }
        
        private List<CardData> SelectRandomCardData(int count)
        {
            var pool = new List<CardData>(allCardData);
            var selected = new List<CardData>();
            
            for (int i = 0; i < count && pool.Count > 0; i++)
            {
                int index = Random.Range(0, pool.Count);
                selected.Add(pool[index]);
                pool.RemoveAt(index);
            }
            
            return selected;
        }
        
        private void CreateCardViews(List<CardModel> cards)
        {
            foreach (var cardModel in cards)
            {
                var cardObj = Instantiate(cardPrefab, gridParent);
                
                // Try new CardView first, fallback to legacy adapter
                var cardView = cardObj.GetComponent<CardView>();
                if (cardView != null)
                {
                    cardView.Initialize(cardModel, OnCardClicked);
                }
                else
                {
                    // Use legacy adapter for existing Card prefabs
                    var adapter = cardObj.GetComponent<LegacyCardAdapter>();
                    if (adapter == null)
                    {
                        adapter = cardObj.AddComponent<LegacyCardAdapter>();
                    }
                    adapter.InitializeWithModel(cardModel, OnCardClicked);
                }
            }
            
            // Configure grid layout
            ConfigureGridLayout(cards.Count);
        }
        
        private void ConfigureGridLayout(int cardCount)
        {
            var levelData = _levelProgressionService.GetCurrentLevel();
            if (levelData == null) return;
            
            var grid = gridParent.GetComponent<UnityEngine.UI.GridLayoutGroup>();
            
            if (grid != null)
            {
                grid.constraint = UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount;
                grid.constraintCount = levelData.cols;
            }
        }
        
        private void ClearGrid()
        {
            foreach (Transform child in gridParent)
            {
                Destroy(child.gameObject);
            }
            _currentCards.Clear();
        }
        
        private void OnCardClicked(CardModel card)
        {
            if (_gameState?.CanInteract == true && _matchingEngine != null)
            {
                _matchingEngine.ProcessCardFlip(card);
            }
        }
        
        private void OnLevelCompleted(LevelCompletedEvent levelEvent)
        {
            Debug.Log($"[OnLevelCompleted] Starting level completion handling for level {levelEvent.LevelNumber}");
            
            // Stop timer when level completes
            if (_timerService != null)
            {
                _timerService.StopTimer();
                Debug.Log("[OnLevelCompleted] Timer stopped");
            }
            
            float completionTime = _timerService?.ElapsedTime ?? levelEvent.CompletionTime;
            Debug.Log($"Level {levelEvent.LevelNumber} completed in {completionTime:F2}s");
            
            // Advance to next level using progression service
            if (_levelProgressionService != null && _levelProgressionService.AdvanceToNextLevel())
            {
                Debug.Log("[OnLevelCompleted] Advancing to next level, scheduling StartCurrentLevel");
                // Small delay before next level
                Invoke(nameof(StartCurrentLevel), 2f);
            }
            else
            {
                Debug.Log("[OnLevelCompleted] Game complete or cannot advance");
                // Game complete
                if (_gameState != null)
                {
                    _gameState.SetPhase(GamePhase.GameComplete);
                }
                Debug.Log("🎉 Game Complete! All levels finished!");
                
                // Fallback: directly show summary screen if event system fails
                if (gameSummaryScreen != null)
                {
                    gameSummaryScreen.gameObject.SetActive(true);
                    Debug.Log("[GameController] Directly triggering summary screen as fallback");
                    gameSummaryScreen.ShowSummary();
                }
                else
                {
                    Debug.LogError("[GameController] GameSummaryScreen is null! Cannot show summary.");
                }
            }
            
            Debug.Log("[OnLevelCompleted] Level completion handling finished");
        }
        
        private void OnCardFlipped(CardFlippedEvent flipEvent)
        {
            Debug.Log($"Card {flipEvent.Card.Id} flipped at {flipEvent.FlipTime}");
        }
        
        private void OnGamePhaseChanged(GamePhaseChangedEvent phaseEvent)
        {
            Debug.Log($"Game phase: {phaseEvent.PreviousPhase} -> {phaseEvent.NewPhase}");
        }
        
        private void OnTimerUpdated(float elapsedTime)
        {
            // Update game state
            _gameState?.UpdateTime(Time.deltaTime);
            
            // Update UI
            if (gameUIController != null)
            {
                gameUIController.UpdateTimer(elapsedTime);
            }
        }
        
        private void OnCardsMatched(CardsMatchedEvent matchEvent)
        {
            Debug.Log($"Cards matched: {matchEvent.FirstCard.Id} & {matchEvent.SecondCard.Id}");
        }
        
        private void OnScoreUpdated(ScoreUpdatedEvent scoreEvent)
        {
            Debug.Log($"Score updated: {scoreEvent.PreviousScore.Value} -> {scoreEvent.NewScore.Value}");
        }
        
        private void OnCardsMismatched(CardsMismatchedEvent mismatchEvent)
        {
            Debug.Log($"Cards mismatched: {mismatchEvent.FirstCard.Id} vs {mismatchEvent.SecondCard.Id}");
            
            // Flip cards back after a delay
            StartCoroutine(FlipCardsBackAfterDelay(mismatchEvent.FirstCard, mismatchEvent.SecondCard, 1.0f));
        }
        
        private System.Collections.IEnumerator FlipCardsBackAfterDelay(CardModel firstCard, CardModel secondCard, float delay)
        {
            yield return new WaitForSeconds(delay);
            
            // Flip cards back if they're still flipped and not matched
            if (firstCard.IsFlipped && !firstCard.IsMatched)
            {
                firstCard.Flip();
            }
            
            if (secondCard.IsFlipped && !secondCard.IsMatched)
            {
                secondCard.Flip();
            }
        }
        
        private void ShuffleList<T>(List<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                int randomIndex = Random.Range(i, list.Count);
                (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
            }
        }
    }
}