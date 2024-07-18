using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UniVue.Model;
using UniVue.Rule;
using UniVue.Utils;

namespace UniVue.ViewModel
{
    public static class UIBundleBuilder
    {
        /// <summary>
        /// 构建UIBundle对象
        /// </summary>
        /// <param name="model">模型对象</param>
        /// <param name="propertyUIComps">object类型是ModelFilterResult类型</param>
        /// <returns>UIBundle（可能为null）</returns>
        public static UIBundle Build(IBindableModel model, List<object> propertyUIComps, bool allowUIUpdateModel = true)
        {
            List<PropertyUI> propertyUIs = new List<PropertyUI>(propertyUIComps.Count);

            BuildPropertyUI_List_Text(propertyUIs, propertyUIComps);
            BuildPropertyUI_List_Image(propertyUIs, propertyUIComps);
            BuildPropertyUI_List_Toggle(propertyUIs, propertyUIComps);
            BuildPropertyUI_List_Slider(propertyUIs, propertyUIComps);
            BuildPropertyUI_List_Input(propertyUIs, propertyUIComps);
            BuildPropertyUI_List_Dropdown(propertyUIs, propertyUIComps);

            BuildPropertyUI_Int_Toggles(propertyUIs, propertyUIComps, allowUIUpdateModel);
            BuildPropertyUI_FlagsEnum_Toggles(propertyUIs, propertyUIComps, allowUIUpdateModel);
            BuildPropertyUI_Enum_ToggleGroup(propertyUIs, propertyUIComps, allowUIUpdateModel);

            for (int i = 0; i < propertyUIComps.Count; i++)
            {
                ModelFilterResult result = (ModelFilterResult)propertyUIComps[i];
                BuildPropertyUI_Int_Text(propertyUIs, ref result, allowUIUpdateModel);
                BuildPropertyUI_Int_Slider(propertyUIs, ref result, allowUIUpdateModel);
                BuildPropertyUI_Int_Input(propertyUIs, ref result, allowUIUpdateModel);

                BuildPropertyUI_Float_Text(propertyUIs, ref result, allowUIUpdateModel);
                BuildPropertyUI_Float_Slider(propertyUIs, ref result, allowUIUpdateModel);
                BuildPropertyUI_Float_Input(propertyUIs, ref result, allowUIUpdateModel);

                BuildPropertyUI_Enum_Text(propertyUIs, ref result, allowUIUpdateModel);
                BuildPropertyUI_Enum_Input(propertyUIs, ref result, allowUIUpdateModel);
                BuildPropertyUI_Enum_Dropdown(propertyUIs, ref result, allowUIUpdateModel);

                BuildPropertyUI_FlagsEnum_Text(propertyUIs, ref result, allowUIUpdateModel);

                BuildPropertyUI_String_Input(propertyUIs, ref result, allowUIUpdateModel);
                BuildPropertyUI_String_Text(propertyUIs, ref result, allowUIUpdateModel);

                BuildPropertyUI_Bool_Toggle(propertyUIs, ref result, allowUIUpdateModel);

                BuildPropertyUI_Sprite_Image(propertyUIs, ref result, allowUIUpdateModel);
            }

            return propertyUIs.Count == 0 ? null : new UIBundle(model, propertyUIs);
        }

        #region List显示到UI上

        private static void BuildPropertyUI_List_Text(List<PropertyUI> propertyUIs, List<object> propertyUIComps)
        {
            List<TMP_Text> texts = new List<TMP_Text>();
            for (int i = 0; i < propertyUIComps.Count; i++)
            {
                ModelFilterResult result = (ModelFilterResult)propertyUIComps[i];
                if ((int)result.BindType >= 6 && result.UIType == UIType.TMP_Text)
                {
                    for (int j = 0; j < propertyUIComps.Count; j++)
                    {
                        ModelFilterResult temp = (ModelFilterResult)propertyUIComps[j];
                        if (temp.PropertyName != result.PropertyName || (int)result.BindType < 6 || temp.UIType != UIType.TMP_Text) continue;
                        texts.Add(temp.Component as TMP_Text);
                        propertyUIComps.RemoveAt(j--);
                    }
                    propertyUIs.Add(new ListText(result.PropertyName, texts.ToArray()));
                    texts.Clear();
                }
            }
        }

