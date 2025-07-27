using System;
using UnityEngine;
using UnityEngine.UI;
using MiniMatch.Domain.Models;
using MiniMatch.Core.Events;
using MiniMatch.Presentation.Animation;

namespace MiniMatch.Presentation.Views
{
    /// <summary>
    /// Handles the visual representation and user interaction for a card.
    /// Separates presentation logic from business logic following MVP pattern.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class CardView : MonoBehaviour
    {
        [Header("Visual Components")]
        [SerializeField] private Image frontImage;
        [SerializeField] private GameObject frontObject;
        [SerializeField] private GameObject backObject;
        
        [Header("Effects")]
        [SerializeField] private Material outlineMaterial;
        
        [Header("Audio")]
        [SerializeField] private string flipSoundId = "card_flip";
        [SerializeField] private string matchSoundId = "card_match";
        
        private CardModel _cardModel;
        private Button _button;
        private Action<CardModel> _onCardClicked;
        private GameEventBus _eventBus;
        
        // Animation components
        private CardFlipAnimation _flipAnimation;
        private CardGlowEffect _glowEffect;
        
        private void Awake()
        {
            _button = GetComponent<Button>();
            _flipAnimation = GetComponent<CardFlipAnimation>();
            _glowEffect = GetComponent<CardGlowEffect>();
            _eventBus = GameEventBus.Instance;
            
            _button.onClick.AddListener(OnButtonClicked);
        }
        
        public void Initialize(CardModel cardModel, Action<CardModel> onCardClicked)
        {
            _cardModel = cardModel ?? throw new ArgumentNullException(nameof(cardModel));
            _onCardClicked = onCardClicked ?? throw new ArgumentNullException(nameof(onCardClicked));
            
            SetupVisuals();
            SubscribeToModelEvents();
        }
        
        private void OnDestroy()
        {
            UnsubscribeFromModelEvents();
            _button.onClick.RemoveListener(OnButtonClicked);
        }
        
        private void SetupVisuals()
        {
            if (frontImage != null)
                frontImage.sprite = _cardModel.Image;
                
            UpdateVisualState();
        }
        
        private void SubscribeToModelEvents()
        {
            if (_cardModel != null)
            {
                _cardModel.OnFlipped += OnCardFlipped;
                _cardModel.OnMatched += OnCardMatched;
            }
        }
        
        private void UnsubscribeFromModelEvents()
        {
            if (_cardModel != null)
            {
                _cardModel.OnFlipped -= OnCardFlipped;
                _cardModel.OnMatched -= OnCardMatched;
            }
        }
        
        private void OnButtonClicked()
        {
            if (_cardModel != null && _cardModel.CanFlip())
            {
                _onCardClicked?.Invoke(_cardModel);
            }
        }
        
        private void OnCardFlipped(CardModel card)
        {
            UpdateVisualState();
            PlayFlipAnimation();
            PlaySound(flipSoundId);
        }
        
        private void OnCardMatched(CardModel card)
        {
            UpdateVisualState();
            ApplyMatchedEffects();
            PlaySound(matchSoundId);
            
            // Disable interaction
            _button.interactable = false;
        }
        
        private void UpdateVisualState()
        {
            if (_cardModel == null) return;
            
            bool showFront = _cardModel.IsFlipped || _cardModel.IsMatched;
            
            if (frontObject != null)
                frontObject.SetActive(showFront);
                
            if (backObject != null)
                backObject.SetActive(!showFront);
        }
        
        private void PlayFlipAnimation()
        {
            if (_flipAnimation != null)
            {
                if (_cardModel.IsFlipped)
                    _flipAnimation.FlipToFront();
                else
                    _flipAnimation.FlipToBack();
            }
        }
        
        private void ApplyMatchedEffects()
        {
            // Apply glow effect
            if (_glowEffect != null)
            {
                _glowEffect.StartGlow();
            }
            
            // Apply outline material
            if (outlineMaterial != null && frontImage != null)
            {
                frontImage.material = outlineMaterial;
            }
        }
        
        private void PlaySound(string soundId)
        {
            if (!string.IsNullOrEmpty(soundId))
            {
                _eventBus.Publish(new PlaySoundEvent(soundId));
            }
        }
        
        // Public methods for external control (if needed)
        public void SetInteractable(bool interactable)
        {
            if (_button != null)
                _button.interactable = interactable;
        }
        
        public CardModel GetCardModel()
        {
            return _cardModel;
        }
    }
}