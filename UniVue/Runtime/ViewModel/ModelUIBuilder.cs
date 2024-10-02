using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UniVue.Common;
using UniVue.Internal;
using UniVue.Model;
using UniVue.Rule;

namespace UniVue.ViewModel
{
    public static class ModelUIBuilder
    {
        /// <summary>
        /// 构建UIBundle对象
        /// </summary>
        /// <remarks>不要改变List<ModelRuleResult>中元素之间的次序，因此不能进行尾删</remarks>
        /// <param name="model">模型对象</param>
        /// <param name="propertyUIComps">object类型是ModelFilterResult类型</param>
        /// <returns>UIBundle（可能为null）</returns>
        public static ModelUI Build(string modelName, IBindableModel model, List<ModelRuleResult> propertyUIComps, bool allowUIUpdateModel = true)
        {
            List<PropertyUI> propertyUIs = (List<PropertyUI>)CachePool.GetCache(InternalType.List_PropertyUI);

            BuildPropertyUI_List_Text(propertyUIs, propertyUIComps);
            BuildPropertyUI_List_Image(propertyUIs, propertyUIComps);
            BuildPropertyUI_List_Toggle(propertyUIs, propertyUIComps);
            BuildPropertyUI_List_Slider(propertyUIs, propertyUIComps);
            BuildPropertyUI_List_Input(propertyUIs, propertyUIComps);
            BuildPropertyUI_List_Dropdown(propertyUIs, propertyUIComps);

            BuildPropertyUI_Int_Toggles(propertyUIs, propertyUIComps, allowUIUpdateModel);
            BuildPropertyUI_FlagsEnum_ToggleGroup(propertyUIs, propertyUIComps, allowUIUpdateModel);
            BuildPropertyUI_Enum_ToggleGroup(propertyUIs, propertyUIComps, allowUIUpdateModel);

            for (int i = 0; i < propertyUIComps.Count; i++)
            {
                ModelRuleResult result = propertyUIComps[i];
                BuildPropertyUI_Int_Text(propertyUIs, result, allowUIUpdateModel);
                BuildPropertyUI_Int_Slider(propertyUIs, result, allowUIUpdateModel);
                BuildPropertyUI_Int_Input(propertyUIs, result, allowUIUpdateModel);
                BuildPropertyUI_Int_Image(propertyUIs, result, allowUIUpdateModel);

                BuildPropertyUI_Float_Text(propertyUIs, result, allowUIUpdateModel);
                BuildPropertyUI_Float_Slider(propertyUIs, result, allowUIUpdateModel);
                BuildPropertyUI_Float_Input(propertyUIs, result, allowUIUpdateModel);
                BuildPropertyUI_Float_Image(propertyUIs, result, allowUIUpdateModel);

                if (Enums.TryGetEnumInfo(result.propertyTypeFullName, out var _))
                {
                    BuildPropertyUI_Enum_Text(propertyUIs, result, allowUIUpdateModel);
                    BuildPropertyUI_Enum_Input(propertyUIs, result, allowUIUpdateModel);
                    BuildPropertyUI_Enum_Dropdown(propertyUIs, result, allowUIUpdateModel);

                    BuildPropertyUI_FlagsEnum_Text(propertyUIs, result, allowUIUpdateModel);
                }

                BuildPropertyUI_String_Input(propertyUIs, result, allowUIUpdateModel);
                BuildPropertyUI_String_Text(propertyUIs, result, allowUIUpdateModel);

                BuildPropertyUI_Bool_Toggle(propertyUIs, result, allowUIUpdateModel);

                BuildPropertyUI_Sprite_Image(propertyUIs, result, allowUIUpdateModel);
            }

            if (propertyUIs.Count == 0)
            {
                CachePool.AddCache(InternalType.List_PropertyUI, propertyUIs, false);
                return null;
            }

            return new ModelUI(modelName, model, propertyUIs);
        }


        #region List显示到UI上

