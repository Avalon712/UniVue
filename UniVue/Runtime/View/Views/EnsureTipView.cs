using UnityEngine;
using UnityEngine.Events;
using UniVue.View.Widgets;

namespace UniVue.View.Views
{
    public sealed class EnsureTipView : BaseView
    {
        private EnsureTip _comp;

        public EnsureTipView(EnsureTip tipComp, GameObject viewObject, string viewName = null, ViewLevel level = ViewLevel.Common) : base(viewObject, viewName, level)
        {
            _comp = tipComp;
        }

        public override void OnUnload()
        {
            _comp.Destroy();
            _comp = null;
            base.OnUnload();
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
            _comp.Open(title, message, sure, cancel);
        }

        public override void Close()
        {
            base.Close();
            _comp.Close();
        }

        public override T GetWidget<T>()
        {
            return _comp as T;
        }
    }
}
