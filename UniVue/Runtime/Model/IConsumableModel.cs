using UniVue.ViewModel;

namespace UniVue.Model
{
    public interface IConsumableModel
    {
        /// <summary>
        /// 更新指定的ModelUI中的指定属性的UI
        /// </summary>
        /// <param name="propertyName">要更新的属性的属性名称</param>
        /// <param name="modelUI">待更新的ModelUI</param>
        void UpdateUI(string propertyName, ModelUI modelUI);

        /// <summary>
        /// 将模型的所有属性都更新到指定的ModelUI中
        /// </summary>
        /// <param name="modelUI">待更新的ModelUIe</param>
        void UpdateAll(ModelUI modelUI);
    }
}
