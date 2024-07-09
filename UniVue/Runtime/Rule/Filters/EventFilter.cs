using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UniVue.Evt;
using UniVue.Utils;

namespace UniVue.Rule
{
    /// <summary>
    /// <para>自定义事件匹配规则：</para>
    /// <para>事件触发器：Evt + EvtName + UI组件名称（可简写）</para>
    /// <para>事件参数：Arg + EvtName[ArgName] + UI组件名称（可简写）</para>
    /// <para>如果一个UI即是事件参数又是事件触发器，则命名规则为: Evt&amp;Arg + EvtName[ArgName] + UI组件名称（可简写）</para>
    /// <para>事件触发器命名举例：Evt_Buy_Btn ：购买事件，事件名称为"Buy"，触发器为Button</para>
    /// <para>事件参数命名：Arg_Buy[Num]_Slider：购买事件的参数，参数名称为Num，UI为Slider</para>
    /// <para>如果想要参数对象映射到一个对象，则可以按以下规则进行：</para>
    /// <para>
    /// 登录事件举例：Evt_Login_Btn。Arg_Login[Name]_Input、 Arg_Login[Password]_Input，如果想要映射
    /// 为一个User对象，则这个User对象必须有属性Name、Password，即参数名称于事件名称一致，这样当Login
    /// 事件触发是，通过"GetCurrentEventArg&lt;User&gt;()"函数可用获得一个User对象。
    /// </para>
    /// </summary>
    public sealed class EventFilter : IRuleFilter
    {
        public string ViewName { get; private set; }

        public EventFilter(string viewName)
        {
            ViewName = viewName;
        }

        public bool Check(ref (Component, UIType) component, List<object> results)
        {
            if (component.Item2 == UIType.None) return false;
            int count = results.Count;
            string uiName = component.Item1.name;

            Match match = Regex.Match(uiName, GetEventAndArgRule(out int eventNameIdx, out int argNameIdx));
            if (match.Success)
            {
                uiName = uiName.Replace(match.Value, string.Empty);
                results.Add(new EventFilterResult(UIEventFlag.ArgAndEvent, match.Groups[eventNameIdx].Value, match.Groups[argNameIdx].Value, component.Item1, component.Item2));
            }

            match = Regex.Match(uiName, GetArgRule(out eventNameIdx, out argNameIdx));
            if (match.Success)
            {
                uiName = uiName.Replace(match.Value, string.Empty);
                results.Add(new EventFilterResult(UIEventFlag.OnlyArg, match.Groups[eventNameIdx].Value, match.Groups[argNameIdx].Value, component.Item1, component.Item2));
            }

            match = Regex.Match(uiName, GetEventRule(out eventNameIdx));
            if (match.Success)
            {
                results.Add(new EventFilterResult(UIEventFlag.OnlyEvent, match.Groups[eventNameIdx].Value, null, component.Item1, component.Item2));
            }

            return results.Count - count >= 1;
        }

        public void OnComplete(List<object> results)
        {
            if (results.Count > 0)
                UIEventBuilder.Build(ViewName, results);
        }


        #region 规则定义

