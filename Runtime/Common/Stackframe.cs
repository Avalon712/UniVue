using System;
#if UNITY_EDITOR
using System.Diagnostics;
using System.Reflection;
#endif

namespace UniVue.Common
{
    public readonly struct Stackframe : IEquatable<Stackframe>
    {
        public readonly string methodFullName;
        public readonly string file;
        public readonly int line;
        public readonly object extraData;

        public Stackframe(string methodFullName, string file, int line, object extraData)
        {
            this.methodFullName = methodFullName;
            this.file = file;
            this.line = line;
            this.extraData = extraData;
        }

        public static Stackframe Track(int index, object extraData = null)
        {
#if UNITY_EDITOR
            StackTrace st = new(true);
            StackFrame frame = st.GetFrame(index);
            MethodBase method = frame.GetMethod();
            string file = frame.GetFileName();
            int line = frame.GetFileLineNumber();
            string methodFullName = $"{method?.DeclaringType?.FullName}.{method?.Name}";
            return new Stackframe(methodFullName, file, line, extraData);
#else
            return new Stackframe("UnknownMethod", "UnknownFile", 0, "NOT IN EDITOR");
#endif
        }

        public override string ToString()
        {
            return $"{methodFullName} ({file}:{line}) ExtraData: {extraData})";
        }

        public bool Equals(Stackframe other)
        {
            return methodFullName == other.methodFullName && file == other.file && line == other.line &&
                   Equals(extraData, other.extraData);
        }

        public override bool Equals(object obj)
        {
            return obj is Stackframe other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(methodFullName, file, line, extraData);
        }
    }
}