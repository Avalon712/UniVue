using TMPro;

namespace UniVue.View.Widgets
{
    public sealed class Tip : Widget
    {
        /// <summary>
        /// 获取用于显示提示文本的内容的TMP_Text的UI组件
        /// </summary>
        private TMP_Text _content;

        /// <summary>
        /// 视图名称
        /// </summary>
        private string _name;

        public Tip(string viewName, TMP_Text content)
        {
            _name = viewName;
            _content = content;
        }

        public override void Destroy()
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
