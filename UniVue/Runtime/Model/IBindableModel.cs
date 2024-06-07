

namespace UniVue.Model
{
    /*
     * 这个接口只对反射实现的默认方法进行重载
     */
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
        public void Bind(string viewName, bool allowUIUpdateModel)
        {
            Vue.Router.GetView(viewName).BindModel(this, allowUIUpdateModel);
        }
    }
}
