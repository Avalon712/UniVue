using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniVue.Common;
using UniVue.Coroutine;
using UniVue.Internal;
using UniVue.Utils;

namespace UniVue.UI
{
    public sealed class RedPointMgr
    {
        public const uint RootMaxCount = ushort.MaxValue;
        public const uint NotRootMaxCount = byte.MaxValue - 1;

        private readonly Dictionary<ulong, RedPointNode> _allNodes = new(64);
        private readonly HashSet<ulong> _dirtyTrees = new(16);

        /// <summary>
        /// 动态创建的红点树，只存根节点的key
        /// </summary>
        private readonly HashSet<ulong> _dynamicRoots = new(64);

        private readonly Dictionary<ulong, Action<bool>> _listeners = new(64);
        private readonly Dictionary<ulong, bool> _recordForceStaus = new(32); //不根据内置激活规则设置的状态

        /// <summary>
        /// key=根节点的Key，所有红点树的根节点
        /// </summary>
        private readonly Dictionary<ulong, RedPointNode> _trees = new();

        public RedPointMgr()
        {
            CoroutineMgr.Run(UpdateRedPointSystem());
        }

        public RedPointMgr(Type redPointKeyType)
        {
            Initialize(redPointKeyType);
        }

        private void Initialize(Type redPointKeyType)
        {
            if (Enum.GetValues(redPointKeyType) is not ulong[] keys)
            {
                LogUtil.Warn("RedPointMgr初始化失败，RedPointKey枚举类型必须继承自ulong");
                return;
            }

            foreach (ulong key in keys)
            {
                RedPointNode node = new();
                _allNodes[key] = node;
                node.key = key;
#if UNITY_EDITOR
                node.keyName = Enum.GetName(redPointKeyType, key);
#endif
            }

            Array.Sort(keys, Compare);
            foreach (ulong key in keys)
            {
                RedPointNode node = _allNodes[key];
                if (IsRoot(key))
                {
                    _trees[key] = node;
                }
                else
                {
                    ulong parentKey = GetParentKey(key);
                    RedPointNode parent = _allNodes[parentKey];
                    parent.children.Add(node);
                    node.parent = parent;
                }
            }

            foreach (ulong key in keys)
            {
                RedPointNode node = _allNodes[key];
                if (!node.IsLeaf)
                    node.rule = GetRule(key);
            }

            CoroutineMgr.Run(UpdateRedPointSystem());
        }

        private IEnumerator UpdateRedPointSystem()
        {
            WaitForEndOfFrame endOfFrame = new();
            while (true)
            {
                yield return endOfFrame;
                using InternalTempCollection<HashSet<ulong>, ulong> copyDirtyTree = new(_dirtyTrees);
                foreach (ulong dirtyTree in copyDirtyTree)
                {
                    if (_dirtyTrees.Remove(dirtyTree) && _trees.TryGetValue(dirtyTree, out RedPointNode root))
                        UpdateRedPointTree(root);
                }
            }
        }

        private void UpdateRedPointTree(RedPointNode node)
        {
            if (node.IsLeaf) return;

            foreach (RedPointNode child in node.children) UpdateRedPointTree(child);

            // 再根据强制状态或子节点状态计算当前节点状态
            bool beforeStatus = node.status;
            node.status = IsForceStatus(node.key) ? _recordForceStaus[node.key] : GetChildrenStatus(node);

            if (beforeStatus != node.status && _listeners.TryGetValue(node.key, out Action<bool> onStatusChanged))
                onStatusChanged?.Invoke(node.status);
        }

        private bool GetChildrenStatus(RedPointNode parent)
        {
            bool status = parent.children[0].status;
            bool isOr = parent.rule == RedPointRule.Or;
            foreach (RedPointNode child in parent.children)
                status = isOr ? status || child.status : status && child.status;
            return status;
        }

        public void ListenerRedPointStatus(ulong key, Action<bool> onStatusChanged)
        {
            if (!_allNodes.ContainsKey(key) || onStatusChanged == null)
                return;

            if (_listeners.TryGetValue(key, out Action<bool> callbacks))
                _listeners[key] = callbacks + onStatusChanged;
            else
                _listeners[key] = onStatusChanged;
        }

        public void UnListenerRedPointStatus(ulong key, Action<bool> onStatusChanged)
        {
            if (!_listeners.TryGetValue(key, out Action<bool> callbacks) || !_allNodes.ContainsKey(key) ||
                onStatusChanged == null) return;
            _listeners[key] = callbacks - onStatusChanged;
            if (_listeners[key] == null)
                _listeners.Remove(key);
        }

        /// <summary>
        /// 根据红点树的激活规则设置状态
        /// </summary>
        /// <param name="key">必须是一个叶子节点</param>
        /// <param name="status"></param>
        public void SetActive(ulong key, bool status)
        {
            if (_allNodes.TryGetValue(key, out RedPointNode node))
            {
                if (!node.IsLeaf)
                {
                    LogUtil.Error($"红点key={node.keyName}不是叶子节点！SetActive()仅能设置叶子节点的激活状态，父节点状态受到孩子节点状态以及激活规则而自动改变");
                }
                else
                {
                    if (node.status != status)
                    {
                        node.status = status;
                        _dirtyTrees.Add(GetRootKey(key));
                    }
                }
            }
        }