        private static void BuildPropertyUI_List_Slider(List<PropertyUI> propertyUIs, List<object> propertyUIComps)
        {
            List<Slider> texts = new List<Slider>();
            for (int i = 0; i < propertyUIComps.Count; i++)
            {
                ModelFilterResult result = (ModelFilterResult)propertyUIComps[i];
                if ((int)result.BindType >= 6 && result.UIType == UIType.Slider)
                {
                    for (int j = 0; j < propertyUIComps.Count; j++)
                    {
                        ModelFilterResult temp = (ModelFilterResult)propertyUIComps[j];
                        if (temp.PropertyName != result.PropertyName || (int)result.BindType < 6 || temp.UIType != UIType.Slider) continue;
                        texts.Add(temp.Component as Slider);
                        propertyUIComps.RemoveAt(j--);
                    }
                    propertyUIs.Add(new ListSlider(result.PropertyName, texts.ToArray()));
                    texts.Clear();
                }
            }
        }

        private static void BuildPropertyUI_List_Input(List<PropertyUI> propertyUIs, List<object> propertyUIComps)
        {
            List<TMP_InputField> texts = new List<TMP_InputField>();
            for (int i = 0; i < propertyUIComps.Count; i++)
            {
                ModelFilterResult result = (ModelFilterResult)propertyUIComps[i];
                if ((int)result.BindType >= 6 && result.UIType == UIType.TMP_InputField)
                {
                    for (int j = 0; j < propertyUIComps.Count; j++)
                    {
                        ModelFilterResult temp = (ModelFilterResult)propertyUIComps[j];
                        if (temp.PropertyName != result.PropertyName || (int)result.BindType < 6 || temp.UIType != UIType.TMP_InputField) continue;
                        texts.Add(temp.Component as TMP_InputField);
                        propertyUIComps.RemoveAt(j--);
                    }
                    propertyUIs.Add(new ListInput(result.PropertyName, texts.ToArray()));
                    texts.Clear();
                }
            }
        }

        private static void BuildPropertyUI_List_Image(List<PropertyUI> propertyUIs, List<object> propertyUIComps)
        {
            List<Image> texts = new List<Image>();
            for (int i = 0; i < propertyUIComps.Count; i++)
            {
                ModelFilterResult result = (ModelFilterResult)propertyUIComps[i];
                if ((int)result.BindType >= 6 && result.UIType == UIType.Image)
                {
                    for (int j = 0; j < propertyUIComps.Count; j++)
                    {
                        ModelFilterResult temp = (ModelFilterResult)propertyUIComps[j];
                        if (temp.PropertyName != result.PropertyName || (int)result.BindType < 6 || temp.UIType != UIType.Image) continue;
                        texts.Add(temp.Component as Image);
                        propertyUIComps.RemoveAt(j--);
                    }
                    propertyUIs.Add(new ListImage(result.PropertyName, texts.ToArray()));
                    texts.Clear();
                }
            }
        }

        private static void BuildPropertyUI_List_Toggle(List<PropertyUI> propertyUIs, List<object> propertyUIComps)
        {
            List<Toggle> texts = new List<Toggle>();
            for (int i = 0; i < propertyUIComps.Count; i++)
            {
                ModelFilterResult result = (ModelFilterResult)propertyUIComps[i];
                if ((int)result.BindType >= 6 && result.UIType == UIType.Toggle)
                {
                    for (int j = 0; j < propertyUIComps.Count; j++)
                    {
                        ModelFilterResult temp = (ModelFilterResult)propertyUIComps[j];
                        if (temp.PropertyName != result.PropertyName || (int)result.BindType < 6 || temp.UIType != UIType.Toggle) continue;
                        texts.Add(temp.Component as Toggle);
                        propertyUIComps.RemoveAt(j--);
                    }
                    propertyUIs.Add(new ListToggle(result.PropertyName, texts.ToArray()));
                    texts.Clear();
                }
            }
        }

