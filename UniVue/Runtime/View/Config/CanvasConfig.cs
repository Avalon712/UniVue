using System.Collections.Generic;
using UnityEngine;
using UniVue.View.Views;

namespace UniVue.View.Config
{
    public sealed class CanvasConfig : ScriptableObject
    {
        /// <summary>
        /// 若Canvas不是预制体则通过设置名字来进行查找
        /// </summary>
        [Header("Canvas的名称")]
        [Tooltip("GameObject.Find(canvasName)查找挂载Canvas的游戏对象")]
        public string canvasName;


        [Header("Canvas渲染的视图")]
        public BaseView[] views;
    }
}
