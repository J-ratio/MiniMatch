using UnityEngine;
using TMPro;
using UnityEngine.UI;
using MiniMatch.Core.Events;
using MiniMatch.Core.Services;
using MiniMatch.Domain.ValueObjects;
using MiniMatch.Core.Interfaces;

namespace MiniMatch.Presentation.UI
{
    /// <summary>
    /// Main UI controller that manages all game UI elements reactively
    /// </summary>
    public class GameUIController : MonoBehaviour
    {
        [Header("Score UI")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI levelScoreText;
        
        [Header("Timer UI")]
        [SerializeField] private TextMeshProUGUI timerText;
        
        [Header("Level UI")]
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private Slider progressSlider;
        [SerializeField] private TextMeshProUGUI progressText;
        
        [Header("Game Controls")]
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private GameObject pausePanel;
        
        [Header("Status UI")]
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private GameObject gameCompletePanel;
        
        private GameEventBus _eventBus;
        private bool _isPaused = false;
        
        private void Awake()
        {
            _eventBus = GameEventBus.Instance;
            InitializeUI();
        }
        
        private void Start()
        {
            SubscribeToEvents();
            SetupButtons();
        }
        
        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }
        
        private void InitializeUI()
        {
            UpdateScoreDisplay(new Score(0));
            UpdateLevelScoreDisplay(new Score(0));
            UpdateTimerDisplay(0f);
            UpdateLevelDisplay(1, 1);
            UpdateProgressDisplay(0f);
            UpdateStatusDisplay("Ready to start!");
            
            if (pausePanel != null)
                pausePanel.SetActive(false);
                
            if (gameCompletePanel != null)
                gameCompletePanel.SetActive(false);
        }
        
        private void SetupButtons()
        {
            if (pauseButton != null)
                pauseButton.onClick.AddListener(TogglePause);
                
            if (restartButton != null)
                restartButton.onClick.AddListener(RestartGame);
        }
        
        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<ScoreUpdatedEvent>(OnScoreUpdated);
            _eventBus.Subscribe<LevelStartedEvent>(OnLevelStarted);
            _eventBus.Subscribe<LevelCompletedEvent>(OnLevelCompleted);
            _eventBus.Subscribe<GamePhaseChangedEvent>(OnGamePhaseChanged);
            _eventBus.Subscribe<CardsMatchedEvent>(OnCardsMatched);
            _eventBus.Subscribe<CardsMismatchedEvent>(OnCardsMismatched);
        }
        
        private void UnsubscribeFromEvents()
        {
            _eventBus.Unsubscribe<ScoreUpdatedEvent>(OnScoreUpdated);
            _eventBus.Unsubscribe<LevelStartedEvent>(OnLevelStarted);
            _eventBus.Unsubscribe<LevelCompletedEvent>(OnLevelCompleted);
            _eventBus.Unsubscribe<GamePhaseChangedEvent>(OnGamePhaseChanged);
            _eventBus.Unsubscribe<CardsMatchedEvent>(OnCardsMatched);
            _eventBus.Unsubscribe<CardsMismatchedEvent>(OnCardsMismatched);
        }
        
        private void Update()
        {
            // Update timer display every frame during gameplay
            if (!_isPaused)
            {
                // Timer updates will come through events, but we can also poll if needed
            }
        }
        
        // Event Handlers
        private void OnScoreUpdated(ScoreUpdatedEvent scoreEvent)
        {
            UpdateScoreDisplay(scoreEvent.NewScore);
            
            // Show score change animation/feedback
            ShowScoreChangeEffect(scoreEvent.NewScore.Value - scoreEvent.PreviousScore.Value);
        }
        
        private void OnLevelStarted(LevelStartedEvent levelEvent)
        {
            UpdateLevelDisplay(levelEvent.LevelNumber, levelEvent.TotalPairs);
            UpdateStatusDisplay($"Level {levelEvent.LevelNumber} - Find {levelEvent.TotalPairs} pairs!");
            
            // Reset level score
            UpdateLevelScoreDisplay(new Score(0));
        }
        
        private void OnLevelCompleted(LevelCompletedEvent completedEvent)
        {
            UpdateStatusDisplay($"Level {completedEvent.LevelNumber} Complete! Time: {completedEvent.CompletionTime:F1}s");
            
            // Show completion effect
            ShowLevelCompleteEffect();
        }
        
