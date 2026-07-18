#region

using UnityEngine;

#endregion

namespace UniVue.UI
{
    public interface IUILayerMgr
    {
        /// <summary>
        /// 所有层级的根节点
        /// </summary>
        public GameObject Root { get; }

        /// <summary>
        /// 获取层级的名称
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        public string GetLayerName(int layer);

        /// <summary>
        /// 获取指定层级的层级GameObject
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        public GameObject GetLayerRoot(int layer);
    }
}