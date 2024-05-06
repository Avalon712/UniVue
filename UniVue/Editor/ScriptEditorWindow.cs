using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UniVue.Utils;

namespace UniVue.Editor
{
    /// <summary>
    /// 自动编写脚本窗口
    /// </summary>
    internal sealed class ScriptEditorWindow : EditorWindow
    {
        public enum BindableModelType
        {
            BaseModel,
            ScriptableModel,
            UnityModel,
        }

        private BindableModelType _bindableModelType;

        /// <summary>
        /// 是否自动更新UI
        /// </summary>
        private bool _autoUpdateUI = true;
        /// <summary>
        /// 是否覆盖父类的NotifyAll()方法
        /// </summary>
        private bool _overrideNotifyAll = true;

        /// <summary>
        /// 是否覆盖父类的UpdateModel()方法
        /// </summary>
        private bool _overrideUpdateModel = true;

        /// <summary>
        /// 文件保存目录
        /// </summary>
        private string _saveDirectory = "Scripts/";

        private string _namespace;

        private string _calssName;

        //item1为PropertyName item3为属性类型的全限定性名称,item5为注释内容
        private List<CustomTuple<string, PropertyType,string, AccessSymbol,string, bool>> _properties;

        private Vector2 _scrollPos = Vector2.zero;//滚动条


        [MenuItem("UniVue/ScriptEditor")]
        public static void OpenEditorWindow()
        {
            var window = GetWindow<ScriptEditorWindow>("BaseModel脚本创建编辑器");
            window.position = new Rect(320, 120, 340, 265);
            window.Show();

            window._properties = new List<CustomTuple<string, PropertyType, string,AccessSymbol,string, bool>>();
        }


        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("脚本存放目录(相对Assets下的路径)");
            _saveDirectory = EditorGUILayout.TextField(_saveDirectory);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("类的命名空间");
            _namespace = EditorGUILayout.TextField(_namespace);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("继承的可绑定模型类型");
            _bindableModelType = (BindableModelType)EditorGUILayout.EnumPopup(_bindableModelType);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("类名");
            _calssName = EditorGUILayout.TextField(_calssName);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("自动覆写父类NotifyAll()函数");
            _overrideNotifyAll = EditorGUILayout.Toggle(_overrideNotifyAll);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("自动覆写父类UpdateModel()函数");
            _overrideUpdateModel = EditorGUILayout.Toggle(_overrideUpdateModel);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("属性改变时自动更新UI");
            _autoUpdateUI = EditorGUILayout.Toggle(_autoUpdateUI);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("添加属性"))
            {
                var tuple = new CustomTuple<string, PropertyType, string, AccessSymbol, string, bool>();
                _properties.Add(tuple);
                tuple.Item6 = true;
                if (_properties.Count < 3)
                {
                    position = new Rect(position.x, position.y, position.width, position.height + 145);
                }
               
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            if (_properties.Count > 2)
            {
                _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            }
            for (int i = 0; i < _properties.Count; i++)
            {
                DrawProperty(i);
                EditorGUILayout.Space();
            }
            if (_properties.Count > 2)
            {
                EditorGUILayout.EndScrollView();
            }


            EditorGUILayout.Space();
            if (GUILayout.Button("创建C#脚本"))
            {
                CreatCSharpScript();
            }
            EditorGUILayout.Space();
        }

        private void DrawProperty(int idx)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("属性类型");
            _properties[idx].Item2 = (PropertyType)EditorGUILayout.EnumPopup(_properties[idx].Item2);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("属性名(建议首字母大写)");
            _properties[idx].Item1 = EditorGUILayout.TextField(_properties[idx].Item1);
            EditorGUILayout.EndHorizontal();

