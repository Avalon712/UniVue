using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine.UI;
using UniVue.Model;
using UniVue.Utils;
using UniVue.ViewModel.Models;

namespace UniVue.ViewModel
{
    public sealed class PropertyUIBuilder
    {
        private PropertyUIBuilder() { }

        public static PropertyUI DoBuildSlider(IModelNotifier notifier, string propertyName, BindablePropertyType valuePropertyType, Slider slider, bool allowUIUpdateModel)
        {
            switch (valuePropertyType)
            {
                case BindablePropertyType.Float:
                    return new FloatPropertySlider(slider, notifier, propertyName, allowUIUpdateModel);
                case BindablePropertyType.Int:
                    return new IntPropertySlider(slider, notifier, propertyName, allowUIUpdateModel);
            }

            return null;
        }

        public static PropertyUI DoBuildText(IModelNotifier notifier, PropertyInfo propertyInfo, BindablePropertyType valuePropertyType, TMP_Text text)
        {
            switch (valuePropertyType)
            {
                case BindablePropertyType.Enum:
                    bool isFlags = ReflectionUtil.HasFlags(propertyInfo.PropertyType);
                    Array array = Enum.GetValues(propertyInfo.PropertyType);
                    return isFlags ? new FlagsEnumPropertyText(text, array, notifier, propertyInfo.Name) : new EnumPropertyText(text, array, notifier, propertyInfo.Name);
                case BindablePropertyType.Float:
                    return new FloatPropertyText(text, notifier, propertyInfo.Name);
                case BindablePropertyType.Int:
                    return new IntPropertyText(text, notifier, propertyInfo.Name);
                case BindablePropertyType.String:
                    return new StringPropertyText(text, notifier, propertyInfo.Name);
            }

            return null;
        }

        public static PropertyUI DoBuildInput(IModelNotifier notifier, PropertyInfo propertyInfo, BindablePropertyType valuePropertyType, TMP_InputField input, bool allowUIUpdateModel)
        {
            switch (valuePropertyType)
            {
                case BindablePropertyType.Enum:
                    Array array = Enum.GetValues(propertyInfo.PropertyType);
                    return new EnumPropertyInput(input, array, notifier, propertyInfo.Name, allowUIUpdateModel);
                case BindablePropertyType.Float:
                    return new FloatPropertyInput(input, notifier, propertyInfo.Name, allowUIUpdateModel);
                case BindablePropertyType.Int:
                    return new IntPropertyInput(input, notifier, propertyInfo.Name, allowUIUpdateModel);
                case BindablePropertyType.String:
                    return new StringPropertyInput(input, notifier, propertyInfo.Name, allowUIUpdateModel);
            }
            return null;
        }

        public static PropertyUI DoBuildDropdown(IModelNotifier notifier, PropertyInfo propertyInfo, TMP_Dropdown dropdown, bool allowUIUpdateModel)
        {
            Array array = Enum.GetValues(propertyInfo.PropertyType);
            return new EnumPropertyDropdown(dropdown, array, notifier, propertyInfo.Name, allowUIUpdateModel);
        }

        public static PropertyUI DoBuildToggle(IModelNotifier notifier, string propertyName, Toggle toggle, bool allowUIUpdateModel)
        {
            return new BoolPropertyToggle(toggle, notifier, propertyName, allowUIUpdateModel);
        }

        public static PropertyUI DoBuildSingleChoiceToggles(IModelNotifier notifier, PropertyInfo propertyInfo, List<Toggle> toggles, bool allowUIUpdateModel)
        {
            Array array = Enum.GetValues(propertyInfo.PropertyType);
            CustomTuple<Toggle, string>[] tgls = new CustomTuple<Toggle, string>[toggles.Count];
            for (int i = 0; i < toggles.Count; i++)
            {
                CustomTuple<Toggle, string> tuple = new CustomTuple<Toggle, string>();
                tuple.Item1 = toggles[i];
                tuple.Item2 = toggles[i].GetComponentInChildren<Text>()?.text ??
                              toggles[i].GetComponentInChildren<TMP_Text>()?.text;
                tgls[i] = tuple;
            }
            PropertyUI propertyUI = new EnumPropertyToggleGroup(tgls, array, notifier, propertyInfo.Name, allowUIUpdateModel);
            return propertyUI;
        }

        public static PropertyUI DoBuildMultiChoiceToggles(IModelNotifier notifier, PropertyInfo propertyInfo, List<Toggle> toggles, bool allowUIUpdateModel)
        {
            Array array = Enum.GetValues(propertyInfo.PropertyType);
            CustomTuple<Toggle, string>[] tgls = new CustomTuple<Toggle, string>[toggles.Count];
            for (int i = 0; i < toggles.Count; i++)
            {
                CustomTuple<Toggle, string> tuple = new CustomTuple<Toggle, string>();
                tuple.Item1 = toggles[i];
                tuple.Item2 = toggles[i].GetComponentInChildren<Text>()?.text ??
                              toggles[i].GetComponentInChildren<TMP_Text>()?.text;
                tgls[i] = tuple;
            }
            PropertyUI propertyUI = new FlagsEnumPropertyToggles(tgls, array, notifier, propertyInfo.Name, allowUIUpdateModel);
            return propertyUI;
        }
    }
}
