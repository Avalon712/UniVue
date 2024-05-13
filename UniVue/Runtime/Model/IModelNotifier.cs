
namespace UniVue.Model
{
    public interface IModelNotifier
    {
        /// <summary>
        /// 通知模型更新
        /// </summary>
        /// <param name="propertyName">属性名</param>
        /// <param name="propertyValue">属性值</param>
        public void NotifyModelUpdate(string propertyName, int propertyValue);

        /// <summary>
        /// 通知模型更新
        /// </summary>
        /// <param name="propertyName">属性名</param>
        /// <param name="propertyValue">属性值</param>
        public void NotifyModelUpdate(string propertyName, string propertyValue);

        /// <summary>
        /// 通知模型更新
        /// </summary>
        /// <param name="propertyName">属性名</param>
        /// <param name="propertyValue">属性值</param>
        public void NotifyModelUpdate(string propertyName, bool propertyValue);

        /// <summary>
        /// 通知模型更新
        /// </summary>
        /// <param name="propertyName">属性名</param>
        /// <param name="propertyValue">属性值</param>
        public void NotifyModelUpdate(string propertyName, float propertyValue);

    }
}