        private static string GetEventAndArgRule(out int eventNameIdx, out int argNameIdx)
        {
            if ((Vue.Config.Format & NamingFormat.UI_Prefix) == NamingFormat.UI_Prefix)
            {
                eventNameIdx = 0; argNameIdx = 1;
            }
            else
            {
                eventNameIdx = 1; argNameIdx = 2;
            }

            switch (Vue.Config.Format)
            {
                case NamingFormat.CamelCase | NamingFormat.UI_Suffix:
                    return @"Evt&Arg(\w{1,})\[(\w{1,})\](Slider|Txt|Text|Input|Dropdown|Toggle|Btn|Img|Button|Image)";
                case NamingFormat.CamelCase | NamingFormat.UI_Prefix:
                    return @"(Slider|Txt|Text|Input|Dropdown|Toggle|Btn|Img|Button|Image)Evt&Arg(\w{1,})\[(\w{1,})\]";

                case NamingFormat.UnderlineLower | NamingFormat.UI_Suffix:
                    return @"evt&arg_(\w{1,})\[(\w{1,})\]_(slider|txt|text|input|dropdown|toggle|btn|img|button|image)";
                case NamingFormat.UnderlineLower | NamingFormat.UI_Prefix:
                    return @"(slider|txt|text|input|dropdown|toggle|btn|img|button|image)_evt&arg_(\w{1,})\[(\w{1,})\]";

                case NamingFormat.SpaceLower | NamingFormat.UI_Suffix:
                    return @"evt&arg (\w{1,})\[(\w{1,})\] (slider|txt|text|input|dropdown|toggle|btn|img|button|image)";
                case NamingFormat.SpaceLower | NamingFormat.UI_Prefix:
                    return @"(slider|txt|text|input|dropdown|toggle|btn|img|button|image) evt&arg (\w{1,})\[(\w{1,})\]";

                case NamingFormat.SpaceUpper | NamingFormat.UI_Suffix:
                    return @"Evt&Arg (\w{1,})\[(\w{1,})\] (Slider|Txt|Text|Input|Dropdown|Toggle|Btn|Img|Button|Image)";
                case NamingFormat.SpaceUpper | NamingFormat.UI_Prefix:
                    return @"(Slider|Txt|Text|Input|Dropdown|Toggle|Btn|Img|Button|Image) Evt&Arg (\w{1,})\[(\w{1,})\]";

                case NamingFormat.UnderlineUpper | NamingFormat.UI_Suffix:
                    return @"Evt&Arg_(\w{1,})\[(\w{1,})\]_(Slider|Txt|Text|Input|Dropdown|Toggle|Btn|Img|Button|Image)";
                case NamingFormat.UnderlineUpper | NamingFormat.UI_Prefix:
                    return @"(Slider|Txt|Text|Input|Dropdown|Toggle|Btn|Img|Button|Image)_Evt&Arg_(\w{1,})\[(\w{1,})\]";
            }

            throw new NotSupportedException("非法的命名格式");
        }

        private string GetEventRule(out int eventNameIdx)
        {
            if ((Vue.Config.Format & NamingFormat.UI_Prefix) == NamingFormat.UI_Prefix)
                eventNameIdx = 2;
            else
                eventNameIdx = 1;

            switch (Vue.Config.Format)
            {
                case NamingFormat.CamelCase | NamingFormat.UI_Suffix:
                    return @"Evt(\w{1,})(Slider|Txt|Text|Input|Dropdown|Toggle|Btn|Img|Button|Image)";
                case NamingFormat.CamelCase | NamingFormat.UI_Prefix:
                    return @"(Slider|Txt|Text|Input|Dropdown|Toggle|Btn|Img|Button|Image)Evt(\w{1,})";
                case NamingFormat.UnderlineLower | NamingFormat.UI_Suffix:
                    return @"evt_(\w{1,})_(slider|txt|text|input|dropdown|toggle|btn|img|button|image)";
                case NamingFormat.UnderlineLower | NamingFormat.UI_Prefix:
                    return @"(slider|txt|text|input|dropdown|toggle|btn|img|button|image)_evt_(\w{1,})";

                case NamingFormat.SpaceLower | NamingFormat.UI_Suffix:
                    return @"evt (\w{1,}) (slider|txt|text|input|dropdown|toggle|btn|img|button|image)";
                case NamingFormat.SpaceLower | NamingFormat.UI_Prefix:
                    return @"(slider|txt|text|input|dropdown|toggle|btn|img|button|image) evt (\w{1,})";

                case NamingFormat.SpaceUpper | NamingFormat.UI_Suffix:
                    return @"Evt (\w{1,}) (Slider|Txt|Text|Input|Dropdown|Toggle|Btn|Img|Button|Image)";
                case NamingFormat.SpaceUpper | NamingFormat.UI_Prefix:
                    return @"(Slider|Txt|Text|Input|Dropdown|Toggle|Btn|Img|Button|Image) Evt (\w{1,})";

                case NamingFormat.UnderlineUpper | NamingFormat.UI_Suffix:
                    return @"Evt_(\w{1,})_(Slider|Txt|Text|Input|Dropdown|Toggle|Btn|Img|Button|Image)";
                case NamingFormat.UnderlineUpper | NamingFormat.UI_Prefix:
                    return @"(Slider|Txt|Text|Input|Dropdown|Toggle|Btn|Img|Button|Image)_Evt_(\w{1,})";
            }

            throw new NotSupportedException("非法的命名格式");
        }

