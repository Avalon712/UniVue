using System;

namespace UniVue.Utils
{
    public sealed class UITypeUtil
    {
        private UITypeUtil() { }

        public static UIType GetUIType(string uiName)
        {
            if (uiName.StartsWith("Btn", StringComparison.OrdinalIgnoreCase)
                || uiName.StartsWith("Button", StringComparison.OrdinalIgnoreCase)
                || uiName.EndsWith("Btn", StringComparison.OrdinalIgnoreCase)
                || uiName.EndsWith("Button", StringComparison.OrdinalIgnoreCase)
                ) { return UIType.Button; }

            if (uiName.StartsWith("Img", StringComparison.OrdinalIgnoreCase)
              || uiName.StartsWith("Image", StringComparison.OrdinalIgnoreCase)
              || uiName.EndsWith("Img", StringComparison.OrdinalIgnoreCase)
              || uiName.EndsWith("Image", StringComparison.OrdinalIgnoreCase)
            ) { return UIType.Image; }

            if (uiName.StartsWith("Toggle", StringComparison.OrdinalIgnoreCase)
             || uiName.EndsWith("Toggle", StringComparison.OrdinalIgnoreCase)
            ) { return UIType.Toggle; }

            if (uiName.StartsWith("Input", StringComparison.OrdinalIgnoreCase)
             || uiName.EndsWith("Input", StringComparison.OrdinalIgnoreCase)
             ) { return UIType.TMP_InputField; }

            if (uiName.StartsWith("Txt", StringComparison.OrdinalIgnoreCase)
             || uiName.StartsWith("Text", StringComparison.OrdinalIgnoreCase)
             || uiName.EndsWith("Txt", StringComparison.OrdinalIgnoreCase)
             || uiName.EndsWith("Text", StringComparison.OrdinalIgnoreCase)
             ) { return UIType.TMP_Text; }

            if (uiName.StartsWith("Slider", StringComparison.OrdinalIgnoreCase)
             || uiName.EndsWith("Slider", StringComparison.OrdinalIgnoreCase)
             ) { return UIType.Slider; }

            if (uiName.StartsWith("Dropdown", StringComparison.OrdinalIgnoreCase)
             || uiName.EndsWith("Dropdown", StringComparison.OrdinalIgnoreCase)
           ) { return UIType.TMP_Dropdown; }

            return UIType.None;
        }
    }
}
