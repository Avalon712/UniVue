using System;
using TMPro;
using UnityEngine;
using UniVue.Utils;

namespace UniVue.View.Views.Flexible
{
    public sealed class FTipView : FlexibleView
    {
        private TMP_Text _content;

        public FTipView(string contentName,GameObject viewObject, string viewName = null, ViewLevel level = ViewLevel.Common) : base(viewObject, viewName, level)
        {
            _content = GameObjectFindUtil.DepthFind(contentName, viewObject).GetComponent<TMP_Text>();
            CheckNull();
        }

        public FTipView(TMP_Text content, GameObject viewObject, string viewName = null, ViewLevel level = ViewLevel.Common) : base(viewObject, viewName, level)
        {
            _content = content;
            CheckNull();
        }

        public override void OnUnload()
        {
            _content = null;
            base.OnUnload();
        }

        /// <summary>
        /// 打开当前的消息提示视图
        /// </summary>
        /// <param name="message">提示消息</param>
        /// <param name="top">是否显示与顶部</param>
        public void Open(string message, bool top = true)
        {
            _content.text = message;
            Vue.Router.Open(name, top);
        }

        private void CheckNull()
        {
            if (_content == null)
            {
                throw new ArgumentException("FlexibleTipView必须指定用于显示消息内容的TMP_Text组件的名称!");
            }
        }
    }
}