        private void OnGamePhaseChanged(GamePhaseChangedEvent phaseEvent)
        {
            switch (phaseEvent.NewPhase)
            {
                case GamePhase.Loading:
                    UpdateStatusDisplay("Loading...");
                    break;
                case GamePhase.Preview:
                    UpdateStatusDisplay("Get ready...");
                    break;
                case GamePhase.Playing:
                    UpdateStatusDisplay("Playing");
                    break;
                case GamePhase.Paused:
                    UpdateStatusDisplay("Paused");
                    ShowPausePanel(true);
                    break;
                case GamePhase.LevelComplete:
                    UpdateStatusDisplay("Level Complete!");
                    break;
                case GamePhase.GameComplete:
                    UpdateStatusDisplay("Game Complete!");
                    ShowGameCompletePanel();
                    break;
            }
        }
        
        private void OnCardsMatched(CardsMatchedEvent matchEvent)
        {
            // Show positive feedback
            ShowMatchFeedback(true);
        }
        
        private void OnCardsMismatched(CardsMismatchedEvent mismatchEvent)
        {
            // Show negative feedback
            ShowMatchFeedback(false);
        }
        
        // UI Update Methods
        private void UpdateScoreDisplay(Score score)
        {
            if (scoreText != null)
                scoreText.text = $"Score: {score.Value:N0}";
        }
        
        private void UpdateLevelScoreDisplay(Score levelScore)
        {
            if (levelScoreText != null)
                levelScoreText.text = $"Level Score: {levelScore.Value:N0}";
        }
        
        private void UpdateTimerDisplay(float elapsedTime)
        {
            if (timerText != null)
            {
                int minutes = Mathf.FloorToInt(elapsedTime / 60f);
                int seconds = Mathf.FloorToInt(elapsedTime % 60f);
                timerText.text = $"Time: {minutes:00}:{seconds:00}";
            }
        }
        
        private void UpdateLevelDisplay(int currentLevel, int totalPairs)
        {
            if (levelText != null)
                levelText.text = $"Level {currentLevel}";
        }
        
        private void UpdateProgressDisplay(float progressPercentage)
        {
            if (progressSlider != null)
                progressSlider.value = progressPercentage / 100f;
                
            if (progressText != null)
                progressText.text = $"{progressPercentage:F0}%";
        }
        
        private void UpdateStatusDisplay(string status)
        {
            if (statusText != null)
                statusText.text = status;
        }
        
        // UI Effects and Feedback
        private void ShowScoreChangeEffect(int scoreChange)
        {
            if (scoreChange > 0)
            {
                // Could add positive score animation here
                Debug.Log($"+{scoreChange} points!");
            }
            else if (scoreChange < 0)
            {
                // Could add negative score animation here
                Debug.Log($"{scoreChange} points!");
            }
        }
        
        private void ShowMatchFeedback(bool isMatch)
        {
            if (isMatch)
            {
                // Positive feedback - could be color flash, sound, etc.
                Debug.Log("Match! ✓");
            }
            else
            {
                // Negative feedback
                Debug.Log("No match! ✗");
            }
        }
        
        private void ShowLevelCompleteEffect()
        {
            // Could add celebration animation, particles, etc.
            Debug.Log("🎉 Level Complete!");
        }
        
        private void ShowPausePanel(bool show)
        {
            if (pausePanel != null)
                pausePanel.SetActive(show);
        }
        
        private void ShowGameCompletePanel()
        {
            if (gameCompletePanel != null)
                gameCompletePanel.SetActive(true);
        }
        
        // Button Handlers
        private void TogglePause()
        {
            _isPaused = !_isPaused;
            
            if (_isPaused)
            {
                _eventBus.Publish(new GamePhaseChangedEvent(GamePhase.Playing, GamePhase.Paused));
                Time.timeScale = 0f;
            }
            else
            {
                _eventBus.Publish(new GamePhaseChangedEvent(GamePhase.Paused, GamePhase.Playing));
                Time.timeScale = 1f;
                ShowPausePanel(false);
            }
        }
        
        private void RestartGame()
        {
            Time.timeScale = 1f;
            _isPaused = false;
            ShowPausePanel(false);
            
            // Could publish restart event
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
            );
        }
        
        // Public methods for timer service integration
        public void UpdateTimer(float elapsedTime)
        {
            UpdateTimerDisplay(elapsedTime);
        }
        
        public void UpdateProgress(float progressPercentage)
        {
            UpdateProgressDisplay(progressPercentage);
        }
    }
}