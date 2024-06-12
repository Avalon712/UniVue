using TMPro;
using UniVue.Utils;

namespace UniVue.ViewModel.Models
{
    public sealed class FloatPropertyInput : FloatPropertyUI<TMP_InputField>
    {
        public FloatPropertyInput(TMP_InputField ui, string propertyName, bool allowUIUpdateModel) : base(ui, propertyName, allowUIUpdateModel)
        {
#if UNITY_EDITOR
            if (ui.contentType != TMP_InputField.ContentType.DecimalNumber)
            {
                LogUtil.Warning("一个TMP_InputField的UI组件绑定了一个float类型的属性，但是该UI组件的内容输入类型不是ContentType.DecimalNumber，已经强制修改为了次内容类型！");
            }
#endif
            //限制只能输入小数类型或整数类型
            ui.contentType = TMP_InputField.ContentType.DecimalNumber;

            //当输入框失去焦点时触发
            if (_allowUIUpdateModel) { ui.onEndEdit.AddListener(UpdateModel); }

        }

        private void UpdateModel(string value)
        {
            if (float.TryParse(value, out float f))
            {
                Vue.Updater.Publisher = this;
                _notifier?.NotifyModelUpdate(_propertyName, f);
            }
        }


        public override void Unbind()
        {
            if (_allowUIUpdateModel) { _ui.onEndEdit.RemoveListener(UpdateModel); }
            base.Unbind();
        }

        public override void UpdateUI(float propertyValue)
        {
            if (!IsPublisher())
                _ui.text = propertyValue.ToString("F" + Vue.Config.FloatKeepBit);
        }

    }
}
