using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UniVue.Common
{
    public static class UITypeUtil
    {
        public static UIType GetUIType(string uiName)
        {
            StringComparison comparison = StringComparison.OrdinalIgnoreCase;

            if (uiName.StartsWith("Btn", comparison)
                || uiName.StartsWith("Button", comparison)
                || uiName.EndsWith("Btn", comparison)
                || uiName.EndsWith("Button", comparison)
                ) { return UIType.Button; }

            if (uiName.StartsWith("Img", comparison)
              || uiName.StartsWith("Image", comparison)
              || uiName.EndsWith("Img", comparison)
              || uiName.EndsWith("Image", comparison)
            ) { return UIType.Image; }

            if (uiName.StartsWith("Toggle", comparison)
             || uiName.EndsWith("Toggle", comparison)
            ) { return UIType.Toggle; }

            if (uiName.StartsWith("Input", comparison)
             || uiName.EndsWith("Input", comparison)
             ) { return UIType.TMP_InputField; }

            if (uiName.StartsWith("Txt", comparison)
             || uiName.StartsWith("Text", comparison)
             || uiName.EndsWith("Txt", comparison)
             || uiName.EndsWith("Text", comparison)
             ) { return UIType.TMP_Text; }

            if (uiName.StartsWith("Slider", comparison)
             || uiName.EndsWith("Slider", comparison)
             ) { return UIType.Slider; }

            if (uiName.StartsWith("Dropdown", comparison)
             || uiName.EndsWith("Dropdown", comparison)
           ) { return UIType.TMP_Dropdown; }

            return UIType.None;
        }

        public static bool TryGetUI(GameObject gameObject, out UIType type, out Component ui)
        {
            type = GetUIType(gameObject.name);
            ui = null;
            switch (type)
            {
                case UIType.Image:
                    {
                        ui = gameObject.GetComponent<Image>();
                        break;
                    }
                case UIType.TMP_Dropdown:
                    {
                        ui = gameObject.GetComponent<TMP_Dropdown>();
                        break;
                    }
                case UIType.TMP_Text:
                    {
                        ui = gameObject.GetComponent<TMP_Text>();
                        break;
                    }
                case UIType.TMP_InputField:
                    {
                        ui = gameObject.GetComponent<TMP_InputField>();
                        break;
                    }
                case UIType.Button:
                    {
                        ui = gameObject.GetComponent<Button>();
                        break;
                    }
                case UIType.Toggle:
                    {
                        Toggle toggle = gameObject.GetComponent<Toggle>();
                        if (toggle != null)
                        {
                            ui = toggle;
                            type = toggle.group == null ? UIType.Toggle : UIType.ToggleGroup;
                        }
                        break;
                    }
                case UIType.Slider:
                    {
                        ui = gameObject.GetComponent<Slider>();
                        break;
                    }
            }
            return ui != null;
        }
    }

    /// <summary>
    /// 常见的命名格式
    /// <para>各种UI组件的名称</para>
    /// <para>Toggle => Toggle、toggle</para>
    /// <para>Image => Img、img、image、Image</para>
    /// <para>Slider => Slider、slider</para>
    /// <para>InputField => Input、input</para>
    /// <para>Text => Txt、txt、Text、text</para>
    /// <para>Button => Btn、btn、Button、button</para>
    /// <para>Dropdown => Dropdown、dropdown</para>
    /// </summary>
    public enum UIType
    {
        None,
        Image,
        TMP_Dropdown,
        TMP_Text,
        TMP_InputField,
        Button,
        Toggle,
        Slider,
        ToggleGroup,
    }
}
