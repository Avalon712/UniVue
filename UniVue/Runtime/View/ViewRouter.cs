using System.Collections.Generic;
using UnityEngine;
using UniVue.Common;

namespace UniVue.View
{
    /// <summary>
    /// 管理所有视图的打开、关闭逻辑
    /// </summary>
    public sealed class ViewRouter
    {
        #region 字段
        private readonly Dictionary<string, IView> _views;
        private readonly List<string> _histories; //历史记录
        private readonly Dictionary<string, GameObject> _viewObjects; //所有的ViewOvbject对象, key=viewName
        private readonly Dictionary<string, string> _viewTree; //每个视图的父亲视图, key=viewName value=parentViewName
        private readonly List<RouteUI> _routeUIs; //所有的路由UI
        #endregion

        internal ViewRouter()
        {
            _routeUIs = new List<RouteUI>();
            _views = new Dictionary<string, IView>();
            _histories = new List<string>(Vue.Config.MaxHistoryRecord);
            _viewObjects = new Dictionary<string, GameObject>();
            _viewTree = new Dictionary<string, string>();
        }

        internal Dictionary<string, GameObject> ViewObjects => _viewObjects;

        /// <summary>
        /// 添加路由UI
        /// </summary>
        internal void AddRouteUI(RouteUI routeUI)
        {
            _routeUIs.Add(routeUI);
        }

        /// <summary>
        /// 当前是谁触发的路由事件
        /// </summary>
        internal RouteUI controller { get; set; }

        /// <summary>
        /// 获取当前触发路由事件的UI
        /// </summary>
        /// <remarks>只有在IView.Open()和IView.Close()方法中调用此方法才可能不为null，否则一定为null</remarks>
        /// <returns>
        /// 如果当前是通过UI交互触发的路由事件，此函数的返回值才不会null，如果是通过代码触发的路由事件则返回为null
        /// </returns>
        public RouteUI WhoRouted()
        {
            return controller;
        }

        /// <summary>
        /// 获取指定视图下所有的路由UI
        /// </summary>
        /// <param name="viewName">视图名称</param>
        /// <param name="routeUIs">收集结果</param>
        public void GetAllRouteUI(string viewName, List<RouteUI> routeUIs)
        {
            if (routeUIs != null)
            {
                for (int i = 0; i < _routeUIs.Count; i++)
                {
                    if (_routeUIs[i].ViewName == viewName)
                    {
                        routeUIs.Add(_routeUIs[i]);
                    }
                }
            }
        }

        /// <summary>
        /// 卸载指定视图的资源（不包含它的后代）
        /// </summary>
        internal void UnloadView(string viewName)
        {
            int index = _histories.IndexOf(viewName);
            if (index != -1)
            {
                _histories.RemoveAt(index);
            }
            for (int i = 0; i < _routeUIs.Count; i++)
            {
                if (_routeUIs[i].ViewName == viewName)
                {
                    _routeUIs.RemoveAt(i--);
                }
            }
            if (_viewObjects.ContainsKey(viewName))
            {
                _viewObjects.Remove(viewName);
            }
            if (_views.TryGetValue(viewName, out IView view))
            {
                _views.Remove(viewName);
                view.OnUnload();
            }
            if (_viewTree.ContainsKey(viewName))
            {
                _viewTree.Remove(viewName);
            }
        }

        /// <summary>
        /// 获取一个视图的所有后代视图
        /// </summary>
        /// <param name="viewName">视图名称</param>
        /// <param name="descendants">当前视图的所有后代视图</param>
        public void GetDescendants(string viewName, List<string> descendants)
        {
            if (descendants != null)
            {
                //当前视图的所有子视图也要进行卸载
                foreach (var name in _viewTree.Keys)
                {
                    string parent = _viewTree[name];
                    if (parent == viewName || descendants.Contains(parent))
                    {
                        descendants.Add(name);
                    }
                }
            }
        }

        public T GetView<T>(string viewName) where T : IView
        {
            IView view = GetView(viewName);
            return view != null ? (T)view : default;
        }


        public IView GetView(string viewName)
        {
            if (viewName == null) return null;
            _views.TryGetValue(viewName, out IView view);
            return view;
        }

