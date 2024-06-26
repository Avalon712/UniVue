using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UniVue.Model;
using UniVue.Rule;
using UniVue.Utils;
using UniVue.ViewModel.Models;

namespace UniVue.ViewModel
{
    public sealed class UIBundleBuilder
    {
        private UIBundleBuilder() { }

        /// <summary>
        /// 为指定的GameObject构建UIBundle
        /// </summary>
        /// <param name="modelName">模型名称，如果为null将默认为TypeName</param>
        /// <returns>UIModel</returns>
        public static UIBundle Build<T>(List<ValueTuple<Component, UIType>> uis, T model, string modelName, bool allowUIUpdateModel) where T : IBindableModel
        {
            return CreateUIBundle(model, GetAllUIComponents(uis), modelName, allowUIUpdateModel);
        }

        private static Dictionary<UIType, List<Component>> GetAllUIComponents(List<ValueTuple<Component, UIType>> uis)
        {
            Dictionary<UIType, List<Component>> uiComponents = new Dictionary<UIType, List<Component>>();

            for (int i = 0; i < uis.Count; i++)
            {
                var result = uis[i];
                if (uiComponents.ContainsKey(result.Item2))
                    uiComponents[result.Item2].Add(result.Item1);
                else
                    uiComponents.Add(result.Item2, new List<Component>() { result.Item1 });
            }
            return uiComponents;
        }

        private static UIBundle CreateUIBundle<T>(T model, Dictionary<UIType, List<Component>> comps, string modelName, bool allowUIUpdateModel) where T : IBindableModel
        {
            Type type = model.GetType();
            if (modelName == null) { modelName = type.Name; }

            List<PropertyUI> propertyUIs;

            //AtomModel<>类型
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(AtomModel<>))
                propertyUIs = AtomModelBuild(ref modelName, model, type, comps, allowUIUpdateModel);
            else if (type == typeof(GroupModel))
                propertyUIs = GroupModelBuild(ref modelName, model, type, comps, allowUIUpdateModel);
            //用户自定义类型
            else
                propertyUIs = CommonModelBuild(type, modelName, comps, allowUIUpdateModel);

            if (propertyUIs == null || propertyUIs.Count == 0)
                return null;
            else
                return new UIBundle(model, propertyUIs);
        }

        private static List<PropertyUI> AtomModelBuild<T>(ref string modelName, T model, Type type, Dictionary<UIType, List<Component>> comps, bool allowUIUpdateModel) where T : IBindableModel
        {
            List<PropertyUI> propertyUIs = new();
            BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            modelName = type.GetField("_modelName", flags).GetValue(model) as string;
            string propertyName = type.GetProperty("PropertyName", flags).GetValue(model) as string;
            Type propertyType = type.GetProperty("Value", flags).PropertyType;
            BindableType bindableType = ReflectionUtil.GetBindableType(propertyType);
            BuildPropertyUIs(modelName, propertyName, propertyType, ref bindableType, propertyUIs, comps, allowUIUpdateModel);
            return propertyUIs;
        }

        private static List<PropertyUI> GroupModelBuild<T>(ref string modelName, T model, Type type, Dictionary<UIType, List<Component>> comps, bool allowUIUpdateModel)
        {
            List<PropertyUI> propertyUIs = new();
            BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            modelName = type.GetField("_modelName", flags).GetValue(model) as string;
            List<INotifiableProperty> properties = type.GetField("_properties", flags).GetValue(model) as List<INotifiableProperty>;
            //通过反射获取所有的属性类型以及属性名
            for (int i = 0; i < properties.Count; i++)
            {
                Type pType = properties[i].GetType();
                string propertyName = pType.GetProperty("PropertyName", flags).GetValue(properties[i]) as string;
                Type propertyType = pType.GetProperty("Value", flags).PropertyType;
                BindableType bindableType = ReflectionUtil.GetBindableType(propertyType);
                BuildPropertyUIs(modelName, propertyName, propertyType, ref bindableType, propertyUIs, comps, allowUIUpdateModel);
            }
            return propertyUIs;
        }

        private static List<PropertyUI> CommonModelBuild(Type type, string modelName, Dictionary<UIType, List<Component>> comps, bool allowUIUpdateModel)
        {
            List<PropertyUI> propertyUIs = new();
            PropertyInfo[] properties = type.GetProperties();

            for (int i = 0; i < properties.Length; i++)
            {
                PropertyInfo propertyInfo = properties[i];
                //不能绑定无法读取属性值的属性
                if (!propertyInfo.CanRead) continue;

                BuildPropertyUIs(propertyUIs, modelName, propertyInfo, comps, allowUIUpdateModel);
            }
            return propertyUIs;
        }

