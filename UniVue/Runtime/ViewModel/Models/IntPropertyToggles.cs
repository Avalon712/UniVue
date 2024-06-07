using UnityEngine.UI;

namespace UniVue.ViewModel.Models
{
    public sealed class IntPropertyToggles : IntPropertyUI<Toggle[]>
    {
        public IntPropertyToggles(Toggle[] ui, string propertyName, bool allowUIUpdateModel) : base(ui,propertyName, allowUIUpdateModel)
        {
        }

        public override void SetActive(bool active) { }

        public override void UpdateUI(int propertyValue)
        {
            for (int i = 0; i < _ui.Length; i++)
            {
                _ui[i].isOn = propertyValue > i;
            }
        }
    }
}
