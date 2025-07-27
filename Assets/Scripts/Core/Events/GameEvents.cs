using MiniMatch.Domain.Models;
using MiniMatch.Domain.ValueObjects;
using MiniMatch.Core.Interfaces;

namespace MiniMatch.Core.Events
{
    public interface IGameEvent { }
    
    // Card Events
    public struct CardFlippedEvent : IGameEvent
    {
        public CardModel Card { get; }
        public float FlipTime { get; }
        
        public CardFlippedEvent(CardModel card, float flipTime)
        {
            Card = card;
            FlipTime = flipTime;
        }
    }
    
    public struct CardsMatchedEvent : IGameEvent
    {
        public CardModel FirstCard { get; }
        public CardModel SecondCard { get; }
        public float MatchTime { get; }
        
        public CardsMatchedEvent(CardModel firstCard, CardModel secondCard, float matchTime)
        {
            FirstCard = firstCard;
            SecondCard = secondCard;
            MatchTime = matchTime;
        }
    }
    
    public struct CardsMismatchedEvent : IGameEvent
    {
        public CardModel FirstCard { get; }
        public CardModel SecondCard { get; }
        public float MismatchTime { get; }
        
        public CardsMismatchedEvent(CardModel firstCard, CardModel secondCard, float mismatchTime)
        {
            FirstCard = firstCard;
            SecondCard = secondCard;
            MismatchTime = mismatchTime;
        }
    }
    
    // Game State Events
    public struct GamePhaseChangedEvent : IGameEvent
    {
        public GamePhase PreviousPhase { get; }
        public GamePhase NewPhase { get; }
        
        public GamePhaseChangedEvent(GamePhase previousPhase, GamePhase newPhase)
        {
            PreviousPhase = previousPhase;
            NewPhase = newPhase;
        }
    }
    
    public struct LevelStartedEvent : IGameEvent
    {
        public int LevelNumber { get; }
        public int TotalPairs { get; }
        
        public LevelStartedEvent(int levelNumber, int totalPairs)
        {
            LevelNumber = levelNumber;
            TotalPairs = totalPairs;
        }
    }
    
    public struct LevelCompletedEvent : IGameEvent
    {
        public int LevelNumber { get; }
        public float CompletionTime { get; }
        public int TotalMistakes { get; }
        
        public LevelCompletedEvent(int levelNumber, float completionTime, int totalMistakes)
        {
            LevelNumber = levelNumber;
            CompletionTime = completionTime;
            TotalMistakes = totalMistakes;
        }
    }
    
    public struct AllPairsMatchedEvent : IGameEvent
    {
        public float TotalTime { get; }
        
        public AllPairsMatchedEvent(float totalTime)
        {
            TotalTime = totalTime;
        }
    }
    
    // Score Events
    public struct ScoreUpdatedEvent : IGameEvent
    {
        public Score NewScore { get; }
        public Score PreviousScore { get; }
        
        public ScoreUpdatedEvent(Score newScore, Score previousScore)
        {
            NewScore = newScore;
            PreviousScore = previousScore;
        }
    }
    
    // Audio Events
    public struct PlaySoundEvent : IGameEvent
    {
        public string SoundId { get; }
        public float Volume { get; }
        
        public PlaySoundEvent(string soundId, float volume = 1.0f)
        {
            SoundId = soundId;
            Volume = volume;
        }
    }
}