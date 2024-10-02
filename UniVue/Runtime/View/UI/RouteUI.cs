using UnityEngine;
using UniVue.Common;

namespace UniVue.View
{
    public abstract class RouteUI
    {
        public UIType UIType { get; }

        public RouteEvent RouteEvent { get; }

        /// <summary>
        /// 当前路由的目的视图
        /// </summary>
        /// <remarks>对于Return路由事件，此值为null</remarks>
        public string RouteTo { get; }

        /// <summary>
        /// 当前路由UI所属视图
        /// </summary>
        public string ViewName { get; }

        protected RouteUI(UIType uIType, RouteEvent routeEvent, string viewName, string operationView)
        {
            UIType = uIType;
            RouteEvent = routeEvent;
            RouteTo = operationView;
            ViewName = viewName;
        }

        /// <summary>
        /// 执行路由动作
        /// </summary>
        public void Route()
        {
            Vue.Router.controller = this;
            switch (RouteEvent)
            {
                case RouteEvent.Open:
                    Vue.Router.Open(RouteTo);
                    break;
                case RouteEvent.Close:
                    Vue.Router.Close(RouteTo);
                    break;
                case RouteEvent.Skip:
                    Vue.Router.Skip(ViewName, RouteTo);
                    break;
                case RouteEvent.Return:
                    Vue.Router.Return();
                    break;
            }
            Vue.Router.controller = null;
        }

        public abstract T GetUI<T>() where T : Component;
    }
}