        private static void BuildPropertyUI_List_Text(List<PropertyUI> propertyUIs, List<ModelRuleResult> propertyUIComps)
        {
            List<TMP_Text> texts = null;
            for (int i = 0; i < propertyUIComps.Count; i++)
            {
                ModelRuleResult result = propertyUIComps[i];
                if (IsListBindableType(result.bindType) && result.type == UIType.TMP_Text)
                {
                    for (int j = i; j < propertyUIComps.Count; j++)
                    {
                        ModelRuleResult temp = propertyUIComps[j];
                        if (temp.propertyName == result.propertyName && temp.type == UIType.TMP_Text && IsListBindableType(temp.bindType))
                        {
                            if (texts == null) texts = new List<TMP_Text>();
                            texts.Add(temp.component as TMP_Text);
                            propertyUIComps.RemoveAt(j--);
                        }
                    }
                    if (texts != null && texts.Count > 0)
                    {
                        texts.Add(result.component as TMP_Text);
                        propertyUIs.Add(new ListText(result.propertyName, texts.ToArray()));
                        texts.Clear();
                        i--;
                    }
                }
            }
        }

        private static void BuildPropertyUI_List_Slider(List<PropertyUI> propertyUIs, List<ModelRuleResult> propertyUIComps)
        {
            List<Slider> sliders = null;
            for (int i = 0; i < propertyUIComps.Count; i++)
            {
                ModelRuleResult result = propertyUIComps[i];
                if (IsListBindableType(result.bindType) && result.type == UIType.Slider)
                {
                    for (int j = i; j < propertyUIComps.Count; j++)
                    {
                        ModelRuleResult temp = propertyUIComps[j];
                        if (temp.propertyName == result.propertyName && temp.type == UIType.Slider && IsListBindableType(temp.bindType))
                        {
                            if (sliders == null) sliders = new List<Slider>();
                            sliders.Add(temp.component as Slider);
                            propertyUIComps.RemoveAt(j--);
                        }
                    }
                    if (sliders != null && sliders.Count > 0)
                    {
                        propertyUIs.Add(new ListSlider(result.propertyName, sliders.ToArray()));
                        sliders.Clear();
                        i--;
                    }
                }
            }
        }

        private static void BuildPropertyUI_List_Input(List<PropertyUI> propertyUIs, List<ModelRuleResult> propertyUIComps)
        {
            List<TMP_InputField> inputs = null;
            for (int i = 0; i < propertyUIComps.Count; i++)
            {
                ModelRuleResult result = propertyUIComps[i];
                if (IsListBindableType(result.bindType) && result.type == UIType.TMP_InputField)
                {
                    for (int j = i; j < propertyUIComps.Count; j++)
                    {
                        ModelRuleResult temp = propertyUIComps[j];
                        if (temp.propertyName == result.propertyName && temp.type == UIType.TMP_InputField && IsListBindableType(temp.bindType))
                        {
                            if (inputs == null) inputs = new List<TMP_InputField>();
                            inputs.Add(temp.component as TMP_InputField);
                            propertyUIComps.RemoveAt(j--);
                        }
                    }
                    if (inputs != null && inputs.Count > 0)
                    {
                        propertyUIs.Add(new ListInput(result.propertyName, inputs.ToArray()));
                        inputs.Clear();
                        i--;
                    }
                }
            }
        }

        private static void BuildPropertyUI_List_Image(List<PropertyUI> propertyUIs, List<ModelRuleResult> propertyUIComps)
        {
            List<Image> imgs = null;
            for (int i = 0; i < propertyUIComps.Count; i++)
            {
                ModelRuleResult result = propertyUIComps[i];
                if (IsListBindableType(result.bindType) && result.type == UIType.Image)
                {
                    for (int j = i; j < propertyUIComps.Count; j++)
                    {
                        ModelRuleResult temp = propertyUIComps[j];
                        if (temp.propertyName == result.propertyName && temp.type == UIType.Image && IsListBindableType(temp.bindType))
                        {
                            if (imgs == null) imgs = new List<Image>();
                            imgs.Add(temp.component as Image);
                            propertyUIComps.RemoveAt(j--);
                        }
                    }
                    if (imgs != null && imgs.Count > 0)
                    {
                        propertyUIs.Add(new ListImage(result.propertyName, imgs.ToArray()));
                        imgs.Clear();
                        i--;
                    }
                }
            }
        }

