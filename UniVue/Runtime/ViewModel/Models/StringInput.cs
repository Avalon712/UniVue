using TMPro;

namespace UniVue.ViewModel
{
    public sealed class StringInput : StringUI<TMP_InputField>
    {
        public StringInput(TMP_InputField ui, string propertyName, bool allowUIUpdateModel) : base(ui, propertyName, allowUIUpdateModel)
        {
            if (_allowUIUpdateModel) { ui.onEndEdit.AddListener(UpdateModel); }
        }

        private void UpdateModel(string value)
        {
            Vue.Updater.Publisher = this;
            _notifier?.NotifyModelUpdate(PropertyName, value);
        }

        public override void Unbind()
        {
            if (_allowUIUpdateModel) { _ui.onEndEdit.RemoveListener(UpdateModel); }
            base.Unbind();
        }

        public override void UpdateUI(string propertyValue)
        {
            if (!IsPublisher())
            {
                SetActive(!string.IsNullOrEmpty(propertyValue) || !Vue.Config.WhenValueIsNullThenHide);
                _ui.text = propertyValue;
            }
        }
    }
}
