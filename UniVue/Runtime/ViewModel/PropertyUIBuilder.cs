using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UniVue.Utils;
using UniVue.ViewModel.Models;

namespace UniVue.ViewModel
{
    public sealed class PropertyUIBuilder
    {
        private PropertyUIBuilder() { }

        internal static PropertyUI DoBuildSlider(string propertyName, BindableType valuePropertyType, Slider slider, bool allowUIUpdateModel)
        {
            switch (valuePropertyType)
            {
                case BindableType.Float:
                    return new FloatPropertySlider(slider, propertyName, allowUIUpdateModel);
                case BindableType.Int:
                    return new IntPropertySlider(slider, propertyName, allowUIUpdateModel);
            }

            return null;
        }

        internal static PropertyUI DoBuildText(string propertyName, Type type, BindableType valuePropertyType, TMP_Text text)
        {
            switch (valuePropertyType)
            {
                case BindableType.Enum:
                    bool isFlags = ReflectionUtil.HasFlags(type);
                    Array array = Enum.GetValues(type);
                    return isFlags ? new FlagsEnumPropertyText(text, array, propertyName) :
                        new EnumPropertyText(text, array, propertyName);
                case BindableType.Float:
                    return new FloatPropertyText(text, propertyName);
                case BindableType.Int:
                    return new IntPropertyText(text, propertyName);
                case BindableType.String:
                    return new StringPropertyText(text, propertyName);
            }

            return null;
        }

        internal static PropertyUI DoBuildInput(string propertyName, Type type, BindableType valuePropertyType, TMP_InputField input, bool allowUIUpdateModel)
        {
            switch (valuePropertyType)
            {
                case BindableType.Enum:
                    Array array = Enum.GetValues(type);
                    return new EnumPropertyInput(input, array, propertyName, allowUIUpdateModel);
                case BindableType.Float:
                    return new FloatPropertyInput(input, propertyName, allowUIUpdateModel);
                case BindableType.Int:
                    return new IntPropertyInput(input, propertyName, allowUIUpdateModel);
                case BindableType.String:
                    return new StringPropertyInput(input, propertyName, allowUIUpdateModel);
            }
            return null;
        }

        internal static PropertyUI DoBuildDropdown(string propertyName, Type type, TMP_Dropdown dropdown, bool allowUIUpdateModel)
        {
            Array array = Enum.GetValues(type);
            return new EnumPropertyDropdown(dropdown, array, propertyName, allowUIUpdateModel);
        }

        internal static PropertyUI DoBuildToggle(string propertyName, Toggle toggle, bool allowUIUpdateModel)
        {
            return new BoolPropertyToggle(toggle,propertyName, allowUIUpdateModel);
        }

        internal static PropertyUI DoBuildSingleChoiceToggles(string propertyName, Type type, List<Toggle> toggles, bool allowUIUpdateModel)
        {
            Array array = Enum.GetValues(type);
            ValueTuple<Toggle, string>[] tgls = new ValueTuple<Toggle, string>[toggles.Count];
            for (int i = 0; i < toggles.Count; i++)
            {
                ValueTuple<Toggle, string> tuple = new ValueTuple<Toggle, string>();
                tuple.Item1 = toggles[i];
                tuple.Item2 = toggles[i].GetComponentInChildren<Text>()?.text ??
                              toggles[i].GetComponentInChildren<TMP_Text>()?.text;
                tgls[i] = tuple;
            }
            PropertyUI propertyUI = new EnumPropertyToggleGroup(tgls, array, propertyName, allowUIUpdateModel);
            return propertyUI;
        }

        internal static PropertyUI DoBuildMultiChoiceToggles(string propertyName, Type type, List<Toggle> toggles, bool allowUIUpdateModel)
        {
            Array array = Enum.GetValues(type);
            ValueTuple<Toggle, string>[] tgls = new ValueTuple<Toggle, string>[toggles.Count];
            for (int i = 0; i < toggles.Count; i++)
            {
                ValueTuple<Toggle, string> tuple = new ValueTuple<Toggle, string>();
                tuple.Item1 = toggles[i];
                tuple.Item2 = toggles[i].GetComponentInChildren<Text>()?.text ??
                              toggles[i].GetComponentInChildren<TMP_Text>()?.text;
                tgls[i] = tuple;
            }
            PropertyUI propertyUI = new FlagsEnumPropertyToggles(tgls, array, propertyName, allowUIUpdateModel);
            return propertyUI;
        }
    }
}