        private static void BuildPropertyUI_List_Dropdown(List<PropertyUI> propertyUIs, List<object> propertyUIComps)
        {
            for (int i = 0; i < propertyUIComps.Count; i++)
            {
                ModelFilterResult result = (ModelFilterResult)propertyUIComps[i];
                if ((int)result.BindType >= 6 && result.UIType == UIType.TMP_Dropdown)
                {
                    propertyUIs.Add(new ListDropdown(result.PropertyName, result.Component as TMP_Dropdown));
                    propertyUIComps.RemoveAt(i--);
                }
            }
        }

        #endregion

        #region Int绑定UI
        private static void BuildPropertyUI_Int_Text(List<PropertyUI> propertyUIs, ref ModelFilterResult result, bool allowUIUpdateModel)
        {
            if (result.UIType != UIType.TMP_Text || result.BindType != BindableType.Int) return;
            propertyUIs.Add(new IntText(result.Component as TMP_Text, result.PropertyName));
        }

        private static void BuildPropertyUI_Int_Slider(List<PropertyUI> propertyUIs, ref ModelFilterResult result, bool allowUIUpdateModel)
        {
            if (result.UIType != UIType.Slider || result.BindType != BindableType.Int) return;
            propertyUIs.Add(new IntSlider(result.Component as Slider, result.PropertyName, result.Property.CanWrite && allowUIUpdateModel));
        }

        private static void BuildPropertyUI_Int_Input(List<PropertyUI> propertyUIs, ref ModelFilterResult result, bool allowUIUpdateModel)
        {
            if (result.UIType != UIType.TMP_InputField || result.BindType != BindableType.Int) return;
            propertyUIs.Add(new IntInput(result.Component as TMP_InputField, result.PropertyName, result.Property.CanWrite && allowUIUpdateModel));
        }

        private static void BuildPropertyUI_Int_Toggles(List<PropertyUI> propertyUIs, List<object> propertyUIComps, bool allowUIUpdateModel)
        {
            if (propertyUIComps.Count == 0) return;

            List<Toggle> toggles = new List<Toggle>();

            for (int i = 0; i < propertyUIComps.Count; i++)
            {
                ModelFilterResult result = (ModelFilterResult)propertyUIComps[i];
                if (result.UIType != UIType.Toggle || result.BindType != BindableType.Int) continue;

                //获取当前属性的所有Toggle组件
                for (int j = i; j < propertyUIComps.Count; j++)
                {
                    ModelFilterResult temp = (ModelFilterResult)propertyUIComps[j];
                    if (temp.UIType != UIType.Toggle || temp.BindType != BindableType.Int || temp.PropertyName != result.PropertyName) continue;
                    toggles.Add(temp.Component as Toggle);
                    propertyUIComps.RemoveAt(j--);
                }

                propertyUIs.Add(new IntToggles(toggles.ToArray(), result.PropertyName));
                toggles.Clear();
                i--;
            }
        }

        #endregion

        #region String绑定UI
        private static void BuildPropertyUI_String_Input(List<PropertyUI> propertyUIs, ref ModelFilterResult result, bool allowUIUpdateModel)
        {
            if (result.UIType != UIType.TMP_InputField || result.BindType != BindableType.String) return;
            propertyUIs.Add(new StringInput(result.Component as TMP_InputField, result.PropertyName, result.Property.CanWrite && allowUIUpdateModel));
        }

        private static void BuildPropertyUI_String_Text(List<PropertyUI> propertyUIs, ref ModelFilterResult result, bool allowUIUpdateModel)
        {
            if (result.UIType != UIType.TMP_Text || result.BindType != BindableType.String) return;
            propertyUIs.Add(new StringText(result.Component as TMP_Text, result.PropertyName));
        }

        #endregion

