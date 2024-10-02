using UnityEngine.UI;
using UniVue.Common;

namespace UniVue.View
{
    public sealed class ButtonRoute : RouteUI
    {
        private readonly Button _btn;

        public Button button => _btn;

        public ButtonRoute(Button btn, UIType uIType, RouteEvent routeEvent, string viewName, string operationView) : base(uIType, routeEvent, viewName, operationView)
        {
            _btn = btn;
            btn.onClick.AddListener(Route);
        }

        public override T GetUI<T>()
        {
            return _btn as T;
        }
    }
}
