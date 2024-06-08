using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniVue.Model
{
    public interface INotifiableProperty
    {
        void NotifyUIUpdate();
    }

    public interface IAtomProperty<T> : INotifiableProperty
    {
        T Value { get; set; }

        string PropertyName { get; }
    }


    public sealed class IntProperty : IAtomProperty<int>
    {
        private int _value;
        private string _propertyName;
        private IBindableModel _model;

        public IntProperty(IBindableModel model, string propertyName, int value)
        {
            _model = model;
            _value = value;
            _propertyName = propertyName;
        }

        public int Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    NotifyUIUpdate();
                }
            }
        }

        public string PropertyName => _propertyName;


        public void NotifyUIUpdate()
        {
            _model.NotifyUIUpdate(_propertyName, _value);
        }
    }

    public sealed class EnumProperty<E> : IAtomProperty<E> where E : Enum
    {
        private IBindableModel _model;
        private string _propertyName;
        private E _value;

        public EnumProperty(IBindableModel model, string propertyName, E value)
        {
            _model = model;
            _propertyName = propertyName;
            _value = value;
        }

        public string PropertyName => _propertyName;

        public E Value
        {
            get => _value;
            set
            {
                int v1 = Convert.ToInt32(_value);
                int v2 = Convert.ToInt32(value);
                if (v1 != v2)
                {
                    _value = value;
                    NotifyUIUpdate();
                }
            }
        }

        public void NotifyUIUpdate()
        {
            _model.NotifyUIUpdate(_propertyName, Convert.ToInt32(_value));
        }
    }

    public sealed class FloatProperty : IAtomProperty<float>
    {
        private IBindableModel _model;
        private string _propertyName;
        private float _value;

        public FloatProperty(IBindableModel model, string propertyName, float value)
        {
            _model = model;
            _propertyName = propertyName;
            _value = value;
        }

        public string PropertyName => _propertyName;

        public float Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    NotifyUIUpdate();
                }
            }
        }

        public void NotifyUIUpdate()
        {
            _model.NotifyUIUpdate(_propertyName, _value);
        }
    }

    public sealed class BoolProperty : IAtomProperty<bool>
    {
        private IBindableModel _model;
        private string _propertyName;
        private bool _value;

        public BoolProperty(IBindableModel model, string propertyName, bool value)
        {
            _model = model;
            _propertyName = propertyName;
            _value = value;
        }

        public string PropertyName => _propertyName;

        public bool Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    NotifyUIUpdate();
                }
            }
        }

        public void NotifyUIUpdate()
        {
            _model.NotifyUIUpdate(_propertyName, _value);
        }
    }

    public sealed class StringProperty : IAtomProperty<string>
    {
        private IBindableModel _model;
        private string _propertyName;
        private string _value;

        public StringProperty(IBindableModel model, string propertyName, string value)
        {
            _model = model;
            _propertyName = propertyName;
            _value = value;
        }

        public string PropertyName => _propertyName;

        public string Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    NotifyUIUpdate();
                }
            }
        }

        public void NotifyUIUpdate()
        {
            _model.NotifyUIUpdate(_propertyName, _value);
        }
    }

    public sealed class SpriteProperty : IAtomProperty<Sprite>
    {
        private IBindableModel _model;
        private string _propertyName;
        private Sprite _value;

        public SpriteProperty(IBindableModel model, string propertyName, Sprite value)
        {
            _model = model;
            _propertyName = propertyName;
            _value = value;
        }

        public string PropertyName => _propertyName;

        public Sprite Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    NotifyUIUpdate();
                }
            }
        }

        public void NotifyUIUpdate()
        {
            _model.NotifyUIUpdate(_propertyName, _value);
        }
    }

    public sealed class ListIntProperty : IAtomProperty<List<int>>
    {
        private IBindableModel _model;
        private string _propertyName;
        private List<int> _value;

        public ListIntProperty(IBindableModel model, string propertyName, List<int> value)
        {
            _model = model;
            _propertyName = propertyName;
            _value = value;
        }

        public string PropertyName => _propertyName;

        public List<int> Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    NotifyUIUpdate();
                }
            }
        }

        public void NotifyUIUpdate()
        {
            _model.NotifyUIUpdate(_propertyName, _value);
        }
    }

    public sealed class ListBoolProperty : IAtomProperty<List<bool>>
    {
        private IBindableModel _model;
        private string _propertyName;
        private List<bool> _value;

        public ListBoolProperty(IBindableModel model, string propertyName, List<bool> value)
        {
            _model = model;
            _propertyName = propertyName;
            _value = value;
        }

        public string PropertyName => _propertyName;

        public List<bool> Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    NotifyUIUpdate();
                }
            }
        }

        public void NotifyUIUpdate()
        {
            _model.NotifyUIUpdate(_propertyName, _value);
        }
    }

    public sealed class ListFloatProperty : IAtomProperty<List<float>>
    {
        private IBindableModel _model;
        private string _propertyName;
        private List<float> _value;

        public ListFloatProperty(IBindableModel model, string propertyName, List<float> value)
        {
            _model = model;
            _propertyName = propertyName;
            _value = value;
        }

        public string PropertyName => _propertyName;
        public List<float> Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    NotifyUIUpdate();
                }
            }
        }
        public void NotifyUIUpdate()
        {
            _model.NotifyUIUpdate(_propertyName, _value);
        }
    }

    public sealed class ListStringProperty : IAtomProperty<List<string>>
    {
        private IBindableModel _model;
        private string _propertyName;
        private List<string> _value;

        public ListStringProperty(IBindableModel model, string propertyName, List<string> value)
        {
            _model = model;
            _propertyName = propertyName;
            _value = value;
        }

        public string PropertyName => _propertyName;
        public List<string> Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    NotifyUIUpdate();
                }
            }
        }
        public void NotifyUIUpdate()
        {
            _model.NotifyUIUpdate(_propertyName, _value);
        }
    }

    public sealed class ListSpriteProperty : IAtomProperty<List<Sprite>>
    {
        private IBindableModel _model;
        private string _propertyName;
        private List<Sprite> _value;

        public ListSpriteProperty(IBindableModel model, string propertyName, List<Sprite> value)
        {
            _model = model;
            _propertyName = propertyName;
            _value = value;
        }

        public string PropertyName => _propertyName;

        public List<Sprite> Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    NotifyUIUpdate();
                }
            }
        }
        public void NotifyUIUpdate()
        {
            _model.NotifyUIUpdate(_propertyName, _value);
        }
    }

    public sealed class ListEnumProperty<T> : IAtomProperty<List<T>> where T : Enum
    {
        private IBindableModel _model;
        private string _propertyName;
        private List<T> _value;

        public ListEnumProperty(IBindableModel model, string propertyName, List<T> value)
        {
            _model = model;
            _propertyName = propertyName;
            _value = value;
        }

        public string PropertyName => _propertyName;

        public List<T> Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    NotifyUIUpdate();
                }
            }
        }

        public void NotifyUIUpdate()
        {
            _model.NotifyUIUpdate(_propertyName, _value);
        }
    }
}