        #region Float绑定UI
        private static void BuildPropertyUI_Float_Text(List<PropertyUI> propertyUIs, ref ModelFilterResult result, bool allowUIUpdateModel)
        {
            if (result.UIType != UIType.TMP_Text || result.BindType != BindableType.Float) return;
            propertyUIs.Add(new FloatText(result.Component as TMP_Text, result.PropertyName));
        }

        private static void BuildPropertyUI_Float_Slider(List<PropertyUI> propertyUIs, ref ModelFilterResult result, bool allowUIUpdateModel)
        {
            if (result.UIType != UIType.Slider || result.BindType != BindableType.Float) return;
            propertyUIs.Add(new FloatSlider(result.Component as Slider, result.PropertyName, result.Property.CanWrite && allowUIUpdateModel));
        }

        private static void BuildPropertyUI_Float_Input(List<PropertyUI> propertyUIs, ref ModelFilterResult result, bool allowUIUpdateModel)
        {
            if (result.UIType != UIType.TMP_InputField || result.BindType != BindableType.Float) return;
            propertyUIs.Add(new FloatInput(result.Component as TMP_InputField, result.PropertyName, result.Property.CanWrite && allowUIUpdateModel));
        }
        #endregion

        #region Bool绑定UI
        private static void BuildPropertyUI_Bool_Toggle(List<PropertyUI> propertyUIs, ref ModelFilterResult result, bool allowUIUpdateModel)
        {
            if (result.UIType != UIType.Toggle || result.BindType != BindableType.Bool) return;
            propertyUIs.Add(new BoolToggle(result.Component as Toggle, result.PropertyName, result.Property.CanWrite && allowUIUpdateModel));
        }

        #endregion

        #region Enum绑定UI

        private static void BuildPropertyUI_Enum_Text(List<PropertyUI> propertyUIs, ref ModelFilterResult result, bool allowUIUpdateModel)
        {
            if (result.UIType != UIType.TMP_Text || !EnumUtil.IsEnum(result.BindType, result.Property.PropertyType, out bool hasFlags) || hasFlags) return;
            propertyUIs.Add(new EnumText(result.Component as TMP_Text, result.Property.PropertyType, result.PropertyName));
        }

        private static void BuildPropertyUI_Enum_Input(List<PropertyUI> propertyUIs, ref ModelFilterResult result, bool allowUIUpdateModel)
        {
            if (result.UIType != UIType.TMP_InputField || result.BindType != BindableType.Enum || EnumUtil.HasFlags(result.Property.PropertyType)) return;
            propertyUIs.Add(new EnumInput(result.Component as TMP_InputField, result.Property.PropertyType, result.PropertyName, result.Property.CanWrite && allowUIUpdateModel));
        }

        private static void BuildPropertyUI_Enum_Dropdown(List<PropertyUI> propertyUIs, ref ModelFilterResult result, bool allowUIUpdateModel)
        {
            if (result.UIType != UIType.TMP_Dropdown || result.BindType != BindableType.Enum || EnumUtil.HasFlags(result.Property.PropertyType)) return;
            propertyUIs.Add(new EnumDropdown(result.Component as TMP_Dropdown, result.Property.PropertyType, result.PropertyName, result.Property.CanWrite && allowUIUpdateModel));
        }

