using System;
using System.Collections.Generic;
using UnityEngine;
using UniVue.Model;
using UniVue.Utils;

namespace UniVue.ViewModel.Models
{
    /// <summary>
    /// 枚举类型可以绑定的UI组件：TMP_Dropdrown、ToggleGroup
    /// </summary>
    public abstract class EnumPropertyUI<UI> : PropertyUI
    {
        /// <summary>
        /// 绑定的UI组件
        /// </summary>
        protected UI _ui;
        /// <summary>
        /// item1=枚举值的字符串、item2=枚举值的别名(如果没有别名则等同于item1)、item3=枚举值
        /// </summary>
        protected List<CustomTuple<string, string, int>> _enums;

        protected EnumPropertyUI(UI ui, Array array,IModelNotifier notifier, string propertyName, bool allowUIUpdateModel) : base(notifier, propertyName, allowUIUpdateModel)
        {
            _ui = ui; InitEnumInfos(array);
        }

        private void InitEnumInfos(Array array)
        {
            _enums = new List<CustomTuple<string, string,int>>(array.Length);
            for (int i = 0; i < array.Length; i++)
            {
                CustomTuple<string, string, int> tuple = new();
                object value = array.GetValue(i);
                tuple.Item3 = Convert.ToInt32(value);
                tuple.Item1 = value.ToString();
                tuple.Item2 = ReflectionUtil.GetEnumAlias(value.GetType(), tuple.Item1);
                _enums.Add(tuple);
            }
        }

        public override void Dispose()
        {
            for (int i = 0; i < _enums.Count; i++)
            {
                _enums[i].Dispose();
            }
            _enums.Clear();
            _enums = null;
            _notifier = null;
            _propertyName = null;
            _ui = default;
        }

        /// <summary>
        /// 根据枚举值获取枚举值的别名
        /// </summary>
        protected string GetAlias(int value)
        {
            for (int i = 0; i < _enums.Count; i++)
            {
                if (_enums[i].Item3 == value){ return _enums[i].Item2; }
            }
            return null;
        } 

        /// <summary>
        /// 根据枚举值获取枚举值的字符串
        /// </summary>
        protected string GetName(int value)
        {
            for (int i = 0; i < _enums.Count; i++)
            {
                if (_enums[i].Item3 == value){ return _enums[i].Item1; }
            }
            return null;
        }

        /// <summary> 
        /// 根据枚举值的字符串形式获取枚举值或根据枚举值的别名获取枚举值
        /// </summary>
        protected int GetValue(string aliasOrEnumStr)
        {
            for (int i = 0; i < _enums.Count; i++)
            {
                if (_enums[i].Item1 == aliasOrEnumStr || _enums[i].Item2 == aliasOrEnumStr) 
                {
                    return _enums[i].Item3;
                }
            }
            return 0;
        }

        public sealed override void UpdateUI(bool propertyValue) { }

        public sealed override void UpdateUI(float propertyValue) { }

        public sealed override void UpdateUI(string propertyValue) { }

        public sealed override void UpdateUI(Sprite propertyValue) { }
    }
}
