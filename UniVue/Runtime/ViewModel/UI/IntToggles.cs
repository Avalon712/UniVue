using System.Collections.Generic;
using UnityEngine.UI;

namespace UniVue.ViewModel
{
    public sealed class IntToggles : IntUI<Toggle[]>
    {
        public IntToggles(Toggle[] ui, string propertyName) : base(ui, propertyName, false)
        {
        }

        public override void SetActive(bool active) { }

        public override void UpdateUI(int propertyValue)
        {
            if (_value != propertyValue)
            {
                _value = propertyValue;
                for (int i = 0; i < _ui.Length; i++)
                {
                    _ui[i].isOn = propertyValue > i;
                }
            }
        }

        public override IEnumerable<T> GetUI<T>()
        {
            if (_ui[0] is T)
            {
                for (int i = 0; i < _ui.Length; i++)
                {
                    yield return _ui[i] as T;
                }
            }
        }
    }
}
