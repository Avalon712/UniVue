using System.Collections.Generic;
using UnityEngine;

namespace UniVue.Model
{
    /// <summary>
    /// 该接口对所有接口提供默认实现
    /// </summary>
    public interface IImplementedModel : IUINotifier, IModelUpdater
    {
        internal IBindableModel Binder { get; }

        IUINotifier IModelUpdater.Notifier => this;


        void IUINotifier.NotifyUIUpdate(string propertyName, int propertyValue)
        {
            Vue.Updater.UpdateUI(Binder, propertyName, propertyValue);
        }

        void IUINotifier.NotifyUIUpdate(string propertyName, string propertyValue)
        {
            Vue.Updater.UpdateUI(Binder, propertyName, propertyValue);
        }

        void IUINotifier.NotifyUIUpdate(string propertyName, bool propertyValue)
        {
            Vue.Updater.UpdateUI(Binder, propertyName, propertyValue);
        }

        void IUINotifier.NotifyUIUpdate(string propertyName, float propertyValue)
        {
            Vue.Updater.UpdateUI(Binder, propertyName, propertyValue);
        }

        void IUINotifier.NotifyUIUpdate(string propertyName, Sprite propertyValue)
        {
            Vue.Updater.UpdateUI(Binder, propertyName, propertyValue);
        }

        void IUINotifier.NotifyUIUpdate(string propertyName, List<int> propertyValue)
        {
            Vue.Updater.UpdateUI(Binder, propertyName, propertyValue);
        }

        void IUINotifier.NotifyUIUpdate(string propertyName, List<string> propertyValue)
        {
            Vue.Updater.UpdateUI(Binder, propertyName, propertyValue);
        }

        void IUINotifier.NotifyUIUpdate(string propertyName, List<bool> propertyValue)
        {
            Vue.Updater.UpdateUI(Binder, propertyName, propertyValue);
        }

        void IUINotifier.NotifyUIUpdate(string propertyName, List<float> propertyValue)
        {
            Vue.Updater.UpdateUI(Binder, propertyName, propertyValue);
        }

        void IUINotifier.NotifyUIUpdate(string propertyName, List<Sprite> propertyValue)
        {
            Vue.Updater.UpdateUI(Binder, propertyName, propertyValue);
        }

        void IUINotifier.NotifyUIUpdate<T>(string propertyName, List<T> propertyValue)
        {
            Vue.Updater.UpdateUI(Binder, propertyName, propertyValue);
        }

    }
}
