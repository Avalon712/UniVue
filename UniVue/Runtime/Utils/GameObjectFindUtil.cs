using System.Collections.Generic;
using UnityEngine;

namespace UniVue.Utils
{
    public sealed class GameObjectFindUtil
    {
        private GameObjectFindUtil() { }

        /// <summary>
        /// 从自身开始查找一个指定名称的GameObject(深度优先)
        /// </summary>
        public static GameObject DepthFind(string name,GameObject self)
        {
            if (string.IsNullOrEmpty(name)) { return null; }

            if (self.name.Equals(name)) { return self; }

            int childNum = self.transform.childCount;
            for (int i = 0; i < childNum; i++)
            {
                GameObject curr = self.transform.GetChild(i).gameObject;
                if (curr.name.Equals(name))
                {
                    return curr;
                }
                else
                {
                    GameObject obj = DepthFind(name,curr);
                    if (obj != null) { return obj; }
                }
            }

            return null;
        }

        /// <summary>
        /// 从自身开始查找一个指定名称的GameObject(广度优先)
        /// </summary>
        public static GameObject BreadthFind(string name,GameObject self)
        {
            if (string.IsNullOrEmpty(name)) { return null; }

            if (self.name.Equals(name)) { return self; }

            Queue<Transform> parents = new Queue<Transform>();
            parents.Enqueue(self.transform);

            while(parents.Count > 0)
            {
                Transform transform = parents.Dequeue();

                for (int i = 0; i < transform.childCount; i++)
                {
                    Transform child = transform.GetChild(i);
                    if (child.name == name)
                    {
                        parents.Clear();
                        return child.gameObject;
                    }
                    else if(child.childCount != 0) //非叶子节点再入队
                    {
                        parents.Enqueue(child);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 从自身开始查找多个指定名称的GameObject(广度优先)，不会进行全部遍历，一旦找到某个名字的GameObject，那么该GameObject下的所有GameObject都不会被进行查找
        /// </summary>
        /// <remarks>请不要查询名字重复的</remarks>
        public static IEnumerable<GameObject> BreadthFind(GameObject self,params string[] names)
        {
            if(names == null || self==null) { yield return null; }
            else
            {
                Queue<Transform> queue = new Queue<Transform>();
                queue.Enqueue(self.transform);

                Dictionary<string, bool> record = new Dictionary<string, bool>(names.Length);
                for (int i = 0; i < names.Length; i++)
                {
                    if (!record.ContainsKey(names[i])) { record.Add(names[i], false); }
#if UNITY_EDITOR
                    else
                    {
                        LogUtil.Warning($"你当前要进行查找的所有GameObject中存在同名现象,[{string.Join(", ",names)}],这将导致无法正确找到所有的GameObject!");
                    }
#endif
                }
                
                while (queue.Count > 0)
                {
                    Transform transform = queue.Dequeue();

                    for (int i = 0; i < transform.childCount; i++)
                    {
                        Transform child = transform.GetChild(i);
                        if (record.ContainsKey(child.name) && !record[child.name])
                        {
                            record[child.name] = true;
                            yield return child.gameObject;
                        }
                        else if (child.childCount != 0) //非叶子节点再入队
                        {
                            queue.Enqueue(child);
                        }
                    }
                }

                record.Clear();
            }
        }
    }
}
