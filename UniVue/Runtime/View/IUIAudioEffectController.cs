
namespace UniVue.View
{
    public interface IUIAudioEffectController
    {
        /// <summary>
        /// 播放指定视图的声音效果
        /// </summary>
        /// <param name="viewName">视图名称</param>
        void PlayAudioEffect(string viewName); 

        /// <summary>
        /// 视图打开后调用
        /// </summary>
        /// <param name="openingViewName">正则被打开的视图名称</param>
        void AfterOpen(string openingViewName);

        /// <summary>
        /// 视图关闭后调用
        /// </summary>
        /// <param name="closingViewName">正则被关闭的视图名称</param>
        void AfterClose(string closingViewName);
    }
}
