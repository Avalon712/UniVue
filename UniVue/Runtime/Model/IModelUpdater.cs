using System.Reflection;
using System;

namespace UniVue.Model
{
    /// <summary>
    /// 由UI主动触发模型更新操作
    /// </summary>
    public interface IModelUpdater
    {
        internal IUINotifier Notifier { get; }

        public void UpdateModel(string propertyName, int propertyValue)
        {
            UpdateModel<int>(propertyName, propertyValue);
            Notifier.NotifyUIUpdate(propertyName, propertyValue);
        }

        public void UpdateModel(string propertyName, string propertyValue)
        {
            UpdateModel<string>(propertyName, propertyValue);
            Notifier.NotifyUIUpdate(propertyName, propertyValue);
        }

        public void UpdateModel(string propertyName, float propertyValue)
        {
            UpdateModel<float>(propertyName, propertyValue);
            Notifier.NotifyUIUpdate(propertyName, propertyValue);
        }


        public void UpdateModel(string propertyName, bool propertyValue)
        {
            UpdateModel<bool>(propertyName, propertyValue);
            Notifier.NotifyUIUpdate(propertyName, propertyValue);
        }

        private void UpdateModel<T>(string propertyName, T propertyValue)
        {
            PropertyInfo[] propertyInfos = this.GetType().GetProperties();
            for (int i = 0; i < propertyInfos.Length; i++)
            {
                if (propertyInfos[i].Name == propertyName)
                {
                    if (propertyInfos[i].PropertyType.IsEnum)
                    {
                        propertyInfos[i].SetValue(this, Enum.ToObject(propertyInfos[i].PropertyType, propertyValue));
                    }
                    else
                    {
                        propertyInfos[i].SetValue(this, propertyValue);
                    }
                    return;
                }
            }
        }
    }
}
