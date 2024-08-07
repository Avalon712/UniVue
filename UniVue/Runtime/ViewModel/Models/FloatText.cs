﻿using TMPro;

namespace UniVue.ViewModel
{
    public sealed class FloatText : FloatUI<TMP_Text>
    {
        public FloatText(TMP_Text ui, string propertyName) : base(ui, propertyName, false)
        {
        }

        public override void UpdateUI(float propertyValue)
        {
            _ui.text = propertyValue.ToString("F" + Vue.Config.FloatKeepBit);
        }
    }
}