        /// <summary>
        /// 强制设置某个节点的状态（如果是叶子节点，等价于调用SetActive()方法）
        /// </summary>
        public void ForceSetActive(ulong key, bool status)
        {
            if (!_allNodes.TryGetValue(key, out RedPointNode node)) return;
            if (node.IsLeaf)
            {
                node.status = status;
            }
            else
            {
                _recordForceStaus[key] = status;
                _allNodes[key].status = status;
            }

            _dirtyTrees.Add(GetRootKey(key));
        }

        public bool IsForceStatus(ulong key)
        {
            return _recordForceStaus.ContainsKey(key);
        }

        public void DeleteForceStatus(ulong key)
        {
            _recordForceStaus.Remove(key);
            _dirtyTrees.Add(GetRootKey(key));
        }

        /// <summary>
        /// 获取指定key的状态
        /// </summary>
        /// <param name="key">RedPointKey</param>
        /// <param name="rightNow">true-如果当前红点树被标记为脏则会立即执行一次更新再返回红点key的状态（不推荐，因为这一帧可能有多次状态修改）  false-等待红点系统的统一更新</param>
        /// <returns></returns>
        public bool GetStatus(ulong key, bool rightNow = false)
        {
            if (rightNow && _dirtyTrees.TryGetValue(GetRootKey(key), out ulong rootKey) &&
                _trees.TryGetValue(rootKey, out RedPointNode root))
            {
                _dirtyTrees.Remove(rootKey);
                UpdateRedPointTree(root);
            }

            return _allNodes.TryGetValue(key, out RedPointNode node) && node.status;
        }


        public bool IsDynamicDependency(ulong key)
        {
            return _dynamicRoots.Contains(GetRootKey(key));
        }

        public void GetChildrenKeysNoneAlloc(ulong key, HashSet<ulong> childrenKeys)
        {
            ExceptionUtils.ThrowIfArgNull(childrenKeys, nameof(childrenKeys));
            if (!_allNodes.TryGetValue(key, out RedPointNode node)) return;
            foreach (RedPointNode child in node.children) childrenKeys.Add(child.key);
        }

        public bool HaveChildren(ulong key)
        {
            if (!_allNodes.TryGetValue(key, out RedPointNode node)) return false;
            return node.children.Count > 0;
        }

        /// <summary>
        /// 创建一颗红点树
        /// </summary>
        /// <param name="rule">激活规则</param>
        /// <param name="keyName">调试时可以在窗口中看见别名</param>
        /// <returns>红点树的根节点的key</returns>
        public ulong CreateRedPointTree(RedPointRule rule = RedPointRule.Or, string keyName = "")
        {
            ushort maxRootVal = 0;
            bool isFound = false;
            for (uint i = 0; i <= RootMaxCount; i++)
            {
                if (!_allNodes.ContainsKey(i))
                {
                    maxRootVal = (ushort)i;
                    isFound = true;
                    break;
                }
            }

            if (!isFound)
            {
                LogUtil.Error("红点树根key已经全部被用完(0~65535)，无法再创建新树");
                return 0;
            }

            int ruleBit = rule == RedPointRule.And ? 1 : 0;
            ushort newRootVal = maxRootVal == 0
                ? (ushort)(2 | ruleBit)
                : (ushort)((((maxRootVal >> 1) + 1) << 1) | ruleBit);
            ulong key = (ulong)newRootVal << 48;
            RedPointNode root = new() { key = key, rule = rule };
#if UNITY_EDITOR
            root.keyName = keyName;
#endif
            _allNodes[key] = root;
            _trees.Add(key, root);
            _dynamicRoots.Add(key);
            return key;
        }

