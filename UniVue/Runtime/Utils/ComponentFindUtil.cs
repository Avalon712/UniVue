using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UniVue.View.Views;

namespace UniVue.Utils
{
    public sealed class ComponentFindUtil
    {
        private ComponentFindUtil() { }

        /// <summary>
        /// 获取一个GameObject下所有的具有特殊命名的UI组件
        /// </summary>
        /// <param name="gameObject">要找到组件的根对象</param>
        /// <param name="view">指定当前的GameObject是否是ViewObject，如果是则需要传递此参数</param>
        /// <param name="exclude">要排除的后代（这些GameObject的后代都不会进行查找）</param>
        /// <returns>List<CustomTuple<Component, UIType>></returns>
        public static List<CustomTuple<Component, UIType>> FindAllSpecialUIComponents(GameObject gameObject,IView view = null, params GameObject[] exclude)
        {
            //广度式搜索
            Queue<Transform> queue = new Queue<Transform>();
            Transform root = gameObject.transform;
            List<CustomTuple<Component, UIType>> results = new();
            queue.Enqueue(root);

            //获取视图的所有嵌套视图
            List<IView> nestedViews = null;
            if(view != null)
            {
                using(var v = view.GetNestedViews().GetEnumerator())
                {
                    while (v.MoveNext() && v.Current != null)
                    {
                        if(nestedViews == null) { nestedViews = new List<IView>(); }
                        nestedViews.Add(v.Current);
                    }
                }
            }

            while (queue.Count > 0)
            {
                Transform transform = queue.Dequeue();

                for (int i = 0; i < transform.childCount; i++)
                {
                    Transform child = transform.GetChild(i);

                    //判断当前是否是要被排除查找的GameObject
                    if (exclude != null)
                    {
                        bool skip = false;
                        for (int j = 0; j < exclude.Length; j++)
                        {
                            if (exclude[j] == child.gameObject) { skip = true; break; }
                        }
                        if(skip) { continue; }
                    }

                    //排除开以字符~开头的GameObject
                    //被'~'标记的GameObject及其后代都不会被进行组件查找
                    if (child.name.StartsWith('~')) { continue; }

                    //检查当前chid是否嵌套视图的viewObject
                    //如果当前chid是viewObject则跳过
                    if (nestedViews != null)
                    {
                        bool flag = false;
                        for (int j = 0; j < nestedViews.Count; j++)
                        {
                            if(child.gameObject == nestedViews[j].viewObject) { flag = true;break; }
                        }
                        if (flag) { continue; }
                    }

                    if (child.childCount != 0) //叶子节点无需再入队
                    {
                        queue.Enqueue(child);
                    }

                    //排除以字符'@'开头的GameObject
                    //被'@'标记的GameObject不会被进行查找，但是其后代会被进行查找
                    if (child.name.StartsWith('@')) { continue; }

                    var result = GetResult(child.gameObject);

                    if (result != null)
                    {
                        results.Add(result);
                    }
                }
            }

            return results;
        }

