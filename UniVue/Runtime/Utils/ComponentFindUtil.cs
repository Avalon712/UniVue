using System.Collections.Generic;
using UnityEngine;

namespace UniVue.Utils
{
    public sealed class ComponentFindUtil
    {
        private ComponentFindUtil() { }

        /// <summary>
        /// 从当前GameObject开始向上查找指定组件，返回第一次挂载此组件的GameObject
        /// </summary>
        /// <typeparam name="T">查找组件</typeparam>
        /// <param name="current"></param>
        /// <returns>首次挂载此组件的GameObject身上的组件</returns>
        public static T LookUpFindComponent<T>(GameObject current) where T : Component
        {
            if (current == null)
                return null;
            else if (current.TryGetComponent(out T t))
                return t;
            else
                return LookUpFindComponent<T>(current.transform.parent?.gameObject);
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
            if (self == null) { return null; }

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
            if (comp != null) { return comp; }

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
        public static T DepthFind<T>(GameObject self, string name) where T : Component
        {
            if (self == null) { return null; }

            if (self.name == name)
            {
                return self.GetComponent<T>();
            }

            int childNum = self.transform.childCount;
            for (int i = 0; i < childNum; i++)
            {
                T comp = DepthFind<T>(self.transform.GetChild(i).gameObject);
                if (comp != null && comp.name == name) { return comp; }
            }

            return null;
        }

        /// <summary>
        /// 从自身开始查找一个指定名称的GameObject同时获得其身上指定类型的组件 广度搜索
        /// </summary>
        public static T BreadthFind<T>(GameObject self, string name) where T : Component
        {
            if (self.name == name) { return self.GetComponent<T>(); }

            Queue<Transform> parents = new Queue<Transform>();
            parents.Enqueue(self.transform);

            while (parents.Count > 0)
            {
                Transform transform = parents.Dequeue();

                for (int i = 0; i < transform.childCount; i++)
                {
                    Transform child = transform.GetChild(i);

                    if (child.name == name)
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
}

