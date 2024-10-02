using System.IO;
using UnityEditor;
using UnityEngine;

namespace UniVue.Editor
{
    [InitializeOnLoad]
    internal sealed class UniVueHierachyIconEditor
    {
        private static Texture2D _viewIcon;
        private static Texture2D _ruleIcon;

        private static Texture2D _hierachyColor; //Hierachy背景色

        static UniVueHierachyIconEditor()
        {
            string dir = GetIconDirectory(Application.dataPath);
            if (dir != null)
            {
                dir = dir.Replace('\\', '/').Replace(Application.dataPath, "Assets");
                EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyGUI;
                EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
                // 加载自定义图标资源
                _viewIcon = EditorGUIUtility.Load($"{dir}/UniVue - View.png") as Texture2D;
                _ruleIcon = EditorGUIUtility.Load($"{dir}/UniVue - Rule.png") as Texture2D;
            }
            else
            {
                Debug.LogWarning("未能在Assets目录中找到UniVue默认编辑器资源目录\"UniVue/Editor/Icons\",这将导致无法在Hierachy面板显示UniVue自动的Icon标识");
            }
        }

        private static readonly string[] matchs = new string[3] { "\\UniVue", "\\Editor", "\\Icons" };

        private static string GetIconDirectory(string parent, int index = 0)
        {
            string[] dirs = Directory.GetDirectories(parent);
            for (int i = 0; i < dirs.Length; i++)
            {
                string directory = dirs[i];
                if (directory.EndsWith(matchs[index]))
                {
                    if (index == 2)
                        return directory;
                    else
                    {
                        string p = GetIconDirectory(directory, index + 1);
                        if (p != null) return p;
                    }
                }
                else
                {
                    string p = GetIconDirectory(directory, index);
                    if (p != null) return p;
                }
            }
            return null;
        }

        //rect -> (rect.x, rect.y)这个位置是最左侧的位置
        private static void OnHierarchyGUI(int instanceID, Rect rect)
        {
            GameObject obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (obj != null)
            {
                Rect iconRect = new Rect(rect.x, rect.y, 16, 16);
                if (obj.name.EndsWith("View") || obj.tag == "ViewObject")
                {
                    DrawIcon(iconRect, _viewIcon);
                }
                else if (UniVueEditorUtils.IsRule(obj.name))
                {
                    DrawIcon(iconRect, _ruleIcon);
                }
            }
        }

        private static void DrawIcon(in Rect rect, Texture2D icon)
        {
            if (_hierachyColor == null)
                _hierachyColor = UniVueEditorUtils.CreateUnitTexture(new Color(0.2196f, 0.2196f, 0.2196f, 1));

            GUI.DrawTexture(rect, _hierachyColor); //覆盖原来的默认图标
            GUI.DrawTexture(new Rect(rect.x + 1, rect.y + 1, 14, 14), icon, ScaleMode.ScaleToFit);// 绘制自定义图标
        }
    }
}
