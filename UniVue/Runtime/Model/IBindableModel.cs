
namespace UniVue.Model
{
    public interface IBindableModel : IUINotifier, IModelUpdater
    {
        /// <summary>
        /// 通知所有UI进行更新
        /// </summary>
        void NotifyAll();
    }
}
