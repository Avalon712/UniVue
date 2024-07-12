using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using UniVue.Model;
using UniVue.Utils;
using UniVue.ViewModel;

namespace UniVue.Rule
{
    /// <summary>
    /// <para>数据绑定的基本名称匹配规则如下: </para>
    /// <para>"[ModelName | TypeName] + PropertyName + UI组件名称"  </para>
    /// <para>
    /// 注: 括号部分为可选，但是当一个视图绑定了两个模型数据，如果这两个模型不是同一类型
    /// ，可用使用TypeName区分；如果是相同类型且具有相同属性名称的则应该使用一个指定的模型
    /// 名称去区分；第一个括号完全可省的情况是这个视图只绑定了一个模型数据
    /// </para>
    /// </summary>
    public sealed class ModelFilter : IRuleFilter
    {
        private int _typeFlag;
        private bool _allowUIUpdateModel;

        public string ModelName { get; private set; }

        public IBindableModel Model { get; private set; }

        public Type ModelType { get; private set; }

        public UIBundle Bundle { get; private set; }

        public ModelFilter(IBindableModel model, bool allowUIUpdateModel = true, string modelName = null)
        {
            _typeFlag = -1;
            Model = model;
            ModelType = model.GetType();
            _typeFlag = GetTypeFlag();
            ModelName = GetModelName();
            _allowUIUpdateModel = allowUIUpdateModel;
        }

        public bool Check(ref (Component, UIType) component, List<object> results)
        {
            switch (_typeFlag)
            {
                case 1: return DoCheck_AtomModel(ref component, results);
                case 2: return DoCheck_GroupModel(ref component, results);
                case 3: return DoCheck_CustomModel(ref component, results);
            }
            return false;
        }

        public void OnComplete(List<object> results)
        {
            if (results.Count > 0)
                Bundle = UIBundleBuilder.Build(Model, results, _allowUIUpdateModel);
        }


        private string GetModelName()
        {
            if (_typeFlag < 3)
                return ModelType.GetField("_modelName", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Model) as string;
            else
                return string.IsNullOrEmpty(ModelName) ? ModelType.Name : ModelName;
        }


        private int GetTypeFlag()
        {
            if (ModelType.IsGenericType && ModelType.GetGenericTypeDefinition() == typeof(AtomModel<>))
                return 1;
            else if (ModelType == typeof(GroupModel))
                return 2;
            else
                return 3;
        }


        private bool DoCheck_CustomModel(ref (Component, UIType) component, List<object> results)
        {
            PropertyInfo[] properties = ModelType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            string uiName = component.Item1.name;
            for (int i = 0; i < properties.Length; i++)
            {
                BindableType bindType = ReflectionUtil.GetBindableType(properties[i].PropertyType);
                if (bindType != BindableType.None && Regex.IsMatch(uiName, GetRule(properties[i].Name)))
                {
                    results.Add(new ModelFilterResult(component.Item2, component.Item1, bindType, properties[i]));
                    return true;
                }
            }
            return false;
        }


        private bool DoCheck_AtomModel(ref (Component, UIType) component, List<object> results)
        {
            string uiName = component.Item1.name;
            PropertyInfo propertyInfo = ModelType.GetProperty("Value", BindingFlags.Instance | BindingFlags.Public);
            string propertyName = ModelType.GetProperty("PropertyName", BindingFlags.Instance | BindingFlags.Public).GetValue(Model) as string;

            BindableType bindType = ReflectionUtil.GetBindableType(propertyInfo.PropertyType);
            bool result = bindType != BindableType.None && Regex.IsMatch(uiName, GetRule(propertyName));
            if (result)
                results.Add(new ModelFilterResult(component.Item2, component.Item1, bindType, propertyInfo, propertyName));

            return result;
        }


