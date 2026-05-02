using System.Collections.Generic;
using UnityEngine;
using UniVue.Common;

namespace UniVue.UI
{
    public enum RedPointType
    {
        Dot,
        Number
    }

    public enum RedPointRule
    {
        Or,
        And
    }


    public abstract class RedPoint : BaseUI
    {
        [SerializeField] private RedPointType _type;
        [SerializeField] private RedPointRule _rule;

        private readonly List<ulong> _keys = new();

        public bool Status { get; private set; }

        protected sealed override void OnDispose()
        {
            foreach (ulong key in _keys) UIMgr.RedPointMgr.UnListenerRedPointStatus(key, OnStatusChangedPrivate);
            _keys.Clear();
        }

        /// <summary>
        /// 获取指定状态的所有红点key
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public IEnumerable<ulong> GetKeys(bool status)
        {
            foreach (ulong key in _keys)
            {
                if (UIMgr.RedPointMgr.GetStatus(key) == status)
                    yield return key;
            }
        }

        /// <summary>
        /// 获取指定状态的一个红点key，如果有多个满足条件的key，则返回第一个找到的key，如果没有满足条件的key，则返回0
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public ulong GetKey(bool status)
        {
            foreach (ulong key in _keys)
            {
                if (UIMgr.RedPointMgr.GetStatus(key) == status)
                    return key;
            }

            return 0;
        }

        /// <summary>
        /// 可以绑定多个key，当前红点的状态是否激活受到当前红点的Rule规则限制
        /// </summary>
        /// <param name="key">绑定的红点key</param>
        public void BindKey(ulong key)
        {
            RedPointMgr mgr = UIMgr.RedPointMgr;
            if (!mgr.IsRedPointKey(key))
            {
                LogUtil.Warn("绑定的key不是一个有效的红点key");
                return;
            }

            if (!_keys.Contains(key))
            {
                _keys.Add(key);
                mgr.ListenerRedPointStatus(key, OnStatusChangedPrivate);
            }
        }

        private void OnStatusChangedPrivate(bool status)
        {
            bool r = status;
            bool isOr = _rule == RedPointRule.Or;
            foreach (ulong key in _keys)
            {
                bool s = UIMgr.RedPointMgr.GetStatus(key);
                r = isOr ? r || s : r && s;
            }

            if (Status != r)
            {
                Status = r;
                OnStatusChanged(Status, GetKeys(r));
            }
        }

        /// <summary>
        /// 当红点状态发生改变时调用
        /// </summary>
        /// <param name="status">当前红点状态 true-激活  false-失活</param>
        /// <param name="keys">与当前红点状态一致的RedPointKey</param>
        protected abstract void OnStatusChanged(bool status, IEnumerable<ulong> keys);
    }
}