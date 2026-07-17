using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
using UnityEngine;
using UniVue.Utils;

namespace UniVue.Editor
{
    public static class EditorCoreUtils
    {

        /// <summary>
        /// 获取项目的根目录
        /// </summary>
        /// <returns></returns>
        public static string GetProjectRootDirectory()
        {
            string dataPath = Application.dataPath;
            return dataPath.Substring(0, dataPath.LastIndexOf("/"));
        }
        
        /// <summary>
        /// 深拷贝一个对象
        /// </summary>
        /// <param name="obj">要拷贝的对象</param>
        /// <returns>拷贝后的对象</returns>
        public static object DeepCopy(object obj)
        {
            if (obj is Object uObj)
                return Object.Instantiate(uObj);

            object copy = null;
            using (MemoryStream ms = new())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Seek(0, SeekOrigin.Begin);
                copy = formatter.Deserialize(ms);
            }

            return copy;
        }
    }
}