        private static void BuildPropertyUI_Enum_ToggleGroup(List<PropertyUI> propertyUIs, List<object> propertyUIComps, bool allowUIUpdateModel)
        {
            if (propertyUIComps.Count == 0) return;

            List<ValueTuple<Toggle, string>> toggles = new List<ValueTuple<Toggle, string>>();

            for (int i = 0; i < propertyUIComps.Count; i++)
            {
                ModelFilterResult result = (ModelFilterResult)propertyUIComps[i];
                if (result.UIType != UIType.ToggleGroup || result.BindType != BindableType.Enum || EnumUtil.HasFlags(result.Property.PropertyType)) continue;

                //获取当前属性的所有Toggle组件
                for (int j = i; j < propertyUIComps.Count; j++)
                {
                    ModelFilterResult temp = (ModelFilterResult)propertyUIComps[j];
                    if (temp.UIType != UIType.ToggleGroup || temp.BindType != BindableType.Enum || temp.PropertyName != result.PropertyName) continue;

                    Toggle toggle = temp.Component as Toggle;
                    string alias = toggle.GetComponentInChildren<TMP_Text>()?.text;

                    if (string.IsNullOrEmpty(alias))
                    {
#if UNITY_EDITOR
                        LogUtil.Warning($"未能在Toggle[name={toggle.name}]组件的子物体中获取到此组件对应的枚举值(别名),你应该在此Toggle组件的子物体中添加一个TMP_Text组件用来存储此Toggle的对应的枚举值");
#endif
                        continue;
                    }

                    toggles.Add((toggle, alias));
                    propertyUIComps.RemoveAt(j--);
                }

                propertyUIs.Add(new EnumToggleGroup(toggles.ToArray(), result.Property.PropertyType, result.PropertyName, result.Property.CanWrite && allowUIUpdateModel));
                toggles.Clear();
                i--;
            }
        }

        #endregion

        #region FlagsEnum绑定UI

        private static void BuildPropertyUI_FlagsEnum_Text(List<PropertyUI> propertyUIs, ref ModelFilterResult result, bool allowUIUpdateModel)
        {
            if (result.UIType != UIType.TMP_Text || !EnumUtil.IsEnum(result.BindType, result.Property.PropertyType, out bool hasFlags) || !hasFlags) return;
            propertyUIs.Add(new FlagsEnumText(result.Component as TMP_Text, result.Property.PropertyType, result.PropertyName));
        }

        private static void BuildPropertyUI_FlagsEnum_Toggles(List<PropertyUI> propertyUIs, List<object> propertyUIComps, bool allowUIUpdateModel)
        {
            if (propertyUIComps.Count == 0) return;

            List<ValueTuple<Toggle, string>> toggles = new List<ValueTuple<Toggle, string>>();

            for (int i = 0; i < propertyUIComps.Count; i++)
            {
                ModelFilterResult result = (ModelFilterResult)propertyUIComps[i];
                if (result.UIType != UIType.Toggle || !EnumUtil.IsEnum(result.BindType, result.Property.PropertyType, out bool hasFlags) || !hasFlags) continue;

                //获取当前属性的所有Toggle组件
                for (int j = i; j < propertyUIComps.Count; j++)
                {
                    ModelFilterResult temp = (ModelFilterResult)propertyUIComps[j];
                    if (temp.UIType != UIType.Toggle || temp.BindType != BindableType.Enum || temp.PropertyName != result.PropertyName) continue;

                    Toggle toggle = temp.Component as Toggle;
                    string enumStr = toggle.GetComponentInChildren<TMP_Text>()?.text;

                    if (string.IsNullOrEmpty(enumStr))
                    {
#if UNITY_EDITOR
                        LogUtil.Warning($"未能在Toggle[name={toggle.name}]组件的子物体中获取到此组件对应的枚举值,你应该在此Toggle组件的子物体中添加一个TMP_Text组件用来存储此Toggle的对应的枚举值");
#endif
                        continue;
                    }

                    toggles.Add((toggle, enumStr));
                    propertyUIComps.RemoveAt(j--);
                }

                propertyUIs.Add(new FlagsEnumToggles(toggles.ToArray(), result.Property.PropertyType, result.PropertyName, result.Property.CanWrite && allowUIUpdateModel));
                toggles.Clear();
                i--;
            }
        }


        #endregion

        #region Sprite绑定UI

        private static void BuildPropertyUI_Sprite_Image(List<PropertyUI> propertyUIs, ref ModelFilterResult result, bool allowUIUpdateModel)
        {
            if (result.UIType != UIType.Image || result.BindType != BindableType.Sprite) return;
            propertyUIs.Add(new SpriteImage(result.Component as Image, result.PropertyName));
        }

        #endregion
    }
}