        #region 设置查询结果
        private static CustomTuple<Component, UIType> GetResult(GameObject gameObject)
        {
            //减少GetComponent()函数的调用
            switch (UITypeUtil.GetUIType(gameObject.name))
            {
                case UIType.Image:
                    {
                        Image image = gameObject.GetComponent<Image>();
                        if (image != null)
                        {
                            CustomTuple<Component, UIType> result = new();
                            result.Item1 = image; 
                            result.Item2 = UIType.Image;
                            return result;
                        }
                        break;
                    }
                case UIType.TMP_Dropdown:
                    {
                        TMP_Dropdown dropdown = gameObject.GetComponent<TMP_Dropdown>();
                        if (dropdown != null) 
                        {
                            CustomTuple<Component, UIType> result = new();
                            result.Item1 = dropdown; 
                            result.Item2 = UIType.TMP_Dropdown;
                            return result;
                        }
                        break;
                    }
                case UIType.TMP_Text:
                    {
                        TMP_Text text = gameObject.GetComponent<TMP_Text>();
                        if (text != null)
                        {
                            CustomTuple<Component, UIType> result = new();
                            result.Item1 = text;
                            result.Item2 = UIType.TMP_Text;
                            return result;
                        }
                        break;
                    }
                case UIType.TMP_InputField:
                    {
                        TMP_InputField input = gameObject.GetComponent<TMP_InputField>();
                        if (input != null)
                        {
                            CustomTuple<Component, UIType> result = new();
                            result.Item1 = input;
                            result.Item2 = UIType.TMP_InputField;
                            return result;
                        }
                        break;
                    }
                case UIType.Button:
                    {
                        Button button = gameObject.GetComponent<Button>();
                        if (button != null) 
                        {
                            CustomTuple<Component, UIType> result = new();
                            result.Item1 = button; 
                            result.Item2 = UIType.Button;
                            return result;
                        }
                        break;
                    }
                case UIType.Toggle:
                    {
                        Toggle toggle = gameObject.GetComponent<Toggle>();
                        if (toggle != null)
                        {
                            CustomTuple<Component, UIType> result = new();
                            result.Item1 = toggle;
                            result.Item2 = toggle.group == null ? UIType.Toggle : UIType.ToggleGroup;
                            return result;
                        }
                        break;
                    }
                case UIType.Slider:
                    {
                        Slider slider = gameObject.GetComponent<Slider>();
                        if (slider != null)
                        {
                            CustomTuple<Component, UIType> result = new();
                            result.Item1 = slider;
                            result.Item2 = UIType.Slider;
                            return result;
                        }
                        break;
                    }

                default: return null;
            }

            return null;
        }
        #endregion

        /// <summary>
        /// 从当前GameObject开始向上查找指定组件，返回第一次挂载此组件的GameObject
        /// </summary>
        /// <typeparam name="T">查找组件</typeparam>
        /// <param name="current"></param>
        /// <returns>首次挂载此组件的GameObject</returns>
        public static T LookUpFindComponent<T>(GameObject current) where T : Component
        {
            if (current == null) { return null; }
            else if (current.GetComponentInParent<T>(true) == null) { return LookUpFindComponent<T>(current.transform.parent.gameObject); }
            else { return current.transform.parent.GetComponent<T>(); }
        }
    
