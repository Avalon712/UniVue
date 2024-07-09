using System;
using System.Collections.Generic;
using UnityEngine;
using UniVue.Utils;

namespace UniVue.Rule
{
    /// <summary>
    /// 规则过滤器
    /// </summary>
    /// <remarks>
    /// 如果使用结构体实现此接口，要么重写Equals()和GetHashCode()方法，要么不要进行比较相
    /// 等操作和作为数据结构的key使用。因为不重写Equals()和GetHashCode()方法值类型的比较会造成大量垃圾对象</remarks>
    public interface IRuleFilter
    {
        /// <summary>
        /// 检查某个组件是否符合定义的规则
        /// </summary>
        /// <param name="component">待过滤组件，Item1:UI组件 Item2:UI类型</param>
        /// <param name="results">存储过滤结果</param>
        /// <returns>true:符合规则</returns>
        bool Check(ref ValueTuple<Component, UIType> component, List<object> results);

        /// <summary>
        /// 过滤结束
        /// </summary>
        /// <param name="results">过滤得到的结果集</param>
        void OnComplete(List<object> results);
    }
}
