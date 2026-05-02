using UnityEditor;

namespace UniVue.Editor
{
    [FilePath("ProjectSettings/UniVueEditorSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public sealed class UniVueEditorSettings : ScriptableSingleton<UniVueEditorSettings>
    {
        /// <summary>
        /// RedPointKey.g.cs存放的目录（必须在Assets目录下）
        /// </summary>
        public string redPointKeyExportDirectory;

        /// <summary>
        /// RedPointKey.g.cs的命名空间
        /// </summary>
        public string redPointKeyNamespace;

        public UGUIType[] uiTypes;

        public string[] typeSuffixes;

        public void SaveSettings()
        {
            Save(true);
        }
    }

    public enum UGUIType
    {
        // Graphic 基类
        Image,
        RawImage,
        Text,

        // 交互组件
        Button,
        Toggle,
        Slider,
        Scrollbar,
        Dropdown,
        InputField,
        ScrollRect,

        // 布局组件
        HorizontalLayoutGroup,
        VerticalLayoutGroup,
        GridLayoutGroup,
        LayoutElement,
        ContentSizeFitter,
        AspectRatioFitter,

        // 其他
        Canvas,
        CanvasGroup,
        CanvasScaler,
        GraphicRaycaster,
        Mask,
        RectMask2D,
        Outline,
        Shadow,
        PositionAsUV1,
        ToggleGroup,
        Selectable,

        // TextMeshPro
        TextMeshProUGUI,
        TMP_InputField,
        TMP_Dropdown
    }
}