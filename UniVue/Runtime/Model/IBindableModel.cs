

namespace UniVue.Model
{
    public interface IBindableModel : IImplementedModel
    {
        IBindableModel IImplementedModel.Binder => this;

        /// <summary>
        /// 将所有对此模型的绑定的视图都进行解绑
        /// </summary>
        public sealed void Unbind()
        {
            Vue.Updater.Unbind(this);
        }

        /// <summary>
        /// 绑定到指定视图
        /// </summary>
        /// <param name="viewName">要绑定的视图的名称</param>
        /// <param name="allowUIUpdateModel">是否允许通过UI改变模型数据</param>
        /// <param name="modelName">当前模型的名称</param>
        /// <param name="forceRebind">是否强制重新绑定</param>
        public void Bind(string viewName, bool allowUIUpdateModel, string modelName = null, bool forceRebind = false)
        {
            Vue.Router.GetView(viewName).BindModel(this, allowUIUpdateModel, modelName, forceRebind);
        }
    }
}
