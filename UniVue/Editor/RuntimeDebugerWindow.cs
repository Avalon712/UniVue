using UnityEditor;

namespace UniVue.Editor
{
    internal sealed class RuntimeDebugerWindow : EditorWindow
    {
        [MenuItem("UniVue/RuntimeDebuger")]
        public static void OpenEditorWindow()
        {
            var window = GetWindow<RuntimeDebugerWindow>("UniVue运行时调试器");
            window.Show();
        }
    }
}
