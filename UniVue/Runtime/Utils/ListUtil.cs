using System.Collections.Generic;
using UniVue.Model;

namespace UniVue.Utils
{
    public sealed class ListUtil
    {
        private ListUtil() { }

        /// <summary>
        /// 将删除元素至于尾端再进行删除
        /// </summary>
        public static void TrailDelete<T>(List<T> list, int delIdx)
        {
            int trialIdx = list.Count - 1;
            if (delIdx == trialIdx)
            {
                list.RemoveAt(delIdx);
            }
            else
            {
                T del = list[delIdx];
                T trail = list[trialIdx];
                list[trialIdx] = del;
                list[delIdx] = trail;
                list.RemoveAt(trialIdx);
            }
        }
  
        /// <summary>
        /// 添加一个元素但是不会使List进行扩容，如果当前Count==Capacity时将会自动删除头元素
        /// 然后再添加元素(会导致数据整体向前移动)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="item"></param>
        public static void AddButNoOutOfCapacity<T>(List<T> list,T item)
        {
            if(list.Count == list.Capacity){
                list.RemoveAt(0);
                list.Add(item);
            }
            else { list.Add(item); }
        }

        /// <summary>
        /// 将target中的数据拷贝到source中，同时删掉source中原有的数据
        /// </summary>
        public static void Copy<T1, T2>(List<T1> source, List<T2> target) where T2 : T1
        {
            if (source.Count > target.Count)
            {
                source.RemoveRange(target.Count - 1, source.Count - target.Count);
            }
            for (int i = 0; i < target.Count; i++)
            {
                source.Insert(i, target[i]);
            }
        }
    }
}
