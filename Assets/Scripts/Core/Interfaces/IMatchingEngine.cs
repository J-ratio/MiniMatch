using System;
using MiniMatch.Domain.Models;

namespace MiniMatch.Core.Matching
{
    public interface IMatchingEngine
    {
        event Action<CardModel, CardModel> OnCardsMatched;
        event Action<CardModel, CardModel> OnCardsMismatched;
        event Action OnAllPairsMatched;
        
        void ProcessCardFlip(CardModel card);
        void Reset();
        bool CanFlipCard(CardModel card);
        void SetTotalPairs(int totalPairs);
        int MatchedPairs { get; }
        int TotalPairs { get; }
    }
}