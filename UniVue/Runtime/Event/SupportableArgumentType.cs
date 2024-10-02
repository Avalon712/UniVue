
namespace UniVue.Event
{
    /// <summary>
    /// EventCall支持的方法参数类型
    /// </summary>
    public enum SupportableArgumentType
    {
        /// <summary>
        /// 不被支持的类型
        /// </summary>
        NotSupport,
        Int,
        Float,
        String,
        Enum,
        Bool,
        Sprite,
        Custom,
        EventUI,
        ArgumentUI,
        EventCall,
        Image,
        Slider,
        TMP_Text,
        TMP_InputField,
        Toggle,
        TMP_Dropdown,
    }
}
