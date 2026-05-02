using System;
using System.Collections.Generic;

namespace UniVue.UI
{
    public readonly struct RGraph : IEquatable<RGraph>
    {
        internal readonly RNode g;

        internal RGraph(RNode g)
        {
            this.g = g;
        }

        public static RGraph Create()
        {
            RNode g = RNode.Create();
            RGraph graph = new(g);
            g.Key = graph;
            return graph;
        }

        internal void CollectRKeysNoneAlloc(RKeyType type, List<RKey> keys)
        {
            if (g == null || keys == null || type == RKeyType.None) return;
            g.Visit(node =>
            {
                if ((node.Key.type & type) == node.Key.type)
                    keys.Add(node.Key);
                return true;
            });
        }

        public bool Equals(RGraph other)
        {
            return Equals(g, other.g);
        }

        public override bool Equals(object obj)
        {
            return obj is RGraph other && Equals(other);
        }

        public override int GetHashCode()
        {
            return g != null ? g.GetHashCode() : 0;
        }
    }
}