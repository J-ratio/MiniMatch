using System;
using UnityEngine;
using MiniMatch.Domain.ValueObjects;

namespace MiniMatch.Domain.Models
{
    [Serializable]
    public class CardModel
    {
        public CardId Id { get; private set; }
        public Sprite Image { get; private set; }
        public bool IsFlipped { get; private set; }
        public bool IsMatched { get; private set; }
        public Vector2Int GridPosition { get; private set; }
        
        public event Action<CardModel> OnFlipped;
        public event Action<CardModel> OnMatched;
        
        public CardModel(CardId id, Sprite image, Vector2Int gridPosition)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Image = image ? image : throw new ArgumentNullException(nameof(image));
            GridPosition = gridPosition;
            IsFlipped = false;
            IsMatched = false;
        }
        
        public void Flip()
        {
            if (IsMatched) return;
            
            IsFlipped = !IsFlipped;
            OnFlipped?.Invoke(this);
        }
        
        public void SetMatched()
        {
            if (IsMatched) return;
            
            IsMatched = true;
            IsFlipped = true;
            OnMatched?.Invoke(this);
        }
        
        public void Reset()
        {
            IsFlipped = false;
            IsMatched = false;
        }
        
        public bool CanFlip()
        {
            return !IsFlipped && !IsMatched;
        }
        
        public bool Matches(CardModel other)
        {
            return other != null && Id.Equals(other.Id) && this != other;
        }
    }
}