        private static void BuildPropertyUIs(List<PropertyUI> propertyUIs, string modelName, PropertyInfo propertyInfo, Dictionary<UIType, List<Component>> comps, bool allowUIUpdateModel)
        {
            var bindableType = ReflectionUtil.GetBindableType(propertyInfo.PropertyType);

            if (bindableType == BindableType.None) return;

            //允许通过UI更改属性值的条件
            bool allow = allowUIUpdateModel && propertyInfo.CanWrite;

            BuildPropertyUIs(modelName, propertyInfo.Name, propertyInfo.PropertyType, ref bindableType, propertyUIs, comps, allow);
        }

        private static void BuildPropertyUIs(string modelName, string propertyName, Type propertyType, ref BindableType bindableType, List<PropertyUI> propertyUIs, Dictionary<UIType, List<Component>> comps, bool allow)
        {
            foreach (UIType uiType in comps.Keys)
            {
                List<Component> components = comps[uiType];

                //设置PropertyUI
                switch (uiType)
                {
                    case UIType.TMP_Dropdown:
                        TMP_Dropdowns(modelName, propertyName, propertyType, components, ref bindableType, propertyUIs, allow);
                        break;

                    case UIType.TMP_Text:
                        TMP_Texts(modelName, propertyName, propertyType, components, ref bindableType, propertyUIs);
                        break;

                    case UIType.TMP_InputField:
                        TMP_InputFields(modelName, propertyName, propertyType, components, ref bindableType, propertyUIs, allow);
                        break;

                    case UIType.Toggle:
                        Toggles(modelName, propertyName, propertyType, components, ref bindableType, propertyUIs, allow);
                        break;

                    case UIType.Slider:
                        Sliders(modelName, propertyName, propertyType, components, ref bindableType, propertyUIs, allow);
                        break;

                    case UIType.ToggleGroup:
                        ToggleGroups(modelName, propertyName, propertyType, components, ref bindableType, propertyUIs, allow);
                        break;

                    case UIType.Image:
                        Images(modelName, propertyName, propertyType, components, ref bindableType, propertyUIs);
                        break;
                }
            }
        }


        private static void TMP_Dropdowns(string modelName, string propertyName, Type type, List<Component> components, ref BindableType bindableType, List<PropertyUI> propertyUIs, bool allow)
        {
            for (int j = 0; j < components.Count; j++)
            {
                if (NamingRuleEngine.CheckDataBindMatch(components[j].name, modelName, propertyName))
                {
                    PropertyUI propertyUI = PropertyUIBuilder.DoBuildDropdown(propertyName, type, (TMP_Dropdown)components[j], allow);
                    if (propertyUI != null) { propertyUIs.Add(propertyUI); }
                }
            }
        }

        private static void TMP_Texts(string modelName, string propertyName, Type type, List<Component> components, ref BindableType bindableType, List<PropertyUI> propertyUIs)
        {
            for (int j = 0; j < components.Count; j++)
            {
                if (NamingRuleEngine.CheckDataBindMatch(components[j].name, modelName, propertyName))
                {
                    PropertyUI propertyUI = PropertyUIBuilder.DoBuildText(propertyName, type, bindableType, (TMP_Text)components[j]);
                    if (propertyUI != null) { propertyUIs.Add(propertyUI); }
                }
            }
        }

        private static void TMP_InputFields(string modelName, string propertyName, Type type, List<Component> components, ref BindableType bindableType, List<PropertyUI> propertyUIs, bool allow)
        {
            for (int j = 0; j < components.Count; j++)
            {
                if (NamingRuleEngine.CheckDataBindMatch(components[j].name, modelName, propertyName))
                {
                    PropertyUI uiUpdater = PropertyUIBuilder.DoBuildInput(propertyName, type, bindableType, (TMP_InputField)components[j], allow);
                    if (uiUpdater != null) { propertyUIs.Add(uiUpdater); }
                }
            }
        }

