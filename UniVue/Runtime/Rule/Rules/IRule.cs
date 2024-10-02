using UnityEngine;
using UniVue.Common;

namespace UniVue.Rule
{
    /// <summary>
    /// 规则过滤器
    /// </summary>
    public interface IRule
    {
        /// <summary>
        /// 检查是否符合定义的规则
        /// </summary>
        /// <param name="rule">要检查的规则</param>
        /// <param name="uiType">当前UI组件类型</param>
        /// <param name="ui">UI组件</param>
        bool Check(string rule, UIType uiType, Component ui);

        /// <summary>
        /// 过滤结束
        /// </summary>
        void OnComplete();
    }
}
