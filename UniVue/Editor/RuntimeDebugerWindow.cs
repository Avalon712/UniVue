using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UniVue.Event;
using UniVue.Model;
using UniVue.View;
using UniVue.ViewModel;

namespace UniVue.Editor
{
    internal sealed class RuntimeDebugerWindow : EditorWindow
    {
        private enum ContentType
        {
            Model,
            Event,
            View
        }

        #region 字段
        private readonly string[] _tabs = new string[3] { "模型绑定", "事件绑定", "路由绑定" };
        private ContentType _contentType;
        private bool _initialize;
        private Dictionary<IBindableModel, List<string>> _models;
        private Dictionary<string, List<ModelUI>> _bundles;
        private IBindableModel _currentModel; //当前显示的模型
        private Dictionary<IBindableModel, List<PropertyValue>> _modelValues;
        private Dictionary<PropertyValue, bool> _showPropertyValues; //点击展开属性值的详情 
        private Dictionary<string, bool> _showPropertyUIs; //点击展开属性UI详情, key=viewName
        private List<IEventRegister> _registers;
        private Dictionary<string, List<EventUI>> _events; //key=viewName
        private Dictionary<string, List<EventCall>> _calls;
        private IEventRegister _currentRegister; //当前显示的事件注册器
        private Dictionary<EventCall, bool> _showCallInfo; //点击展开EventCall的详情
        private Dictionary<Argument[], bool> _showArgumentsInfo; //点击折叠方法参数信息
        private Dictionary<EventUI, bool> _showUIEventInfo; //点击折叠UIEvent信息
        private Dictionary<string, bool> _showUIEventViewsInfo; //点击折叠当前视图下所有的UIEvent
        private Dictionary<ArgumentUI[], bool> _showUIEventArgInfo; //点击折叠UIEventArg详情
        private Dictionary<ArgumentUI, object> _tempValues;
        private Dictionary<string, IView> _views; //所有的视图
        private List<string> _historyStack; //视图堆栈
        private string _currentView;
        private Dictionary<string, string> _viewTree; //视图树
        private TreeNode _root; //视图树的根节点
        private Dictionary<TreeNode, bool> _showNode; //视图树的折叠情况
        private List<RouteUI> _routeUIs; //当前视图下的所有路由UI
        #endregion

        [MenuItem("UniVue/RuntimeDebuger")]
        public static void OpenEditorWindow()
        {
            var window = GetWindow<RuntimeDebugerWindow>("UniVue运行时调试器");
            window.position = new Rect(100, 100, 910, 460);
            window.ResetState();
        }

        private void OnGUI()
        {
            DrawTopStateMenu();

            if (!Application.isPlaying)
            {
                ResetState();
                EditorGUILayout.LabelField("Vue未初始化");
                return;
            }

            if (!_initialize)
            {
                Initialize();
            }

            switch (_contentType)
            {
                case ContentType.Model:
                    DrawModelBindView();
                    break;
                case ContentType.Event:
                    DrawEventBindView();
                    break;
                case ContentType.View:
                    DrawRouteBindView();
                    break;
            }
        }

        private void OnDestroy()
        {
            ResetState();
        }

        #region 初始化

