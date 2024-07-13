using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniVue.Model;
using UniVue.ViewModel;

namespace UniVue.Utils
{
    public static class ModelUtil
    {
        public static void UpdateUI(string propertyName, object propertyValue, UIBundle bundle)
        {
            BindableType bindableType = ReflectionUtil.GetBindableType(propertyValue.GetType());
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
                    bundle.UpdateUI(propertyName, propertyValue as string);
                    break;
                case BindableType.Sprite:
                    bundle.UpdateUI(propertyName, (Sprite)propertyValue);
                    break;
                case BindableType.ListEnum:
                    bundle.UpdateUI(propertyName, (IList)propertyValue);
                    break;
                case BindableType.ListBool:
                    bundle.UpdateUI(propertyName, (List<bool>)propertyValue);
                    break;
                case BindableType.ListFloat:
                    bundle.UpdateUI(propertyName, (List<float>)propertyValue);
                    break;
                case BindableType.ListInt:
                    bundle.UpdateUI(propertyName, (List<int>)propertyValue);
                    break;
                case BindableType.ListString:
                    bundle.UpdateUI(propertyName, (List<string>)propertyValue);
                    break;
                case BindableType.ListSprite:
                    bundle.UpdateUI(propertyName, (List<Sprite>)propertyValue);
                    break;
            }
        }
    }
}
