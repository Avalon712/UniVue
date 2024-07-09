using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UniVue.Utils;
using UniVue.View;

namespace UniVue.Rule
{
    /// <summary>
    /// <para>视图导航事件(如:打开视图、关闭视图、返回上一个视图、跳转到另一个视图)的名称匹配规则如下: </para>
    /// <para>"事件+视图名称+Btn|btn|Toggle|toggle"  注：视图事件有:Open、Close、Return、Skip。 此外注意命名的Btn或btn是不可省略的</para>
    /// </summary>
    public sealed class RouteFilter : IRuleFilter
    {
        public string ViewName { get; private set; }

        public RouteFilter(string viewName)
        {
            ViewName = viewName;
        }

        public bool Check(ref (Component, UIType) component, List<object> results)
        {
            if (component.Item2 == UIType.Button || component.Item2 == UIType.Toggle || component.Item2 == UIType.ToggleGroup)
            {
                string uiName = component.Item1.name;
                if (DoCheck(uiName, out RouteEvent routeEvent, out string opViewName))
                {
                    results.Add(new RouteFilterResult(component.Item2, component.Item1, routeEvent, opViewName));
                    return true;
                }
            }
            return false;
        }

        public void OnComplete(List<object> results)
        {
            string currentViewName = ViewName;
            for (int i = 0; i < results.Count; i++)
            {
                RouteFilterResult result = (RouteFilterResult)results[i];
                string opViewName = result.OpViewName;
                switch (result.Event)
                {
                    case RouteEvent.Open:
                        result.AddRouteListener(() => Vue.Router.Open(opViewName));
                        break;
                    case RouteEvent.Close:
                        result.AddRouteListener(() => Vue.Router.Close(opViewName));
                        break;
                    case RouteEvent.Skip:
                        result.AddRouteListener(() => Vue.Router.Skip(currentViewName, opViewName));
                        break;
                    case RouteEvent.Return:
                        result.AddRouteListener(() => Vue.Router.Return());
                        break;
                }
            }
        }

        private bool DoCheck(string uiName, out RouteEvent routeEvent, out string opViewName)
        {
            opViewName = null;
            routeEvent = RouteEvent.Open;
            int index = (Vue.Config.Format & NamingFormat.UI_Suffix) == NamingFormat.UI_Suffix ? 1 : 2;

            for (int i = 0; i < 3; i++)
            {
                Match match = Regex.Match(uiName, GetRule(i));
                if (match.Success)
                {
                    opViewName = match.Groups[index].Value;
                    routeEvent = (RouteEvent)i;
                    return true;
                }
            }
            return false;
        }

