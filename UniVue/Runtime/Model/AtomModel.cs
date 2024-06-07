using System;
using System.Collections.Generic;
using UnityEngine;
using UniVue.Utils;
using UniVue.ViewModel.Models;

namespace UniVue.Model
{
    /// <summary>
    /// UniVue可支持的最小模型单元（这个类不会进行反射操作，没有装箱消耗）
    /// </summary>
    /// <remarks>
    /// 使用此类可以实现更新细腻度的数据绑定，同时实现虚拟模型的概念。
    /// 即通过对int、float、bool、string、枚举、Sprite、List&lt;int&gt;、
    /// List&lt;bool&gt;、List&lt;float&gt;、List&lt;string&gt;、List&lt;Sprite&gt;
    /// 这种最小支持绑定单元指定一个模型名和属性名，使得成为一个能够进行数据绑定的虚拟模型。
    /// </remarks>
    public sealed class AtomModel<T> : IBindableModel
    {
        private string _propertyName;
        private string _modelName;
        private IAtomWrapper<T> _wrapper;

        /// <summary>
        /// 绑定的值
        /// </summary>
        public T Value
        {
            get => _wrapper.GetValue();
            set => _wrapper.SetValue(this, _propertyName, value);
        }

        /// <summary>
        /// 最小可绑定原子数据模型
        /// </summary>
        /// <param name="modelName">模型名称，如果为null则将默认为当前类的类型名称</param>
        /// <param name="propertyName">属性名称</param>
        /// <param name="value">绑定的模型值，必须是可绑定的类型</param>
        public AtomModel(string modelName, string propertyName, T value)
        {
            BuildBinder(CheckSupported(value), value);
            _propertyName = propertyName;
            _modelName = modelName == null ? "AtomModel" : modelName;
        }

        public void Bind(string viewName, bool allowUIUpdateModel)
        {
            Vue.Router.GetView(viewName).BindModel(this, allowUIUpdateModel, _modelName);
        }

        void IUINotifier.NotifyAll()
        {
            _wrapper.SetValue(this, _propertyName, Value, true);
        }

        void IModelUpdater.UpdateModel(string propertyName, bool propertyValue)
        {
            _wrapper.UpdateModel(this, _propertyName, propertyValue);
        }

        void IModelUpdater.UpdateModel(string propertyName, string propertyValue)
        {
            _wrapper.UpdateModel(this, _propertyName, propertyValue);
        }

        void IModelUpdater.UpdateModel(string propertyName, float propertyValue)
        {
            _wrapper.UpdateModel(this, _propertyName, propertyValue);
        }

        void IModelUpdater.UpdateModel(string propertyName, int propertyValue)
        {
            _wrapper.UpdateModel(this, _propertyName, propertyValue);
        }


        private BindableType CheckSupported(T value)
        {
            BindableType bindableType = ReflectionUtil.GetBindableType(value.GetType());
            if (bindableType == BindableType.None || bindableType== BindableType.ListEnum)
                throw new NotSupportedException("AtomModel只能支持int、float、bool、string、枚举、Sprite、List<int>、List<boo>、List<float>、List<string>、List<Sprite>");
            return bindableType;
        }