        /// <summary>
        /// 动态添加依赖
        /// </summary>
        /// <param name="parentKey">父key</param>
        /// <param name="rule">父节点激活规则（如果当前节点以及是一个父节点，则此值不会生效，仅当当前的parentKey对应的节点在未添加孩子节点时是一个孩子节点时生效）</param>
        /// <returns>节点key</returns>
        public ulong AddDependency(ulong parentKey, RedPointRule rule = RedPointRule.Or)
        {
            if (!_allNodes.TryGetValue(parentKey, out RedPointNode parent))
            {
                LogUtil.Error($"RedPointMgr.AddDependency()失败，父节点key={parentKey}不存在");
                return 0;
            }

            int parentDepth = GetDepth(parentKey);
            if (parentDepth >= 6)
            {
                LogUtil.Error("RedPointMgr.AddDependency()失败，父节点已为第7级，无法再添加子节点");
                return 0;
            }

            int siblingIndex = parent.children.Count;
            if (siblingIndex >= 127)
            {
                LogUtil.Error("RedPointMgr.AddDependency()失败，父节点的孩子数量已经达到最大127，无法再添加子节点");
                return 0;
            }

            int ruleBit;
            if (parent.children.Count == 0)
            {
                ruleBit = rule == RedPointRule.And ? 1 : 0;
                parent.rule = rule;
            }
            else
            {
                ruleBit = parent.rule == RedPointRule.And ? 1 : 0;
            }

            byte childByte = (byte)(((siblingIndex + 1) << 1) | ruleBit);
            int shift = 8 * (6 - (parentDepth + 1));
            ulong childKey = parentKey | ((ulong)childByte << shift);

            RedPointNode child = new()
            {
                key = childKey,
                parent = parent
            };
            parent.children.Add(child);

#if UNITY_EDITOR
            string temp = parent.keyName.Replace(" [Dynamic]", "");
            child.keyName = $"{temp}_{siblingIndex + 1} [Dynamic]";
#endif

            _allNodes[childKey] = child;
            _dirtyTrees.Add(GetRootKey(childKey));
            return childKey;
        }

        /// <summary>
        /// 删除动态添加的依赖节点。仅当 IsDynamicDependency(key) 为 true 时可用。
        /// </summary>
        /// <param name="key">要删除的红点Key，如果是非叶子节点，则以当前节点形成的子树会被整个删除</param>
        public void DeleteDependency(ulong key)
        {
            if (!_dynamicRoots.Contains(GetRootKey(key)))
            {
                LogUtil.Warn($"RedPointMgr.DeleteDependency()失败，key={key}不是动态添加的节点");
                return;
            }

            if (!_allNodes.TryGetValue(key, out RedPointNode node))
            {
                LogUtil.Error($"RedPointMgr.DeleteDependency()失败，key={key}不存在");
                return;
            }

            DeleteDependencyRecursive(node, false);
            node.parent?.children.Remove(node);
            node.parent = null;
            if (IsRoot(key))
                _trees.Remove(key);
            else
                _dirtyTrees.Add(GetRootKey(key));
        }

        private void DeleteDependencyRecursive(RedPointNode node, bool setParentNull = true)
        {
            if (node == null) return;

            foreach (RedPointNode child in node.children)
                DeleteDependencyRecursive(child);

            node.children.Clear();
            if (setParentNull)
                node.parent = null;
            _allNodes.Remove(node.key);
            if (IsRoot(node.key))
                _dynamicRoots.Remove(node.key);
            _recordForceStaus.Remove(node.key);
        }

        public bool IsRedPointKey(ulong key)
        {
            return _allNodes.ContainsKey(key);
        }

        public static int Compare(ulong a, ulong b)
        {
            int depthA = GetDepth(a);
            int depthB = GetDepth(b);
            return depthA != depthB ? depthA.CompareTo(depthB) : a.CompareTo(b);
        }

        /// <summary>
        /// 获取根节点的 key 值（仅高 16 位，低 48 位为 0）
        /// </summary>
        public static ulong GetRootKey(ulong key)
        {
            return (key >> 48) << 48;
        }

        /// <summary>
        /// 判断 key1 是否是 key2 的父亲
        /// </summary>
        public static bool IsParentOf(ulong key1, ulong key2)
        {
            return GetParentKey(key2) == key1;
        }

        /// <summary>
        /// 获取父节点的 key 值
        /// </summary>
        public static ulong GetParentKey(ulong key)
        {
            int depth = GetDepth(key);
            if (depth <= 0) return 0;
            int shift = 8 * (6 - depth);
            return key & ~(0xFFUL << shift);
        }

        /// <summary>
        /// 是否为根节点
        /// </summary>
        public static bool IsRoot(ulong key)
        {
            return (key & 0x0000FFFFFFFFFFFFUL) == 0;
        }

        /// <summary>
        /// 获取当前节点与子节点之间的规则
        /// </summary>
        public static RedPointRule GetRule(ulong key)
        {
            int depth = GetDepth(key);
            int shift = depth == 0 ? 48 : 8 * (6 - depth);
            int ruleBit = (int)((key >> shift) & 1);
            return ruleBit == 1 ? RedPointRule.And : RedPointRule.Or;
        }

        /// <summary>
        /// 获取节点深度（子层级数）。根=0，深度1=1个子字节，深度2=2个子字节...
        /// </summary>
        public static int GetDepth(ulong value)
        {
            if ((value & 0x0000FFFFFFFFFFFFUL) == 0) return 0;
            int depth = 0;
            for (int shift = 40; shift >= 0; shift -= 8)
            {
                if (((value >> shift) & 0xFF) != 0)
                    depth++;
            }

            return depth;
        }

        internal sealed class RedPointNode
        {
            public readonly List<RedPointNode> children = new(0);
            public ulong key;
#if UNITY_EDITOR
            public string keyName;
#endif
            public RedPointNode parent;
            public RedPointRule rule;
            public bool status;
            public bool IsLeaf => children == null || children.Count == 0;
        }
    }
}