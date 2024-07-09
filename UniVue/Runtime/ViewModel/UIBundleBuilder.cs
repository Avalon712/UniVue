using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UniVue.Model;
using UniVue.Rule;
using UniVue.Utils;
using UniVue.ViewModel.Models;

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
        public static UIBundle Build(IBindableModel model, List<object> propertyUIComps)
        {
            List<PropertyUI> propertyUIs = new List<PropertyUI>(propertyUIComps.Count);

            BuildPropertyUI_Int_Toggles(propertyUIs, propertyUIComps);
            BuildPropertyUI_Int_Text(propertyUIs, propertyUIComps);
            BuildPropertyUI_Int_Slider(propertyUIs, propertyUIComps);
            BuildPropertyUI_Int_Input(propertyUIs, propertyUIComps);

            BuildPropertyUI_Float_Text(propertyUIs, propertyUIComps);
            BuildPropertyUI_Float_Slider(propertyUIs, propertyUIComps);
            BuildPropertyUI_Float_Input(propertyUIs, propertyUIComps);

            BuildPropertyUI_Enum_ToggleGroup(propertyUIs, propertyUIComps);
            BuildPropertyUI_Enum_Text(propertyUIs, propertyUIComps);
            BuildPropertyUI_Enum_Input(propertyUIs, propertyUIComps);
            BuildPropertyUI_Enum_Dropdown(propertyUIs, propertyUIComps);

            BuildPropertyUI_FlagsEnum_Toggles(propertyUIs, propertyUIComps);
            BuildPropertyUI_FlagsEnum_Text(propertyUIs, propertyUIComps);

            BuildPropertyUI_String_Input(propertyUIs, propertyUIComps);
            BuildPropertyUI_String_Text(propertyUIs, propertyUIComps);

            BuildPropertyUI_Bool_Toggle(propertyUIs, propertyUIComps);

            BuildPropertyUI_Sprite_Image(propertyUIs, propertyUIComps);

            return propertyUIs.Count == 0 ? null : new UIBundle(model, propertyUIs);
        }

        #region Int绑定UI
        private static void BuildPropertyUI_Int_Text(List<PropertyUI> propertyUIs, List<object> propertyUIComps)
        {
            for (int i = 0; i < propertyUIComps.Count; i++)
            {
                ModelFilterResult result = (ModelFilterResult)propertyUIComps[i];
                if (result.UIType != UIType.TMP_Text || (result.BindType != BindableType.Int && result.BindType != BindableType.ListInt)) continue;
                propertyUIs.Add(new IntText(result.Component as TMP_Text, result.PropertyName));
                ListUtil.TrailDelete(propertyUIComps, i--);
            }
        }

        private static void BuildPropertyUI_Int_Slider(List<PropertyUI> propertyUIs, List<object> propertyUIComps)
        {
            for (int i = 0; i < propertyUIComps.Count; i++)
            {
                ModelFilterResult result = (ModelFilterResult)propertyUIComps[i];
                if (result.UIType != UIType.Slider || (result.BindType != BindableType.Int && result.BindType != BindableType.ListInt)) continue;
                propertyUIs.Add(new IntSlider(result.Component as Slider, result.PropertyName, result.Property.CanWrite));
                ListUtil.TrailDelete(propertyUIComps, i--);
            }
        }

        private static void BuildPropertyUI_Int_Input(List<PropertyUI> propertyUIs, List<object> propertyUIComps)
        {
            for (int i = 0; i < propertyUIComps.Count; i++)
            {
                ModelFilterResult result = (ModelFilterResult)propertyUIComps[i];
                if (result.UIType != UIType.TMP_InputField || result.BindType != BindableType.Int) continue;
                propertyUIs.Add(new IntInput(result.Component as TMP_InputField, result.PropertyName, result.Property.CanWrite));
                ListUtil.TrailDelete(propertyUIComps, i--);
            }
        }

        private static void BuildPropertyUI_Int_Toggles(List<PropertyUI> propertyUIs, List<object> propertyUIComps)
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
                    propertyUIComps.RemoveAt(j--); //不能采用尾删
                }

                propertyUIs.Add(new IntToggles(toggles.ToArray(), result.PropertyName));
                i--; //当前位置被删除了，因此减一
                toggles.Clear();
            }
        }

        #endregion

        #region String绑定UI
        private static void BuildPropertyUI_String_Input(List<PropertyUI> propertyUIs, List<object> propertyUIComps)
        {
            for (int i = 0; i < propertyUIComps.Count; i++)
            {
                ModelFilterResult result = (ModelFilterResult)propertyUIComps[i];
                if (result.UIType != UIType.TMP_InputField || result.BindType != BindableType.String) continue;
                propertyUIs.Add(new StringInput(result.Component as TMP_InputField, result.PropertyName, result.Property.CanWrite));
                ListUtil.TrailDelete(propertyUIComps, i--);
            }
        }

        private static void BuildPropertyUI_String_Text(List<PropertyUI> propertyUIs, List<object> propertyUIComps)
        {
            for (int i = 0; i < propertyUIComps.Count; i++)
            {
                ModelFilterResult result = (ModelFilterResult)propertyUIComps[i];
                if (result.UIType != UIType.TMP_Text || (result.BindType != BindableType.String && result.BindType != BindableType.ListString)) continue;
                propertyUIs.Add(new StringText(result.Component as TMP_Text, result.PropertyName));
                ListUtil.TrailDelete(propertyUIComps, i--);
            }
        }

        #endregion

        #region Float绑定UI
        private static void BuildPropertyUI_Float_Text(List<PropertyUI> propertyUIs, List<object> propertyUIComps)
        {
            for (int i = 0; i < propertyUIComps.Count; i++)
            {
                ModelFilterResult result = (ModelFilterResult)propertyUIComps[i];
                if (result.UIType != UIType.TMP_Text || (result.BindType != BindableType.Float && result.BindType != BindableType.ListFloat)) continue;
                propertyUIs.Add(new FloatText(result.Component as TMP_Text, result.PropertyName));
                ListUtil.TrailDelete(propertyUIComps, i--);
            }
        }

        private static void BuildPropertyUI_Float_Slider(List<PropertyUI> propertyUIs, List<object> propertyUIComps)
        {
            for (int i = 0; i < propertyUIComps.Count; i++)
            {
                ModelFilterResult result = (ModelFilterResult)propertyUIComps[i];
                if (result.UIType != UIType.Slider || (result.BindType != BindableType.Float && result.BindType != BindableType.ListFloat)) continue;
                propertyUIs.Add(new FloatSlider(result.Component as Slider, result.PropertyName, result.Property.CanWrite));
                ListUtil.TrailDelete(propertyUIComps, i--);
            }
        }

        private static void BuildPropertyUI_Float_Input(List<PropertyUI> propertyUIs, List<object> propertyUIComps)
        {
            for (int i = 0; i < propertyUIComps.Count; i++)
            {
                ModelFilterResult result = (ModelFilterResult)propertyUIComps[i];
                if (result.UIType != UIType.TMP_InputField || result.BindType != BindableType.Float) continue;
                propertyUIs.Add(new FloatInput(result.Component as TMP_InputField, result.PropertyName, result.Property.CanWrite));
                ListUtil.TrailDelete(propertyUIComps, i--);
            }
        }
        #endregion

        #region Bool绑定UI
        private static void BuildPropertyUI_Bool_Toggle(List<PropertyUI> propertyUIs, List<object> propertyUIComps)
        {
            for (int i = 0; i < propertyUIComps.Count; i++)
            {
                ModelFilterResult result = (ModelFilterResult)propertyUIComps[i];
                if (result.UIType != UIType.Toggle || (result.BindType != BindableType.Bool && result.BindType != BindableType.ListBool)) continue;
                propertyUIs.Add(new BoolToggle(result.Component as Toggle, result.PropertyName, result.Property.CanWrite));
                ListUtil.TrailDelete(propertyUIComps, i--);
            }
        }

        #endregion

        #region Enum绑定UI

        private static void BuildPropertyUI_Enum_Text(List<PropertyUI> propertyUIs, List<object> propertyUIComps)
        {
            for (int i = 0; i < propertyUIComps.Count; i++)
            {
                ModelFilterResult result = (ModelFilterResult)propertyUIComps[i];
                if (result.UIType != UIType.TMP_Text || (result.BindType != BindableType.Enum && result.BindType != BindableType.ListEnum) || ReflectionUtil.HasFlags(result.Property.PropertyType)) continue;
                propertyUIs.Add(new EnumText(result.Component as TMP_Text, Enum.GetValues(result.Property.PropertyType), result.PropertyName));
                ListUtil.TrailDelete(propertyUIComps, i--);
            }
        }

        private static void BuildPropertyUI_Enum_Input(List<PropertyUI> propertyUIs, List<object> propertyUIComps)
        {
            for (int i = 0; i < propertyUIComps.Count; i++)
            {
                ModelFilterResult result = (ModelFilterResult)propertyUIComps[i];
                if (result.UIType != UIType.TMP_InputField || result.BindType != BindableType.Enum || ReflectionUtil.HasFlags(result.Property.PropertyType)) continue;
                propertyUIs.Add(new EnumInput(result.Component as TMP_InputField, Enum.GetValues(result.Property.PropertyType), result.PropertyName, result.Property.CanWrite));
                ListUtil.TrailDelete(propertyUIComps, i--);
            }
        }

        private static void BuildPropertyUI_Enum_Dropdown(List<PropertyUI> propertyUIs, List<object> propertyUIComps)
        {
            for (int i = 0; i < propertyUIComps.Count; i++)
            {
                ModelFilterResult result = (ModelFilterResult)propertyUIComps[i];
                if (result.UIType != UIType.TMP_Dropdown || result.BindType != BindableType.Enum || ReflectionUtil.HasFlags(result.Property.PropertyType)) continue;
                propertyUIs.Add(new EnumDropdown(result.Component as TMP_Dropdown, Enum.GetValues(result.Property.PropertyType), result.PropertyName, result.Property.CanWrite));
                ListUtil.TrailDelete(propertyUIComps, i--);
            }
        }

        private static void BuildPropertyUI_Enum_ToggleGroup(List<PropertyUI> propertyUIs, List<object> propertyUIComps)
        {
            if (propertyUIComps.Count == 0) return;

            List<ValueTuple<Toggle, string>> toggles = new List<ValueTuple<Toggle, string>>();

            for (int i = 0; i < propertyUIComps.Count; i++)
            {
                ModelFilterResult result = (ModelFilterResult)propertyUIComps[i];
                if (result.UIType != UIType.ToggleGroup || result.BindType != BindableType.Enum || ReflectionUtil.HasFlags(result.Property.PropertyType)) continue;

                //获取当前属性的所有Toggle组件
                for (int j = i; j < propertyUIComps.Count; j++)
                {
                    ModelFilterResult temp = (ModelFilterResult)propertyUIComps[j];
                    if (temp.UIType != UIType.ToggleGroup || temp.BindType != BindableType.Enum || temp.PropertyName != result.PropertyName) continue;

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
                    propertyUIComps.RemoveAt(j--); //不能采用尾删
                }

                propertyUIs.Add(new EnumToggleGroup(toggles.ToArray(), Enum.GetValues(result.Property.PropertyType), result.PropertyName, result.Property.CanWrite));
                i--; //当前位置被删除了，因此减一
                toggles.Clear();
            }
        }

        #endregion

        #region FlagsEnum绑定UI

        private static void BuildPropertyUI_FlagsEnum_Text(List<PropertyUI> propertyUIs, List<object> propertyUIComps)
        {
            for (int i = 0; i < propertyUIComps.Count; i++)
            {
                ModelFilterResult result = (ModelFilterResult)propertyUIComps[i];
                if (result.UIType != UIType.TMP_Text || result.BindType != BindableType.Enum || !ReflectionUtil.HasFlags(result.Property.PropertyType)) continue;
                propertyUIs.Add(new FlagsEnumText(result.Component as TMP_Text, Enum.GetValues(result.Property.PropertyType), result.PropertyName));
                ListUtil.TrailDelete(propertyUIComps, i--);
            }
        }

        private static void BuildPropertyUI_FlagsEnum_Toggles(List<PropertyUI> propertyUIs, List<object> propertyUIComps)
        {
            if (propertyUIComps.Count == 0) return;

            List<ValueTuple<Toggle, string>> toggles = new List<ValueTuple<Toggle, string>>();

            for (int i = 0; i < propertyUIComps.Count; i++)
            {
                ModelFilterResult result = (ModelFilterResult)propertyUIComps[i];
                if (result.UIType != UIType.Toggle || result.BindType != BindableType.Enum || !ReflectionUtil.HasFlags(result.Property.PropertyType)) continue;

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
                    propertyUIComps.RemoveAt(j--); //不能采用尾删
                }

                propertyUIs.Add(new FlagsEnumToggles(toggles.ToArray(), Enum.GetValues(result.Property.PropertyType), result.PropertyName, result.Property.CanWrite));
                i--; //当前位置被删除了，因此减一
                toggles.Clear();
            }
        }


        #endregion

        #region Sprite绑定UI

        private static void BuildPropertyUI_Sprite_Image(List<PropertyUI> propertyUIs, List<object> propertyUIComps)
        {
            for (int i = 0; i < propertyUIComps.Count; i++)
            {
                ModelFilterResult result = (ModelFilterResult)propertyUIComps[i];
                if (result.UIType != UIType.Image || (result.BindType != BindableType.Sprite && result.BindType != BindableType.ListSprite)) continue;
                propertyUIs.Add(new SpriteImage(result.Component as Image, result.PropertyName));
                ListUtil.TrailDelete(propertyUIComps, i--);
            }
        }

        #endregion
    }
}