        private static void BuildPropertyUI_List_Toggle(List<PropertyUI> propertyUIs, List<ModelRuleResult> propertyUIComps)
        {
            List<Toggle> toggles = null;
            for (int i = 0; i < propertyUIComps.Count; i++)
            {
                ModelRuleResult result = propertyUIComps[i];
                if (IsListBindableType(result.bindType) && result.type == UIType.Toggle)
                {
                    for (int j = 0; j < propertyUIComps.Count; j++)
                    {
                        ModelRuleResult temp = propertyUIComps[j];
                        if (temp.propertyName == result.propertyName && temp.type == UIType.Toggle && IsListBindableType(temp.bindType))
                        {
                            if (toggles == null) toggles = new List<Toggle>();
                            toggles.Add(temp.component as Toggle);
                            propertyUIComps.RemoveAt(j--);
                        }
                    }
                    if (toggles != null && toggles.Count > 0)
                    {
                        propertyUIs.Add(new ListToggle(result.propertyName, toggles.ToArray()));
                        toggles.Clear();
                        i--;
                    }
                }
            }
        }

        private static void BuildPropertyUI_List_Dropdown(List<PropertyUI> propertyUIs, List<ModelRuleResult> propertyUIComps)
        {
            for (int i = 0; i < propertyUIComps.Count; i++)
            {
                ModelRuleResult result = propertyUIComps[i];
                if (IsListBindableType(result.bindType) && result.type == UIType.TMP_Dropdown)
                {
                    propertyUIs.Add(new ListDropdown(result.propertyName, result.component as TMP_Dropdown));
                    propertyUIComps.RemoveAt(i--);
                }
            }
        }

        #endregion

        #region Int绑定UI
        private static void BuildPropertyUI_Int_Text(List<PropertyUI> propertyUIs, in ModelRuleResult result, bool allowUIUpdateModel)
        {
            if (result.type != UIType.TMP_Text || result.bindType != BindableType.Int) return;
            propertyUIs.Add(new IntText(result.component as TMP_Text, result.propertyName));
        }

        private static void BuildPropertyUI_Int_Slider(List<PropertyUI> propertyUIs, in ModelRuleResult result, bool allowUIUpdateModel)
        {
            if (result.type != UIType.Slider || result.bindType != BindableType.Int) return;
            propertyUIs.Add(new IntSlider(result.component as Slider, result.propertyName, allowUIUpdateModel));
        }

        private static void BuildPropertyUI_Int_Input(List<PropertyUI> propertyUIs, in ModelRuleResult result, bool allowUIUpdateModel)
        {
            if (result.type != UIType.TMP_InputField || result.bindType != BindableType.Int) return;
            propertyUIs.Add(new IntInput(result.component as TMP_InputField, result.propertyName, allowUIUpdateModel));
        }

        private static void BuildPropertyUI_Int_Toggles(List<PropertyUI> propertyUIs, List<ModelRuleResult> propertyUIComps, bool allowUIUpdateModel)
        {
            if (propertyUIComps.Count == 0) return;

            List<Toggle> toggles = null;

            for (int i = 0; i < propertyUIComps.Count; i++)
            {
                ModelRuleResult result = propertyUIComps[i];
                if (result.type != UIType.Toggle || result.bindType != BindableType.Int) continue;

                //获取当前属性的所有Toggle组件
                for (int j = i; j < propertyUIComps.Count; j++)
                {
                    ModelRuleResult temp = propertyUIComps[j];
                    if (temp.type == UIType.Toggle && temp.bindType == BindableType.Int && temp.propertyName == result.propertyName)
                    {
                        if (toggles == null) toggles = new List<Toggle>();
                        toggles.Add(temp.component as Toggle);
                        propertyUIComps.RemoveAt(j--);
                    }
                }

                if (toggles != null && toggles.Count > 0)
                {
                    propertyUIs.Add(new IntToggles(toggles.ToArray(), result.propertyName));
                    toggles.Clear();
                    i--;//因为result被移除了，这个位置是新元素，需要进行检测
                }
            }
        }

        private static void BuildPropertyUI_Int_Image(List<PropertyUI> propertyUIs, in ModelRuleResult result, bool allowUIUpdateModel)
        {
            if (result.type != UIType.Image || result.bindType != BindableType.Int) return;
            propertyUIs.Add(new IntImage(result.component as Image, result.propertyName));
        }


        #endregion

