using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UniVue.Common;
using UniVue.Event;
using UniVue.Internal;

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
    public sealed class EventRule : IRule
    {
        private const string EventArg = @"Evt&Arg_(\w{1,})\[(\w{1,})\]_(Slider|Txt|Text|Input|Dropdown|Toggle|Btn|Img|Button|Image)";
        private const string Event = @"Evt_(\w{1,})_(Slider|Txt|Text|Input|Dropdown|Toggle|Btn|Img|Button|Image)";
        private const string Arg = @"Arg_(\w{1,})\[(\w{1,})\]_(Slider|Txt|Text|Input|Dropdown|Toggle|Btn|Img|Button|Image)";

        private static readonly Regex EVENT_ARG = new Regex(EventArg, RegexOptions.Compiled);
        private static readonly Regex EVENT = new Regex(Event, RegexOptions.Compiled);
        private static readonly Regex ARG = new Regex(Arg, RegexOptions.Compiled);


        private readonly List<EventRuleResult> _results;

        public string ViewName { get; internal set; }

        public EventRule(string viewName)
        {
            _results = (List<EventRuleResult>)CachePool.GetCache(InternalType.List_EventRuleResult);
            ViewName = viewName;
        }

        public bool Check(string rule, UIType uiType, Component ui)
        {
            if (uiType == UIType.None) return false;
            Match match = null;
            if (rule.StartsWith("Evt&Arg"))
            {
                match = EVENT_ARG.Match(rule);
                if (match.Success)
                    _results.Add(new EventRuleResult(UIEventFlag.EventAndArg, match.Groups[1].Value, match.Groups[2].Value, ui, uiType));
            }
            else if (rule.StartsWith("Evt"))
            {
                match = EVENT.Match(rule);
                if (match.Success)
                    _results.Add(new EventRuleResult(UIEventFlag.Event, match.Groups[1].Value, null, ui, uiType));
            }
            else if (rule.StartsWith("Arg"))
            {
                match = ARG.Match(rule);
                if (match.Success)
                    _results.Add(new EventRuleResult(UIEventFlag.Arg, match.Groups[1].Value, match.Groups[2].Value, ui, uiType));
            }
            return match != null && match.Success;
        }

        public void OnComplete()
        {
            if (_results.Count > 0)
                UIEventBuilder.Build(ViewName, _results);
            _results.Clear();
            CachePool.AddCache(InternalType.List_EventRuleResult, _results);
        }

    }

    public readonly struct EventRuleResult
    {
        public readonly UIEventFlag flag;

        public readonly string eventName;

        public readonly string argName;

        public readonly Component component;

        public readonly UIType uiType;

        public EventRuleResult(UIEventFlag flag, string eventName, string argName, Component component, UIType uiType)
        {
            this.flag = flag;
            this.eventName = eventName;
            this.argName = argName;
            this.component = component;
            this.uiType = uiType;
        }

        public override bool Equals(object obj)
        {
            return obj is EventRuleResult result &&
                   flag == result.flag &&
                   eventName == result.eventName &&
                   argName == result.argName &&
                   component == result.component &&
                   uiType == result.uiType;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(flag, eventName, argName, component, uiType);
        }

        public override string ToString()
        {
            return $"EventRuleResult{{UIEventFlag={flag} EventName={eventName} ArgumentName={argName} UIType={uiType} Component={component.name}}}";
        }
    }

    public enum UIEventFlag
    {
        Arg,
        Event,
        EventAndArg,
    }
}
