using System.Collections.Generic;
using UniVue.Common;

namespace UniVue.ViewModel
{
    /// <summary>
    /// 多选
    /// </summary>
    public sealed class FlagsEnumToggleGroup : EnumUI
    {
        private EnumToggleInfo[] _uis;

        public FlagsEnumToggleGroup(EnumToggleInfo[] ui, string enumTypeFullName, string propertyName, bool allowUIUpdateModel)
            : base(enumTypeFullName, propertyName, allowUIUpdateModel)
        {
            _uis = ui;
            if (_allowUIUpdateModel)
            {
                for (int i = 0; i < ui.Length; i++)
                {
                    ui[i].toggle.onValueChanged.AddListener(UpdateModel);
                }
            }
            for (int i = 0; i < ui.Length; i++)
            {
                int value = GetIntValueByStringValue(ui[i].text.name);
                ThrowUtil.ThrowExceptionIfTrue(value == int.MinValue, "Enum绑定Toggle时,Toggle组件下的子物体TMP_Text的名称必须是它所对于的枚举值的字符串形式。");
                ui[i].value = GetAliasByValue(value);
            }
        }

        public override void SetActive(bool active)
        {
            for (int i = 0; i < _uis.Length; i++)
            {
                GameObjectUtil.SetActive(_uis[i].toggle.gameObject, active);
            }
        }

        private void UpdateModel(bool isOn)
        {
            int v = 0;
            for (int i = 0; i < _uis.Length; i++)
            {
                if (_uis[i].toggle.isOn)
                {
                    v |= GetIntValueByAlias(_uis[i].value);
                }
            }
            _value = v;
            Bundle?.UpdateModel(PropertyName, v);
        }

        public override void OnLanguageEnvironmentChanged()
        {
            for (int i = 0; i < _uis.Length; i++)
            {
                EnumToggleInfo ui = _uis[i];
                ui.value = GetNewAlias(ui.value);
            }
        }

        private void SetIsOn(string value, bool isOn)
        {
            for (int i = 0; i < _uis.Length; i++)
            {
                if (value.Equals(_uis[i].value))
                {
                    _uis[i].toggle.isOn = isOn;
                    return;
                }
            }
        }

        public override void UpdateUI(int propertyValue)
        {
            if (_value != propertyValue)
            {
                _value = propertyValue;
                int count = ValueCount;
                for (int i = 0; i < count; i++)
                {
                    string alias = GetAliasByIndex(i);
                    int value = GetIntValueByAlias(alias);
                    SetIsOn(alias, (propertyValue & value) == value);
                }
            }
        }

        public override IEnumerable<T> GetUI<T>()
        {
            if (_uis[0].toggle is T)
            {
                for (int i = 0; i < _uis.Length; i++)
                {
                    yield return _uis[i].toggle as T;
                }
            }
            if (_uis[0].text is T)
            {
                for (int i = 0; i < _uis.Length; i++)
                {
                    yield return _uis[i].text as T;
                }
            }
        }
    }

}