        /// <summary>
        /// 注册视图
        /// </summary>
        /// <remarks>注册视图时会根据当前视图状态state对ViewObject的状态进行初始化，即GameObject.activeSelf=IView.state</remarks>
        /// <param name="view">视图</param>
        public void AddView(IView view)
        {
            if (_views.ContainsKey(view.Name))
            {
                ThrowUtil.ThrowWarn($"当前场景下的视图名称{view.Name}存在重复!");
                return;
            }

            _views.Add(view.Name, view);

            //如果当前视图的初始状态处于打开状态
            ViewLevel level = view.Level;

            //初始化视图状态
            GameObjectUtil.SetActive(GetViewObject(view.Name), view.state || level == ViewLevel.Permanent);

            if (view.state && (level == ViewLevel.Common || level == ViewLevel.System || level == ViewLevel.Modal))
            {
                PushHistory(view.Name);
            }
        }

        internal GameObject GetViewObject(string viewName)
        {
            ThrowUtil.ThrowExceptionIfTrue(!_viewObjects.TryGetValue(viewName, out GameObject viewObject), "视图尚未加载! 请调用Vue.LoadAllViewObjects()函数加载视图对象!");
            return viewObject;
        }

        /// <summary>
        /// 获取指定视图名称的父视图
        /// </summary>
        /// <param name="viewName">视图名称</param>
        /// <returns>指定视图的父视图</returns>
        public IView GetParent(string viewName)
        {
            if (_views.ContainsKey(viewName))
            {
                string parent = _viewTree[viewName];
                return GetView(parent);
            }
            return null;
        }

        /// <summary>
        /// 重建视图树
        /// </summary>
        /// <remarks>当视图的ViewObject的层级关系发生改变时，调用此函数进行更新视图树</remarks>
        public void RebuildVTree()
        {
            _viewTree.Clear();
            foreach (var viewObject in _viewObjects.Values)
            {
                _viewTree.Add(viewObject.name, GameObjectUtil.FindParentViewObject(viewObject));
            }
        }

        #region 视图动作相关
        /// <summary>
        /// 关闭当前视图，跳转打开指定名称的视图
        /// </summary>
        /// <param name="currentViewName">当前视图</param>
        /// <param name="viewName">待打开的视图</param>
        public void Skip(string currentViewName, string viewName)
        {
            Close(currentViewName);
            Open(viewName);
        }

        /// <summary>
        /// 关闭最近被打开的视图，打开最近被关闭的视图
        /// </summary>
        public void Return()
        {
            IView newestClosedView = GetRecentlyClosedView();
            IView newestOpendView = GetRecentlyOpenedView();
            if (newestClosedView != null) { Open(newestClosedView.Name); }
            if (newestOpendView != null) { Close(newestOpendView.Name); }
        }

        /// <summary>
        /// 打开一个指定名称的视图
        /// </summary>
        /// <param name="viewName">视图名称</param>
        /// <param name="top">是否将打开的视图置于同级视图最前方（只对根视图生效）</param>
        public void Open(string viewName, bool top = false)
        {
            IView opening = GetView(viewName);
            if (opening == null)
            {
                ThrowUtil.ThrowWarn($"未找到名称为{viewName}的视图进行打开操作，不存在这个名称的视图！");
                return;
            }

            //1.非托管类型检查
            if (opening.Level == ViewLevel.Unmanaged)
            {
                opening.Open();
                return;
            }

            //2.检查当前视图是否以及处于打开状态
            //3.检查是否为Permanent级别
            if (opening.state || opening.Level == ViewLevel.Permanent) { return; }

            //4.检验当前是否有一个Modal视图被打开 
            IView opened = GetRecentlyOpenedView(); //当前被打开的视图
            if (opened != null && opened.Level == ViewLevel.Modal)
            {
                ThrowUtil.ThrowWarn($"当前有一个模态Modal视图被打开[viewName={opened.Name}],无法对视图viewName={opening.Name}执行打开操作，无法再打开其它视图，除非关闭它");
                return;
            }

            //5.检查其父视图是否被打开
            IView parent = opening.GetParent();
            if (parent != null && !parent.state)
            {
                ThrowUtil.ThrowWarn($"当前正在执行打开操作的视图{viewName}的父视图{parent.Name}尚未被打开，只有先打开其父视图才允许其被打开！");
                return;
            }

            //6. System级别的视图打开逻辑：同级互斥，关闭上一个与当前正在打开的视图同一级的已经打开了的System级的视图
            if (opening.Level == ViewLevel.System)
            {
                for (int i = 0; i < _histories.Count; i++)
                {
                    IView view = GetView(_histories[i]);
                    if (view.state && view.Level == ViewLevel.System && view.GetParent() == parent)
                    {
                        Close(view.Name);
                        break; //跳出的原因：同级中永远只有一个System级的视图被打开
                    }
                }
            }

            //7.将其设置为最后一个子物体，保证被打开的视图能被显示，只对根视图有效
            if (top && parent == null)
            {
                opening.GetViewObject().transform.SetAsLastSibling();
            }

            //8.打开视图
            opening.Open();

            //9.加入历史记录中，只有不为瞬态的视图才加入
            if (opening.Level != ViewLevel.Transient)
            {
                PushHistory(viewName);
            }

        }

