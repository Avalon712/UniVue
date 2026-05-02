using System;
using UniVue.Common;

namespace UniVue.Event
{
    internal readonly struct EventCallback : IEquatable<EventCallback>
    {
        public readonly Type argsType;
        public readonly object callback;
        public readonly Stackframe trackFrame;

        public EventCallback(Action callback, object target)
        {
            argsType = null;
            this.callback = callback;
            Target = target;
            trackFrame = Stackframe.Track(3);
        }

        public EventCallback(Type argsType, object callback, object target)
        {
            this.argsType = argsType;
            this.callback = callback;
            Target = target;
            trackFrame = Stackframe.Track(3);
        }

        public object Target { get; }

        public bool Is(Action callback)
        {
            return this.callback is Action action && action == callback;
        }

        public bool Is<T>(Action<T> callback)
        {
            return this.callback is Action<T> action && action == callback;
        }

        public bool Invoke()
        {
            if (callback is Action callbackWithoutArgs)
            {
                callbackWithoutArgs.Invoke();
                return true;
            }

            return false;
        }

        public bool Invoke<T>(T args)
        {
            if (argsType != null && callback is Action<T> callbackWithArgs)
            {
                callbackWithArgs.Invoke(args);
                return true;
            }

            return Invoke();
        }

        public bool Equals(EventCallback other)
        {
            return argsType == other.argsType && Equals(callback, other.callback) && Equals(Target, other.Target);
        }

        public override bool Equals(object obj)
        {
            return obj is EventCallback other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(argsType, callback, Target);
        }
    }
}