        private static void Toggles(string modelName, string propertyName, Type type, List<Component> components, ref BindableType bindableType, List<PropertyUI> propertyUIs, bool allow)
        {
            List<Toggle> multiChoice = null;//多选
            List<Toggle> intToToggles = null; //int类型的值绑定Toggles

            if (bindableType == BindableType.Bool || bindableType == BindableType.ListBool)
            {
                for (int j = 0; j < components.Count; j++)
                {
                    if (NamingRuleEngine.CheckDataBindMatch(components[j].name, modelName, propertyName))
                    {
                        PropertyUI propertyUI = PropertyUIBuilder.DoBuildToggle(propertyName, (Toggle)components[j], allow);
                        if (propertyUI != null) { propertyUIs.Add(propertyUI); }
                    }
                }
            }
            else if (bindableType == BindableType.Enum) //多选
            {
                for (int j = 0; j < components.Count; j++)
                {
                    if (NamingRuleEngine.CheckDataBindMatch(components[j].name, modelName, propertyName))
                    {
                        if (multiChoice == null) { multiChoice = new List<Toggle>(); }
                        multiChoice.Add((Toggle)components[j]);
                    }
                }
            }
            else if (bindableType == BindableType.Int)
            {
                for (int j = 0; j < components.Count; j++)
                {
                    if (NamingRuleEngine.CheckDataBindMatch(components[j].name, modelName, propertyName))
                    {
                        if (intToToggles == null) { intToToggles = new List<Toggle>(); }
                        intToToggles.Add((Toggle)components[j]);
                    }
                }
            }

            //多选
            bool isFlags = ReflectionUtil.HasFlags(type);
            if (multiChoice != null && bindableType == BindableType.Enum && multiChoice.Count > 1 && isFlags)
            {
                PropertyUI propertyUI = PropertyUIBuilder.DoBuildMultiChoiceToggles(propertyName, type, multiChoice, allow);
                if (propertyUI != null) { propertyUIs.Add(propertyUI); }
                multiChoice.Clear();
            }

            //int绑定Toogle
            if (intToToggles != null && intToToggles.Count > 1 && bindableType == BindableType.Int)
            {
                PropertyUI uiUpdater = new IntPropertyToggles(intToToggles.ToArray(), propertyName, allow);
                propertyUIs.Add(uiUpdater);
                intToToggles.Clear();
            }
        }

        private static void Sliders(string modelName, string propertyName, Type type, List<Component> components, ref BindableType bindableType, List<PropertyUI> propertyUIs, bool allow)
        {
            for (int j = 0; j < components.Count; j++)
            {
                if (NamingRuleEngine.CheckDataBindMatch(components[j].name, modelName, propertyName))
                {
                    PropertyUI propertyUI = PropertyUIBuilder.DoBuildSlider(propertyName, bindableType, (Slider)components[j], allow);
                    if (propertyUI != null) { propertyUIs.Add(propertyUI); }
                }
            }
        }

        private static void ToggleGroups(string modelName, string propertyName, Type type, List<Component> components, ref BindableType bindableType, List<PropertyUI> propertyUIs, bool allow)
        {
            if (bindableType == BindableType.Enum || bindableType == BindableType.ListEnum)
            {
                List<Toggle> singleChoice = null;//单选
                for (int j = 0; j < components.Count; j++)
                {
                    if (NamingRuleEngine.CheckDataBindMatch(components[j].name, modelName, propertyName))
                    {
                        Toggle toggle = (Toggle)components[j];

                        if (singleChoice == null)
                            singleChoice = new List<Toggle>();

                        if (singleChoice.Count > 0 && singleChoice[0].group == toggle.group)
                            singleChoice.Add(toggle);
                        else if (singleChoice.Count == 0)
                            singleChoice.Add(toggle);
                    }
                }

                //单选
                if (singleChoice != null && bindableType == BindableType.Enum && singleChoice.Count > 1)
                {
                    PropertyUI propertyUI = PropertyUIBuilder.DoBuildSingleChoiceToggles(propertyName, type, singleChoice, allow);
                    if (propertyUI != null) { propertyUIs.Add(propertyUI); }
                    singleChoice.Clear();
                }
            }
        }

        private static void Images(string modelName, string propertyName, Type type, List<Component> components, ref BindableType bindableType, List<PropertyUI> propertyUIs)
        {
            if (bindableType == BindableType.Sprite || bindableType == BindableType.ListSprite)
            {
                for (int j = 0; j < components.Count; j++)
                {
                    if (NamingRuleEngine.CheckDataBindMatch(components[j].name, modelName, propertyName))
                    {
                        propertyUIs.Add(new PropertyImage((Image)components[j], propertyName));
                    }
                }
            }
        }
    }
}
