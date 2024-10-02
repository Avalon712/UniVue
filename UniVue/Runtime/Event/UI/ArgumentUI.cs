using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UniVue.Common;
using UniVue.ViewModel;

namespace UniVue.Event
{
    /// <summary>
    /// 事件参数封装对象
    /// </summary>
    public sealed class ArgumentUI
    {
        /// <summary>
        /// 获取参数值的UI
        /// </summary>
        private Component _argUI;

        /// <summary>
        /// UI类型
        /// </summary>
        public UIType UIType { get; private set; }

        /// <summary>
        /// 事件参数名称（如果是自定义的实体对象，这将是属性名称）
        /// </summary>
        public string ArgumentName { get; private set; }

        public ArgumentUI(string name, UIType type, Component argUI)
        {
            ArgumentName = name;
            UIType = type;
            _argUI = argUI;
        }

        /// <summary>
        /// 设置UI值
        /// </summary>
        public void SetArgumentValue(in ArgumentValue value)
        {
            if (value.type == SupportableArgumentType.NotSupport || (int)value.type > 6)
            {
                ThrowUtil.ThrowWarn("UIEventArg参数值设置失败，不被支持的参数值类型,仅支持int/float/string/enum/bool/Sprite类型!");
                return;
            }

            switch (UIType)
            {
                case UIType.TMP_Dropdown:
                    if (value.TryGetArgumentValue(SupportableArgumentType.String, out object v1))
                        ((TMP_Dropdown)_argUI).captionText.text = v1 as string;
                    break;
                case UIType.TMP_Text:
                    if (value.TryGetArgumentValue(SupportableArgumentType.String, out object v2))
                        ((TMP_Text)_argUI).text = v2 as string;
                    break;
                case UIType.TMP_InputField:
                    if (value.TryGetArgumentValue(SupportableArgumentType.String, out object v3))
                        ((TMP_InputField)_argUI).text = v3 as string;
                    break;
                case UIType.Toggle:
                case UIType.ToggleGroup:
                    if (value.TryGetArgumentValue(SupportableArgumentType.Bool, out object v4))
                        ((Toggle)_argUI).isOn = (bool)v4;
                    break;
                case UIType.Slider:
                    if (value.TryGetArgumentValue(SupportableArgumentType.Float, out object v5))
                        ((Slider)_argUI).value = (float)v5;
                    break;
                case UIType.Image:
                    if (value.TryGetArgumentValue(SupportableArgumentType.Sprite, out object v6))
                        ((Image)_argUI).sprite = v6 as Sprite;
                    break;
            }
        }

