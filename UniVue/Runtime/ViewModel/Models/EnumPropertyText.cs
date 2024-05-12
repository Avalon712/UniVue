﻿using System;
using TMPro;
using UniVue.Model;

namespace UniVue.ViewModel.Models
{
    public sealed class EnumPropertyText : EnumPropertyUI<TMP_Text>
    {
        public EnumPropertyText(TMP_Text ui, Array array, IModelNotifier notifier, string propertyName)
            : base(ui, array, notifier, propertyName, false)
        {
        }


        public override void UpdateUI(int propertyValue)
        {
            _ui.text = GetAlias(propertyValue);
        }

    }
}
