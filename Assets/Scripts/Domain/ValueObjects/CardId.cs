using System;

namespace MiniMatch.Domain.ValueObjects
{
    [Serializable]
    public class CardId : IEquatable<CardId>
    {
        public int Value { get; private set; }
        
        public CardId(int value)
        {
            if (value < 0)
                throw new ArgumentException("Card ID must be non-negative", nameof(value));
                
            Value = value;
        }
        
        public bool Equals(CardId other)
        {
            return other != null && Value == other.Value;
        }
        
        public override bool Equals(object obj)
        {
            return Equals(obj as CardId);
        }
        
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
        
        public override string ToString()
        {
            return $"CardId({Value})";
        }
        
        public static implicit operator int(CardId cardId)
        {
            return cardId?.Value ?? -1;
        }
        
        public static implicit operator CardId(int value)
        {
            return new CardId(value);
        }
    }
}