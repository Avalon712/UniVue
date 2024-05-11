using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UniVue.Evt.Evts;
using UniVue.Rule;
using UniVue.Utils;

namespace UniVue.Evt
{
    public sealed class UIEventBuilder
    {
        private UIEventBuilder() { }

        public static void Build(string viewName,List<CustomTuple<Component,UIType>> uis)
        {
            Dictionary<string, List<EventArg>> args = new();
            List<UIEvent> events = new();

            for (int i = 0; i < uis.Count; i++)
            {
                CustomTuple<Component, UIType> result = uis[i];

                string evtName, argName; bool isOnlyEvt, isOnlyArg;
                if (NamingRuleEngine.CheckCustomEventAndArgMatch(result.Item1.name, out evtName, out argName,out isOnlyEvt,out isOnlyArg))
                {
                    if (isOnlyArg || !(isOnlyArg || isOnlyEvt))
                    {
                        if (args.ContainsKey(evtName))
                        {
                            args[evtName].Add(new EventArg(argName, result.Item2, result.Item1));
                        }
                        else
                        {
                            args.Add(evtName, new List<EventArg>() { new EventArg(argName, result.Item2, result.Item1) });
                        }
                    }

                    if(isOnlyEvt || (!isOnlyArg && !isOnlyEvt))
                    {
                        switch (result.Item2)
                        {
                            case UIType.TMP_Dropdown:
                                events.Add(new DropdownEvent(viewName, evtName, result.Item1 as TMP_Dropdown));
                                break;
                            case UIType.Button:
                                events.Add(new ButtonEvent(viewName, evtName, result.Item1 as Button));
                                break;
                            case UIType.TMP_InputField:
                                events.Add(new InputEvent(viewName, evtName, result.Item1 as TMP_InputField));
                                break;
                            case UIType.Toggle:
                                events.Add(new ToggleEvent(viewName, evtName, result.Item1 as Toggle));
                                break;
                            case UIType.ToggleGroup:
                                events.Add(new ToggleEvent(viewName, evtName, result.Item1 as Toggle));
                                break;
                            case UIType.Slider:
                                events.Add(new SliderEvent(viewName, evtName, result.Item1 as Slider));
                                break;
                        }
                    }
                }
            }

            for (int i = 0; i < events.Count; i++)
            {
                if (args.ContainsKey(events[i].EventName))
                {
                    events[i].EventArgs = args[events[i].EventName].ToArray();
                }
            }

            args.Clear();
            events.Clear();
        }
    }
}
