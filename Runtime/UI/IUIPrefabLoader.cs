using System;
using UnityEngine;

namespace UniVue.UI
{
    public interface IUIPrefabLoader
    {
        /// <summary>
        /// 加载UI预制体
        /// </summary>
        /// <param name="uiType">UI类型</param>
        /// <param name="callback">加载完成时回调，参数为加载的UI预制体</param>
        public void LoadUIPrefabAsync(Type uiType, Action<GameObject> callback);

        /// <summary>
        /// 加载UI预制体
        /// </summary>
        /// <param name="uiName">ui名称</param>
        /// <param name="callback">加载完成时回调，参数为加载的UI预制体</param>
        public void LoadUIPrefabAsync(string uiName, Action<GameObject> callback);
    }
}