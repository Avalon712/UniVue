using TMPro;

namespace UniVue.View.Views
{
    public sealed class TipComp : IViewComp
    {
        /// <summary>
        /// 获取用于显示提示文本的内容的TMP_Text的UI组件
        /// </summary>
        public TMP_Text content { get; set; }

        /// <summary>
        /// 视图名称
        /// </summary>
        public string name { get; set; }

        public void Destroy()
        {
            content = null;
        }

        /// <summary>
        /// 打开当前的消息提示视图
        /// </summary>
        /// <param name="message">提示消息</param>
        /// <param name="top">是否显示与顶部</param>
        public void Open(string message, bool top = true)
        {
            content.text = message;
            Vue.Router.Open(name, top);
        }

    }
}
