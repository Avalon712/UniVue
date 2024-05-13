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
        public static UIBundle Build<T>(string viewName, List<CustomTuple<Component, UIType>> uis, T model,string modelName, bool allowUIUpdateModel) where T :IBindableModel
        {
            Dictionary<UIType, List<Component>> uiComponents = GetAllUIComponents(uis);

            return CreateUIBundle(viewName,model,uiComponents,modelName,allowUIUpdateModel);
        }

        private static Dictionary<UIType, List<Component>> GetAllUIComponents(List<CustomTuple<Component, UIType>> uis)
        {
            Dictionary<UIType, List<Component>> uiComponents = new Dictionary<UIType, List<Component>>();

            for (int i = 0; i < uis.Count; i++)
            {
                var result = uis[i];
                if (uiComponents.ContainsKey(result.Item2))
                {
                    uiComponents[result.Item2].Add(result.Item1);
                }
                else
                {
                    uiComponents.Add(result.Item2, new List<Component>() { result.Item1 }) ;
                }
            }

            return uiComponents;
        }
   
        private static UIBundle CreateUIBundle<T>(string viewName,T model,Dictionary<UIType, List<Component>> uiComponents,string modelName,bool allowUIUpdateModel) where T : IBindableModel
        {
            List<PropertyUI> propertyUIs = new List<PropertyUI>();
            UIBundle bundle = new UIBundle(viewName, model);

            //防止空
            Type type = model.GetType();
            if (modelName == null) { modelName = type.Name; }

            PropertyInfo[] properties = type.GetProperties();

            //属性绑定UI
            for (int i = 0; i < properties.Length; i++)
            {
                PropertyInfo propertyInfo = properties[i];

                if (!propertyInfo.CanRead) { continue; } //不能绑定无法读取属性值的属性

                BindablePropertyType bindableType = ReflectionUtil.GetBindablePropertyType(propertyInfo.PropertyType);

                if (bindableType == BindablePropertyType.None) { continue; }


                List<Toggle> singleChoice = null;//单选
                List<Toggle> multiChoice = null;//多选
                List<Toggle> intToToggles = null; //int类型的值绑定Toggles

                //允许通过UI更改属性值的条件
                bool allow = allowUIUpdateModel && propertyInfo.CanWrite;

                foreach (UIType uiType in uiComponents.Keys)
                {
                    List<Component> components = uiComponents[uiType];

                    //设置PropertyUI
                    switch (uiType)
                    {
                        case UIType.TMP_Dropdown:
                            for (int j = 0; j < components.Count; j++)
                            {
                                if (NamingRuleEngine.CheckDataBindMatch(components[j].name, modelName, propertyInfo.Name))
                                {
                                    PropertyUI uiUpdater = PropertyUIBuilder.DoBuildDropdown(bundle, propertyInfo, (TMP_Dropdown)components[j], allow);
                                    if (uiUpdater != null) { propertyUIs.Add(uiUpdater); }
                                }
                            }
                            break;

                        case UIType.TMP_Text:
                            for (int j = 0; j < components.Count; j++)
                            {
                                if (NamingRuleEngine.CheckDataBindMatch(components[j].name, modelName, propertyInfo.Name))
                                {
                                    PropertyUI uiUpdater = PropertyUIBuilder.DoBuildText(bundle, propertyInfo, bindableType, (TMP_Text)components[j]);
                                    if (uiUpdater != null) { propertyUIs.Add(uiUpdater); }
                                }
                            }
                            break;

                        case UIType.TMP_InputField:
                            for (int j = 0; j < components.Count; j++)
                            {
                                if (NamingRuleEngine.CheckDataBindMatch(components[j].name, modelName, propertyInfo.Name))
                                {
                                    PropertyUI uiUpdater = PropertyUIBuilder.DoBuildInput(bundle, propertyInfo, bindableType, (TMP_InputField)components[j], allow);
                                    if (uiUpdater != null) { propertyUIs.Add(uiUpdater); }
                                }
                            }
                            break;

                        case UIType.Toggle:
                            if (bindableType == BindablePropertyType.Bool || bindableType == BindablePropertyType.ListBool)
                            {
                                for (int j = 0; j < components.Count; j++)
                                {
                                    if (NamingRuleEngine.CheckDataBindMatch(components[j].name, modelName, propertyInfo.Name))
                                    {
                                        PropertyUI uiUpdater = PropertyUIBuilder.DoBuildToggle(bundle, propertyInfo.Name, (Toggle)components[j], allow);
                                        if (uiUpdater != null) { propertyUIs.Add(uiUpdater); }
                                    }
                                }
                            }
                            else if (bindableType == BindablePropertyType.Enum) //多选
                            {
                                for (int j = 0; j < components.Count; j++)
                                {
                                    if (NamingRuleEngine.CheckDataBindMatch(components[j].name, modelName, propertyInfo.Name))
                                    {
                                        if (multiChoice == null) { multiChoice = new List<Toggle>(); }
                                        multiChoice.Add((Toggle)components[j]);
                                    }
                                }
                            }
                            else if (bindableType == BindablePropertyType.Int)
                            {
                                for (int j = 0; j < components.Count; j++)
                                {
                                    if (NamingRuleEngine.CheckDataBindMatch(components[j].name, modelName, propertyInfo.Name))
                                    {
                                        if (intToToggles == null) { intToToggles = new List<Toggle>(); }
                                        intToToggles.Add((Toggle)components[j]);
                                    }
                                }
                            }
                            break;

                        case UIType.Slider:
                            for (int j = 0; j < components.Count; j++)
                            {
                                if (NamingRuleEngine.CheckDataBindMatch(components[j].name, modelName, propertyInfo.Name))
                                {
                                    PropertyUI uiUpdater = PropertyUIBuilder.DoBuildSlider(bundle, propertyInfo.Name, bindableType, (Slider)components[j], allow);
                                    if (uiUpdater != null) { propertyUIs.Add(uiUpdater); }
                                }
                            }
                            break;

                        case UIType.ToggleGroup:
                            if (bindableType == BindablePropertyType.Enum || bindableType == BindablePropertyType.ListEnum)
                            {
                                for (int j = 0; j < components.Count; j++)
                                {
                                    if (NamingRuleEngine.CheckDataBindMatch(components[j].name, modelName, propertyInfo.Name))
                                    {
                                        if (singleChoice == null) { singleChoice = new List<Toggle>(); }
                                        Toggle toggle = (Toggle)components[j];
                                        if (singleChoice.Count > 0 && singleChoice[0].group == toggle.group)
                                        {
                                            if (singleChoice[0].group == toggle.group)
                                            {
                                                singleChoice.Add(toggle);
                                            }
                                        }
                                        else if (singleChoice.Count == 0) { singleChoice.Add(toggle); }

                                    }
                                }
                            }
                            break;

                        case UIType.Image:
                            {
                                if(bindableType == BindablePropertyType.Sprite || bindableType== BindablePropertyType.ListSprite)
                                {
                                    for (int j = 0; j < components.Count; j++)
                                    {
                                        if (NamingRuleEngine.CheckDataBindMatch(components[j].name, modelName, propertyInfo.Name))
                                        {
                                            propertyUIs.Add(new PropertyImage((Image)components[j], bundle, propertyInfo.Name));
                                        }
                                    }
                                }
                            }
                            break;
                    }

                    //单选
                    if (singleChoice != null && bindableType == BindablePropertyType.Enum && singleChoice.Count > 1)
                    {
                        PropertyUI uiUpdater = PropertyUIBuilder.DoBuildSingleChoiceToggles(bundle, propertyInfo, singleChoice, allow);
                        if (uiUpdater != null) { propertyUIs.Add(uiUpdater); }
                        singleChoice.Clear();
                    }

                    //多选
                    bool isFlags = ReflectionUtil.HasFlags(propertyInfo.PropertyType);
                    if (multiChoice != null && bindableType == BindablePropertyType.Enum && multiChoice.Count > 1 && isFlags)
                    {
                        PropertyUI uiUpdater = PropertyUIBuilder.DoBuildMultiChoiceToggles(bundle, propertyInfo, multiChoice, allow);
                        if (uiUpdater != null) { propertyUIs.Add(uiUpdater); }
                        multiChoice.Clear();
                    }

                    //int绑定Toogle
                    if(intToToggles != null && intToToggles.Count > 1 && bindableType == BindablePropertyType.Int)
                    {
                        PropertyUI uiUpdater = new IntPropertyToggles(intToToggles.ToArray(), bundle, propertyInfo.Name, allow);
                        propertyUIs.Add(uiUpdater);
                        intToToggles.Clear();
                    }
                }

            }

            if (propertyUIs.Count == 0)
            {
                bundle = null;
            }
            else
            {
                bundle.SetProperties(propertyUIs.ToArray());
                propertyUIs.Clear();
            }

            return bundle;
        }
    }
}
