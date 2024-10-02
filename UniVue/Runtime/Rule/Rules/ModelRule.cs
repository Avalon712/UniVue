using System;
using System.Collections.Generic;
using UnityEngine;
using UniVue.Common;
using UniVue.Internal;
using UniVue.Model;
using UniVue.ViewModel;

namespace UniVue.Rule
{
    /// <summary>
    /// <para>数据绑定的基本名称匹配规则如下: </para>
    /// <para>"[ModelName | TypeName] + PropertyName + UI组件名称"  </para>
    /// <para>
    /// 注: 括号部分为可选，但是当一个视图绑定了两个模型数据，如果这两个模型不是同一类型
    /// ，可用使用TypeName区分；如果是相同类型且具有相同属性名称的则应该使用一个指定的模型
    /// 名称去区分；第一个括号完全可省的情况是这个视图只绑定了一个模型数据
    /// </para>
    /// </summary>
    public sealed class ModelRule : IRule
    {
        private readonly List<ModelRuleResult> _results;
        private readonly bool _allowUIUpdateModel;
        private readonly IBindableModel _model;
        private readonly string _modelName;

        public ModelUI ModelUI { get; private set; }

        public ModelRule(IBindableModel model, bool allowUIUpdateModel = true, string modelName = null)
        {
            _allowUIUpdateModel = allowUIUpdateModel;
            _results = (List<ModelRuleResult>)CachePool.GetCache(InternalType.List_ModelRuleResult);
            _model = model;
            _modelName = string.IsNullOrEmpty(modelName) ? model.TypeInfo.typeName : modelName;
        }

        public bool Check(string rule, UIType type, Component ui)
        {
            if (type != UIType.None)
            {
                BindableTypeInfo typeInfo = _model.TypeInfo;
                for (int i = 0; i < typeInfo.propertyCount; i++)
                {
                    BindablePropertyInfo property = typeInfo.GetProperty(i);
                    if (IsMatch(rule, property.propertyName))
                    {
                        _results.Add(new ModelRuleResult(type, property.bindType, ui, property.propertyName, property.typeFullName));
                        return true;
                    }
                }
            }
            return false;
        }

        public void OnComplete()
        {
            if (_results.Count > 0)
                ModelUI = ModelUIBuilder.Build(_modelName, _model, _results, _allowUIUpdateModel);
            _results.Clear();
            CachePool.AddCache(InternalType.List_ModelRuleResult, _results);
        }

        //正则表述：@$"({modelName}_){{0,1}}{propertyName}_(Slider|Txt|Text|Input|Dropdown|Toggle|Btn|Img|Button|Image)"
        private bool IsMatch(string rule, string propertyName)
        {
            int index = rule.IndexOf(propertyName);
            if (index != -1)
            {
                if (index >= 0) return true;
                else return rule.StartsWith(_modelName);
            }
            return false;
        }
    }

    public readonly struct ModelRuleResult
    {
        public readonly UIType type;

        public readonly BindableType bindType;

        public readonly Component component;

        public readonly string propertyName;

        public readonly string propertyTypeFullName;

        public ModelRuleResult(UIType type, BindableType bindType, Component component, string propertyName, string propertyTypeFullName)
        {
            this.type = type;
            this.bindType = bindType;
            this.component = component;
            this.propertyName = propertyName;
            this.propertyTypeFullName = propertyTypeFullName;
        }

        public override bool Equals(object obj)
        {
            return obj is ModelRuleResult result &&
                   type == result.type &&
                   bindType == result.bindType &&
                   component == result.component &&
                   propertyName == result.propertyName &&
                   propertyTypeFullName == result.propertyTypeFullName;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(type, bindType, component, propertyName, propertyTypeFullName);
        }

        public override string ToString()
        {
            return $"ModelRuleResult{{PropertyName={propertyName} BindType={bindType} UIType={type} Component={component.name}}}";
        }
    }
}
