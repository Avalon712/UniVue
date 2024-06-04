using System;
using TMPro;
using UnityEngine;
using UniVue.Utils;

namespace UniVue.View.Views
{
    public sealed class FTipView : FlexibleView
    {
        private TipComp _tipComp;

        public FTipView(string contentName, GameObject viewObject, string viewName = null, ViewLevel level = ViewLevel.Common) : base(viewObject, viewName, level)
        {
            var _content = GameObjectFindUtil.DepthFind(contentName, viewObject).GetComponent<TMP_Text>();
            CheckNull(_content);
            _tipComp = new() { content = _content, name = name };
        }

        public FTipView(TMP_Text content, GameObject viewObject, string viewName = null, ViewLevel level = ViewLevel.Common) : base(viewObject, viewName, level)
        {
            var _content = content;
            CheckNull(_content);
            _tipComp = new() { content = _content, name = name };
        }


        public override void OnUnload()
        {
            _tipComp.Destroy();
            _tipComp = null;
            base.OnUnload();
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

        private void CheckNull(TMP_Text content)
        {
            if (content == null)
            {
                throw new ArgumentException("FlexibleTipView必须指定用于显示消息内容的TMP_Text组件的名称!");
            }
        }

    }
}
