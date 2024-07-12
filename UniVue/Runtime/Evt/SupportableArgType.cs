
namespace UniVue.Evt
{
    /// <summary>
    /// EventCall支持的方法参数类型
    /// </summary>
    public enum SupportableArgType
    {
        /// <summary>
        /// 不被支持的类型
        /// </summary>
        None,
        Int,
        Float,
        String,
        Enum,
        Bool,
        Sprite,
        Custom,
        UIEvent,
        EventArg,
        EventCall
    }
}
