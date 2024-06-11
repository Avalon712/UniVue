using System;
using TMPro;
using UnityEngine;

namespace UniVue.View.Views
{
    [Serializable]
    public sealed class TipWidget
    {
        /// <summary>
        /// 获取用于显示提示文本的内容的TMP_Text的UI组件
        /// </summary>
        [SerializeField] private TMP_Text _content;

        /// <summary>
        /// 视图名称
        /// </summary>
        [SerializeField] private string _name;

        public TipWidget(string viewName, TMP_Text content)
        {
            _name = viewName;
            _content = content;
        }

        public void Destroy()
        {
            _content = null;
        }

        /// <summary>
        /// 打开当前的消息提示视图
        /// </summary>
        /// <param name="message">提示消息</param>
        /// <param name="top">是否显示与顶部</param>
        public void Open(string message, bool top = true)
        {
            _content.text = message;
            Vue.Router.Open(_name, top);
        }

    }
}
