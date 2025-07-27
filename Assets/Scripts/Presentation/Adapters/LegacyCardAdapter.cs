using UnityEngine;
using MiniMatch.Domain.Models;
using MiniMatch.Domain.ValueObjects;
using MiniMatch.Core.Events;

namespace MiniMatch.Presentation.Adapters
{
    /// <summary>
    /// Adapter to bridge legacy Card.cs with new CardModel system
    /// This allows gradual migration without breaking existing functionality
    /// </summary>
    [RequireComponent(typeof(Card))]
    public class LegacyCardAdapter : MonoBehaviour
    {
        private Card _legacyCard;
        private CardModel _cardModel;
        private GameEventBus _eventBus;
        
        private void Awake()
        {
            _legacyCard = GetComponent<Card>();
            _eventBus = GameEventBus.Instance;
        }
        
        private void OnEnable()
        {
            // Subscribe to legacy card events
            Card.OnAnyCardClicked += OnLegacyCardClicked;
            Card.OnAnyCardFlipped += OnLegacyCardFlipped;
        }
        
        private void OnDisable()
        {
            // Unsubscribe from legacy card events
            Card.OnAnyCardClicked -= OnLegacyCardClicked;
            Card.OnAnyCardFlipped -= OnLegacyCardFlipped;
        }
        

        
        private System.Action<CardModel> _onCardClicked;
        
        public void InitializeWithModel(CardModel cardModel, System.Action<CardModel> onCardClicked = null)
        {
            _cardModel = cardModel;
            _onCardClicked = onCardClicked;
            
            // Set up legacy card with model data
            if (_legacyCard != null)
            {
                _legacyCard.SetCard(_cardModel.Id.Value, _cardModel.Image);
            }
            
            // Subscribe to model events
            _cardModel.OnFlipped += OnModelFlipped;
            _cardModel.OnMatched += OnModelMatched;
        }
        
        private void OnLegacyCardClicked(Card card)
        {
            if (card != _legacyCard || _cardModel == null) return;
            
            // Call the callback provided by GameController
            _onCardClicked?.Invoke(_cardModel);
        }
        
        private void OnLegacyCardFlipped(Card card)
        {
            if (card != _legacyCard || _cardModel == null) return;
            
            // Update model state to match legacy card
            if (_legacyCard.IsFlipped != _cardModel.IsFlipped)
            {
                _cardModel.Flip();
            }
        }
        
        private void OnModelFlipped(CardModel model)
        {
            // Update legacy card to match model
            if (_legacyCard != null && _legacyCard.IsFlipped != model.IsFlipped)
            {
                _legacyCard.Flip();
            }
        }
        
        private void OnModelMatched(CardModel model)
        {
            // Update legacy card to show matched state
            if (_legacyCard != null)
            {
                _legacyCard.SetMatched();
            }
        }
        
        public CardModel GetCardModel()
        {
            return _cardModel;
        }
        
        private void OnDestroy()
        {
            if (_cardModel != null)
            {
                _cardModel.OnFlipped -= OnModelFlipped;
                _cardModel.OnMatched -= OnModelMatched;
            }
        }
    }
}