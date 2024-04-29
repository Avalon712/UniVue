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

                default: return null; //不可能执行到这一步
            }
        }

        public T GetUI<T>() where T : Component
        {
            return _argUI as T;
        }
    }
}
