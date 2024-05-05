
namespace UniVue.Model
{
    public interface IBindableModel : IUINotifier, IModelUpdater
    {
        /// <summary>
        /// 通知所有UI进行更新
        /// </summary>
        void NotifyAll();

        /// <summary>
        /// 解除此模型与所有它绑定的视图的关系
        /// </summary>
        void Unbind();

        /// <summary>
        /// 将此模型绑定到指定视图名称的视图上
        /// </summary>
        /// <param name="viewName">视图名称</param>
        /// <param name="allowUIUpdateModel">是否允许UI更新模型数据</param>
        void Bind(string viewName,bool allowUIUpdateModel=true);

    }
}
