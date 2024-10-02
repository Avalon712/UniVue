using TMPro;
using UnityEngine;
using UniVue.Common;

namespace UniVue.ViewModel
{
    public sealed class FloatInput : FloatUI<TMP_InputField>
    {
        public FloatInput(TMP_InputField ui, string propertyName, bool allowUIUpdateModel) : base(ui, propertyName, allowUIUpdateModel)
        {
            ThrowUtil.ThrowWarnIfTrue(ui.contentType != TMP_InputField.ContentType.DecimalNumber, "一个TMP_InputField的UI组件绑定了一个float类型的属性，但是该UI组件的内容输入类型不是ContentType.DecimalNumber，已经强制修改为了次内容类型！");

            //限制只能输入小数类型或整数类型
            ui.contentType = TMP_InputField.ContentType.DecimalNumber;

            //当输入框失去焦点时触发
            if (_allowUIUpdateModel) { ui.onEndEdit.AddListener(UpdateModel); }

        }

        private void UpdateModel(string value)
        {
            if (float.TryParse(value, out float f))
            {
                _value = f;
                Bundle?.UpdateModel(PropertyName, f);
            }
        }

        public override void UpdateUI(float propertyValue)
        {
            if (!Mathf.Approximately(_value, propertyValue))
            {
                _value = propertyValue;
                _ui.text = propertyValue.ToString("F" + Vue.Config.FloatKeepBit);
            }
        }

    }
}
