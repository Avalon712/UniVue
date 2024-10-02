using System.Collections.Generic;
using TMPro;
using UniVue.Common;

namespace UniVue.ViewModel
{
    public sealed class IntInput : IntUI<TMP_InputField>
    {
        public IntInput(TMP_InputField input, string propertyName, bool allowUIUpdateModel) : base(input, propertyName, allowUIUpdateModel)
        {
            ThrowUtil.ThrowWarnIfTrue(input.contentType != TMP_InputField.ContentType.IntegerNumber, "一个TMP_InputField的UI组件绑定了一个float类型的属性，但是该UI组件的内容输入类型不是ContentType.IntegerNumber，已经强制修改为了此内容类型！");

            //限制只能输入小数类型或整数类型
            input.contentType = TMP_InputField.ContentType.IntegerNumber;

            if (_allowUIUpdateModel)
            {
                input.onEndEdit.AddListener(UpdateModel);//当输入框失去焦点时触发
            }
        }

        private void UpdateModel(string value)
        {
            if (int.TryParse(value, out int f))
            {
                _value = f;
                Bundle?.UpdateModel(PropertyName, f);
            }
        }

        public override void UpdateUI(int propertyValue)
        {
            if (_value != propertyValue)
            {
                _value = propertyValue;
                _ui.text = propertyValue.ToString();
            }
        }

        public override IEnumerable<T> GetUI<T>()
        {
            yield return _ui as T;
        }
    }
}
