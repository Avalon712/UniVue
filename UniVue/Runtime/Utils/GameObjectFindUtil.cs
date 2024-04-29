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
        public static GameObject DepthFindSelf(string name,GameObject self)
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
                    GameObject obj = DepthFindSelf(name,curr);
                    if (obj != null) { return obj; }
                }
            }

            return null;
        }

        /// <summary>
        /// 从自身开始查找一个指定名称的GameObject(广度优先)
        /// </summary>
        public static GameObject BreadthFindSelf(string name,GameObject self)
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

    }
}
