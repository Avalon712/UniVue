using System.Collections.Generic;
using UniVue.Common;

namespace UniVue.ViewModel
{
    /// <summary>
    /// Toggle绑定枚举时获取枚举值的方式: 
    /// 单选效果
    /// </summary>
    public sealed class EnumToggleGroup : EnumUI
    {
        private EnumToggleInfo[] _uis;

        public EnumToggleGroup(EnumToggleInfo[] ui, string enumTypeFullName, string propertyName, bool allowUIUpdateModel)
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
            if (isOn)
            {
                EnumToggleInfo toggleInfo = GetActiveToggle();
                _value = GetIntValueByAlias(toggleInfo.value);
                Bundle?.UpdateModel(PropertyName, _value);
            }
        }

        public override void UpdateUI(int propertyValue)
        {
            if (_value != propertyValue)
            {
                _value = propertyValue;
                string v = GetAliasByValue(propertyValue);
                SetIsOn(v, true);
            }
        }

        public override void OnLanguageEnvironmentChanged()
        {
            for (int i = 0; i < _uis.Length; i++)
            {
                EnumToggleInfo ui = _uis[i];
                ui.value = GetNewAlias(ui.value);
            }
        }

        private void SetIsOn(string alias, bool isOn)
        {
            for (int i = 0; i < _uis.Length; i++)
            {
                if (alias.Equals(_uis[i].value))
                {
                    _uis[i].toggle.isOn = isOn;
                    break;
                }
            }
        }

        private EnumToggleInfo GetActiveToggle()
        {
            for (int i = 0; i < _uis.Length; i++)
            {
                if (_uis[i].toggle.isOn) { return _uis[i]; }
            }
            return default;
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