        #region 规则定义
        private string GetRule(int routeNumber)
        {
            RouteEvent routeEvent = (RouteEvent)routeNumber;
            switch (Vue.Config.Format)
            {
                case NamingFormat.CamelCase | NamingFormat.UI_Suffix:
                    {
                        switch (routeEvent)
                        {
                            case RouteEvent.Open:
                                return @"Open(\w{1,})(Btn|Button|Toggle)";
                            case RouteEvent.Close:
                                return @"Close(\w{1,})(Btn|Button|Toggle)";
                            case RouteEvent.Skip:
                                return @"Skip(\w{1,})(Btn|Button|Toggle)";
                            case RouteEvent.Return:
                                return "Return(Btn|Button|Toggle)";
                        }
                    }
                    break;

                case NamingFormat.CamelCase | NamingFormat.UI_Prefix:
                    {
                        switch (routeEvent)
                        {
                            case RouteEvent.Open:
                                return @"(Btn|Button|Toggle)Open(\w{1,})";
                            case RouteEvent.Close:
                                return @"(Btn|Button|Toggle)Close(\w{1,})";
                            case RouteEvent.Skip:
                                return @"(Btn|Button|Toggle)Skip(\w{1,})";
                            case RouteEvent.Return:
                                return "(Btn|Button|Toggle)Return";
                        }
                    }
                    break;

                case NamingFormat.UnderlineLower | NamingFormat.UI_Suffix:
                    {
                        switch (routeEvent)
                        {
                            case RouteEvent.Open:
                                return @"open_(\w{1,})_(btn|button|toggle)";
                            case RouteEvent.Close:
                                return @"close_(\w{1,})_(btn|button|toggle)";
                            case RouteEvent.Skip:
                                return @"skip_(\w{1,})_(btn|button|toggle)";
                            case RouteEvent.Return:
                                return "return_(btn|button|toggle)";
                        }
                    }
                    break;

                case NamingFormat.UnderlineLower | NamingFormat.UI_Prefix:
                    {
                        switch (routeEvent)
                        {
                            case RouteEvent.Open:
                                return @"(btn|button|toggle)_open_(\w{1,})";
                            case RouteEvent.Close:
                                return @"(btn|button|toggle)_close_(\w{1,})";
                            case RouteEvent.Skip:
                                return @"(btn|button|toggle)_skip_(\w{1,})";
                            case RouteEvent.Return:
                                return "(btn|button|toggle)_return";
                        }
                    }
                    break;

                case NamingFormat.SpaceLower | NamingFormat.UI_Suffix:
                    {
                        switch (routeEvent)
                        {
                            case RouteEvent.Open:
                                return @"open (\w{1,}) (btn|button|toggle)";
                            case RouteEvent.Close:
                                return @"close (\w{1,}) (btn|button|toggle)";
                            case RouteEvent.Skip:
                                return @"skip (\w{1,}) (btn|button|toggle)";
                            case RouteEvent.Return:
                                return "return (btn|button|toggle)";
                        }
                    }
                    break;

                case NamingFormat.SpaceLower | NamingFormat.UI_Prefix:
                    {
                        switch (routeEvent)
                        {
                            case RouteEvent.Open:
                                return @"(btn|button|toggle) open (\w{1,})";
                            case RouteEvent.Close:
                                return @"(btn|button|toggle) close (\w{1,})";
                            case RouteEvent.Skip:
                                return @"(btn|button|toggle) skip (\w{1,})";
                            case RouteEvent.Return:
                                return "(btn|button|toggle) return";
                        }
                    }
                    break;

                case NamingFormat.SpaceUpper | NamingFormat.UI_Suffix:
                    {
                        switch (routeEvent)
                        {
                            case RouteEvent.Open:
                                return @"Open (\w{1,}) (Btn|Button|Toggle)";
                            case RouteEvent.Close:
                                return @"Close (\w{1,}) (Btn|Button|Toggle)";
                            case RouteEvent.Skip:
                                return @"Skip (\w{1,}) (Btn|Button|Toggle)";
                            case RouteEvent.Return:
                                return "Return (Btn|Button|Toggle)";
                        }
                    }
                    break;

                case NamingFormat.SpaceUpper | NamingFormat.UI_Prefix:
                    {
                        switch (routeEvent)
                        {
                            case RouteEvent.Open:
                                return @"(Btn|Button|Toggle) Open (\w{1,})";
                            case RouteEvent.Close:
                                return @"(Btn|Button|Toggle) Close (\w{1,})";
                            case RouteEvent.Skip:
                                return @"(Btn|Button|Toggle) Skip (\w{1,})";
                            case RouteEvent.Return:
                                return "(Btn|Button|Toggle) Return";
                        }
                    }
                    break;

                case NamingFormat.UnderlineUpper | NamingFormat.UI_Suffix:
                    {
                        switch (routeEvent)
                        {
                            case RouteEvent.Open:
                                return @"Open_(\w{1,})_(Btn|Button|Toggle)";
                            case RouteEvent.Close:
                                return @"Close_(\w{1,})_(Btn|Button|Toggle)";
                            case RouteEvent.Skip:
                                return @"Skip_(\w{1,})_(Btn|Button|Toggle)";
                            case RouteEvent.Return:
                                return "Return_(Btn|Button|Toggle)";
                        }
                    }
                    break;

                case NamingFormat.UnderlineUpper | NamingFormat.UI_Prefix:
                    {
                        switch (routeEvent)
                        {
                            case RouteEvent.Open:
                                return @"(Btn|Button|Toggle)_Open_(\w{1,})";
                            case RouteEvent.Close:
                                return @"(Btn|Button|Toggle)_Close_(\w{1,})";
                            case RouteEvent.Skip:
                                return @"(Btn|Button|Toggle)_Skip_(\w{1,})";
                            case RouteEvent.Return:
                                return "(Btn|Button|Toggle)_Return";
                        }
                    }
                    break;
            }

            throw new NotSupportedException("非法的命名格式");
        }

        #endregion

    }

    internal struct RouteFilterResult
    {
        public UIType UIType { get; private set; }

        public RouteEvent Event { get; private set; }

        public Component Component { get; private set; }

        public string OpViewName { get; private set; }


        public RouteFilterResult(UIType uIType, Component component, RouteEvent @event, string opViewName)
        {
            UIType = uIType;
            Component = component;
            OpViewName = opViewName;
            Event = @event;
        }

        public void AddRouteListener(UnityAction action)
        {
            if (UIType == UIType.Toggle || UIType == UIType.ToggleGroup)
            {
                (Component as Toggle).onValueChanged.AddListener(isOn => { if (isOn) action.Invoke(); });
            }
            else if (UIType == UIType.Button)
            {
                (Component as Button).onClick.AddListener(action);
            }
        }
    }
}
