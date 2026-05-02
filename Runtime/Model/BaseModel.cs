using System;
using UniVue.Internal;

namespace UniVue.Model
{
    public abstract class BaseModel
    {
        /// <summary>
        /// 参数1：当前实例  参数2：属性名  参数3：属性值
        /// </summary>
        public event Action<BaseModel, string, object> OnPropertyChanged;

        protected void CheckPropertyChanged<T>(string propertyName, T oldValue, T newValue)
        {
            if (OnPropertyChanged == null) return;
            if (!InternalEqualityComparer<T>.Comparer.Equals(oldValue, newValue))
                OnPropertyChanged.Invoke(this, propertyName, newValue);
        }
    }
}