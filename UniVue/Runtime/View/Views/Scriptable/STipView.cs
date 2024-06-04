using System;
using TMPro;
using UnityEngine;
using UniVue.Utils;

namespace UniVue.View.Views
{
    public sealed class STipView : ScriptableView
    {
        [Header("用于显示提示消息的TMP_Text组件的名称")]
        [SerializeField] private string content;

        private TipComp _tipComp; 
   

        public override void OnLoad()
        {
            var contentTxt = GameObjectFindUtil.DepthFind(content, viewObject).GetComponent<TMP_Text>();
           
            if (string.IsNullOrEmpty(content) || contentTxt == null)
            {
                throw new ArgumentException("TipView必须指定用于显示消息内容的TMP_Text组件的名称!");
            }
            _tipComp = new() { content = contentTxt, name = this.name };

            base.OnLoad();
        }

        public override void OnUnload()
        {
            base.OnUnload();
            _tipComp.Destroy();
            _tipComp = null;
        }

        /// <summary>
        /// 打开当前的消息提示视图
        /// </summary>
        /// <param name="message">提示消息</param>
        /// <param name="top">是否显示与顶部</param>
        public void Open(string message, bool top = true)
        {
            _tipComp.Open(message, top);
        }
    }
}
