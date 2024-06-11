using System;
using System.Collections.Generic;

namespace UniVue.Utils
{
    /// <summary>
    /// 事件调用顺序：OnAdded | OnRemoved | OnReplaced  ----> OnChanged
    /// </summary>
    public interface IObservableList
    {
        /// <summary>
        /// 获得当前回调函数的观察者的id
        /// </summary>
        /// <remarks>没有的话将返回int.MinValue</remarks>
        int CurrentObserverId { get; }

        int Capacity { get; set; }

        int Count { get; }


        V Get<V>(int index);

        void Set<V>(V item, int index);

        void Add<V>(V item);

        bool Remove<V>(V item);

        bool Contains<V>(V item);

        void RemoveAt(int index);

        int IndexOf<V>(V item);

        void Sort();

        void Sort<V>(IComparer<V> comparer);

        /// <summary>
        /// 当集合内容发生改变时调用（包括元素的增加、删除、替换、顺序改变）
        /// </summary>
        void AddListener_OnChanged(int observerId, Action<NotificationMode> onChanged);

        /// <summary>
        /// 当有元素被移除时回调（arg0: 被移除的元素 arg1: 被移除元素的索引；）
        /// </summary>
        void AddListener_OnRemoved<V>(int observerId, Action<V, int> onRemoved);

        /// <summary>
        /// 当有新数据添加时回调
        /// </summary>
        void AddListener_OnAdded<V>(int observerId, Action<V> onAdded);

        /// <summary>
        /// 当元素被替换时回调。arg0是元素的索引；arg1是替换前的元素；arg2是替换后的元素
        /// </summary>
        void AddListener_OnReplaced<V>(int observerId, Action<int, V, V> onReplaced);

        void ClearObservers();

        void RemoveListeners(int observerId);

        /// <summary>
        /// 当集合内容发生改变时调用（包括元素的增加、删除、替换、顺序改变）
        /// </summary>
        void RemoveListener_OnChanged(int observerId);

        /// <summary>
        /// 当有元素被移除时回调（arg0: 被移除元素的索引； arg1: 被移除的元素）
        /// </summary>
        void RemoveListener_OnRemoved(int observerId);

        /// <summary>
        /// 当有新数据添加时回调
        /// </summary>
        void RemoveListener_OnAdded(int observerId);

        /// <summary>
        /// 当元素被替换时回调。arg0是元素的索引；arg1是替换前的元素；arg2是替换后的元素
        /// </summary>
        void RemoveListener_OnReplaced(int observerId);

    }

    public enum NotificationMode
    {
        Sort,
        Remove,
        Add,
        Replace,
    }
}
