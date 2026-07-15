using System;
using UnityEngine;

namespace UniVue.UI
{
    public interface IUIPrefabLoader
    {
        /// <summary>
        /// 加载UI对象
        /// <remarks>如果加载的是一个根视图对象(通过UIMgr.Open方法加载)，则其GameObject身上必须挂有BaseView脚本</remarks>
        /// </summary>
        /// <param name="uiName">ui名称</param>
        /// <param name="callback">加载完成时回调，参数为加载的UI对象（可以是预制体也可以是实列好的对象）</param>
        public void LoadUIObjectAsync(string uiName, Action<GameObject> callback);
    }
}