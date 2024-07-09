using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UniVue.Evt.Evts;
using UniVue.Rule;
using UniVue.Utils;

namespace UniVue.Evt
{
    public static class UIEventBuilder
    {
        public static void Build(string viewName, List<object> uis)
        {
            List<EventArg> args = new List<EventArg>();

            for (int i = 0; i < uis.Count; i++)
            {
                EventFilterResult result = (EventFilterResult)uis[i];

                if (result.Flag == UIEventFlag.OnlyArg) continue;

                for (int j = 0; j < uis.Count; j++)
                {
                    EventFilterResult arg = (EventFilterResult)uis[j];
                    if (arg.EventName != result.EventName || arg.Flag == UIEventFlag.OnlyEvent) continue;
                    args.Add(new EventArg(arg.ArgName, arg.UIType, arg.Component));
                }

                BuildUIEvent(viewName, ref result, args);
                args.Clear();
            }
        }

        private static void BuildUIEvent(string viewName, ref EventFilterResult result, List<EventArg> args)
        {
            EventArg[] eventArgs = args.Count > 0 ? args.ToArray() : null;
            switch (result.UIType)
            {
                case UIType.TMP_Dropdown:
                    new DropdownEvent(viewName, result.EventName, result.Component as TMP_Dropdown, eventArgs);
                    break;
                case UIType.Button:
                    new ButtonEvent(viewName, result.EventName, result.Component as Button, eventArgs);
                    break;
                case UIType.TMP_InputField:
                    new InputEvent(viewName, result.EventName, result.Component as TMP_InputField, eventArgs);
                    break;
                case UIType.Toggle:
                    new ToggleEvent(viewName, result.EventName, result.Component as Toggle, eventArgs);
                    break;
                case UIType.ToggleGroup:
                    new ToggleEvent(viewName, result.EventName, result.Component as Toggle, eventArgs);
                    break;
                case UIType.Slider:
                    new SliderEvent(viewName, result.EventName, result.Component as Slider, eventArgs);
                    break;
            }
        }
    }
}
