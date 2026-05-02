using System;

namespace UniVue.Coroutine
{
    public readonly struct CoroutineID : IEquatable<CoroutineID>
    {
        public readonly ulong id;

        internal CoroutineID(ulong coroutineId)
        {
            id = coroutineId;
        }

        public static implicit operator ulong(CoroutineID coroutineId)
        {
            return coroutineId.id;
        }

        public static implicit operator CoroutineID(ulong coroutineId)
        {
            return new CoroutineID(coroutineId);
        }

        public bool Equals(CoroutineID other)
        {
            return id == other.id;
        }

        public override bool Equals(object obj)
        {
            return obj is CoroutineID other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (int)id;
        }

        public override string ToString()
        {
            return id.ToString();
        }
    }
}