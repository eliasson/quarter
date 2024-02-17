using System;

namespace Quarter.Core.Utils
{
    public class Email
    {
        public string Value { get; }

        public Email(string value)
        {
            if (value.Length == 0)
                throw new ArgumentException("Email cannot be empty");
            if (!value.Contains("@"))
                throw new ArgumentException("Email must contain an @ character");
            if (value.Contains(" "))
                throw new ArgumentException("Email cannot contain a space");
            Value = value.ToLowerInvariant();
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            var other = (Email)obj;
            return Value == other.Value;
        }

        public override int GetHashCode()
            => Value?.GetHashCode() ?? 0;

        public string AsString()
            => Value;
    }
}
