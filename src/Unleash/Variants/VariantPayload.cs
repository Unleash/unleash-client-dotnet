using System;

namespace Unleash.Variants
{
    public class Payload
    {
        public Payload(string type, string value)
        {
            Type = type;
            Value = value;
        }

        public string Type { get; private set; }
        public string Value { get; private set; }

        public override bool Equals(object obj)
        {
            if (obj == this) return true;

            var otherPayload = obj as Payload;
            if (otherPayload == null) return false;

            return Equals(otherPayload.Type, Type) && Equals(otherPayload.Value, Value);
        }

        public override int GetHashCode()
        {
            return new { Type, Value }.GetHashCode();
        }
    }
}