        #region String绑定UI
        private static void BuildPropertyUI_String_Input(List<PropertyUI> propertyUIs, in ModelRuleResult result, bool allowUIUpdateModel)
        {
            if (result.type != UIType.TMP_InputField || result.bindType != BindableType.String) return;
            propertyUIs.Add(new StringInput(result.component as TMP_InputField, result.propertyName, allowUIUpdateModel));
        }

        private static void BuildPropertyUI_String_Text(List<PropertyUI> propertyUIs, in ModelRuleResult result, bool allowUIUpdateModel)
        {
            if (result.type != UIType.TMP_Text || result.bindType != BindableType.String) return;
            propertyUIs.Add(new StringText(result.component as TMP_Text, result.propertyName));
        }

        #endregion

        #region Float绑定UI
        private static void BuildPropertyUI_Float_Text(List<PropertyUI> propertyUIs, in ModelRuleResult result, bool allowUIUpdateModel)
        {
            if (result.type != UIType.TMP_Text || result.bindType != BindableType.Float) return;
            propertyUIs.Add(new FloatText(result.component as TMP_Text, result.propertyName));
        }

        private static void BuildPropertyUI_Float_Slider(List<PropertyUI> propertyUIs, in ModelRuleResult result, bool allowUIUpdateModel)
        {
            if (result.type != UIType.Slider || result.bindType != BindableType.Float) return;
            propertyUIs.Add(new FloatSlider(result.component as Slider, result.propertyName, allowUIUpdateModel));
        }

        private static void BuildPropertyUI_Float_Input(List<PropertyUI> propertyUIs, in ModelRuleResult result, bool allowUIUpdateModel)
        {
            if (result.type != UIType.TMP_InputField || result.bindType != BindableType.Float) return;
            propertyUIs.Add(new FloatInput(result.component as TMP_InputField, result.propertyName, allowUIUpdateModel));
        }

        private static void BuildPropertyUI_Float_Image(List<PropertyUI> propertyUIs, in ModelRuleResult result, bool allowUIUpdateModel)
        {
            if (result.type != UIType.Image || result.bindType != BindableType.Float) return;
            propertyUIs.Add(new FloatImage(result.component as Image, result.propertyName));
        }

        #endregion

        #region Bool绑定UI
        private static void BuildPropertyUI_Bool_Toggle(List<PropertyUI> propertyUIs, in ModelRuleResult result, bool allowUIUpdateModel)
        {
            if (result.type != UIType.Toggle || result.bindType != BindableType.Bool) return;
            propertyUIs.Add(new BoolToggle(result.component as Toggle, result.propertyName, allowUIUpdateModel));
        }

        #endregion

        #region Enum绑定UI

        private static void BuildPropertyUI_Enum_Text(List<PropertyUI> propertyUIs, in ModelRuleResult result, bool allowUIUpdateModel)
        {
            if (result.type != UIType.TMP_Text || result.bindType != BindableType.Enum) return;
            propertyUIs.Add(new EnumText(result.component as TMP_Text, result.propertyTypeFullName, result.propertyName));
        }

        private static void BuildPropertyUI_Enum_Input(List<PropertyUI> propertyUIs, in ModelRuleResult result, bool allowUIUpdateModel)
        {
            if (result.type != UIType.TMP_InputField || result.bindType != BindableType.Enum) return;
            propertyUIs.Add(new EnumInput(result.component as TMP_InputField, result.propertyTypeFullName, result.propertyName, allowUIUpdateModel));
        }

        private static void BuildPropertyUI_Enum_Dropdown(List<PropertyUI> propertyUIs, in ModelRuleResult result, bool allowUIUpdateModel)
        {
            if (result.type != UIType.TMP_Dropdown || result.bindType != BindableType.Enum) return;
            propertyUIs.Add(new EnumDropdown(result.component as TMP_Dropdown, result.propertyTypeFullName, result.propertyName, allowUIUpdateModel));
        }

