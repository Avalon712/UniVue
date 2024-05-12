
using TMPro;
using UniVue.Model;
using UniVue.Utils;

namespace UniVue.ViewModel.Models
{
    public sealed class IntPropertyInput : IntPropertyUI<TMP_InputField>
    {
        public IntPropertyInput(TMP_InputField input, IModelNotifier notifier, string propertyName, bool allowUIUpdateModel) 
            : base(input, notifier, propertyName, allowUIUpdateModel)
        {
#if UNITY_EDITOR
            if (input.contentType != TMP_InputField.ContentType.IntegerNumber)
            {
                LogUtil.Warning("一个TMP_InputField的UI组件绑定了一个float类型的属性，但是该UI组件的内容输入类型不是ContentType.DecimalNumber，已经强制修改为了次内容类型！");
            }
#endif
            //限制只能输入小数类型或整数类型
            input.contentType = TMP_InputField.ContentType.IntegerNumber;

            if (_allowUIUpdateModel)
            {
                input.onEndEdit.AddListener(UpdateModel);//当输入框失去焦点时触发
            }
        }

        private void UpdateModel(string value)
        {
            if (!_needUpdate) { _needUpdate = true; return; }
            _needUpdate = false; //指示不用更新当前的UI

            int f;
            if (int.TryParse(value, out f)) 
            {
                _notifier.NotifyModelUpdate(_propertyName, f);
            }
        }
         
        public override void Dispose()
        {
            if (_allowUIUpdateModel) { _ui.onEndEdit.RemoveListener(UpdateModel); }
            base.Dispose();
        }

        public override void UpdateUI(int propertyValue)
        {
            if (!_needUpdate) { _needUpdate = true; return; }
            _needUpdate = false; //不要触发OnValueChanged事件

            _ui.text = propertyValue.ToString();
        }
    }
}
