using System;
using System.Collections.Generic;
using UnityEngine;
using UniVue.ViewModel;
using UniVue.ViewModel.Models;

namespace UniVue.Utils
{
    public static class ModelUtil
    {
        public static void UpdateUI<T>(string propertyName, T propertyValue, UIBundle bundle)
        {
            BindableType bindableType = ReflectionUtil.GetBindableType(typeof(T));
            switch (bindableType)
            {
                case BindableType.Enum:
                    bundle.UpdateUI(propertyName, Convert.ToInt32(propertyValue));
                    break;
                case BindableType.Bool:
                    bundle.UpdateUI(propertyName, Convert.ToBoolean(propertyValue));
                    break;
                case BindableType.Float:
                    bundle.UpdateUI(propertyName, Convert.ToSingle(propertyValue));
                    break;
                case BindableType.Int:
                    bundle.UpdateUI(propertyName, Convert.ToInt32(propertyValue));
                    break;
                case BindableType.String:
                    bundle.UpdateUI(propertyName, propertyValue.ToString());
                    break;
                case BindableType.Sprite:
                    bundle.UpdateUI(propertyName, (Sprite)(object)propertyValue);
                    break;
                case BindableType.ListEnum:
                    bundle.UpdateUI(propertyName, (List<T>)(object)propertyValue);
                    break;
                case BindableType.ListBool:
                    bundle.UpdateUI(propertyName, (List<bool>)(object)propertyValue);
                    break;
                case BindableType.ListFloat:
                    bundle.UpdateUI(propertyName, (List<float>)(object)propertyValue);
                    break;
                case BindableType.ListInt:
                    bundle.UpdateUI(propertyName, (List<int>)(object)propertyValue);
                    break;
                case BindableType.ListString:
                    bundle.UpdateUI(propertyName, (List<string>)(object)propertyValue);
                    break;
                case BindableType.ListSprite:
                    bundle.UpdateUI(propertyName, (List<Sprite>)(object)propertyValue);
                    break;
            }
        }
    }
}