        private void Initialize()
        {
            _initialize = true;
            VMTable table = ReflectionUtils.GetPropertyValue<VMTable>(Vue.Updater, "Table");
            _models = ReflectionUtils.GetFieldValue<Dictionary<IBindableModel, List<string>>>(table, "_models");
            _bundles = ReflectionUtils.GetFieldValue<Dictionary<string, List<ModelUI>>>(table, "_views");
            _modelValues = new Dictionary<IBindableModel, List<PropertyValue>>();
            _registers = Vue.Event.GetAllRegisters();
            _calls = ReflectionUtils.GetFieldValue<Dictionary<string, List<EventCall>>>(Vue.Event, "_table");
            List<EventUI> events = ReflectionUtils.GetFieldValue<List<EventUI>>(Vue.Event, "_events");
            _showPropertyValues = new Dictionary<PropertyValue, bool>();
            _showPropertyUIs = new Dictionary<string, bool>();
            _showCallInfo = new Dictionary<EventCall, bool>();
            _showArgumentsInfo = new Dictionary<Argument[], bool>();
            _showUIEventInfo = new Dictionary<EventUI, bool>();
            _events = new Dictionary<string, List<EventUI>>();
            _showUIEventViewsInfo = new Dictionary<string, bool>();
            _showUIEventArgInfo = new Dictionary<ArgumentUI[], bool>();
            _tempValues = new Dictionary<ArgumentUI, object>();
            _views = ReflectionUtils.GetFieldValue<Dictionary<string, IView>>(Vue.Router, "_views");
            _historyStack = ReflectionUtils.GetFieldValue<List<string>>(Vue.Router, "_histories");
            _viewTree = ReflectionUtils.GetFieldValue<Dictionary<string, string>>(Vue.Router, "_viewTree");
            _routeUIs = new List<RouteUI>();
            if (_views.Count > 0)
            {
                _currentView = _views.ElementAt(0).Key;
                _routeUIs.Clear();
                Vue.Router.GetAllRouteUI(_currentView, _routeUIs);
            }
            for (int i = 0; i < events.Count; i++)
            {
                ArgumentUI[] arguments = events[i].Arguments;
                _showUIEventViewsInfo.TryAdd(events[i].ViewName, false);
                if (arguments != null)
                {
                    _showUIEventArgInfo.Add(arguments, false);
                    for (int j = 0; j < arguments.Length; j++)
                    {
                        _tempValues.Add(arguments[j], arguments[j].GetRawValue());
                    }
                }
                _showUIEventInfo.Add(events[i], false);
                if (_events.TryGetValue(events[i].ViewName, out List<EventUI> ets))
                {
                    ets.Add(events[i]);
                }
                else
                {
                    ets = new List<EventUI>() { events[i] };
                    _events.Add(events[i].ViewName, ets);
                }
            }
            if (_registers.Count > 0)
            {
                _currentRegister = _registers[0];
                List<EventCall> calls = _calls[_currentRegister.GetType().FullName];
                for (int i = 0; i < calls.Count; i++)
                {
                    _showCallInfo.Add(calls[i], false);
                    if (calls[i].Arguments != null)
                        _showArgumentsInfo.Add(calls[i].Arguments, false);
                }
            }
            foreach (var model in _models.Keys)
            {
                if (_currentModel == null)
                {
                    _currentModel = model;
                    List<string> views = _models[model];
                    for (int i = 0; i < views.Count; i++)
                    {
                        _showPropertyUIs.Add(views[i], false);
                    }
                }
                BindableTypeInfo typeInfo = model.TypeInfo;
                List<PropertyValue> values = new List<PropertyValue>(typeInfo.propertyCount);
                _modelValues.Add(model, values);
                for (int i = 0; i < typeInfo.propertyCount; i++)
                {
                    PropertyValue propertyValue = new PropertyValue(model, typeInfo.GetProperty(i));
                    values.Add(propertyValue);
                    _showPropertyValues.Add(propertyValue, false);
                }
            }
            //构建视图树
            _showNode = new Dictionary<TreeNode, bool>();
            _root = new TreeNode(string.Empty);
            RebuildViewTree(_root);
        }

        private void ResetState()
        {
            if (_initialize)
            {
                _initialize = false;
                _showNode?.Clear();
                _contentType = ContentType.Model;
                _modelValues?.Clear();
                _tempValues?.Clear();
                _showPropertyValues?.Clear();
                _showPropertyUIs?.Clear();
                _showArgumentsInfo?.Clear();
                _showCallInfo?.Clear();
                _showUIEventArgInfo?.Clear();
                _showUIEventInfo?.Clear();
                _tempValues = null;
                _showUIEventInfo = null;
                _showNode = null;
                _showUIEventArgInfo = null;
                _showCallInfo = null;
                _showArgumentsInfo = null;
                _showPropertyUIs = null;
                _showPropertyValues = null;
                _modelValues = null;
                _bundles = null;
                _models = null;
                _currentModel = null;
                _root = null;
            }
        }

        #endregion

        #region 绘制顶部状态栏
        private void DrawTopStateMenu()
        {
            EditorGUILayout.BeginVertical();
            _contentType = (ContentType)GUILayout.Toolbar((int)_contentType, _tabs, GUILayout.Height(24));
            UniVueEditorUtils.DrawHorizontalLine(Color.black);
            EditorGUILayout.EndVertical();
        }

