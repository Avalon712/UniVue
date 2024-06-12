using UnityEngine;
using UniVue.View.Widgets;

namespace UniVue.View.Views
{
    public sealed class TipView : BaseView
    {
        private Tip _tipComp;

        public TipView(Tip tipComp, GameObject viewObject, string viewName = null, ViewLevel level = ViewLevel.Common) : base(viewObject, viewName, level)
        {
            _tipComp = tipComp;
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

        public override T GetWidget<T>()
        {
            return _tipComp as T;
        }
    }
}
