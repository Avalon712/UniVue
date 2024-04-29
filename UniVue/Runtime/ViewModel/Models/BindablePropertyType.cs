
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

        /// <summary>
        /// 不能进行绑定的类型
        /// </summary>
        None,
    }
}
