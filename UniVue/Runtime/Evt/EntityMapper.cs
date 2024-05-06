using System;
using System.Reflection;
using UniVue.Utils;

namespace UniVue.Evt
{
    /// <summary>
    /// 将EventArg映射为一个指定类型的对象
    /// </summary>
    public sealed class EntityMapper
    {
        private EntityMapper() { }

        /// <summary>
        /// 将事件参数映射为一个对象类型
        /// </summary>
        /// <param name="type">对象类型</param>
        /// <param name="args">参数</param>
        /// <returns>映射对象</returns>
        public static object Map(Type type,EventArg[] args)
        {
            object instance = Activator.CreateInstance(type);
            SetValues(instance, args);
            return instance;
        }

        /// <summary>
        /// 为一个已经实例的对象进行赋值
        /// </summary>
        /// <param name="instance">已经实例的对象</param>
        /// <param name="args">参数</param>
        public static void SetValues(object instance, EventArg[] args)
        {
            PropertyInfo[] propertyInfos = instance.GetType().GetProperties();

            for (int i = 0; i < propertyInfos.Length; i++)
            {
                PropertyInfo propertyInfo = propertyInfos[i];

                for (int j = 0; j < args.Length; j++)
                {
                    if (args[j].ArgumentName == propertyInfo.Name)
                    {
                        object value = args[j].GetArgumentValue();
                        if (value.GetType() != propertyInfo.PropertyType)
                        {
#if UNITY_EDITOR
                            LogUtil.Warning($"尝试为类型为{instance.GetType()}的对象进行属性赋值时: 属性名为{propertyInfo.Name}的类型为{propertyInfos[i].PropertyType}，与UI返回的事件参数类型{value.GetType()}不一致，无法正确赋值!");
#endif
                        }
                        else
                        {
                            propertyInfo.SetValue(instance, value);
                        }
                    }
                }
            }
        }
    }
}
