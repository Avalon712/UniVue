using System;

namespace UniVue.Event
{
    public enum EventKeyType
    {
        NotEventKey,
        Int,
        String
    }

    public readonly struct EventKey : IEquatable<EventKey>
    {
        private readonly int _intKey;
        private readonly string _stringKey;

        public EventKey(int intKey)
        {
            Type = EventKeyType.Int;
            _intKey = intKey;
            _stringKey = null;
        }

        public EventKey(string stringKey)
        {
            Type = EventKeyType.String;
            _intKey = 0;
            _stringKey = stringKey;
        }

        public EventKeyType Type { get; }

        public static implicit operator EventKey(int intKey)
        {
            return new EventKey(intKey);
        }

        public static implicit operator EventKey(Enum enumKey)
        {
            return new EventKey(Convert.ToInt32(enumKey));
        }

        public static implicit operator EventKey(string stringKey)
        {
            return new EventKey(stringKey);
        }

        public bool Equals(EventKey other)
        {
            return Type == other.Type && _intKey == other._intKey && _stringKey == other._stringKey;
        }

        public override bool Equals(object obj)
        {
            return obj is EventKey other && Equals(other);
        }

        public override string ToString()
        {
            return Type switch
            {
                EventKeyType.Int => $"EventKey[IntKey: {_intKey}]",
                EventKeyType.String => $"EventKey[StringKey: {_stringKey}]",
                _ => nameof(EventKeyType.NotEventKey)
            };
        }

        public override int GetHashCode()
        {
            return Type switch
            {
                EventKeyType.Int => _intKey,
                EventKeyType.String => _stringKey != null ? _stringKey.GetHashCode() : 0,
                _ => 0
            };
        }
    }
}