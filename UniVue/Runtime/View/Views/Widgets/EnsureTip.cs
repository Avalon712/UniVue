using System;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UniVue.View.Widgets
{
    public sealed class EnsureTip : Widget
    {
        private string _viewName;
        private Button _cancelBtn;
        private Button _sureBtn;
        private TMP_Text _message;
        private TMP_Text _title;

        public EnsureTip(string viewName, Button cancelBtn, Button sureBtn, TMP_Text message, TMP_Text title)
        {
            if (sureBtn == null)
                throw new Exception($"必须指定确认按钮");

            if (cancelBtn == null)
                throw new Exception($"必须指定取消按钮");

            if (message == null)
                throw new Exception($"必须指定用于显示消息的TMP_Text组件!");
            _viewName = viewName;
            _cancelBtn = cancelBtn;
            _sureBtn = sureBtn;
            _message = message;
            _title = title;
        }

        /// <summary>
        /// 打开此视图
        /// </summary>
        /// <remarks>注：事件绑定是暂时的，当视图关闭后会自动注销回调事件</remarks>
        /// <param name="title">消息的标题,可以为null</param>
        /// <param name="message">要显示的消息</param>
        /// <param name="sure">点击"确定"按钮时回调函数</param>
        /// <param name="cancel">点击"取消"按钮时回调函数</param>
        public void Open(string title, string message, UnityAction sure, UnityAction cancel)
        {
            if (_title != null) { _title.text = title; }
            _message.text = message;
            if (sure != null) { _sureBtn.onClick.AddListener(sure); }
            if (cancel != null) { _cancelBtn.onClick.AddListener(cancel); }
            Vue.Router.Open(_viewName);
        }

        public void Close()
        {
            _sureBtn.onClick.RemoveAllListeners();
            _cancelBtn.onClick.RemoveAllListeners();
        }

        public override void Destroy()
        {
            _cancelBtn = null;
            _sureBtn = null;
            _message = null;
            _title = null;
        }
    }
}
