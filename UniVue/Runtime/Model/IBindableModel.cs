

namespace UniVue.Model
{
    public interface IBindableModel : IModelUpdater, IConsumableModel
    {
        BindableTypeInfo TypeInfo { get; }
    }
}
