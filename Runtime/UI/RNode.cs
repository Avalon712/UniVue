using System;
using System.Collections.Generic;
using UniVue.Event;
using UniVue.Internal;
using UniVue.Model;

namespace UniVue.UI
{
    internal sealed class RNode
    {
        public readonly Dictionary<RKey, RNode> next = new(8);
        public int In { get; set; }

        public int Out => next.Count;

        /// <summary>
        /// 从上一个节点的next[Key]抵达当前节点
        /// </summary>
        public RKey Key { get; set; }

        /// <summary>
        /// 当前节点是否可达
        /// </summary>
        public bool Reachable => In > 0;

        /// <summary>
        /// 访问当前节点及其后面的所有节点
        /// </summary>
        /// <param name="visitor">参数1-当前被访问的节点  返回值-true：继续访问后继节点，false-访问终止后继节点</param>
        public void Visit(Func<RNode, bool> visitor)
        {
            if (visitor == null) return;

            if (!visitor.Invoke(this)) return;

            foreach (RNode node in next.Values) node.Visit(visitor);
        }

        public static RNode Create()
        {
            RNode node = InternalObjectPool<RNode>.Shared.Rent();
            node.next.Clear();
            node.In = 0;
            return node;
        }

        /// <summary>
        /// 安全地释放那些从当前节点可达并且入度为0的节点
        /// </summary>
        /// <param name="node"></param>
        public static void SafeDispose(RNode node)
        {
            if (node == null) return;
            --node.In;
            if (node.Out <= 0)
            {
                //如果不可回收，说明可通过其他节点可达
                if (!node.Reachable)
                    Recycle(node);
            }
            else
            {
                foreach (RNode next in node.next.Values) SafeDispose(next);
                node.next.Clear();
                if (!node.Reachable) Recycle(node);
            }
        }

        /// <summary>
        /// 强制释放当前节点可达的所有节点
        /// </summary>
        /// <param name="node"></param>
        public static void ForceDispose(RNode node)
        {
            if (node == null) return;
            if (node.Out <= 0)
            {
                Recycle(node);
            }
            else
            {
                foreach (RNode next in node.next.Values) ForceDispose(next);
                node.next.Clear();
                Recycle(node);
            }
        }

        private static void Recycle(RNode node)
        {
            if (node == null) return;
            node.In = 0;
            node.Key = default;
            node.next.Clear();
            InternalObjectPool<RNode>.Shared.Return(ref node);
        }
    }

    [Flags]
    internal enum RKeyType
    {
        None = 0,
        Graph = 1 << 0,
        Event = 1 << 1,
        Model = 1 << 2,
        Property = 1 << 3,
        Rendering = 1 << 4
    }

    internal readonly struct RKey : IEquatable<RKey>
    {
        public readonly RKeyType type;
        private readonly object _data;

        public T As<T>()
        {
            return _data is T t ? t : default;
        }

        public RKey(RGraph graph)
        {
            type = RKeyType.Graph;
            _data = graph;
        }

        public RKey(EventKey eventKey)
        {
            type = RKeyType.Event;
            _data = eventKey;
        }

        public RKey(string propertyName)
        {
            type = RKeyType.Property;
            _data = propertyName;
        }

        public RKey(BaseModel model)
        {
            type = RKeyType.Model;
            _data = model;
        }

        public RKey(Action renderFn)
        {
            type = RKeyType.Rendering;
            _data = renderFn;
        }

        public static implicit operator RKey(RGraph graph)
        {
            return new RKey(graph);
        }

        public static implicit operator RKey(EventKey eventKey)
        {
            return new RKey(eventKey);
        }

        public static implicit operator RKey(string propertyName)
        {
            return new RKey(propertyName);
        }

        public static implicit operator RKey(BaseModel model)
        {
            return new RKey(model);
        }

        public static implicit operator RKey(Action renderFn)
        {
            return new RKey(renderFn);
        }

        public bool Equals(RKey other)
        {
            if (type == other.type)
            {
                switch (type)
                {
                    case RKeyType.Graph: return As<RGraph>().Equals(other.As<RGraph>());
                    case RKeyType.Event: return As<EventKey>().Equals(other.As<EventKey>());
                    case RKeyType.Model: return As<BaseModel>() == other.As<BaseModel>();
                    case RKeyType.Property: return As<string>() == other.As<string>();
                    case RKeyType.Rendering: return As<Action>() == other.As<Action>();
                }
            }

            return false;
        }

        public override bool Equals(object obj)
        {
            return obj is RKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            return type switch
            {
                RKeyType.Graph => As<RGraph>().GetHashCode(),
                RKeyType.Model => As<BaseModel>()?.GetHashCode() ?? 0,
                RKeyType.Property => As<string>()?.GetHashCode() ?? 0,
                RKeyType.Rendering => As<Action>()?.GetHashCode() ?? 0,
                RKeyType.Event => As<EventKey>().GetHashCode(),
                _ => 0
            };
        }
    }
}