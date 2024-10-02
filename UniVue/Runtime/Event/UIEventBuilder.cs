using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UniVue.Common;
using UniVue.Rule;

namespace UniVue.Event
{
    public static class UIEventBuilder
    {
        public static void Build(string viewName, List<EventRuleResult> uis)
        {
            List<ArgumentUI> args = new List<ArgumentUI>();

            for (int i = 0; i < uis.Count; i++)
            {
                EventRuleResult result = uis[i];

                if (result.flag == UIEventFlag.Arg) continue;

                for (int j = 0; j < uis.Count; j++)
                {
                    EventRuleResult arg = uis[j];
                    if (arg.eventName != result.eventName || arg.flag == UIEventFlag.Event) continue;
                    args.Add(new ArgumentUI(arg.argName, arg.uiType, arg.component));
                }

                BuildUIEvent(viewName, result, args);
                args.Clear();
            }
        }

        private static void BuildUIEvent(string viewName, in EventRuleResult result, List<ArgumentUI> args)
        {
            ArgumentUI[] eventArgs = args.Count > 0 ? args.ToArray() : null;
            switch (result.uiType)
            {
                case UIType.TMP_Dropdown:
                    Vue.Event.AddUIEvent(new DropdownEvent(viewName, result.eventName, result.component as TMP_Dropdown, eventArgs));
                    break;
                case UIType.Button:
                    Vue.Event.AddUIEvent(new ButtonEvent(viewName, result.eventName, result.component as Button, eventArgs));
                    break;
                case UIType.TMP_InputField:
                    Vue.Event.AddUIEvent(new InputEvent(viewName, result.eventName, result.component as TMP_InputField, eventArgs));
                    break;
                case UIType.Toggle:
                case UIType.ToggleGroup:
                    Vue.Event.AddUIEvent(new ToggleEvent(viewName, result.eventName, result.component as Toggle, eventArgs));
                    break;
                case UIType.Slider:
                    Vue.Event.AddUIEvent(new SliderEvent(viewName, result.eventName, result.component as Slider, eventArgs));
                    break;
            }
        }
    }
}