        public bool TryGetArgumentValue(Argument argument, out object value)
        {
            value = null;
            object thisValue = GetRawValue();
            switch (argument.type)
            {
                case SupportableArgumentType.Int:
                    switch (UIType)
                    {
                        case UIType.TMP_Dropdown:
                        case UIType.TMP_Text:
                        case UIType.TMP_InputField:
                            if (int.TryParse(thisValue as string, out int result))
                            {
                                value = result;
                            }
                            break;
                        case UIType.Slider:
                            value = (int)(float)thisValue;
                            break;
                    }
                    break;
                case SupportableArgumentType.Float:
                    switch (UIType)
                    {
                        case UIType.TMP_Dropdown:
                        case UIType.TMP_Text:
                        case UIType.TMP_InputField:
                            if (float.TryParse(thisValue as string, out float result))
                            {
                                value = result;
                            }
                            break;
                        case UIType.Slider:
                            value = (float)thisValue;
                            break;
                    }
                    break;
                case SupportableArgumentType.String:
                    switch (UIType)
                    {
                        case UIType.TMP_Dropdown:
                        case UIType.TMP_Text:
                        case UIType.TMP_InputField:
                            value = thisValue;
                            break;
                    }
                    break;
                case SupportableArgumentType.Enum:
                    switch (UIType)
                    {
                        case UIType.TMP_Dropdown:
                        case UIType.TMP_Text:
                        case UIType.TMP_InputField:
                            if (Enums.TryGetEnumInfo(argument.typeFullName, out EnumInfo enumInfo) &&
                                enumInfo.TryGetValue(value as string, out EnumValueInfo valueInfo))
                            {
                                value = valueInfo.enumValue;
                            }
                            else
                            {
                                ThrowUtil.ThrowWarn($"未能从Enums中获取到枚举类型{argument.typeFullName}的信息，你需要手动添加此枚举的信息到Enums中。");
                            }
                            break;
                        case UIType.Slider:
                            int v = (int)(float)value;
                            if (Enums.TryGetEnumInfo(argument.typeFullName, out EnumInfo enumInfo2) &&
                                enumInfo2.TryGetValue(v, out EnumValueInfo valueInfo2))
                            {
                                value = valueInfo2.enumValue;
                            }
                            else
                            {
                                ThrowUtil.ThrowWarn($"未能从Enums中获取到枚举类型{argument.typeFullName}的信息，你需要手动添加此枚举的信息到Enums中。");
                            }
                            break;
                    }
                    break;
                case SupportableArgumentType.Bool:
                    switch (UIType)
                    {
                        case UIType.TMP_Dropdown:
                        case UIType.TMP_Text:
                        case UIType.TMP_InputField:
                            if (bool.TryParse(thisValue as string, out bool result))
                            {
                                value = result;
                            }
                            break;
                        case UIType.ToggleGroup:
                        case UIType.Toggle:
                            value = (bool)thisValue;
                            break;
                    }
                    break;
                case SupportableArgumentType.Sprite:
                    if (UIType == UIType.Image)
                        value = thisValue;
                    break;
                case SupportableArgumentType.ArgumentUI:
                    value = this;
                    break;
                case SupportableArgumentType.Image:
                case SupportableArgumentType.Slider:
                case SupportableArgumentType.TMP_Text:
                case SupportableArgumentType.TMP_InputField:
                case SupportableArgumentType.Toggle:
                case SupportableArgumentType.TMP_Dropdown:
                    switch (UIType)
                    {
                        case UIType.Image:
                            value = _argUI as Image;
                            break;
                        case UIType.TMP_Dropdown:
                            value = _argUI as TMP_Dropdown;
                            break;
                        case UIType.TMP_Text:
                            value = _argUI as TMP_Text;
                            break;
                        case UIType.TMP_InputField:
                            value = _argUI as TMP_InputField;
                            break;
                        case UIType.ToggleGroup:
                        case UIType.Toggle:
                            value = _argUI as Toggle;
                            break;
                        case UIType.Slider:
                            value = _argUI as Slider;
                            break;
                    }
                    break;
            }
            return value != null;
        }

        /// <summary>
        /// 获取原始值
        /// </summary>
        /// <returns>直接从UI组件上获取到的值</returns>
        public object GetRawValue()
        {
            switch (UIType)
            {
                case UIType.TMP_Dropdown:
                    return ((TMP_Dropdown)_argUI).captionText.text;

                case UIType.TMP_Text:
                    return ((TMP_Text)_argUI).text;

                case UIType.TMP_InputField:
                    return ((TMP_InputField)_argUI).text;

                case UIType.Toggle:
                case UIType.ToggleGroup:
                    return ((Toggle)_argUI).isOn;

                case UIType.Slider:
                    return ((Slider)_argUI).value;

                case UIType.Image:
                    return ((Image)_argUI).sprite;

                default: return null; //不可能执行到这一步
            }
        }

        /// <summary>
        /// 直接设置UI接受的原始值
        /// </summary>
        /// <remarks>注意这是不进行类型检查和转换的，可能会抛出强制转换不合法异常，建议使用SetArgumentValue()函数而不是此函数</remarks>
        /// <param name="value">UI可以接受的原始值</param>
        public void SetRawValue(object value)
        {
            switch (UIType)
            {
                case UIType.TMP_Dropdown:
                    ((TMP_Dropdown)_argUI).captionText.text = (string)value;
                    break;

                case UIType.TMP_Text:
                    ((TMP_Text)_argUI).text = (string)value;
                    break;

                case UIType.TMP_InputField:
                    ((TMP_InputField)_argUI).text = (string)value;
                    break;

                case UIType.Toggle:
                case UIType.ToggleGroup:
                    ((Toggle)_argUI).isOn = (bool)value;
                    break;

                case UIType.Slider:
                    ((Slider)_argUI).value = (float)value;
                    break;

                case UIType.Image:
                    ((Image)_argUI).sprite = (Sprite)value;
                    break;
            }
        }

        public T GetUI<T>() where T : Component
        {
            return _argUI as T;
        }

    }
}
