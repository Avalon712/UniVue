using UnityEngine.UI;
using UniVue.Model;

namespace UniVue.ViewModel.Models
{
    public sealed class IntPropertyToggles : IntPropertyUI<Toggle[]>
    {
        public IntPropertyToggles(Toggle[] ui, IModelNotifier notifier, string propertyName, bool allowUIUpdateModel) : base(ui, notifier, propertyName, allowUIUpdateModel)
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
