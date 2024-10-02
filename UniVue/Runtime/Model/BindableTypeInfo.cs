
namespace UniVue.Model
{
    public sealed class BindableTypeInfo
    {
        public readonly string typeName;

        public readonly string typeFullName;

        private readonly BindablePropertyInfo[] properties;

        public readonly int propertyCount;

        public BindablePropertyInfo GetProperty(int index) => properties[index];

        public BindableTypeInfo(string typeName, string typeFullName, BindablePropertyInfo[] properties)
        {
            this.typeName = typeName;
            this.properties = properties;
            propertyCount = properties.Length;
            this.typeFullName = typeFullName;
        }
    }

    public sealed class BindablePropertyInfo
    {
        public readonly BindableType bindType;
        public readonly string propertyName;
        public readonly string typeFullName;

        public BindablePropertyInfo(BindableType bindType, string propertyName, string typeFullName)
        {
            this.bindType = bindType;
            this.propertyName = propertyName;
            this.typeFullName = typeFullName;
        }
    }
}
