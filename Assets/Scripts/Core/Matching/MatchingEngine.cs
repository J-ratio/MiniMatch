using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MiniMatch.Domain.Models;
using MiniMatch.Core.Events;

namespace MiniMatch.Core.Matching
{
    public class MatchingEngine : IMatchingEngine
    {
        public event Action<CardModel, CardModel> OnCardsMatched;
        public event Action<CardModel, CardModel> OnCardsMismatched;
        public event Action OnAllPairsMatched;
        
        public int MatchedPairs { get; private set; }
        public int TotalPairs { get; private set; }
        
        private readonly List<CardModel> _flippedCards = new();
        private readonly GameEventBus _eventBus;
        private bool _isProcessing = false;
        
        public MatchingEngine(GameEventBus eventBus = null)
        {
            _eventBus = eventBus ?? GameEventBus.Instance;
        }
        
        public void ProcessCardFlip(CardModel card)
        {
            if (card == null)
                throw new ArgumentNullException(nameof(card));
                
            if (!CanFlipCard(card) || _isProcessing)
                return;
                
            card.Flip();
            _flippedCards.Add(card);
            
            _eventBus.Publish(new CardFlippedEvent(card, Time.time));
            
            if (_flippedCards.Count == 2)
            {
                ProcessMatch();
            }
        }
        
        public bool CanFlipCard(CardModel card)
        {
            return card != null && 
                   card.CanFlip() && 
                   !_flippedCards.Contains(card) && 
                   _flippedCards.Count < 2;
        }
        
        public void Reset()
        {
            _flippedCards.Clear();
            MatchedPairs = 0;
            TotalPairs = 0;
            _isProcessing = false;
        }
        
        public void SetTotalPairs(int totalPairs)
        {
            if (totalPairs < 0)
                throw new ArgumentException("Total pairs cannot be negative", nameof(totalPairs));
                
            TotalPairs = totalPairs;
        }
        
        private void ProcessMatch()
        {
            if (_flippedCards.Count != 2) return;
            
            _isProcessing = true;
            var firstCard = _flippedCards[0];
            var secondCard = _flippedCards[1];
            
            if (firstCard.Matches(secondCard))
            {
                HandleMatch(firstCard, secondCard);
            }
            else
            {
                HandleMismatch(firstCard, secondCard);
            }
            
            _flippedCards.Clear();
            _isProcessing = false;
        }   
        
        private void HandleMatch(CardModel firstCard, CardModel secondCard)
        {
            try
            {
                firstCard.SetMatched();
                secondCard.SetMatched();
                MatchedPairs++;
                
                OnCardsMatched?.Invoke(firstCard, secondCard);
                _eventBus?.Publish(new CardsMatchedEvent(firstCard, secondCard, Time.time));
                
                Debug.Log($"Cards matched! {MatchedPairs}/{TotalPairs} pairs complete");
                
                if (MatchedPairs >= TotalPairs && TotalPairs > 0)
                {
                    Debug.Log("All pairs matched! Level complete!");
                    OnAllPairsMatched?.Invoke();
                    _eventBus?.Publish(new AllPairsMatchedEvent(Time.time));
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error in HandleMatch: {ex.Message}");
                Debug.LogException(ex);
            }
        }
        
        private void HandleMismatch(CardModel firstCard, CardModel secondCard)
        {
            // Cards will be flipped back after a delay
            OnCardsMismatched?.Invoke(firstCard, secondCard);
            _eventBus.Publish(new CardsMismatchedEvent(firstCard, secondCard, Time.time));
            
            Debug.Log($"Cards mismatched: {firstCard.Id} vs {secondCard.Id}");
        }
    }
}