            if (_properties[idx].Item2 == PropertyType.Custom)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("自定义类型的属性类型全类名");
                _properties[idx].Item3 = EditorGUILayout.TextField(_properties[idx].Item3);
                EditorGUILayout.EndHorizontal();
            }

            if (_properties[idx].Item2 == PropertyType.Enum)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("枚举属性类型的全类名");
                _properties[idx].Item3 = EditorGUILayout.TextField(_properties[idx].Item3);
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("注释内容");
            _properties[idx].Item5 = EditorGUILayout.TextField(_properties[idx].Item5);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Set方法的可见性");
            _properties[idx].Item4 = (AccessSymbol)EditorGUILayout.EnumPopup(_properties[idx].Item4);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("是否绑定到UI组件上以显示数据");
            if (_properties[idx].Item2 == PropertyType.Custom)
            {
                _properties[idx].Item6 = false;
            }
            _properties[idx].Item6 = EditorGUILayout.Toggle(_properties[idx].Item6);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("属性: " + _properties[idx].Item1);
            if (GUILayout.Button("移除属性"))
            {
                _properties[idx].Dispose();
                _properties.Remove(_properties[idx]);
                if (_properties.Count < 3)
                {
                    position = new Rect(position.x, position.y, position.width, position.height - 145);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("----------------------------------------------------------");
        }

        private void CreatCSharpScript()
        {
            if(string.IsNullOrEmpty(_calssName) || string.IsNullOrEmpty(_saveDirectory) || _properties.Count==0)
            {
                Debug.LogWarning("必须包含类名和脚本存放的目录，以及至少包含一个属性!");
                return;
            }

            //记录需要引入的命名空间
            StringBuilder namespaces = new StringBuilder();
            //类结构信息
            StringBuilder classStructure = new StringBuilder();

            namespaces.AppendLine("using UniVue.Model;");//引入命名空间
             
            string space = !string.IsNullOrEmpty(_namespace) ? "    " : string.Empty;

            if (!string.IsNullOrEmpty(_namespace))
            {
                classStructure.AppendLine("namespace " + _namespace);
                classStructure.AppendLine("{");
            }
           
            classStructure.AppendLine(space+"/*");
            classStructure.AppendLine(space + $"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")} Build By UniVue ScriptEditor");
            classStructure.AppendLine(space + "UniVue 作者: Avalon712");
            classStructure.AppendLine(space + "Github地址: https://github.com/Avalon712/UniVue");
            classStructure.AppendLine(space + "*/\n");
            classStructure.AppendLine(space + "public sealed class " + _calssName + " : "+_bindableModelType.ToString());
            classStructure.AppendLine(space + "{");

            string intervalSpace =space + "    "; //属性首行缩进

            //判断是否需要生成字段信息
            foreach (var property in _properties)
            {
                if (property.Item6 && _autoUpdateUI)
                {
                    classStructure.Append(intervalSpace);
                    classStructure.Append("private ");
                    classStructure.Append(GetTypeStr(property.Item2, property.Item3));
                    classStructure.Append(" _");
                    classStructure.Append(property.Item1.ToLower());
                    classStructure.Append(";\n");
                }
            }

            //生成属性信息
            foreach (var property in _properties)
            {
                classStructure.Append('\n');

                //属性注释
                if (!string.IsNullOrEmpty(property.Item5))
                {
                    classStructure.AppendLine(intervalSpace + "/// <summary>");
                    classStructure.AppendLine(intervalSpace + $"/// {property.Item5}");
                    classStructure.AppendLine(intervalSpace + "/// </summary>");
                }

                classStructure.Append(intervalSpace);
                classStructure.Append("public ");
                classStructure.Append(GetTypeStr(property.Item2, property.Item3));
                classStructure.Append(' ');
                classStructure.Append(property.Item1);

                if (!_autoUpdateUI || (!_autoUpdateUI && property.Item2 == PropertyType.Custom))
                {
                    classStructure.Append(" { get; ");
                    if (property.Item4 != AccessSymbol.Public)
                    {
                        classStructure.Append(property.Item4.ToString().ToLower());
                        classStructure.Append(' ');
                    }
                    classStructure.Append("set; }");
                    classStructure.Append('\n');
                }
                else if(_autoUpdateUI && property.Item2 != PropertyType.Custom)
                {
                    classStructure.Append('\n');
                    classStructure.Append(intervalSpace);
                    classStructure.Append("{\n");
                    classStructure.Append(intervalSpace);
                    classStructure.Append("    ");
                    classStructure.Append("get => _");
                    classStructure.Append(property.Item1.ToLower());
                    classStructure.Append(";\n");
                    classStructure.Append(intervalSpace);
                    classStructure.Append("    ");
                    classStructure.Append("set\n");
                    classStructure.Append(intervalSpace);
                    classStructure.Append("    {\n");
                    classStructure.Append(intervalSpace);
                    classStructure.Append("        ");
                    classStructure.Append("if(_");
                    classStructure.Append(property.Item1.ToLower());
                    classStructure.Append(" != value)\n");
                    classStructure.Append(intervalSpace);
                    classStructure.Append("        {\n");
                    classStructure.Append(intervalSpace);
                    classStructure.Append("            ");
                    classStructure.Append("_");
                    classStructure.Append(property.Item1.ToLower());
                    classStructure.Append(" = ");
                    classStructure.Append("value;\n");
                    classStructure.Append(intervalSpace);
                    classStructure.Append("            ");
                    classStructure.Append("NotifyUIUpdate(nameof(");
                    classStructure.Append(property.Item1);
                    if(property.Item2 == PropertyType.Enum) { classStructure.Append("), (int)value);\n");}
                    else { classStructure.Append("), value);\n"); }
                    classStructure.Append(intervalSpace);
                    classStructure.Append("        }\n");
                    classStructure.Append(intervalSpace);
                    classStructure.Append("    }\n");
                    classStructure.Append(intervalSpace);
                    classStructure.Append("}\n");
                }
            
                if(string.IsNullOrEmpty(property.Item3) && (property.Item2 == PropertyType.Enum || property.Item2 == PropertyType.Custom))
                {
                    string[] strs = property.Item3.Split('.');
                    string n = property.Item3.Replace("." + strs[strs.Length - 1], string.Empty);
                    if(n != _namespace)
                    {
                        namespaces.Append("using ");
                        namespaces.Append(n);
                        namespaces.Append(";\n");
                    }
                }
            }

            if (_overrideNotifyAll)
            {
                classStructure.Append('\n');
                classStructure.Append(intervalSpace);
                classStructure.Append("#region 重写父类NotifyAll()函数\n\n");
                classStructure.Append(intervalSpace);
                classStructure.Append("public override void NotifyAll()\n");
                classStructure.Append(intervalSpace);
                classStructure.Append("{\n");

                bool flag = false;
                foreach (var property in _properties)
                {
                    if (!flag) { flag = property.Item2 == PropertyType.Sprite; }
                    if(property.Item2 != PropertyType.Custom)
                    {
                        classStructure.Append(intervalSpace);
                        classStructure.Append("    ");
                        classStructure.Append("NotifyUIUpdate(nameof(");
                        classStructure.Append(property.Item1);
                        classStructure.Append("), ");
                         //枚举类型进行一次强制转换
                        if(property.Item2 == PropertyType.Enum){
                            classStructure.Append("(int)");
                        }
                        classStructure.Append(property.Item1);
                        classStructure.Append(");\n");
                    }
                }
                if (flag) { namespaces.AppendLine("using UnityEngine;"); }

                classStructure.Append(intervalSpace);
                classStructure.Append("}\n\n");

                classStructure.Append(intervalSpace);
                classStructure.Append("#endregion\n");
            }

            if (_overrideUpdateModel)
            {
                classStructure.Append('\n');
                classStructure.Append(intervalSpace);
                classStructure.Append("#region 重写父类UpdateModel()函数\n\n");
                OverrideUpdateModel(classStructure, intervalSpace, PropertyType.Int);
                OverrideUpdateModel(classStructure, intervalSpace, PropertyType.String);
                OverrideUpdateModel(classStructure, intervalSpace, PropertyType.Float);
                OverrideUpdateModel(classStructure, intervalSpace, PropertyType.Bool);
                classStructure.Append(intervalSpace);
                classStructure.Append("#endregion\n");
            }

            if (!string.IsNullOrEmpty(_namespace))
            {
                classStructure.AppendLine("    }");
            }
            classStructure.AppendLine("}");

            string dir = Application.dataPath+ '/' + _saveDirectory;
            if (!dir.EndsWith('/')) { dir += '/'; }

            if (Directory.Exists(dir))
            {
                string filePath = dir + _calssName + ".cs";
                try
                {
                    File.WriteAllText(filePath, namespaces.ToString() + "\n" + classStructure.ToString());
                    namespaces.Clear();
                    classStructure.Clear();

                    Debug.Log(filePath + " 文件成功创建!");
                    AssetDatabase.Refresh();

                }catch (Exception e)
                {
                    Debug.LogError("存放目录: " + dir + "不存在! 异常原因: "+e);
                }
            }
            else
            {
                Debug.LogError("存放目录: " + dir + "不存在!");
            }
        }

        private void OverrideUpdateModel(StringBuilder classStructure,string intervalSpace, PropertyType type)
        {
            classStructure.Append('\n');
            classStructure.Append(intervalSpace);
            classStructure.Append("public override void UpdateModel(string propertyName, ");
            if(type == PropertyType.Enum) { classStructure.Append("int"); }
            else { classStructure.Append(type.ToString().ToLower()); }
            classStructure.Append(" propertyValue)\n");

            classStructure.Append(intervalSpace);
            classStructure.Append("{\n");


            int l = 0;
            foreach (var property in _properties)
            {
                if (property.Item2 == type || (property.Item2==PropertyType.Enum && type== PropertyType.Int))
                {
                    if(l == 0)
                    {
                        classStructure.Append(intervalSpace);
                        classStructure.Append("    ");
                    }
                    if (l == 0){classStructure.Append("if(nameof("); l++; }
                    else
                    {
                        classStructure.Append(intervalSpace);
                        classStructure.Append("    "); 
                        classStructure.Append("else if(nameof(");
                    }

                    classStructure.Append(property.Item1);
                    classStructure.Append(").Equals(propertyName)){ ");
                    classStructure.Append(property.Item1);
                    if (property.Item2 == PropertyType.Enum && type == PropertyType.Int)
                    {
                        classStructure.Append(" = (");
                        classStructure.Append(GetTypeStr(property.Item2, property.Item3));
                        classStructure.Append(") propertyValue; }\n");
                    }
                    else { classStructure.Append(" = propertyValue; }\n"); }
                }
            }

            classStructure.Append(intervalSpace);
            classStructure.Append("}\n");
        }

        private string GetTypeStr(PropertyType propertyType,string item3)
        {
            switch (propertyType)
            {
                case PropertyType.Float:return "float";
                case PropertyType.Int: return "int";
                case PropertyType.String:return "string";
                case PropertyType.Enum:
                    {
                        string[] strs = item3.Split('.');
                        return strs[strs.Length - 1];
                    }
                case PropertyType.Custom:
                    {
                        string[] strs = item3.Split('.');
                        return strs[strs.Length - 1];
                    }
                case PropertyType.Bool:return "bool";
                case PropertyType.Sprite: return "Sprite";
            }

            return null;
        }
    }

    public enum PropertyType
    {
        Float, 
        Int,
        String,
        Enum,
        /// <summary>
        /// 自定义类型
        /// </summary>
        Custom,
        Bool,
        Sprite
    }

    public enum AccessSymbol
    {
        Public,
        Private,
        Internal
    }

}