        /// <summary>
        /// 关闭一个指定名称的视图
        /// </summary>
        /// <param name="viewName">视图名称</param>
        public void Close(string viewName)
        {
            IView closing = GetView(viewName);

            if (closing == null)
            {
                ThrowUtil.ThrowWarn($"未找到名称为{viewName}的视图进行关闭操作，不存在这个名称的视图！");
                return;
            }

            //1.非托管类型检查
            if (closing.Level == ViewLevel.Unmanaged)
            {
                closing.Close();
                return;
            }

            //2.检查当前视图是否以及处于关闭状态
            if (!closing.state) { return; }

            //3.检查是否为Permanent级别
            if (closing.Level == ViewLevel.Permanent)
            {
                ThrowUtil.ThrowWarn($"不能关闭一个视图级别为{closing.Level}的视图!");
                return;
            }

            //5.检验当前是否有一个Modal视图被打开 
            IView opened = GetRecentlyOpenedView(); //当前被打开的视图
            if (opened != null && opened.Level == ViewLevel.Modal && opened != closing)
            {
                ThrowUtil.ThrowWarn($"当前有一个模态Modal视图被打开[viewName={opened.Name}],无法对视图viewName={closing.Name}执行关闭操作，无法再对其它视图执行操作，除非关闭它");
                return;
            }

            //6.关闭视图
            closing.Close();
        }

        #endregion

        /// <summary>
        /// 获取最近被打开的视图的名称
        /// </summary>
        /// <returns>最近被打开的视图名称，可能为null</returns>
        public IView GetRecentlyOpenedView()
        {
            if (_histories.Count > 0)
            {
                for (int i = _histories.Count - 1; i >= 0; i--)
                {
                    if (GetView(_histories[i]).state) { return _views[_histories[i]]; }
                }
            }
            return null;
        }

        /// <summary>
        /// 获取最近被关闭的视图的名称
        /// </summary>
        /// <returns>最近被关闭的视图的名称，可能为null</returns>
        public IView GetRecentlyClosedView()
        {
            if (_histories.Count > 0)
            {
                for (int i = _histories.Count - 1; i >= 0; i--)
                {
                    if (!GetView(_histories[i]).state) { return _views[_histories[i]]; }
                }
            }
            return null;
        }

        /// <summary>
        /// 卸载所有资源
        /// </summary>
        internal void UnloadResources()
        {
            foreach (IView view in _views.Values)
            {
                view.OnUnload();
            }
            _views.Clear();
            _histories.Clear();
            _viewObjects.Clear();
            _routeUIs.Clear();
            controller = null;
        }

        /// <summary>
        /// 添加一条视图打开的记录
        /// </summary>
        /// <remarks>需要注意是否会破坏Return路由事件的逻辑</remarks>
        public void PushHistory(string view)
        {
            if (_histories.Contains(view))
            {
                _histories.Remove(view);
            }

            if (_histories.Count == _histories.Capacity)
            {
                _histories.RemoveAt(0);
                _histories.Add(view);
            }

            _histories.Add(view);
        }
    }

}