        private bool DoCheck_GroupModel(ref (Component, UIType) component, List<object> results)
        {
            string uiName = component.Item1.name;
            BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            List<INotifiableProperty> properties = ModelType.GetField("_properties", flags).GetValue(Model) as List<INotifiableProperty>;

            for (int i = 0; i < properties.Count; i++)
            {
                Type pType = properties[i].GetType();
                string propertyName = pType.GetProperty("PropertyName", flags).GetValue(properties[i]) as string;
                PropertyInfo propertyInfo = pType.GetProperty("Value", flags);
                BindableType bindType = ReflectionUtil.GetBindableType(propertyInfo.PropertyType);
                if (bindType != BindableType.None && Regex.IsMatch(uiName, GetRule(propertyName)))
                {
                    results.Add(new ModelFilterResult(component.Item2, component.Item1, bindType, propertyInfo, propertyName));
                    return true;
                }
            }

            return false;
        }


        #region 规则定义
        private string GetRule(string propertyName)
        {
            string modelName = ModelName;
            switch (Vue.Config.Format)
            {
                case NamingFormat.CamelCase | NamingFormat.UI_Suffix:
                    return @$"({modelName}){{0,1}}{propertyName}(Slider|Txt|Text|Input|Dropdown|Toggle|Btn|Img|Button|Image)";
                case NamingFormat.CamelCase | NamingFormat.UI_Prefix:
                    return @$"(Slider|Txt|Text|Input|Dropdown|Toggle|Btn|Img|Button|Image)({modelName}){{0,1}}{propertyName}";

                case NamingFormat.UnderlineLower | NamingFormat.UI_Suffix:
                    return @$"({modelName}_){{0,1}}{propertyName}_(slider|txt|text|input|dropdown|toggle|btn|img|button|image)";
                case NamingFormat.UnderlineLower | NamingFormat.UI_Prefix:
                    return @$"(slider|txt|text|input|dropdown|toggle|btn|img|button|image)_({modelName}_){{0,1}}{propertyName}";

                case NamingFormat.SpaceLower | NamingFormat.UI_Suffix:
                    return @$"({modelName} ){{0,1}}{propertyName} (slider|txt|text|input|dropdown|toggle|btn|img|button|image)";
                case NamingFormat.SpaceLower | NamingFormat.UI_Prefix:
                    return @$"(slider|txt|text|input|dropdown|toggle|btn|img|button|image) ({modelName} ){{0,1}}{propertyName}";

                case NamingFormat.SpaceUpper | NamingFormat.UI_Suffix:
                    return @$"({modelName} ){{0,1}}{propertyName} (Slider|Txt|Text|Input|Dropdown|Toggle|Btn|Img|Button|Image)";
                case NamingFormat.SpaceUpper | NamingFormat.UI_Prefix:
                    return @$"(Slider|Txt|Text|Input|Dropdown|Toggle|Btn|Img|Button|Image) ({modelName} ){{0,1}}{propertyName}";

                case NamingFormat.UnderlineUpper | NamingFormat.UI_Suffix:
                    return @$"({modelName}_){{0,1}}{propertyName}_(Slider|Txt|Text|Input|Dropdown|Toggle|Btn|Img|Button|Image)";
                case NamingFormat.UnderlineUpper | NamingFormat.UI_Prefix:
                    return @$"(Slider|Txt|Text|Input|Dropdown|Toggle|Btn|Img|Button|Image)_({modelName}_){{0,1}}{propertyName}";
            }

            throw new NotSupportedException("非法的命名格式");
        }

        #endregion

    }

    internal struct ModelFilterResult
    {

        public UIType UIType { get; private set; }

        public Component Component { get; private set; }

        public PropertyInfo Property { get; private set; }

        public string PropertyName { get; private set; }

        public BindableType BindType { get; private set; }

        public ModelFilterResult(UIType uIType, Component comp, BindableType bindType, PropertyInfo property, string propertyName = null)
        {
            UIType = uIType;
            Component = comp;
            Property = property;
            BindType = bindType;
            PropertyName = property.Name;
            if (!string.IsNullOrEmpty(propertyName))
                PropertyName = propertyName;
        }
    }
}
