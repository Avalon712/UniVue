using UnityEditor;
using UnityEngine;
using UniVue.View.Config;

namespace UniVue.Editor
{
    public sealed class ViewBuilderInEditor 
    {
        private ViewBuilderInEditor() { }

        public static void CreateSceneConfig(string fileName, string sceneName,string directory)
        {
            string path = $"{directory}{fileName}.asset";
            SceneConfig config;
            if (!Contains(path, out config))
            {
                config = ScriptableObject.CreateInstance<SceneConfig>();
                AssetDatabase.CreateAsset(config, path);
                config.sceneName = sceneName;
            }
            else
            {
                Debug.LogWarning($"路径{directory}下已经存在一个同名{fileName}的资产!");
            }
        }

        public static void CreateCanvasConfig(string fileName, string directory)
        {
            string path = $"{directory}{fileName}.asset";
            CanvasConfig config;
            if (!Contains(path, out config))
            {
                config = ScriptableObject.CreateInstance<CanvasConfig>();
                AssetDatabase.CreateAsset(config, path);
                config.canvasName = fileName;
            }
            else
            {
                Debug.LogWarning($"路径{directory}下已经存在一个同名{fileName}的资产!");
            }
        }

        public static V CreateViewConfig<V>(string fileName, string directory) where V : ScriptableObject
        {
            string path = $"{directory}{fileName}.asset";
            V config;
            if (!Contains(path, out config))
            {
                config = ScriptableObject.CreateInstance<V>();
                return config;
            }
            else
            {
                Debug.LogWarning($"路径{directory}下已经存在一个同名{fileName}的资产!");
            }
            return null;
        }

        private static bool Contains<T>(string path,out T config) where T : ScriptableObject
        {
            config = AssetDatabase.LoadAssetAtPath<T>(path);
            return config != null;
        }

    }
}
