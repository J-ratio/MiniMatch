using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using MiniMatch.Core.Events;
using MiniMatch.Core.Services;
using MiniMatch.Core.Interfaces;
using MiniMatch.Domain.Services;
using MiniMatch.Domain.ValueObjects;

namespace MiniMatch.Presentation.UI
{
    /// <summary>
    /// Enhanced summary screen that integrates with the new architecture
    /// </summary>
    public class GameSummaryScreen : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private GameObject summaryPanel;
        [SerializeField] private TextMeshProUGUI summaryText;
        
        [Header("Buttons")]
        [SerializeField] private Button playAgainButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button shareResultsButton;
        
        [Header("Visual Effects")]
        [SerializeField] private GameObject celebrationEffect;
        [SerializeField] private AudioClip completionSound;
        
        private GameEventBus _eventBus;
        private IMetricsCollector _metricsCollector;
        private ScoreService _scoreService;
        private CognitiveScoreAnalyzer _cognitiveAnalyzer;
        private AudioSource _audioSource;
        
        private void Awake()
        {
            Debug.Log("[GameSummaryScreen] Awake called");
            _eventBus = GameEventBus.Instance;
            _cognitiveAnalyzer = new CognitiveScoreAnalyzer();
            _audioSource = GetComponent<AudioSource>();
            
            if (summaryPanel != null)
            {
                summaryPanel.SetActive(false);
                Debug.Log("[GameSummaryScreen] Summary panel found and deactivated");
            }
            else
            {
                Debug.LogWarning("[GameSummaryScreen] Summary panel is null in Awake!");
            }
        }
        