        private void BuildBinder(BindableType bindableType, T value)
        {
            switch (bindableType)
            {
                case BindableType.Enum:
                    _wrapper = new EnumWrapper<T>(value);
                    break;
                case BindableType.Bool:
                    _wrapper = new BoolWrapper(Convert.ToBoolean(value)) as IAtomWrapper<T>;
                    break;
                case BindableType.Float:
                    _wrapper = new FloatWrapper(Convert.ToSingle(value)) as IAtomWrapper<T>;
                    break;
                case BindableType.Int:
                    _wrapper = new IntWrapper(Convert.ToInt32(value)) as IAtomWrapper<T>; 
                    break;
                case BindableType.String:
                    _wrapper = new StringWrapper(Convert.ToString(value)) as IAtomWrapper<T>;
                    break;
                case BindableType.Sprite:
                    _wrapper = new SpriteWrapper(((object)value) as Sprite) as IAtomWrapper<T>;
                    break;
                case BindableType.ListBool:
                    _wrapper = new ListBoolWrapper(((object)value) as List<bool>) as IAtomWrapper<T>;
                    break;
                case BindableType.ListFloat:
                    _wrapper = new ListFloatWrapper(((object)value) as List<float>) as IAtomWrapper<T>;
                    break;
                case BindableType.ListInt:
                    _wrapper = new ListIntWrapper(((object)value) as List<int>) as IAtomWrapper<T>;
                    break;
                case BindableType.ListString:
                    _wrapper = new ListStringWrapper(((object)value) as List<string>) as IAtomWrapper<T>;
                    break;
                case BindableType.ListSprite:
                    _wrapper = new ListSpriteWrapper(((object)value) as List<Sprite>) as IAtomWrapper<T>;
                    break;
            }
        }
    }

    //减少反射和装箱
    internal interface IAtomWrapper<T>
    {
        T GetValue();

        public void UpdateModel(IBindableModel model, string propertyName, bool propertyValue) { }

        public void UpdateModel(IBindableModel model, string propertyName, string propertyValue) { }

        public void UpdateModel(IBindableModel model, string propertyName, float propertyValue) { }

        public void UpdateModel(IBindableModel model, string propertyName, int propertyValue) { }

        void SetValue(IBindableModel model, string propertyName, T propertyValue, bool forceUpdate=false);
    }


    internal sealed class IntWrapper : IAtomWrapper<int>
    {
        private int _value;

        public IntWrapper(int value) { _value = value; }

        public int GetValue() => _value;

        public void UpdateModel(IBindableModel model, string propertyName, int propertyValue) 
        {
            SetValue(model, propertyName, propertyValue, false);
        }

        public void SetValue(IBindableModel model, string propertyName, int propertyValue, bool forceUpdate)
        {
            if(_value != propertyValue || forceUpdate)
            {
                _value = propertyValue;
                model.NotifyUIUpdate(propertyName, propertyValue);
            }
        }

    }

    internal sealed class EnumWrapper<E> : IAtomWrapper<E>
    {
        private E _value;

        public EnumWrapper(E value) { _value = value; }

        public E GetValue() => _value;

        public void UpdateModel(IBindableModel model, string propertyName, int propertyValue)
        {
            SetValue(model, propertyName,(E)Enum.Parse(typeof(E),_value.ToString()), false);
        }

        public void SetValue(IBindableModel model, string propertyName, E propertyValue, bool forceUpdate)
        {
            int v1 = Convert.ToInt32(_value);
            int v2 = Convert.ToInt32(propertyValue);
            if (v1 != v2 || forceUpdate)
            {
                _value = propertyValue;
                model.NotifyUIUpdate(propertyName, v2);
            }
        }
    }

    internal sealed class FloatWrapper : IAtomWrapper<float>
    {
        private float _value;

        public FloatWrapper(float value)
        {
            _value = value;
        }

        public float GetValue() => _value;

        public void UpdateModel(IBindableModel model, string propertyName, float propertyValue)
        {
            SetValue(model, propertyName, propertyValue, false);
        }

        public void SetValue(IBindableModel model, string propertyName, float propertyValue, bool forceUpdate)
        {
            if (_value != propertyValue || forceUpdate)
            {
                _value = propertyValue;
                model.NotifyUIUpdate(propertyName, propertyValue);
            }
        }
    }

    internal sealed class BoolWrapper : IAtomWrapper<bool>
    {
        private bool _value;

        public BoolWrapper(bool value)
        {
            _value = value;
        }

        public void UpdateModel(IBindableModel model, string propertyName, bool propertyValue)
        {
            SetValue(model, propertyName, propertyValue, false);
        }

        public bool GetValue() => _value;

