using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UniVue.Utils;

namespace UniVue.Evt
{
    /// <summary>
    /// 事件参数封装对象
    /// </summary>
    public struct EventArg
    {
        /// <summary>
        /// 获取参数值的UI
        /// </summary>
        private Component _argUI;

        /// <summary>
        /// UI类型
        /// </summary>
        private UIType _type;

        /// <summary>
        /// 事件参数名称（如果是自定义的实体对象，这将是属性名称）
        /// </summary>
        public string ArgumentName { get; private set; }

        internal EventArg(string name,UIType type,Component argUI)
        {
            ArgumentName = name ; _type = type; _argUI = argUI;
        }

        public void SetArgumentValue(object value)
        {
            SupportableArgType argType = EventUtil.GetSupportableArgType(value.GetType());

            if(argType==SupportableArgType.None || ((int)argType) > 6) 
            {
#if UNITY_EDITOR
                LogUtil.Warning("EventArg参数值设置失败，不被支持的参数值类型,仅支持int/float/string/enum/bool/Sprite类型!");
#endif
                return;
            }

            _SetArgumentValue(value,ref argType);
        }

        private void _SetArgumentValue(object value,ref SupportableArgType argType)
        {
            switch (_type)
            {
                case UIType.TMP_Dropdown:
                    string s1 = value as string;
                    if (argType != SupportableArgType.String) { s1 = value.ToString(); }
                    ((TMP_Dropdown)_argUI).itemText.text = s1;
                    break;
                case UIType.TMP_Text:
                    string s2= value as string;
                    if (argType != SupportableArgType.String) { s2 = value.ToString(); }
                    ((TMP_Text)_argUI).text = s2;
                    break;
                case UIType.TMP_InputField:
                    string s3 = value as string;
                    if(argType != SupportableArgType.String) { s3 = value.ToString(); }
                    ((TMP_InputField)_argUI).text = s3;
                    break;
                case UIType.Toggle:
                    if(argType == SupportableArgType.Bool)
                    {
                        ((Toggle)_argUI).isOn = Convert.ToBoolean(value);
                    }
                    break;
                case UIType.Slider:
                    if(argType == SupportableArgType.Int || argType == SupportableArgType.Float)
                    {
                        ((Slider)_argUI).value = Convert.ToSingle(value);
                    }
                    break;
                case UIType.Image:
                    if(argType == SupportableArgType.Sprite)
                    {
                        ((Image)_argUI).sprite = value as Sprite;
                    }
                    break;
            }
        }

        public object GetArgumentValue()
        {
            switch (_type)
            {
                case UIType.TMP_Dropdown:
                    return ((TMP_Dropdown)_argUI).itemText.text;

                case UIType.TMP_Text:
                    return ((TMP_Text)_argUI).text;

                case UIType.TMP_InputField:
                    return ((TMP_InputField)_argUI).text;

                case UIType.Toggle:
                    return ((Toggle)_argUI).isOn;

                case UIType.Slider:
                    return ((Slider)_argUI).value;

                case UIType.Image:
                    return ((Image)_argUI).sprite;

                default: return null; //不可能执行到这一步
            }
        }

        public T GetUI<T>() where T : Component
        {
            return _argUI as T;
        }
    }
}
