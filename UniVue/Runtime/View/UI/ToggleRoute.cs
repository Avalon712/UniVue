using UnityEngine.UI;
using UniVue.Common;

namespace UniVue.View
{
    public sealed class ToggleRoute : RouteUI
    {
        private readonly Toggle _toggle;

        public Toggle toggle => _toggle;

        /// <summary>
        /// 只有当Toggle.isOn=true时才触发路由事件
        /// </summary>
        /// <remarks>默认为true</remarks>
        public bool WhenIsTrueRouted { get; set; } = true;

        public ToggleRoute(Toggle toggle, UIType uIType, RouteEvent routeEvent, string viewName, string operationView) : base(uIType, routeEvent, viewName, operationView)
        {
            _toggle = toggle;
            _toggle.onValueChanged.AddListener(Route);
        }

        public override T GetUI<T>()
        {
            return _toggle as T;
        }

        private void Route(bool isOn)
        {
            if (isOn || !WhenIsTrueRouted)
            {
                Route();
            }
        }
    }
}
