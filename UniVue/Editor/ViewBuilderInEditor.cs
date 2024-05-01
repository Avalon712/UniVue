using UnityEditor;
using UnityEngine;
using UniVue.View.Config;
using UniVue.View.Views;

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
        }

        public static void CreateViewConfig(string fileName,string directory)
        {
            string path = $"{directory}{fileName}.asset";
            BaseView config;
            if(!Contains(path,out config))
            {
                config = ScriptableObject.CreateInstance<BaseView>();
                AssetDatabase.CreateAsset(config, path);
            }
        }

        public static void CreateListViewConfig(string fileName, string directory)
        {
            string path = $"{directory}{fileName}.asset";
            ListView config;
            if (!Contains(path, out config))
            {
                config = ScriptableObject.CreateInstance<ListView>();
                AssetDatabase.CreateAsset(config, path);
            }
        }

        public static void CreateGridViewConfig(string fileName, string directory)
        {
            string path = $"{directory}{fileName}.asset";
            GridView config;
            if (!Contains(path, out config))
            {
                config = ScriptableObject.CreateInstance<GridView>();
                AssetDatabase.CreateAsset(config, path);
            }
        }

        private static bool Contains<T>(string path,out T config) where T : ScriptableObject
        {
            config = AssetDatabase.LoadAssetAtPath<T>(path);
            return config != null;
        }

    }
}
