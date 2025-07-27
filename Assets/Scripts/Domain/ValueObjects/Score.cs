using System;

namespace MiniMatch.Domain.ValueObjects
{
    [Serializable]
    public class Score : IEquatable<Score>, IComparable<Score>
    {
        public int Value { get; private set; }
        
        public Score(int value = 0)
        {
            if (value < 0)
                throw new ArgumentException("Score cannot be negative", nameof(value));
                
            Value = value;
        }
        
        public Score Add(int points)
        {
            if (points < 0)
                throw new ArgumentException("Cannot add negative points", nameof(points));
                
            return new Score(Value + points);
        }
        
        public Score Subtract(int points)
        {
            if (points < 0)
                throw new ArgumentException("Cannot subtract negative points", nameof(points));
                
            return new Score(Math.Max(0, Value - points));
        }
        
        public bool Equals(Score other)
        {
            return other != null && Value == other.Value;
        }
        
        public override bool Equals(object obj)
        {
            return Equals(obj as Score);
        }
        
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
        
        public int CompareTo(Score other)
        {
            if (other == null) return 1;
            return Value.CompareTo(other.Value);
        }
        
        public override string ToString()
        {
            return Value.ToString();
        }
        
        public static implicit operator int(Score score)
        {
            return score?.Value ?? 0;
        }
        
        public static implicit operator Score(int value)
        {
            return new Score(value);
        }
        
        public static Score operator +(Score left, Score right)
        {
            return new Score((left?.Value ?? 0) + (right?.Value ?? 0));
        }
        
        public static Score operator -(Score left, Score right)
        {
            return new Score(Math.Max(0, (left?.Value ?? 0) - (right?.Value ?? 0)));
        }
    }
}