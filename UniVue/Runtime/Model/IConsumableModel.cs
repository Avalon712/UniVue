using UniVue.ViewModel;

namespace UniVue.Model
{
    public interface IConsumableModel
    {
        /// <summary>
        /// 更新指定的UIBundle中的指定属性的UI
        /// </summary>
        /// <param name="propertyName">要更新的属性的属性名称</param>
        /// <param name="bundle">待更新的UIBundle</param>
        void UpdateUI(string propertyName, UIBundle bundle);

        /// <summary>
        /// 将模型的所有属性都更新到指定的UIBundle中
        /// </summary>
        /// <param name="bundle">待更新的UIBundle</param>
        void UpdateAll(UIBundle bundle);
    }
}
