using System;
using System.Collections.Generic;
using UnityEngine;
using UniVue.Utils;

namespace UniVue.Rule
{
    /// <summary>
    /// 规则过滤器
    /// </summary>
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
