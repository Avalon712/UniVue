
namespace UniVue.ViewModel.Models
{
    public enum BindablePropertyType
    {
        Enum,

        Bool,

        Float,

        Int,

        String,

        Sprite,

        ListEnum,

        ListBool,

        ListFloat,

        ListInt,

        ListString,

        ListSprite,

        /// <summary>
        /// 不能进行绑定的类型
        /// </summary>
        None,
    }
}