        private static void BuildPropertyUI_Enum_ToggleGroup(List<PropertyUI> propertyUIs, List<ModelRuleResult> propertyUIComps, bool allowUIUpdateModel)
        {
            if (propertyUIComps.Count == 0) return;

            List<EnumToggleInfo> toggles = null;

            for (int i = 0; i < propertyUIComps.Count; i++)
            {
                ModelRuleResult result = propertyUIComps[i];
                if (!Enums.TryGetEnumInfo(result.propertyTypeFullName, out var _)) continue;
                if (result.type != UIType.ToggleGroup || result.bindType != BindableType.Enum) continue;

                //获取当前属性的所有Toggle组件
                for (int j = i; j < propertyUIComps.Count; j++)
                {
                    ModelRuleResult temp = propertyUIComps[j];
                    if (temp.type != UIType.ToggleGroup || temp.bindType != BindableType.Enum || temp.propertyName != result.propertyName) continue;

                    Toggle toggle = temp.component as Toggle;
                    TMP_Text text = toggle.GetComponentInChildren<TMP_Text>();

                    ThrowUtil.ThrowExceptionIfNull(text, "Enum绑定Toggle时，必须在Toggle组件的孩子中有一个TMP_Text组件用于显示枚举值，此外这个TMP_Text组件的名称为枚举值的字符串名称。");

                    if (toggles == null)
                        toggles = new List<EnumToggleInfo>();
                    toggles.Add(new EnumToggleInfo(toggle, text));
                    propertyUIComps.RemoveAt(j--);
                }

                if (toggles != null)
                {
                    propertyUIs.Add(new EnumToggleGroup(toggles.ToArray(), result.propertyTypeFullName, result.propertyName, allowUIUpdateModel));
                    toggles.Clear();
                    i--;
                }
            }
        }

        #endregion

        #region FlagsEnum绑定UI

        private static void BuildPropertyUI_FlagsEnum_Text(List<PropertyUI> propertyUIs, in ModelRuleResult result, bool allowUIUpdateModel)
        {
            if (result.type != UIType.TMP_Text || result.bindType != BindableType.FlagsEnum) return;
            propertyUIs.Add(new FlagsEnumText(result.component as TMP_Text, result.propertyTypeFullName, result.propertyName));
        }

        private static void BuildPropertyUI_FlagsEnum_ToggleGroup(List<PropertyUI> propertyUIs, List<ModelRuleResult> propertyUIComps, bool allowUIUpdateModel)
        {
            if (propertyUIComps.Count == 0) return;

            List<EnumToggleInfo> toggles = null;

            for (int i = 0; i < propertyUIComps.Count; i++)
            {
                ModelRuleResult result = propertyUIComps[i];
                if (result.type != UIType.Toggle || result.bindType != BindableType.FlagsEnum) continue;
                if (!Enums.TryGetEnumInfo(result.propertyTypeFullName, out var _)) continue;

                //获取当前属性的所有Toggle组件
                for (int j = i; j < propertyUIComps.Count; j++)
                {
                    ModelRuleResult temp = propertyUIComps[j];
                    if (temp.type != UIType.Toggle || temp.bindType != BindableType.FlagsEnum || temp.propertyName != result.propertyName) continue;

                    Toggle toggle = temp.component as Toggle;
                    TMP_Text text = toggle.GetComponentInChildren<TMP_Text>();

                    ThrowUtil.ThrowExceptionIfNull(text, "Enum绑定Toggle时，必须在Toggle组件的孩子中有一个TMP_Text组件用于显示枚举值，此外这个TMP_Text组件的名称为枚举值的字符串名称。");

                    if (toggles == null)
                        toggles = new List<EnumToggleInfo>();
                    toggles.Add(new EnumToggleInfo(toggle, text));
                    propertyUIComps.RemoveAt(j--);
                }

                if (toggles != null && toggles.Count > 0)
                {
                    propertyUIs.Add(new FlagsEnumToggleGroup(toggles.ToArray(), result.propertyTypeFullName, result.propertyName, allowUIUpdateModel));
                    toggles.Clear();
                    i--;
                }
            }
        }


        #endregion

        #region Sprite绑定UI

        private static void BuildPropertyUI_Sprite_Image(List<PropertyUI> propertyUIs, in ModelRuleResult result, bool allowUIUpdateModel)
        {
            if (result.type != UIType.Image || result.bindType != BindableType.Sprite) return;
            propertyUIs.Add(new SpriteImage(result.component as Image, result.propertyName));
        }

        #endregion

        private static bool IsListBindableType(BindableType type)
        {
            return (int)type >= 8;
        }
    }
}
