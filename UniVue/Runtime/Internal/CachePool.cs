using System.Collections.Generic;
using UniVue.Rule;
using UniVue.ViewModel;

namespace UniVue.Internal
{
    internal static class CachePool
    {
        private static List<Cache> _caches;

        /// <summary>
        /// 将一个内置对象放到缓存区（如果已经存在相同的对象了不会重复添加）
        /// </summary>
        /// <remarks>如果启用了缓存则会放到缓存区，没有则不会</remarks>
        /// <param name="type">内置对象类型</param>
        /// <param name="obj">内置对象</param>
        /// <param name="checkExist">添加时是否检查已经存在相同对象</param>
        public static void AddCache(InternalType type, object obj, bool checkExist = true)
        {
            if (Vue.Config.UseCache)
            {
                if (_caches == null)
                {
                    _caches = new List<Cache>(Vue.Config.CachePoolSize);
                }
                Cache cache = new Cache(type, obj);
                //防止扩容
                if ((!checkExist || !Contains(cache)) && _caches.Count < _caches.Capacity)
                {
                    _caches.Add(cache);
                }
            }
        }

        private static bool Contains(in Cache cache)
        {
            for (int i = 0; i < _caches.Count; i++)
            {
                if (cache.type == _caches[i].type && cache.obj == _caches[i].obj)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 从缓存区获取一个指定类型的对象
        /// </summary>
        /// <param name="type">内置可缓存的类型</param>
        /// <returns>
        /// 启用了缓存了则从缓存中查找，没找到会创建一个新对象返回，找到了则从缓冲区移除然后返回此对象；
        /// 没有启用缓存则直接创建一个新的对象返回
        /// </returns>
        public static object GetCache(InternalType type)
        {
            if (Vue.Config.UseCache && _caches != null)
            {
                for (int i = 0; i < _caches.Count; i++)
                {
                    if (_caches[i].type == type)
                    {
                        object obj = _caches[i].obj;
                        TrailDelete(i);
                        return obj;
                    }
                }
            }
            switch (type)
            {
                case InternalType.List_String: return new List<string>();
                case InternalType.List_ModelUI: return new List<ModelUI>();
                case InternalType.List_PropertyUI: return new List<PropertyUI>();
                case InternalType.List_ModelRuleResult: return new List<ModelRuleResult>();
                case InternalType.List_EventRuleResult: return new List<EventRuleResult>();
            }
            return null;
        }

        /// <summary>
        /// 清空缓存区
        /// </summary>
        public static void ClearCache()
        {
            if (_caches != null)
            {
                _caches.Clear();
            }
        }

        private static void TrailDelete(int deleteIndex)
        {
            int trialIdx = _caches.Count - 1;
            if (deleteIndex == trialIdx)
            {
                _caches.RemoveAt(deleteIndex);
            }
            else
            {
                Cache del = _caches[deleteIndex];
                Cache trail = _caches[trialIdx];
                _caches[trialIdx] = del;
                _caches[deleteIndex] = trail;
                _caches.RemoveAt(trialIdx);
            }
        }
    }
}
