using System;

namespace UniVue.Evt
{
    public interface IEntityMapper
    {
        public Type EntityType { get; }

        public object CreateEntity();

        public void SetValues(object entity, EventArg[] eventArgs);
    }
}
