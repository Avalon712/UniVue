using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UniVue.Common;
using UniVue.View;

namespace UniVue.Rule
{
    /// <summary>
    /// <para>视图导航事件(如:打开视图、关闭视图、返回上一个视图、跳转到另一个视图)的名称匹配规则如下: </para>
    /// <para>"事件+视图名称+Btn|btn|Toggle|toggle"  注：视图事件有:Open、Close、Return、Skip。 此外注意命名的Btn或btn是不可省略的</para>
    /// </summary>
    public sealed class RouteRule : IRule
    {
        private static readonly Regex ROUTE = new Regex("(Open|Close|Skip)_(.+)_(Btn|Button|Toggle)", RegexOptions.Compiled);

        public string ViewName { get; internal set; }

        public RouteRule(string viewName)
        {
            ViewName = viewName;
        }

        public bool Check(string rule, UIType uiType, Component ui)
        {
            if (uiType == UIType.Button || uiType == UIType.Toggle || uiType == UIType.ToggleGroup)
            {
                if (DoCheck(rule, out RouteEvent routeEvent, out string operateViewName))
                {
                    if (uiType == UIType.Button)
                        Vue.Router.AddRouteUI(new ButtonRoute(ui as Button, uiType, routeEvent, ViewName, operateViewName));
                    else
                        Vue.Router.AddRouteUI(new ToggleRoute(ui as Toggle, uiType, routeEvent, ViewName, operateViewName));
                    return true;
                }
            }
            return false;
        }

        public void OnComplete() { }


        private bool DoCheck(string uiName, out RouteEvent routeEvent, out string opViewName)
        {
            opViewName = null;
            if (MatchReturn(uiName))
            {
                routeEvent = RouteEvent.Return;
                return true;
            }
            else
            {
                routeEvent = RouteEvent.Open;
                Match match = ROUTE.Match(uiName);
                if (match.Success)
                {
                    routeEvent = GetRouteEvent(match.Groups[1].Value);
                    if (routeEvent != RouteEvent.Return)
                        opViewName = match.Groups[2].Value;
                    return true;
                }
            }
            return false;
        }

        private static bool MatchReturn(string uiName)
        {
            return uiName == "Return_Btn" || uiName == "Return_Button" || uiName == "Return_Toggle";
        }

        private static RouteEvent GetRouteEvent(string str)
        {
            switch (str)
            {
                case nameof(RouteEvent.Open): return RouteEvent.Open;
                case nameof(RouteEvent.Close): return RouteEvent.Close;
                case nameof(RouteEvent.Return): return RouteEvent.Return;
                case nameof(RouteEvent.Skip): return RouteEvent.Skip;
            }
            return RouteEvent.Open;
        }
    }
}
