using System;

namespace UniVue.Utils
{
    public sealed class UITypeUtil
    {
        private UITypeUtil() { }

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
    }
}
