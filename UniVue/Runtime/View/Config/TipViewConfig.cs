using System;
using TMPro;
using UnityEngine;
using UniVue.Utils;
using UniVue.View.Views;
using UniVue.View.Widgets;

namespace UniVue.View.Config
{
    public sealed class TipViewConfig : ViewConfig
    {
        [Header("用于显示提示消息的TMP_Text组件的名称")]
        public string content;

        internal override IView CreateView(GameObject viewObject)
        {
            ViewUtil.SetActive(viewObject, initState);
            var contentTxt = GameObjectFindUtil.DepthFind(content, viewObject).GetComponent<TMP_Text>();

            if (string.IsNullOrEmpty(content) || contentTxt == null)
            {
                throw new ArgumentException("TipView必须指定用于显示消息内容的TMP_Text组件的名称!");
            }
            Tip tipComp = new(viewName, contentTxt);

            TipView view = new(tipComp, viewObject, viewName, level);
            BaseSettings(view);
            return view;
        }
    }
}