        /// <summary>
        /// 判断当前GameObject或其子孙GameObject身上是否含有包含组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool ContainsComponent<T>(GameObject obj) where T : Component
        {
            if (obj == null) { return false; }

            Transform transform = obj.transform;

            //从自身获取
            T t = transform.GetComponent<T>();
            if (t != null) { return true; }

            for (int i = 0; i < transform.childCount; i++)
            {
                if (ContainsComponent<T>(transform.GetChild(i).gameObject))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary> 
        /// 从自身开始查找一个指定类型的组件 深度搜索
        /// </summary>
        public static T DepthFind<T>(GameObject self) where T : Component
        {
            if(self == null) { return null; }

            T comp = self.GetComponent<T>();
            if (comp != null) { return comp; }

            int childNum = self.transform.childCount;
            for (int i = 0; i < childNum; i++)
            {
                comp = DepthFind<T>(self.transform.GetChild(i).gameObject);
                if (comp != null)
                {
                    return comp;
                }
            }

            return null;
        }

        /// <summary>
        /// 从自身开始查找一个指定类型的组件 广度搜索
        /// </summary>
        public static T BreadthFind<T>(GameObject self) where T : Component
        {
            T comp = self.GetComponent<T>();
            if(comp != null) { return comp; }

            Queue<Transform> parents = new Queue<Transform>();
            parents.Enqueue(self.transform);

            while (parents.Count > 0)
            {
                Transform transform = parents.Dequeue();

                for (int i = 0; i < transform.childCount; i++)
                {
                    Transform child = transform.GetChild(i);
                    comp = child.GetComponent<T>();
                    if (comp != null)
                    {
                        parents.Clear();
                        return comp;
                    }
                    else if (child.childCount > 0) //非叶子节点再入队
                    {
                        parents.Enqueue(child);
                    }
                }
            }

            return null;
        }

        /// <summary> 
        /// 从自身开始查找一个指定名称的GameObject同时获得其身上指定类型的组件 深度搜索
        /// </summary>
        public static T DepthFind<T>(GameObject self,string name) where T : Component
        {
            if (self == null) { return null; }

            if(self.name == name)
            {
                return self.GetComponent<T>();
            }

            int childNum = self.transform.childCount;
            for (int i = 0; i < childNum; i++)
            {
                 T comp = DepthFind<T>(self.transform.GetChild(i).gameObject);
                 if(comp != null && comp.name == name) { return comp; }
            }

            return null;
        }

        /// <summary>
        /// 从自身开始查找一个指定名称的GameObject同时获得其身上指定类型的组件 广度搜索
        /// </summary>
        public static T BreadthFind<T>(GameObject self,string name) where T : Component
        {
            if (self.name == name){ return self.GetComponent<T>(); }

            Queue<Transform> parents = new Queue<Transform>();
            parents.Enqueue(self.transform);

            while (parents.Count > 0)
            {
                Transform transform = parents.Dequeue();

                for (int i = 0; i < transform.childCount; i++)
                {
                    Transform child = transform.GetChild(i);

                    if(child.name == name)
                    {
                        parents.Clear();
                        return child.GetComponent<T>();
                    }
                    else if (child.childCount > 0) //非叶子节点再入队
                    {
                        parents.Enqueue(child);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 获取所有指定类型的UI组件，无条件匹配
        /// </summary>
        public static List<T> GetAllComponents<T>(GameObject gameObject) where T : Component
        {
            List<T> values = new List<T>();
            T comp = gameObject.GetComponent<T>();

            if (comp != null) { values.Add(comp); }

            Queue<Transform> queue = new Queue<Transform>();
            queue.Enqueue(gameObject.transform);

            while (queue.Count > 0)
            {
                Transform transform = queue.Dequeue();

                for (int i = 0; i < transform.childCount; i++)
                {
                    Transform child = transform.GetChild(i);
                    comp = child.GetComponent<T>();
                    if (comp != null)
                    {
                        values.Add(comp);
                    }

                    if (child.childCount > 0) //非叶子节点再入队
                    {
                        queue.Enqueue(child);
                    }
                }
            }

            return values;
        }

    }

    public sealed class CustomTuple<T1, T2>
    {
        public T1 Item1 { get; set; }

        public T2 Item2 { get; set; }

        public CustomTuple() { }

        public CustomTuple(T1 t1,T2 t2)
        {
            Item1 = t1;Item2 = t2;
        }

        public void Dispose()
        {
            Item1 = default;
            Item2 = default;
        }
    }

    public sealed class CustomTuple<T1, T2, T3>
    {
        public T1 Item1 { get; set; }

        public T2 Item2 { get; set; }

        public T3 Item3 { get; set; }

        public void Dispose()
        {
            Item1 = default;
            Item2 = default;
            Item3 = default;
        }
    }

    public sealed class CustomTuple<T1, T2, T3, T4>
    {
        public T1 Item1 { get; set; }

        public T2 Item2 { get; set; }

        public T3 Item3 { get; set; }

        public T4 Item4 { get; set; }

        public void Dispose()
        {
            Item1 = default;
            Item2 = default;
            Item3 = default;
            Item4 = default;
        }
    }

    public sealed class CustomTuple<T1, T2, T3, T4, T5, T6>
    {
        public T1 Item1 { get; set; }

        public T2 Item2 { get; set; }

        public T3 Item3 { get; set; }

        public T4 Item4 { get; set; }

        public T5 Item5 { get; set; }

        public T6 Item6 { get; set; }

        public void Dispose()
        {
            Item1 = default;
            Item2 = default;
            Item3 = default;
            Item4 = default;
            Item5 = default;
            Item6 = default;
        }
    }
}

