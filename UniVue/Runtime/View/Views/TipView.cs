using System;
using TMPro;
using UnityEngine;
using UniVue.Utils;

namespace UniVue.View.Views
{
    public sealed class TipView : BaseView
    {
        [Header("用于显示提示消息的TMP_Text组件的名称")]
        [SerializeField] private string content;

        private struct RuntimeData
        {
            public TMP_Text text;
        }

        private RuntimeData _runtime;

        public override void OnLoad()
        {
            _runtime.text = GameObjectFindUtil.DepthFind(content, viewObject).GetComponent<TMP_Text>();
           
            if (string.IsNullOrEmpty(content) || _runtime.text == null)
            {
                throw new ArgumentException("TipView必须指定用于显示消息内容的TMP_Text组件的名称!");
            }

            base.OnLoad();
        }

        /// <summary>
        /// 打开当前的消息提示视图
        /// </summary>
        /// <param name="message">提示消息</param>
        /// <param name="top">是否显示与顶部</param>
        public void Open(string message,bool top=true)
        {
            _runtime.text.text = message;
            Vue.Router.Open(name, top);
        }

        public override void OnUnload()
        {
            base.OnUnload();
            _runtime = default;
        }
    }
}