        #endregion

        #region 绘制模型绑定视图

        //240 + 330 + 330
        private void DrawModelBindView()
        {
            if (_currentModel == null)
            {
                EditorGUILayout.LabelField("尚未绑定任何模型");
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                ModelView_Left();
                UniVueEditorUtils.DrawVerticalLine(Color.black);
                ModelView_Center();
                UniVueEditorUtils.DrawVerticalLine(Color.black);
                ModelView_Right();
                EditorGUILayout.EndHorizontal();
            }
        }

        //240
        private Vector2 _modelScrollViewPos_Left;
        private void ModelView_Left()
        {
            EditorGUILayout.BeginVertical(GUILayout.MinWidth(240));

            GUILayout.Label("所有绑定的模型", "BoldLabel", GUILayout.MinWidth(240));
            UniVueEditorUtils.DrawHorizontalLine(Color.black);

            _modelScrollViewPos_Left = EditorGUILayout.BeginScrollView(_modelScrollViewPos_Left);

            foreach (var model in _models.Keys)
            {
                Rect rect = EditorGUILayout.BeginHorizontal(model == _currentModel ? "MeTransitionSelectHead" : "ProjectBrowserHeaderBgTop", GUILayout.Height(21));
                GUILayout.Label(model.TypeInfo.typeName + " - " + model.GetHashCode().ToString("X"));
                EditorGUILayout.EndHorizontal();

                if (UniVueEditorUtils.Clicked(rect) && model != _currentModel)
                {
                    _currentModel = model;
                    UnityEngine.Event.current.Use();
                    //更新视图详情展示
                    _showPropertyUIs.Clear();
                    List<string> views = _models[model];
                    for (int i = 0; i < views.Count; i++)
                    {
                        _showPropertyUIs.Add(views[i], false);
                    }
                }
                UniVueEditorUtils.DrawHorizontalLine(Color.black);
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndVertical();
        }

        //330
        private Vector2 _modelScrollViewPos_Center;
        private void ModelView_Center()
        {
            if (_currentModel == null) return;
            BindableTypeInfo typeInfo = _currentModel.TypeInfo;

            EditorGUILayout.BeginVertical(GUILayout.MinWidth(330));

            GUILayout.Label("模型类型信息", "BoldLabel");
            UniVueEditorUtils.DrawHorizontalLine(Color.black);
            GUILayout.Label($"TypeName:  {typeInfo.typeName}");
            GUILayout.Label($"TypeFullName:  {typeInfo.typeFullName}");
            UniVueEditorUtils.DrawHorizontalLine(Color.black);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("模型绑定的属性信息", "BoldLabel");
            GUILayout.Label("(点击可以展开/折叠)", new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Italic, // 将字体样式设置为斜体  
                fontSize = 10, // 设置字体大小  
                alignment = TextAnchor.LowerCenter // 设置文本对齐方式  
            });
            if (GUILayout.Button(UniVueEditorUtils.RefreshIcon))
            {
                foreach (var vs in _modelValues.Values)
                    for (int i = 0; i < vs.Count; i++)
                        vs[i].RefreshValue();
            }
            EditorGUILayout.EndHorizontal();

            UniVueEditorUtils.DrawHorizontalLine(Color.black);

            _modelScrollViewPos_Center = EditorGUILayout.BeginScrollView(_modelScrollViewPos_Center, GUILayout.MinWidth(330));

            List<PropertyValue> values = _modelValues[_currentModel];
            for (int i = 0; i < values.Count; i++)
            {
                PropertyValue propertyValue = values[i];
                BindablePropertyInfo propertyInfo = propertyValue.propertyInfo;

                //if (i > 0) { UniVueEditorUtils.DrawHorizontalLine(Color.black); }

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                if (GUILayout.Button(propertyInfo.propertyName))
                {
                    _showPropertyValues[propertyValue] = !_showPropertyValues[propertyValue];
                }
                if (_showPropertyValues[propertyValue])
                {
                    GUILayout.Label($"Name:  {propertyInfo.propertyName}");
                    GUILayout.Label($"Type:  {propertyInfo.typeFullName.Replace("System.Collections.Generic.", "")}");
                    GUILayout.Label($"BindType:  {propertyInfo.bindType}");
                    UniVueEditorUtils.DrawPropertyValue(propertyValue);
                }
                EditorGUILayout.EndVertical();
                //UniVueEditorUtils.DrawHorizontalLine(Color.black);
                EditorGUILayout.Space(5);
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndVertical();
        }

        //330
        private Vector2 _modelScrollViewPos_Right;
        private void ModelView_Right()
        {
            if (_currentModel == null) return;
            EditorGUILayout.BeginVertical(GUILayout.MinWidth(330));

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("模型绑定的所有视图", "BoldLabel");
            GUILayout.Label("(点击可以展开/折叠)", new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Italic, // 将字体样式设置为斜体  
                fontSize = 10, // 设置字体大小  
                alignment = TextAnchor.UpperCenter // 设置文本对齐方式  
            });
            EditorGUILayout.EndHorizontal();

            UniVueEditorUtils.DrawHorizontalLine(Color.black);

            _modelScrollViewPos_Right = EditorGUILayout.BeginScrollView(_modelScrollViewPos_Right);
            //EditorUtils.DrawHorizontalLine(Color.black);
            List<string> views = _models[_currentModel];
            for (int i = 0; i < views.Count; i++)
            {
                string viewName = views[i];

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                if (GUILayout.Button(viewName, "LargeButtonMid"))
                {
                    _showPropertyUIs[viewName] = !_showPropertyUIs[viewName];
                }

                if (_showPropertyUIs[viewName])
                {
                    List<ModelUI> bundles = _bundles[viewName];
                    for (int j = 0; j < bundles.Count; j++)
                    {
                        if (bundles[j].Model != _currentModel) continue;

                        List<PropertyUI> propertyUIs = bundles[j].ProertyUIs;
                        for (int k = 0; k < propertyUIs.Count; k++)
                        {
                            PropertyUI propertyUI = propertyUIs[k];
                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                            EditorGUILayout.LabelField($"{propertyUI.PropertyName} - {propertyUI.GetType().Name}");
                            using (var it = propertyUI.GetUI<Component>().GetEnumerator())
                            {
                                while (it.MoveNext())
                                {
                                    GUILayout.BeginHorizontal();
                                    EditorGUILayout.ObjectField(it.Current, it.Current.GetType(), true, GUILayout.Width(200));
                                    GUILayout.Space(5);
                                    object value = UniVueEditorUtils.GetUIValue(it.Current);
                                    if (value is Sprite sprite)
                                        EditorGUILayout.ObjectField(sprite, sprite.GetType(), true);
                                    else
                                        GUILayout.Label(value as string);
                                    GUILayout.EndHorizontal();
                                }
                            }
                            EditorGUILayout.EndVertical();
                        }
                    }
                }

                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        #endregion

        #region 绘制事件绑定视图

        //240 + 330 + 330
        private void DrawEventBindView()
        {
            if (_currentRegister == null)
            {
                EditorGUILayout.LabelField("尚未注册任何事件注册器");
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                EventView_Left();
                UniVueEditorUtils.DrawVerticalLine(Color.black);
                EventView_Center();
                UniVueEditorUtils.DrawVerticalLine(Color.black);
                EventView_Right();
                EditorGUILayout.EndHorizontal();
            }
        }

        //240
        private Vector2 _eventScrollViewPos_Left;
        private void EventView_Left()
        {
            EditorGUILayout.BeginVertical(GUILayout.MinWidth(240));

            GUILayout.Label("所有事件注册器", "BoldLabel", GUILayout.MinWidth(240));
            UniVueEditorUtils.DrawHorizontalLine(Color.black);

            _eventScrollViewPos_Left = EditorGUILayout.BeginScrollView(_eventScrollViewPos_Left);

            foreach (var register in _registers)
            {
                Rect rect = EditorGUILayout.BeginHorizontal(register == _currentRegister ? "MeTransitionSelectHead" : "ProjectBrowserHeaderBgTop", GUILayout.Height(21));
                GUILayout.Label(register.GetType().Name + " - " + register.GetHashCode().ToString("X").Substring(0, 4));
                EditorGUILayout.EndHorizontal();

                if (UniVueEditorUtils.Clicked(rect) && register != _currentRegister)
                {
                    _currentRegister = register;
                    UnityEngine.Event.current.Use();
                    //重置
                    _showCallInfo.Clear();
                    _showArgumentsInfo.Clear();
                    List<EventCall> calls = _calls[register.GetType().FullName];
                    for (int i = 0; i < calls.Count; i++)
                    {
                        _showCallInfo.Add(calls[i], false);
                        if (calls[i].Arguments != null)
                            _showArgumentsInfo.Add(calls[i].Arguments, false);
                    }
                }
                UniVueEditorUtils.DrawHorizontalLine(Color.black);
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        //330
        private Vector2 _eventScrollViewPos_Center;
        private void EventView_Center()
        {
            Type type = _currentRegister.GetType();
            string typeId = type.FullName;

            EditorGUILayout.BeginVertical(GUILayout.MinWidth(330));

            GUILayout.Label("注册器详细信息", "BoldLabel");
            UniVueEditorUtils.DrawHorizontalLine(Color.black);
            GUILayout.Label($"TypeFullName:  {typeId}");
            UniVueEditorUtils.DrawHorizontalLine(Color.black);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("注册器注册的所有事件", "BoldLabel");
            GUILayout.Label("(点击可以展开/折叠)", new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Italic, // 将字体样式设置为斜体  
                fontSize = 10, // 设置字体大小  
                alignment = TextAnchor.UpperCenter // 设置文本对齐方式  
            });
            EditorGUILayout.EndHorizontal();
            UniVueEditorUtils.DrawHorizontalLine(Color.black);

            _eventScrollViewPos_Center = EditorGUILayout.BeginScrollView(_eventScrollViewPos_Center, GUILayout.MinWidth(330));
            List<EventCall> calls = _calls[typeId];
            for (int i = 0; i < calls.Count; i++)
            {
                EventCall call = calls[i];

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                if (GUILayout.Button(call.EventName))
                {
                    _showCallInfo[call] = !_showCallInfo[call];
                }
                if (_showCallInfo[call])
                {
                    GUILayout.Label($"触发范围:  {(call.Views == null ? "所有视图" : "[" + string.Join(", ", call.Views) + "]")}");
                    GUILayout.Label($"MethodName:  {call.MethodName}");
                    if (call.Arguments != null)
                    {
                        _showArgumentsInfo[call.Arguments] = EditorGUILayout.Foldout(_showArgumentsInfo[call.Arguments], $"Arguments:  [count={call.Arguments.Length}]");
                        if (_showArgumentsInfo[call.Arguments])
                        {
                            GUILayout.BeginVertical(EditorStyles.helpBox);
                            for (int j = 0; j < call.Arguments.Length; j++)
                            {
                                UniVueEditorUtils.DrawArgumentValue(call, call.Arguments[j]);
                                EditorGUILayout.Space(3);
                            }
                            GUILayout.EndVertical();
                        }
                    }
                    if (GUILayout.Button("Invoke Method - No Safe Check"))
                    {
                        _currentRegister.Invoke(call);
                    }
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndVertical();
        }

        //330
        private Vector2 _eventScrollViewPos_Right;
        private void EventView_Right()
        {
            EditorGUILayout.BeginVertical(GUILayout.MinWidth(330));
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("所有UI事件UIEvent", "BoldLabel");
            GUILayout.Label("(点击可以展开/折叠)", new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Italic, // 将字体样式设置为斜体  
                fontSize = 10, // 设置字体大小  
                alignment = TextAnchor.UpperCenter // 设置文本对齐方式  
            });
            EditorGUILayout.EndHorizontal();

            UniVueEditorUtils.DrawHorizontalLine(Color.black);

            _eventScrollViewPos_Right = EditorGUILayout.BeginScrollView(_eventScrollViewPos_Right);
            foreach (var viewName in _events.Keys)
            {
                List<EventUI> events = _events[viewName];
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                if (GUILayout.Button(viewName, "LargeButtonMid"))
                {
                    _showUIEventViewsInfo[viewName] = !_showUIEventViewsInfo[viewName];
                }

                if (_showUIEventViewsInfo[viewName])
                {
                    for (int i = 0; i < events.Count; i++)
                    {
                        EventUI @event = events[i];
                        _showUIEventInfo[@event] = EditorGUILayout.Foldout(_showUIEventInfo[@event], @event.EventName + " - " + @event.GetType().Name);
                        if (_showUIEventInfo[@event])
                        {
                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                            EditorGUILayout.LabelField($"UIType: {@event.UIType}");
                            EditorGUILayout.LabelField($"EventName: {@event.EventName}");
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("EventUI: ", GUILayout.Width(50));
                            Component component = @event.GetUI<Component>();
                            EditorGUILayout.ObjectField(component, component.GetType(), true);
                            EditorGUILayout.EndHorizontal();
                            if (@event.Arguments != null)
                            {
                                _showUIEventArgInfo[@event.Arguments] = EditorGUILayout.Foldout(_showUIEventArgInfo[@event.Arguments], $"ArgumentUI: [count={@event.Arguments.Length}]");

                                if (_showUIEventArgInfo[@event.Arguments])
                                {
                                    for (int k = 0; k < @event.Arguments.Length; k++)
                                    {
                                        ArgumentUI eventArg = @event.Arguments[k];
                                        Component comp = eventArg.GetUI<Component>();
                                        object value = _tempValues[eventArg];

                                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                                        EditorGUILayout.LabelField($"ArgumentName: {eventArg.ArgumentName}");
                                        EditorGUILayout.LabelField($"UIType: {eventArg.UIType}");

                                        EditorGUILayout.BeginHorizontal();
                                        EditorGUILayout.LabelField("ArgumentUI: ", GUILayout.Width(80));
                                        EditorGUILayout.ObjectField(comp, comp.GetType(), true);
                                        EditorGUILayout.EndHorizontal();

                                        EditorGUILayout.BeginHorizontal();
                                        EditorGUILayout.LabelField("Value: ", GUILayout.Width(50));
                                        switch (eventArg.UIType)
                                        {
                                            case Common.UIType.Image:
                                                _tempValues[eventArg] = EditorGUILayout.ObjectField((Sprite)value, typeof(Sprite), true, GUILayout.Width(160));
                                                break;
                                            case Common.UIType.TMP_Dropdown:
                                            case Common.UIType.TMP_Text:
                                            case Common.UIType.TMP_InputField:
                                                _tempValues[eventArg] = EditorGUILayout.TextField((string)value, GUILayout.Width(160));
                                                break;
                                            case Common.UIType.ToggleGroup:
                                            case Common.UIType.Toggle:
                                                _tempValues[eventArg] = EditorGUILayout.Toggle((bool)value, GUILayout.Width(160));
                                                break;
                                            case Common.UIType.Slider:
                                                _tempValues[eventArg] = EditorGUILayout.FloatField((float)value, GUILayout.Width(160));
                                                break;
                                        }
                                        if (GUILayout.Button("Set Value"))
                                        {
                                            eventArg.SetRawValue(_tempValues[eventArg]);
                                        }
                                        EditorGUILayout.EndHorizontal();
                                        EditorGUILayout.EndVertical();
                                        EditorGUILayout.Space(3);
                                    }
                                }
                            }
                            if (GUILayout.Button("Execute"))
                            {
                                @event.Execute();
                            }
                            EditorGUILayout.EndVertical();
                        }
                    }
                }

                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        #endregion

        #region 绘制路由绑定视图
        //180 + 220 + 200 + 300
        private void DrawRouteBindView()
        {
            if (_views.Count <= 0)
            {
                EditorGUILayout.LabelField("尚未注册任何视图");
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                RouteView_0();
                UniVueEditorUtils.DrawVerticalLine(Color.black);
                RouteView_1();
                UniVueEditorUtils.DrawVerticalLine(Color.black);
                RouteView_2();
                UniVueEditorUtils.DrawVerticalLine(Color.black);
                RouteView_3();
                EditorGUILayout.EndHorizontal();
            }
        }

        //180
        private Vector2 _routeScrollViewPos0;
        private void RouteView_0()
        {
            EditorGUILayout.BeginVertical(GUILayout.MinWidth(180));

            GUILayout.Label("所有视图", "BoldLabel", GUILayout.MinWidth(180));
            UniVueEditorUtils.DrawHorizontalLine(Color.black);

            _routeScrollViewPos0 = EditorGUILayout.BeginScrollView(_routeScrollViewPos0);

            foreach (var viewName in _views.Keys)
            {
                Rect rect = EditorGUILayout.BeginHorizontal(viewName == _currentView ? "MeTransitionSelectHead" : "ProjectBrowserHeaderBgTop", GUILayout.Height(19));
                GUILayout.Label(viewName);
                EditorGUILayout.EndHorizontal();

                if (UniVueEditorUtils.Clicked(rect) && viewName != _currentView)
                {
                    _currentView = viewName;
                    _routeUIs.Clear();
                    Vue.Router.GetAllRouteUI(viewName, _routeUIs);
                    UnityEngine.Event.current.Use();
                }
                UniVueEditorUtils.DrawHorizontalLine(Color.black);
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndVertical();
        }

        //220
        private Vector2 _routeScrollViewPos1;
        private void RouteView_1()
        {
            IView view = _views[_currentView];
            IView parent = view.GetParent();
            EditorGUILayout.BeginVertical(GUILayout.MinWidth(220));

            GUILayout.Label("视图详细信息", "BoldLabel");
            UniVueEditorUtils.DrawHorizontalLine(Color.black);

            EditorGUILayout.LabelField($"Name:  {view.Name}");
            EditorGUILayout.LabelField($"Level:  {view.Level}");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("ViewObject: ", GUILayout.Width(80));
            EditorGUILayout.ObjectField(view.GetViewObject(), typeof(GameObject), true, GUILayout.Width(120));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField($"State:  {(view.state ? "Opend" : "Closed")}");
            if (parent != null)
            {
                if (GUILayout.Button("查看父视图信息 - " + parent.Name))
                {
                    _currentView = parent.Name;
                    _routeUIs.Clear();
                    Vue.Router.GetAllRouteUI(_currentView, _routeUIs);
                    Repaint();
                }
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Close"))
            {
                Vue.Router.Close(view.Name);
            }
            if (GUILayout.Button("Open"))
            {
                Vue.Router.Open(view.Name);
            }
            EditorGUILayout.EndHorizontal();
            UniVueEditorUtils.DrawHorizontalLine(Color.black);
            GUILayout.Label("当前视图下所有的路由UI", "BoldLabel");
            UniVueEditorUtils.DrawHorizontalLine(Color.black);

            _routeScrollViewPos1 = EditorGUILayout.BeginScrollView(_routeScrollViewPos1, GUILayout.MinWidth(220));

            for (int i = 0; i < _routeUIs.Count; i++)
            {
                RouteUI routeUI = _routeUIs[i];
                string opName = routeUI.RouteEvent.ToString();
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField($"UIType:  {routeUI.UIType}");
                EditorGUILayout.LabelField($"RouteEvent:  {opName}");
                EditorGUILayout.LabelField($"RouteTo:  {routeUI.RouteTo}");
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Route UI: ", GUILayout.Width(80));
                EditorGUILayout.ObjectField(routeUI.GetUI<Component>(), typeof(Component), true, GUILayout.Width(100));
                EditorGUILayout.EndHorizontal();
                if (GUILayout.Button("Route - " + opName))
                {
                    routeUI.Route();
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndVertical();
        }

        //200
        private Vector2 _routeScrollViewPos2;
        private void RouteView_2()
        {
            EditorGUILayout.BeginVertical(GUILayout.MinWidth(200));

            GUILayout.Label($"视图堆栈 - {_historyStack.Count} / {_historyStack.Capacity}", "BoldLabel");

            UniVueEditorUtils.DrawHorizontalLine(Color.black);
            if (_historyStack.Count > 0)
            {
                _routeScrollViewPos2 = EditorGUILayout.BeginScrollView(_routeScrollViewPos2);

                string top = _historyStack[_historyStack.Count - 1];
                GUILayout.Label(top + " - " + (_views[top].state ? "Opend" : "Closed") + "     <color=#FFFF00><i>←</i></color>", new GUIStyle(GUI.skin.label) { richText = true });
                UniVueEditorUtils.DrawHorizontalLine(Color.black);

                for (int i = _historyStack.Count - 2; i >= 0; i--)
                {
                    string viewName = _historyStack[i];
                    Rect rect = EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(viewName + " - " + (_views[viewName].state ? "Opend" : "Closed"));
                    EditorGUILayout.EndHorizontal();
                    UniVueEditorUtils.DrawHorizontalLine(Color.black);
                    EditorGUILayout.Space(3);

                    if (UniVueEditorUtils.Clicked(rect) && _currentView != viewName)
                    {
                        _currentView = viewName;
                        _routeUIs.Clear();
                        Vue.Router.GetAllRouteUI(viewName, _routeUIs);
                        UnityEngine.Event.current.Use();
                    }
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();
        }
        //300
        private Vector2 _routeScrollViewPos3;
        private void RouteView_3()
        {
            EditorGUILayout.BeginVertical(GUILayout.MinWidth(300));

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("视图树", "BoldLabel");
            if (GUILayout.Button("Rebuild"))
            {
                _root = new TreeNode(string.Empty);
                _showNode.Clear();
                RebuildViewTree(_root);
            }
            EditorGUILayout.EndHorizontal();
            UniVueEditorUtils.DrawHorizontalLine(Color.black);

            _routeScrollViewPos3 = EditorGUILayout.BeginScrollView(_routeScrollViewPos3);
            DrawTree(_root);
            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndVertical();
        }

        private void DrawTree(TreeNode node, int level = 0)
        {
            if (level > 0)
                EditorGUI.indentLevel++;

            if (!string.IsNullOrEmpty(node.viewName))
            {
                Color bg = GUI.color;
                if (_currentView == node.viewName)
                {
                    GUI.color = Color.yellow;
                }
                Rect rect = EditorGUILayout.BeginHorizontal();
                string text = $"{node.viewName}  <size=7><i>[{_views[node.viewName].Level}]</i></size>";
                if (node.children.Count > 0)
                {
                    GUIStyle foldout = EditorStyles.foldout;
                    foldout.richText = true;
                    _showNode[node] = EditorGUILayout.Foldout(_showNode[node], text, foldout);
                }
                else
                {
                    GUIStyle label = EditorStyles.label;
                    label.richText = true;
                    EditorGUILayout.LabelField(text);
                }
                EditorGUILayout.EndHorizontal();
                if (UniVueEditorUtils.Clicked(rect) && _currentView != node.viewName)
                {
                    _currentView = node.viewName;
                    _routeUIs.Clear();
                    Vue.Router.GetAllRouteUI(_currentView, _routeUIs);
                    UnityEngine.Event.current.Use();
                    Repaint();
                }
                GUI.color = bg;
            }
            if (level == 0 || _showNode[node])
            {
                List<TreeNode> children = node.children;
                for (int i = 0; i < children.Count; i++)
                {
                    DrawTree(children[i], level + 1);
                }
            }
            if (level > 0)
                EditorGUI.indentLevel--;
        }

        private void RebuildViewTree(TreeNode node)
        {
            //1.找到所有的根视图
            List<string> children = new List<string>();
            foreach (var viewName in _viewTree.Keys)
            {
                if (_viewTree[viewName] == node.viewName)
                {
                    children.Add(viewName);
                }
            }
            for (int i = 0; i < children.Count; i++)
            {
                TreeNode child = new TreeNode(children[i]);
                node.children.Add(child);
                _showNode.Add(child, false);
            }
            for (int i = 0; i < node.children.Count; i++)
            {
                RebuildViewTree(node.children[i]);
            }
        }


        #endregion
    }

    internal sealed class TreeNode
    {
        public string viewName;
        public List<TreeNode> children = new List<TreeNode>();

        public TreeNode(string viewName)
        {
            this.viewName = viewName;
        }
    }

    internal sealed class PropertyValue
    {
        public IBindableModel model;
        public BindablePropertyInfo propertyInfo;
        public object value;

        /// <summary>
        /// 存储一些临时值
        /// </summary>
        public object temp;

        public PropertyValue(IBindableModel model, BindablePropertyInfo bindablePropertyInfo)
        {
            this.model = model;
            this.propertyInfo = bindablePropertyInfo;
            RefreshValue();
        }

        public void RefreshValue()
        {
            this.value = ReflectionUtils.GetPropertyValue<object>(model, propertyInfo.propertyName);
        }

        public void ApplyValue()
        {
            ReflectionUtils.SetPropertyValue(model, propertyInfo.propertyName, value);
        }
    }

}
