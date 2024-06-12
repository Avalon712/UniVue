using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UniVue.View.Config;

namespace UniVue.Editor
{
    internal sealed class ViewEditorWindow : EditorWindow
    {
        private ConfigType _configType = ConfigType.View;

        private ViewType _viewConfigType;
        private string _configFileName;
        private string _sceneName;

        private string _saveDirectory = "Assets/";

        private bool _attachToRoot;
        private ViewConfig _root;

        [MenuItem("UniVue/ViewEditor")]
        public static void OpenEditorWindow()
        {
            var window = GetWindow<ViewEditorWindow>("视图配置编辑器");
            window.position = new Rect(320, 240, 340, 240);
            window.Show();

            window._sceneName = EditorSceneManager.GetActiveScene().name;
        }


        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("选择创建的配置文件类型");
            _configType = (ConfigType)EditorGUILayout.EnumPopup(_configType);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            if (_configType == ConfigType.View)
            {
                ViewConfigEdit();
            }
            else if(_configType == ConfigType.Canvas)
            {
                CanvasConfigEdit();
            }
            else
            {
                SceneConfigEdit();
            }
           
        }

        private bool CreateSceneConfig()
        {
            if (string.IsNullOrEmpty(_configFileName) || string.IsNullOrWhiteSpace(_configFileName))
            {
                Debug.LogWarning("配置文件的名称不能为空");
                return false;
            }

            ViewBuilderInEditor.CreateSceneConfig(_configFileName, _sceneName, _saveDirectory);

            return true;
        }

        private bool CreateViewConfig()
        {
            if(string.IsNullOrEmpty(_configFileName) || string.IsNullOrWhiteSpace(_configFileName))
            {
                Debug.LogWarning("配置文件的名称不能为空");
                return false;
            }

            ViewConfig view = null;

            switch (_viewConfigType)
            {
                case ViewType.BaseView:
                    view = ViewBuilderInEditor.CreateViewConfig<ViewConfig>(_configFileName, _saveDirectory);
                    break;
                case ViewType.ListView:
                    view = ViewBuilderInEditor.CreateViewConfig<ListViewConfig>(_configFileName, _saveDirectory);
                    break;
                case ViewType.GridView:
                    view = ViewBuilderInEditor.CreateViewConfig<GridViewConfig>(_configFileName, _saveDirectory);
                    break;
                case ViewType.TipView:
                    view = ViewBuilderInEditor.CreateViewConfig<TipViewConfig>(_configFileName, _saveDirectory);
                    break;
                case ViewType.EnsureTipView:
                    view = ViewBuilderInEditor.CreateViewConfig<EnsureTipViewConfig>(_configFileName, _saveDirectory);
                    break;
                case ViewType.ClampListView:
                    view = ViewBuilderInEditor.CreateViewConfig<ClampListViewConfig>(_configFileName, _saveDirectory);
                    break;
            }

            if (_attachToRoot && _root != null && view!=null)
            {
                Undo.RegisterCreatedObjectUndo(view, "create view");
                (view as ScriptableObject).name = _configFileName;
                AssetDatabase.AddObjectToAsset(view, _root);
                AssetDatabase.SaveAssets();
            }
            else if(view != null)
            {
                (view as ScriptableObject).name = _configFileName;
                view.viewName = _configFileName;
                AssetDatabase.CreateAsset(view, $"{_saveDirectory}{_configFileName}.asset");
                AssetDatabase.SaveAssets();
            }

            return view!=null;
        }

        private bool CreateCanvasConfig()
        {
            if (string.IsNullOrEmpty(_configFileName) || string.IsNullOrWhiteSpace(_configFileName))
            {
                Debug.LogWarning("配置文件的名称不能为空");
                return false;
            }

            ViewBuilderInEditor.CreateCanvasConfig(_configFileName, _saveDirectory);

            return true;
        }

        private void SceneConfigEdit()
        {
            if (string.IsNullOrEmpty(_configFileName)) { _configFileName = _sceneName; }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("创建的Scene配置文件的名称");
            _configFileName = EditorGUILayout.TextField(_configFileName);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("场景名称");
            _sceneName = EditorGUILayout.TextField(_sceneName);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("保存目录 (注: 相对路径,必须以Assets开头,必须存在)");
            _saveDirectory = EditorGUILayout.TextField(_saveDirectory);
            EditorGUILayout.Space();

            if (GUILayout.Button("创建场景配置")) { if (CreateSceneConfig()) { AssetDatabase.Refresh(); } }
        }


        private void CanvasConfigEdit()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("创建的Canvas配置文件的名称");
            _configFileName = EditorGUILayout.TextField(_configFileName);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("保存目录 (注: 相对路径,必须以Assets开头,必须存在)");
            _saveDirectory = EditorGUILayout.TextField(_saveDirectory);
            EditorGUILayout.Space();

            if (GUILayout.Button("创建Canvas配置")) { if (CreateCanvasConfig()) { AssetDatabase.Refresh(); } }
        }

        private void ViewConfigEdit()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("选择创建的视图配置文件类型");
            _viewConfigType = (ViewType)EditorGUILayout.EnumPopup(_viewConfigType);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("创建的视图配置文件的名称");
            _configFileName = EditorGUILayout.TextField(_configFileName);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("要创建的视图是否是嵌套视图");
            _attachToRoot = EditorGUILayout.Toggle(_attachToRoot);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            if (_attachToRoot)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("选择此视图的根视图");
                _root = (ViewConfig)EditorGUILayout.ObjectField(_root, typeof(ViewConfig), false);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();
            }

            EditorGUILayout.LabelField("保存目录 (注: 相对路径,必须以Assets开头,必须存在)");
            _saveDirectory = EditorGUILayout.TextField(_saveDirectory);
            EditorGUILayout.Space();

            if (GUILayout.Button("创建视图配置")) { if (CreateViewConfig()) { AssetDatabase.Refresh(); } }
        }
    }

    public enum ConfigType
    {
        Scene,
        Canvas,
        View
    }

    public enum ViewType
    {
        BaseView,
        ListView,
        GridView,
        TipView,
        EnsureTipView,
        ChatView,
        ClampListView,
    }

    internal sealed class ViewBuilderInEditor
    {
        private ViewBuilderInEditor() { }

        public static void CreateSceneConfig(string fileName, string sceneName, string directory)
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

        private static bool Contains<T>(string path, out T config) where T : ScriptableObject
        {
            config = AssetDatabase.LoadAssetAtPath<T>(path);
            return config != null;
        }

    }
}