        public void SetValue(IBindableModel model, string propertyName, bool propertyValue, bool forceUpdate)
        {
            if (_value != propertyValue || forceUpdate)
            {
                _value = propertyValue;
                model.NotifyUIUpdate(propertyName, propertyValue);
            }
        }
    }

    internal sealed class StringWrapper : IAtomWrapper<string>
    {
        private string _value;

        public StringWrapper(string value)
        {
            _value = value;
        }
        public void UpdateModel(IBindableModel model, string propertyName, string propertyValue)
        {
            SetValue(model, propertyName, propertyValue, false);
        }

        public string GetValue() => _value;

        public void SetValue(IBindableModel model, string propertyName, string propertyValue, bool forceUpdate)
        {
            if (_value != propertyValue || forceUpdate)
            {
                _value = propertyValue;
                model.NotifyUIUpdate(propertyName, propertyValue);
            }
        }
    }

    internal sealed class SpriteWrapper : IAtomWrapper<Sprite>
    {
        private Sprite _value;

        public SpriteWrapper(Sprite value)
        {
            _value = value;
        }

        public Sprite GetValue() => _value;

        public void SetValue(IBindableModel model, string propertyName, Sprite propertyValue, bool forceUpdate)
        {
            if (_value != propertyValue || forceUpdate)
            {
                _value = propertyValue;
                model.NotifyUIUpdate(propertyName, propertyValue);
            }
        }
    }

    internal sealed class ListIntWrapper : IAtomWrapper<List<int>>
    {
        private List<int> _value;

        public ListIntWrapper(List<int> value)
        {
            _value = value;
        }

        public List<int> GetValue() => _value;

        public void SetValue(IBindableModel model, string propertyName, List<int> propertyValue, bool forceUpdate)
        {
            if (_value != propertyValue || forceUpdate)
            {
                _value = propertyValue;
                model.NotifyUIUpdate(propertyName, propertyValue);
            }
        }
    }

    internal sealed class ListBoolWrapper : IAtomWrapper<List<bool>>
    {
        private List<bool> _value;

        public ListBoolWrapper(List<bool> value)
        {
            _value = value;
        }

        public List<bool> GetValue() => _value;

        public void SetValue(IBindableModel model, string propertyName, List<bool> propertyValue, bool forceUpdate)
        {
            if (_value != propertyValue || forceUpdate)
            {
                _value = propertyValue;
                model.NotifyUIUpdate(propertyName, propertyValue);
            }
        }
    }

    internal sealed class ListFloatWrapper : IAtomWrapper<List<float>>
    {
        private List<float> _value;

        public ListFloatWrapper(List<float> value)
        {
            _value = value;
        }

        public List<float> GetValue() => _value;

        public void SetValue(IBindableModel model, string propertyName, List<float> propertyValue, bool forceUpdate)
        {
            if (_value != propertyValue || forceUpdate)
            {
                _value = propertyValue;
                model.NotifyUIUpdate(propertyName, propertyValue);
            }
        }
    }

    internal sealed class ListStringWrapper : IAtomWrapper<List<string>>
    {
        private List<string> _value;

        public ListStringWrapper(List<string> value)
        {
            _value = value;
        }

        public List<string> GetValue() => _value;

        public void SetValue(IBindableModel model, string propertyName, List<string> propertyValue, bool forceUpdate)
        {
            if (_value != propertyValue || forceUpdate)
            {
                _value = propertyValue;
                model.NotifyUIUpdate(propertyName, propertyValue);
            }
        }
    }

    internal sealed class ListSpriteWrapper : IAtomWrapper<List<Sprite>>
    {
        private List<Sprite> _value;

        public ListSpriteWrapper(List<Sprite> value)
        {
            _value = value;
        }

        public List<Sprite> GetValue() => _value;

        public void SetValue(IBindableModel model, string propertyName, List<Sprite> propertyValue, bool forceUpdate)
        {
            if (_value != propertyValue || forceUpdate)
            {
                _value = propertyValue;
                model.NotifyUIUpdate(propertyName, propertyValue);
            }
        }
    }
}