        private string GetArgRule(out int eventNameIdx, out int argNameIdx)
        {
            if ((Vue.Config.Format & NamingFormat.UI_Prefix) == NamingFormat.UI_Prefix)
            {
                eventNameIdx = 2; argNameIdx = 3;
            }
            else
            {
                eventNameIdx = 1; argNameIdx = 2;
            }

            switch (Vue.Config.Format)
            {
                case NamingFormat.CamelCase | NamingFormat.UI_Suffix:
                    return @"Arg(\w{1,})\[(\w{1,})\](Slider|Txt|Text|Input|Dropdown|Toggle|Btn|Img|Button|Image)";
                case NamingFormat.CamelCase | NamingFormat.UI_Prefix:
                    return @"(Slider|Txt|Text|Input|Dropdown|Toggle|Btn|Img|Button|Image)Arg(\w{1,})\[(\w{1,})\]";

                case NamingFormat.UnderlineLower | NamingFormat.UI_Suffix:
                    return @"arg_(\w{1,})\[(\w{1,})\]_(slider|txt|text|input|dropdown|toggle|btn|img|button|image)";
                case NamingFormat.UnderlineLower | NamingFormat.UI_Prefix:
                    return @"(slider|txt|text|input|dropdown|toggle|btn|img|button|image)_arg_(\w{1,})\[(\w{1,})\]";

                case NamingFormat.SpaceLower | NamingFormat.UI_Suffix:
                    return @"arg (\w{1,})\[(\w{1,})\] (slider|txt|text|input|dropdown|toggle|btn|img|button|image)";
                case NamingFormat.SpaceLower | NamingFormat.UI_Prefix:
                    return @"(slider|txt|text|input|dropdown|toggle|btn|img|button|image) arg (\w{1,})\[(\w{1,})\]";

                case NamingFormat.SpaceUpper | NamingFormat.UI_Suffix:
                    return @"Arg (\w{1,})\[(\w{1,})\] (Slider|Txt|Text|Input|Dropdown|Toggle|Btn|Img|Button|Image)";
                case NamingFormat.SpaceUpper | NamingFormat.UI_Prefix:
                    return @"(Slider|Txt|Text|Input|Dropdown|Toggle|Btn|Img|Button|Image) Arg (\w{1,})\[(\w{1,})\]";

                case NamingFormat.UnderlineUpper | NamingFormat.UI_Suffix:
                    return @"Arg_(\w{1,})\[(\w{1,})\]_(Slider|Txt|Text|Input|Dropdown|Toggle|Btn|Img|Button|Image)";
                case NamingFormat.UnderlineUpper | NamingFormat.UI_Prefix:
                    return @"(Slider|Txt|Text|Input|Dropdown|Toggle|Btn|Img|Button|Image)_Arg_(\w{1,})\[(\w{1,})\]";
            }

            throw new NotSupportedException("非法的命名格式");
        }


        #endregion

    }

    internal struct EventFilterResult
    {
        public UIEventFlag Flag { get; private set; }

        public string EventName { get; private set; }

        public string ArgName { get; private set; }

        public Component Component { get; private set; }

        public UIType UIType { get; private set; }

        public EventFilterResult(UIEventFlag flag, string eventName, string argName, Component component, UIType uIType)
        {
            Flag = flag;
            EventName = eventName;
            ArgName = argName;
            Component = component;
            UIType = uIType;
        }
    }

    internal enum UIEventFlag
    {
        OnlyArg,
        OnlyEvent,
        ArgAndEvent,
    }
}