        private void Start()
        {
            Debug.Log("[GameSummaryScreen] Start called");
            SubscribeToEvents();
            SetupButtons();
        }
        
        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }
        
        public void Initialize(IMetricsCollector metricsCollector, ScoreService scoreService)
        {
            _metricsCollector = metricsCollector;
            _scoreService = scoreService;
            Debug.Log("[GameSummaryScreen] Initialized with MetricsCollector: " + (_metricsCollector != null) + ", ScoreService: " + (_scoreService != null));
        }
        
        private void SubscribeToEvents()
        {
            Debug.Log("[GameSummaryScreen] Subscribing to GamePhaseChangedEvent");
            _eventBus.Subscribe<GamePhaseChangedEvent>(OnGamePhaseChanged);
        }
        
        private void UnsubscribeFromEvents()
        {
            _eventBus.Unsubscribe<GamePhaseChangedEvent>(OnGamePhaseChanged);
        }
        
        private void SetupButtons()
        {
            if (playAgainButton != null)
                playAgainButton.onClick.AddListener(PlayAgain);
                
            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(GoToMainMenu);
                
            if (shareResultsButton != null)
                shareResultsButton.onClick.AddListener(ShareResults);
        }
        
        private void OnGamePhaseChanged(GamePhaseChangedEvent phaseEvent)
        {
            Debug.Log($"[GameSummaryScreen] GamePhase changed: {phaseEvent.PreviousPhase} -> {phaseEvent.NewPhase}");
            
            if (phaseEvent.NewPhase == GamePhase.GameComplete)
            {
                Debug.Log("[GameSummaryScreen] Game complete detected, showing summary...");
                ShowSummary();
            }
        }
        
        public void ShowSummary()
        {
            Debug.Log("[GameSummaryScreen] ShowSummary called");
            
            if (_metricsCollector == null || _scoreService == null)
            {
                Debug.LogError("SummaryScreen not properly initialized! MetricsCollector: " + (_metricsCollector != null) + ", ScoreService: " + (_scoreService != null));
                return;
            }
            
            var allMetrics = _metricsCollector.GetAllMetrics();
            var totalScore = _scoreService.TotalScore;
            
            Debug.Log($"[GameSummaryScreen] Retrieved {allMetrics.Count} level metrics, total score: {totalScore.Value}");
            
            ShowSummary(allMetrics);
        }
        
        // Legacy-compatible method that matches the old SummaryScreen interface
        public void ShowSummary(List<LevelMetrics> metrics, Score totalScore)
        {
            DisplaySummary(metrics, totalScore);
            
            if (summaryPanel != null)
            {
                summaryPanel.SetActive(true);
                Debug.Log("[GameSummaryScreen] Summary panel activated");
            }
            else
            {
                Debug.LogError("[GameSummaryScreen] Summary panel is null! Cannot show summary.");
            }
                
            PlayCompletionEffects();
        }
        
        // Even simpler legacy-compatible method (matches exactly the old SummaryScreen)
        public void ShowSummary(List<LevelMetrics> metrics)
        {
            if (summaryPanel != null)
                summaryPanel.SetActive(true);
                
            if (summaryText != null)
            {
                var analyzer = new CognitiveScoreAnalyzer();
                var assessment = analyzer.AnalyzeMetrics(metrics);
                
                string summary = "<b>Cognitive Assessment Summary</b>\n\n";
                
                // Basic metrics (matching legacy format)
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
                Debug.Log("[GameSummaryScreen] Legacy-style summary displayed");
            }
            
            PlayCompletionEffects();
        }
        
        private void DisplaySummary(List<LevelMetrics> metrics, Score totalScore)
        {
            if (metrics == null || metrics.Count == 0)
            {
                DisplayEmptyResults();
                return;
            }
            
            // Create comprehensive summary in one text field (like legacy SummaryScreen)
            string summary = CreateComprehensiveSummary(metrics, totalScore);
            
            if (summaryText != null)
            {
                summaryText.text = summary;
                Debug.Log("[GameSummaryScreen] Summary text updated");
            }
            else
            {
                Debug.LogError("[GameSummaryScreen] Summary text is null!");
            }
        }
        
        private string CreateComprehensiveSummary(List<LevelMetrics> metrics, Score totalScore)
        {
            var summary = "<b>🎉 Game Complete!</b>\n\n";
            
            // Overall stats
            float totalTime = metrics.Sum(m => m.timeTaken);
            int totalFlips = metrics.Sum(m => m.flips);
            int totalMistakes = metrics.Sum(m => m.mistakes);
            int levelsCompleted = metrics.Count;
            
            // Score section
            summary += $"<b>Final Score: {totalScore.Value:N0} Points</b>\n\n";
            
            // Overall performance
            summary += $"<b>Overall Performance</b>\n";
            summary += $"Levels Completed: {levelsCompleted}\n";
            summary += $"Total Time: {FormatTime(totalTime)}\n";
            summary += $"Total Card Flips: {totalFlips}\n";
            summary += $"Total Mistakes: {totalMistakes}\n";
            summary += $"Average Time per Level: {FormatTime(totalTime / levelsCompleted)}\n\n";
            
            // Level-by-level breakdown
            summary += "<b>Level Details</b>\n";
            for (int i = 0; i < metrics.Count; i++)
            {
                var m = metrics[i];
                summary += $"<b>Level {i + 1}</b> - Time: {m.timeTaken:F1}s, Flips: {m.flips}, Mistakes: {m.mistakes}\n";
            }
            
            // Cognitive assessment
            var assessment = _cognitiveAnalyzer.AnalyzeMetrics(metrics);
            summary += $"\n<b>Cognitive Assessment</b>\n";
            summary += $"Memory Score: {assessment.MemoryScore:F1}%\n";
            summary += $"Response Speed: {assessment.ResponseSpeed:F2}s avg\n";
            summary += $"Error Rate: {assessment.ErrorRate:F1}%\n";
            summary += $"Learning Curve: {GetLearningDescription(assessment.LearningCurve)}\n\n";
            
            summary += $"<b>Assessment Result:</b>\n";
            summary += $"<color={GetStatusColor(assessment.CognitiveStatus)}>{assessment.CognitiveStatus}</color>\n\n";
            
            // Recommendations
            summary += GetRecommendations(assessment);
            
            return summary;
        }
        
        private void DisplayEmptyResults()
        {
            if (summaryText != null)
            {
                summaryText.text = "<b>No Results</b>\n\nNo gameplay data available.\nScore: 0\nNo cognitive analysis available.";
            }
        }
        
        private void PlayCompletionEffects()
        {
            // Play completion sound
            if (_audioSource != null && completionSound != null)
            {
                _audioSource.PlayOneShot(completionSound);
            }
            
            // Show celebration effect
            if (celebrationEffect != null)
            {
                celebrationEffect.SetActive(true);
                // Could add particle effects, animations, etc.
            }
        }
        
        // Helper Methods
        private string FormatTime(float seconds)
        {
            int minutes = Mathf.FloorToInt(seconds / 60f);
            int secs = Mathf.FloorToInt(seconds % 60f);
            return $"{minutes:00}:{secs:00}";
        }
        
        private string GetLearningDescription(float learningCurve)
        {
            if (learningCurve > 1.0f) return "Excellent improvement";
            if (learningCurve > 0.5f) return "Good improvement";
            if (learningCurve > 0.0f) return "Some improvement";
            if (learningCurve > -0.5f) return "Stable performance";
            return "Room for improvement";
        }
        
        private string GetStatusColor(string status)
        {
            return status switch
            {
                "Normal Cognitive Function" => "#00FF00",
                "Mild Cognitive Concern" => "#FFFF00",
                "Moderate Cognitive Concern" => "#FFA500",
                "Significant Cognitive Concern" => "#FF0000",
                _ => "#FFFFFF"
            };
        }
        
        private string GetRecommendations(CognitiveScoreAnalyzer.CognitiveAssessment assessment)
        {
            var recommendations = "<b>Recommendations:</b>\n";
            
            if (assessment.ErrorRate > 30)
            {
                recommendations += "• Practice memory exercises\n";
            }
            
            if (assessment.ResponseSpeed > 3.0f)
            {
                recommendations += "• Work on decision speed\n";
            }
            
            if (assessment.LearningCurve < 0)
            {
                recommendations += "• Take breaks between sessions\n";
            }
            
            recommendations += "• Play regularly for best results";
            
            return recommendations;
        }
        
        // Button Handlers
        private void PlayAgain()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
            );
        }
        
        private void GoToMainMenu()
        {
            // Would load main menu scene
            Debug.Log("Going to main menu...");
        }
        
        private void ShareResults()
        {
            // Could implement sharing functionality
            Debug.Log("Sharing results...");
        }
    }
}