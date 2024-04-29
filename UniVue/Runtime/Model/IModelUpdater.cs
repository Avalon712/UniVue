
namespace UniVue.Model
{
    public interface IModelUpdater
    {
        /// <summary>
        /// 更新模型数据 (UI -> Model)
        /// </summary>
        /// <param name="propertyName">属性名</param>
        /// <param name="propertyValue">属性值</param>
        public void UpdateModel(string propertyName, int propertyValue);

        /// <summary>
        /// 更新模型数据 (UI -> Model)
        /// </summary>
        /// <param name="propertyName">属性名</param>
        /// <param name="propertyValue">属性值</param>
        public void UpdateModel(string propertyName, string propertyValue);

        /// <summary>
        /// 更新模型数据 (UI -> Model)
        /// </summary>
        /// <param name="propertyName">属性名</param>
        /// <param name="propertyValue">属性值</param>
        public void UpdateModel(string propertyName, float propertyValue);

        /// <summary>
        /// 更新模型数据 (UI -> Model)
        /// </summary>
        /// <param name="propertyName">属性名</param>
        /// <param name="propertyValue">属性值</param>
        public void UpdateModel(string propertyName, bool propertyValue);
    }
}
