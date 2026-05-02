using System;
using System.Collections.Generic;
using UnityEngine;
using UniVue.UI;

namespace UniVue.Editor
{
    public abstract class UICodeGenRule : IComparable<UICodeGenRule>
    {
        /// <summary>
        /// 值越小越先被调用
        /// </summary>
        public abstract int Order { get; }

        /// <summary>
        /// 内置规则的排序，保证内置规则可以不被外部覆盖
        /// </summary>
        internal virtual int InternalRuleOrder { get; set; }


        public int CompareTo(UICodeGenRule other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (other is null) return 1;
            long order = Order + InternalRuleOrder;
            return order.CompareTo(other.InternalRuleOrder + other.Order);
        }

        /// <summary>
        /// 能否生成代码
        /// </summary>
        /// <param name="prefab">预制体</param>
        /// <param name="baseUI">预制体身上挂载的UI组件</param>
        /// <returns>true-能够自动生成UI代码， false-不能（只要被过滤就不会再被生成）</returns>
        protected abstract bool Filter(GameObject prefab, BaseUI baseUI);

        /// <summary>
        /// 尝试生成属性
        /// </summary>
        /// <param name="clazz">生成UI代码的类</param>
        /// <param name="go">预制体</param>
        /// <param name="properties">生成的属性</param>
        /// <returns>true-生成成功， false-生成失败</returns>
        protected abstract bool TryGenProperties(Type clazz, GameObject go, HashSet<GeneratedProperty> properties);

        internal bool InvokeFilter(GameObject prefab, BaseUI baseUI)
        {
            return Filter(prefab, baseUI);
        }

        internal bool InvokeTryGenProperties(Type clazz, GameObject go, HashSet<GeneratedProperty> properties)
        {
            return TryGenProperties(clazz, go, properties);
        }

        /// <summary>
        /// 访问go的所有后代GameObject（go不会被访问）
        /// </summary>
        /// <param name="go">GameObject</param>
        /// <param name="visitor">
        ///     <para>Func&lt;string, GameObject, bool&gt;</para>
        ///     <para>参数1：从根节点到当前GameObject的路径（Root/../Current）</para>
        ///     <para>参数2：当前被访问的GameObject  </para>
        ///     <para>返回指：true-当前GameObject的后代被继续访问，false-不访问当前GameObject的后代</para>
        /// </param>
        protected void Traverse(GameObject go, Func<string, GameObject, bool> visitor)
        {
            if (visitor == null || !go) return;
            DoTraverse(go.name, go.transform, visitor);
        }

        private void DoTraverse(string parentPath, Transform parent, Func<string, GameObject, bool> visitor)
        {
            foreach (Transform child in parent)
            {
                string childPath = parentPath + "/" + child.name;
                if (visitor.Invoke(childPath, child.gameObject)) DoTraverse(childPath, child, visitor);
            }
        }
    }

    public readonly struct GeneratedProperty : IEquatable<GeneratedProperty>
    {
        public readonly string propertyTypeFullName;
        public readonly string propertyName;
        public readonly string path;

        public GeneratedProperty(string propertyTypeFullName, string propertyName, string path)
        {
            this.propertyTypeFullName = propertyTypeFullName;
            this.propertyName = propertyName;
            this.path = path;
        }

        public bool Equals(GeneratedProperty other)
        {
            return propertyName == other.propertyName;
        }

        public override bool Equals(object obj)
        {
            return obj is GeneratedProperty other && Equals(other);
        }

        public override int GetHashCode()
        {
            return propertyName?.GetHashCode() ?? 0;
        }
    }
}