using System;
using System.Collections;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UniVue.Common;
using UniVue.Event;
using UniVue.Model;
using UniVue.ViewModel;

namespace UniVue.Editor
{
    internal static class UniVueEditorUtils
    {
        private static readonly GUIContent _refreshIcon = EditorGUIUtility.IconContent("d_Refresh");

        public static GUIContent RefreshIcon => _refreshIcon;

        /// <summary>
        /// Hierachy面板中鼠标悬浮时UI的背景色
        /// </summary>
        public static Color HierachyHoverColor => new Color(0.3019f, 0.3019f, 0.3019f, 1);

        /// <summary>
        ///  Hierachy面板中鼠标选中时UI的背景色
        /// </summary>
        public static Color HierachySelectedColor => new Color(0.1568f, 0.3294f, 0.4862f, 1);

        public static void DrawHorizontalLine(Color color, float h = 2f)
        {
            Rect rect = GUILayoutUtility.GetRect(0, h, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, color);
        }

        public static void DrawVerticalLine(Color color, float w = 2)
        {
            Rect rect = GUILayoutUtility.GetRect(w, 1, GUILayout.ExpandHeight(true));
            EditorGUI.DrawRect(rect, color);
        }

        public static void DrawPropertyValue(PropertyValue propertyValue)
        {
            int flag = (int)propertyValue.propertyInfo.bindType;
            if (flag < 8)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Value: ", GUILayout.Width(50));
            }
            switch (propertyValue.propertyInfo.bindType)
            {
                case BindableType.Enum:
                    propertyValue.value = EditorGUILayout.EnumPopup((Enum)propertyValue.value);
                    break;
                case BindableType.Bool:
                    propertyValue.value = EditorGUILayout.Toggle((bool)propertyValue.value);
                    break;
                case BindableType.Float:
                    propertyValue.value = EditorGUILayout.FloatField((float)propertyValue.value);
                    break;
                case BindableType.Int:
                    propertyValue.value = EditorGUILayout.IntField((int)propertyValue.value);
                    break;
                case BindableType.String:
                    propertyValue.value = EditorGUILayout.TextField(propertyValue.value as string);
                    break;
                case BindableType.Sprite:
                    propertyValue.value = EditorGUILayout.ObjectField(propertyValue.value as Sprite, typeof(Sprite), true);
                    break;
                case BindableType.FlagsEnum:
                    propertyValue.value = EditorGUILayout.EnumFlagsField((Enum)propertyValue.value);
                    break;
                case BindableType.ListEnum:
                    if (ReflectionUtils.IsListFlagsEnumType(propertyValue.value.GetType()))
                        DrawListPropertyValue(BindableType.FlagsEnum, propertyValue);
                    else
                        DrawListPropertyValue(BindableType.Enum, propertyValue);
                    break;
                case BindableType.ListBool:
                    DrawListPropertyValue(BindableType.Bool, propertyValue);
                    break;
                case BindableType.ListFloat:
                    DrawListPropertyValue(BindableType.Float, propertyValue);
                    break;
                case BindableType.ListInt:
                    DrawListPropertyValue(BindableType.Int, propertyValue);
                    break;
                case BindableType.ListString:
                    DrawListPropertyValue(BindableType.String, propertyValue);
                    break;
                case BindableType.ListSprite:
                    DrawListPropertyValue(BindableType.Sprite, propertyValue);
                    break;
            }
            if (flag < 8)
            {
                if (GUILayout.Button(_refreshIcon))
                {
                    propertyValue.RefreshValue();
                }
                if (GUILayout.Button("Apply"))
                {
                    propertyValue.ApplyValue();
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        private static void DrawListPropertyValue(BindableType bindType, PropertyValue propertyValue)
        {
            IList value = propertyValue.value as IList;
            if (propertyValue == null || value.Count == 0) return;

            EditorGUILayout.LabelField($"Values: [count={value.Count}]");
            for (int i = 0; i < value.Count; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"Item{i}", GUILayout.Width(40));

                switch (bindType)
                {
                    case BindableType.Enum:
                        value[i] = EditorGUILayout.EnumPopup((Enum)value[i], GUILayout.Width(180));
                        break;
                    case BindableType.Bool:
                        value[i] = EditorGUILayout.Toggle((bool)value[i], GUILayout.Width(180));
                        break;
                    case BindableType.Float:
                        value[i] = EditorGUILayout.FloatField((float)value[i], GUILayout.Width(180));
                        break;
                    case BindableType.Int:
                        value[i] = EditorGUILayout.IntField((int)value[i], GUILayout.Width(180));
                        break;
                    case BindableType.String:
                        value[i] = EditorGUILayout.TextField((string)value[i], GUILayout.Width(180));
                        break;
                    case BindableType.Sprite:
                        value[i] = EditorGUILayout.ObjectField(value[i] as Sprite, typeof(Sprite), true, GUILayout.Width(180));
                        break;
                    case BindableType.FlagsEnum:
                        value[i] = EditorGUILayout.EnumFlagsField((Enum)value[i], GUILayout.Width(180));
                        break;
                }
                if (GUILayout.Button("Delete"))
                {
                    value.RemoveAt(i);
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();

            if (propertyValue.temp == null)
                propertyValue.temp = value[0];

            switch (bindType)
            {
                case BindableType.Enum:
                    propertyValue.temp = EditorGUILayout.EnumPopup((Enum)propertyValue.temp, GUILayout.Width(170));
                    break;
                case BindableType.Bool:
                    propertyValue.temp = EditorGUILayout.Toggle((bool)propertyValue.temp, GUILayout.Width(170));
                    break;
                case BindableType.Float:
                    propertyValue.temp = EditorGUILayout.FloatField((float)propertyValue.temp, GUILayout.Width(170));
                    break;
                case BindableType.Int:
                    propertyValue.temp = EditorGUILayout.IntField((int)propertyValue.temp, GUILayout.Width(170));
                    break;
                case BindableType.String:
                    propertyValue.temp = EditorGUILayout.TextField((string)propertyValue.temp, GUILayout.Width(170));
                    break;
                case BindableType.Sprite:
                    propertyValue.temp = EditorGUILayout.ObjectField(propertyValue.temp as Sprite, typeof(Sprite), true, GUILayout.Width(170));
                    break;
                case BindableType.FlagsEnum:
                    propertyValue.temp = EditorGUILayout.EnumFlagsField((Enum)propertyValue.temp, GUILayout.Width(170));
                    break;
            }
            if (GUILayout.Button("Add Item"))
            {
                value.Add(ReflectionUtils.DeepCopy(propertyValue.temp));
            }
            if (GUILayout.Button("Update UI"))
            {
                using (var it = UIQuerier.QueryModelUI(b => b.Model == propertyValue.model).GetEnumerator())
                {
                    while (it.MoveNext())
                    {
                        propertyValue.model.UpdateUI(propertyValue.propertyInfo.propertyName, it.Current);
                    }
                }
            }
            GUILayout.EndHorizontal();
        }

        public static object GetUIValue(Component current)
        {
            if (current is Toggle toggle)
                return toggle.isOn ? "√" : "×";
            else if (current is TMP_Text text)
                return text.text;
            else if (current is Image image)
                return image.sprite;
            else if (current is TMP_Dropdown dropdown)
                return dropdown.captionText.text;
            else if (current is Slider slider)
                return slider.value.ToString();
            else if (current is TMP_InputField inputField)
                return inputField.text;

            return string.Empty;
        }

        public static void SetDefaultValue(Argument argument)
        {
            switch (argument.type)
            {
                case SupportableArgumentType.Int:
                    argument.UnsafeSetValue(0);
                    break;
                case SupportableArgumentType.Float:
                    argument.UnsafeSetValue(0f);
                    break;
                case SupportableArgumentType.String:
                    argument.UnsafeSetValue("");
                    break;
                case SupportableArgumentType.Enum:
                    argument.UnsafeSetValue(Enum.GetValues(Type.GetType(argument.typeFullName, false)).GetValue(0));
                    break;
                case SupportableArgumentType.Bool:
                    argument.UnsafeSetValue(false);
                    break;
            }
        }

        public static void DrawArgumentValue(EventCall call, Argument argument)
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField($"Name: {argument.name}");
            EditorGUILayout.LabelField($"Type: {argument.typeFullName}");
            EditorGUILayout.LabelField($"SupportableType: {argument.type}");

            if ((int)argument.type <= 6)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Value: ", GUILayout.Width(50));
                if (argument.value == null)
                {
                    SetDefaultValue(argument);
                }
                switch (argument.type)
                {
                    case SupportableArgumentType.Int:
                        argument.UnsafeSetValue(EditorGUILayout.IntField((int)argument.value));
                        break;
                    case SupportableArgumentType.Float:
                        argument.UnsafeSetValue(EditorGUILayout.FloatField((float)argument.value));
                        break;
                    case SupportableArgumentType.String:
                        argument.UnsafeSetValue(EditorGUILayout.TextField(argument.value as string));
                        break;
                    case SupportableArgumentType.Enum:
                        if (ReflectionUtils.IsFlagsEnumType(argument.value.GetType()))
                            argument.UnsafeSetValue(EditorGUILayout.EnumFlagsField((Enum)argument.value));
                        else
                            argument.UnsafeSetValue(EditorGUILayout.EnumPopup((Enum)argument.value));
                        break;
                    case SupportableArgumentType.Bool:
                        argument.UnsafeSetValue(EditorGUILayout.Toggle((bool)argument.value));
                        break;
                    case SupportableArgumentType.Sprite:
                        argument.UnsafeSetValue(EditorGUILayout.ObjectField(argument.value as Sprite, typeof(Sprite), true));
                        break;
                }
                if (GUILayout.Button("Apply To UI", GUILayout.Width(100)))
                {
                    if (call.Views != null)
                    {
                        for (int i = 0; i < call.Views.Length; i++)
                        {
                            Vue.Event.SetEventArg(call.EventName, call.Views[i], argument.name, new ArgumentValue(argument.type, argument.value));
                        }
                    }
                    else
                    {
                        Vue.Event.SetEventArg(call.EventName, argument.name, new ArgumentValue(argument.type, argument.value));
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
        }

        /// <summary>
        /// 判断鼠标是否在指定区域内触发了点击事件
        /// </summary>
        public static bool Clicked(in Rect rect)
        {
            return UnityEngine.Event.current.type == EventType.MouseDown && rect.Contains(UnityEngine.Event.current.mousePosition);
        }

        /// <summary>
        /// 创建一个1像素大小的颜色纹理
        /// </summary>
        public static Texture2D CreateUnitTexture(Color color)
        {
            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }

        /// <summary>
        /// 简单匹配UI的命名是否是一个规则命名
        /// </summary>
        /// <param name="uiName"></param>
        public static bool IsRule(string uiName)
        {
            if (UITypeUtil.GetUIType(uiName) == UIType.None) return false;
            return uiName.Contains('_');
        }
    }
}
