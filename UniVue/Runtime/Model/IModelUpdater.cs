
namespace UniVue.Model
{
    /// <summary>
    /// 由UI主动触发模型更新操作
    /// </summary>
    public interface IModelUpdater
    {
        internal IUINotifier Notifier { get; }

        public void UpdateModel(string propertyName, int propertyValue);

        public void UpdateModel(string propertyName, string propertyValue);

        public void UpdateModel(string propertyName, float propertyValue);

        public void UpdateModel(string propertyName, bool propertyValue);

    }
}
