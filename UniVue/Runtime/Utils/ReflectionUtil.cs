using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UniVue.Evt;
using UniVue.Evt.Attr;
using UniVue.ViewModel.Attr;
using UniVue.ViewModel.Models;

namespace UniVue.Utils
{
    public sealed class ReflectionUtil
    {
        private ReflectionUtil() { }

        /// <summary>
        /// 判断一个PropertyInfo是否为可绑定的类型
        /// </summary>
        public static BindablePropertyType GetBindablePropertyType(Type type)
        {
            if (type.IsEnum)
            {
                return BindablePropertyType.Enum;
            }
            else if (type == typeof(string))
            {
                return BindablePropertyType.String;
            }
            else if (type == typeof(float))
            {
                return BindablePropertyType.Float;
            }
            else if (type == typeof(int))
            {
                return BindablePropertyType.Int;
            }
            else if (type == typeof(bool))
            {
                return BindablePropertyType.Bool;
            }else if(type == typeof(Sprite))
            {
                return BindablePropertyType.Sprite;
            }else if(type == typeof(List<Sprite>))
            {
                return BindablePropertyType.ListSprite;
            }else if(type == typeof(List<int>))
            {
                return BindablePropertyType.ListInt;
            }
            else if (type == typeof(List<float>))
            {
                return BindablePropertyType.ListFloat;
            }
            else if (type == typeof(List<string>))
            {
                return BindablePropertyType.ListString;
            }
            else if (type == typeof(List<bool>))
            {
                return BindablePropertyType.ListBool;
            }

            if(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                Type[] types = type.GenericTypeArguments;
                if (types != null && types.Length == 1)
                {
                    if (types[0].IsEnum) { return BindablePropertyType.ListEnum; }
                }
            }

            return BindablePropertyType.None;
        }
    
        /// <summary>
        /// 获取一个枚举类上是否有[Flags]特性
        /// </summary>
        public static bool HasFlags(Type type)
        {
            return type.GetCustomAttribute<FlagsAttribute>(false) != null;
        }

        /// <summary>
        /// 获取枚举值的别名
        /// </summary>
        /// <param name="enumType">枚举类型</param>
        /// <param name="enumValueStr">枚举值的字符串</param>
        /// <returns>如果有别名则返回别名，没有则返回enumValueStr</returns>
        public static string GetEnumAlias(Type enumType, string enumValueStr)
        {
            FieldInfo fieldInfo = enumType.GetField(enumValueStr);
            return fieldInfo?.GetCustomAttribute<EnumAliasAttribute>()?.Alias ?? enumValueStr;
        }

        /// <summary>
        /// 检查T1类型与T2类型是否一致
        /// </summary>
        /// <returns>true:类型一致</returns>
        public static bool CheckTypeMatch<T1, T2>()
        {
            return typeof(T1).Equals(typeof(T2));
        }

        /// <summary>
        /// 获取所有注解了[EventCallAttribute]的方法
        /// </summary>
        /// <returns>List<MethodInfo></returns>
        public static IEnumerable<CustomTuple<MethodInfo,EventCallAttribute>> GetEventCallMethods<T>(T register, CustomTuple<MethodInfo, EventCallAttribute> tuple) where T : IEventRegister
        {
            MethodInfo[] methods = register.GetType().GetMethods(BindingFlags.NonPublic|BindingFlags.Public|BindingFlags.Static|BindingFlags.Instance|BindingFlags.DeclaredOnly);

            for (int i = 0; i < methods.Length; i++)
            {
                EventCallAttribute attribute = methods[i].GetCustomAttribute<EventCallAttribute>(true);

                if (attribute != null)
                {
                    if (methods[i].IsGenericMethod)
                    {
#if UNITY_EDITOR
                        LogUtil.Warning("EventCallAttribute特性不能注解在一个泛型方法上!");
#endif
                        continue;
                    }

                    ParameterInfo[] parameters = methods[i].GetParameters();
                    if(parameters != null)
                    {
                        bool flag = false;
                        for (int j = 0; j < parameters.Length; j++)
                        {
                            if (parameters[j].IsIn || parameters[j].IsOut)
                            {
#if UNITY_EDITOR
                                LogUtil.Warning("EventCallAttribute特性注解的方法参数不能有in、out修饰!");
#endif
                                flag = true;
                                break;
                            }
                        }
                        if (flag) continue;
                    }

                    tuple.Item1 = methods[i];
                    tuple.Item2 = attribute;
                    yield return tuple;
                }
            }

        }
    
        public static bool IsCustomType(Type type)
        {
            return !type.IsValueType && type != typeof(string);
        }
      
        public static T CreateInstance<T>(Type type)
        {
            return (T)Activator.CreateInstance(type);
        }

        /// <summary>
        /// 获取指定扫描的程序集的AutowireInfo
        /// </summary>
        /// <param name="assemblyNames">要搜索的程序集名称</param>
        /// <returns>IEnumerable<AutowireInfo></returns>
        public static IEnumerable<AutowireInfo> GetAutowireInfos(string[] assemblyNames)
        {
            using(var a = GetAssemblies(assemblyNames).GetEnumerator())
            {
                while (a.MoveNext())
                {
                    Type[] types = a.Current.GetTypes();
                    for (int i = 0; i < types.Length; i++)
                    {
                        var attribute = types[i].GetCustomAttribute<EventCallAutowireAttribute>();
                        if(attribute != null)
                        {
                            if (types[i].GetInterface("IEventRegister") != null)
                            {
                                yield return new AutowireInfo(types[i], attribute);
                            }
#if UNITY_EDITOR
                            else
                            {
                                LogUtil.Warning("EventCallAutowireAttribute特性只能注解在实现了接口IEventRegister的类上!");
                            }
#endif
                        }
                    }
                }
            }
        }

        private static IEnumerable<Assembly> GetAssemblies(string[] names)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                string name = assemblies[i].GetName().Name;
                //排除内置的程序集
                if (IsBuiltInAssembly(name)) { continue; }

                for (int j = 0; j < names.Length; j++)
                {
                    if (name == names[j])
                    {
                        yield return assemblies[i];
                    }
                }
            }
        }

        /// <summary>
        /// 判断是否是一个内置的程序集
        /// </summary>
        private static bool IsBuiltInAssembly(string assemblyName)
        {
            return assemblyName.StartsWith("Unity")
                || assemblyName.StartsWith("System")
                || assemblyName.StartsWith("Mono")
                || assemblyName.StartsWith("JetBrains")
                || assemblyName.StartsWith("Bee")
                || assemblyName.StartsWith("mscorlib")
                || assemblyName.StartsWith("PsdPlugin")
                || assemblyName.StartsWith("nunit")
                || assemblyName.StartsWith("log4net")
                || assemblyName.StartsWith("netstandard")
                || assemblyName.StartsWith("Microsoft")
                || assemblyName.StartsWith("UniVue")
                || assemblyName.StartsWith("ExCSS")
                || assemblyName.StartsWith("PlayerBuildProgramLibrary");
        }
    }

}
