using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniVue.Rule;
using UniVue.Utils;
using UniVue.View.Views;

namespace UniVue.View
{
    /// <summary>
    /// 管理ViewPanel的行为
    /// </summary>
    public sealed class ViewRouter
    {
        #region 字段
        private Dictionary<string, IView> _views;
        private List<string> _histories; //历史记录
        private IUIAudioEffectController _audioEffectCtr;
        #endregion

        internal ViewRouter() 
        {
            _views = new Dictionary<string, IView>();
            _histories = new List<string>(Vue.Config.MaxHistoryRecord);
        }

        /// <summary>
        /// 指示当前视图的打开关闭行为是否是路由器控制
        /// </summary>
        public bool IsRouterCtrl { get; private set; }

        public IEnumerable<IView> GetAllView()
        {
            foreach (var view in _views.Values)
            {
                yield return view;
            }
        }

        public T GetView<T>(string viewName) where T : IView
        {
            if (string.IsNullOrEmpty(viewName)) { return default; }
            if (_views.ContainsKey(viewName)) { return (T) _views[viewName]; }
            return default;
        }

        public void UnloadView(string viewName)
        {
            if (_views.ContainsKey(viewName))
            {
                IView view = _views[viewName];
                view.OnUnload();
                _views.Remove(viewName);
            }
        }

        public IView GetView(string viewName)
        {
            if(string.IsNullOrEmpty(viewName)) { return null; }
            if (_views.ContainsKey(viewName)) { return _views[viewName]; }
            return null;
        }

        internal void AddView(IView view)
        {
            if (_views.ContainsKey(view.name))
            {
#if UNITY_EDITOR
                LogUtil.Warning($"当前场景下的视图名称{view.name}存在重复!");
#endif
                return;
            }

            _views.Add(view.name, view);

            //如果当前视图的初始状态处于打开状态
            if (view.state && view.level != ViewLevel.Permanent) 
            { 
                ListUtil.AddButNoOutOfCapacity(_histories, view.name);
            }
        }

        
        #region 视图动作相关
        /// <summary>
        /// 关闭当前视图，跳转打开指定名称的视图
        /// </summary>
        /// <param name="viewName">待打开的视图</param>
        public void Skip(string currentViewName,string viewName)
        {
            Close(currentViewName);
            Open(viewName);
        }

        /// <summary>
        /// 关闭当前最新被打开的视图，打开上一个被关闭的视图
        /// </summary>
        public void Return()
        {
            string newestClosedView = CurrentClosedView();
            string newestOpendView = CurrentOpenedView();
            if (newestClosedView != null) { Open(newestClosedView); }
            if (newestOpendView != null) { Close(newestOpendView); }
        }

        /// <summary>
        /// 打开一个指定名称的视图
        /// </summary>
        /// <param name="viewName">视图名称</param>
        /// <param name="top">是否将打开的视图置于同级视图最前方</param>
        public void Open(string viewName,bool top=true)
        {
            IView opening = GetView(viewName);

            if (opening == null) {
#if UNITY_EDITOR
                LogUtil.Warning($"未找到名称为{viewName}的视图进行打开操作，不存在这个名称的视图！");
#endif
                return; 
            }

            //1.检查当前视图是否以及处于打开状态
            //2.检查是否为Permanent级别
            if (opening.state || opening.level == ViewLevel.Permanent) { return; }

            //3.检验当前是否有一个ForbidOpenOther的视图被打开 
            IView opened = GetView(CurrentOpenedView()); //当前被打开的视图
            if (opened != null && opened.forbid)
            {
#if UNITY_EDITOR
                LogUtil.Warning($"当前被打开的视图viewName={opened.name}禁止打开viewName={viewName}的视图，无法再打开其它视图，除非关闭它");
#endif
                return;
            }

            //4.检验是否被关联 是则看关联主体是否被打开->否则拒绝打开
            if (!string.IsNullOrEmpty(opening.master))
            {
                string masterViewName = opening.master;
                if (!GetView(masterViewName).state)
                {
#if UNITY_EDITOR
                    LogUtil.Warning($"名称为{viewName}的视图已被关联到一个{masterViewName}的视图，而它没有被打开，因此无法打开{opening.name}视图!");
#endif
                    return;
                }
            }

            //5.检查根视图是否被打开（如果存在的话），是则要看根视图是否被打开->否则拒绝打开
            string rootViewName = opening.root;
            if (!string.IsNullOrEmpty(rootViewName) && !GetView(rootViewName).state)
            {
#if UNITY_EDITOR
                LogUtil.Warning($"当前打开的视图与名称为{rootViewName}的视图存在父子关系，而此视图的父视图没有被打开，因此无法打开{viewName}视图!");
#endif
                return;
            }

            //6.System级别视图的打开逻辑
            if (opening.level == ViewLevel.System)
            {
                //6.1如果当前打开的系统级别的视图有根视图，则只能关闭相同根视图的系统级别视图
                if (!string.IsNullOrEmpty(opening.root))
                {
                    for (int i = 0; i < _histories.Count; i++)
                    {
                        IView view = GetView(_histories[i]);
                        if(view.state && view.level == ViewLevel.System && view.root == opening.root)
                        {
                            Close(view.name);
                            break;
                        }
                    }
                }

                //6.2如果当前打开的系统级别的视图没有有根视图，则只能关闭上一个打开的同样没有根视图的系统级别视图
                else
                {
                    for (int i = 0; i < _histories.Count; i++)
                    {
                        IView view = GetView(_histories[i]);
                        if (view.state && view.level == ViewLevel.System && string.IsNullOrEmpty(view.root))
                        {
                            Close(view.name);
                            break;
                        }
                    }
                }
            }
            
            //7.将其设置为最后一个子物体，保证被打开的视图能被显示
            if (top)
            {
                opening.viewObject.transform.SetAsLastSibling();
            }

            //8.播放音效
            _audioEffectCtr?.PlayAudioEffect(viewName);

            //9.设置状态
            IsRouterCtrl = true;

            //10.打开视图
            opening.Open();

            //11.执行回调
            _audioEffectCtr?.AfterOpen(viewName);

            //12.加入历史记录中，只有不为瞬态的视图才加入
            if (opening.level != ViewLevel.Transient)
            {
                //如果当前历史记录里面已经有过其记录，则先移除再添加
                if (_histories.Contains(viewName)) { _histories.Remove(viewName); }
                ListUtil.AddButNoOutOfCapacity(_histories, viewName);
            }

            //13.设置状态
            IsRouterCtrl = false;
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
#if UNITY_EDITOR
                LogUtil.Warning($"未找到名称为{viewName}的视图进行关闭操作，不存在这个名称的视图！");
#endif
                return;
            }

            //1.检查当前视图是否以及处于关闭状态
            //2.检查是否为Permanent级别
            if (!closing.state) { return; }

            //2.视图级别检查
            if(closing.level == ViewLevel.Permanent)
            {
#if UNITY_EDITOR
                LogUtil.Warning($"不能关闭一个视图级别为{closing.level}的视图!");
#endif
                return;
            }

            //3.检查是否被关联
            if (!string.IsNullOrEmpty(closing.master))
            {
                IView master = GetView(closing.master);
                if (!master.state)
                {
#if UNITY_EDITOR
                    LogUtil.Warning($"当前视图{closing.name}已被{master.name}所关联，而{master.name}未被打开，因此无法进行关闭操作");
#endif
                    return;
                }
            }

            //4.当前关闭的视图是否被其它视图关联，是==> 如果该视图是master，则关闭所有关联了它的视图
            if (closing.isMaster)
            {
                using(var view = GetAllView().GetEnumerator())
                {
                    while (view.MoveNext())
                    {
                        if(view.Current.master == closing.name)
                        {
                            Close(view.Current.name);
                        }
                    }
                }
            }

            //5.设置状态
            IsRouterCtrl = true;

            //6.关闭视图
            closing.Close();

            //7.执行回调
            _audioEffectCtr?.AfterClose(viewName);

            //8.设置状态
            IsRouterCtrl = false;
        }

        /// <summary>
        /// 获取当前最新被打开的视图
        /// </summary>
        /// <returns>最新被打开的视图名称</returns>
        public string CurrentOpenedView()
        {
            if (_histories.Count > 0) {
                for (int i = _histories.Count-1; i >= 0; i--)
                {
                    if (GetView(_histories[i]).state) { return _histories[i]; }
                }
            }
            return null;
        }

        /// <summary>
        /// 当前最新被关闭的视图
        /// </summary>
        /// <returns></returns>
        public string CurrentClosedView()
        {
            if (_histories.Count > 0)
            {
                for (int i = _histories.Count - 1; i >= 0; i--)
                {
                    if (!GetView(_histories[i]).state) { return _histories[i]; }
                }
            }
            return null;
        }

        #endregion

        /// <summary>
        /// 注册UI音效控制器
        /// </summary>
        /// <param name="controller">实现IUIAudioEffectController接口类型对象</param>
        public void RegisterUIAudioEffectCtr<T>(T controller) where T : IUIAudioEffectController
        {
            _audioEffectCtr = controller;
        }

        /// <summary>
        /// 清空场景视图
        /// </summary>
        public void UnloadAllViews()
        {
            foreach (IView view in _views.Values)
            {
                view.OnUnload();
            }
            _views.Clear();
            _histories.Clear();
        }

        /// <summary>
        /// 绑定路由事件
        /// </summary>
        internal void BindRouteEvt(string currentViewName,List<CustomTuple<Component, UIType>> uis)
        {
            for (int i = 0; i < uis.Count; i++)
            {
                if (uis[i].Item2 == UIType.Button)
                {
                    Button btn = uis[i].Item1 as Button;
                    string btnName = btn.name;
                    string viewName;
                    if (NamingRuleEngine.CheckRouterEventMatch(btnName, RouterEvent.Close, out viewName))
                    {
                        btn.onClick.AddListener(() => Close(viewName));
                    }
                    else if (NamingRuleEngine.CheckRouterEventMatch(btnName, RouterEvent.Open, out viewName))
                    {
                        btn.onClick.AddListener(() => Open(viewName));
                    }
                    else if (NamingRuleEngine.CheckRouterEventMatch(btnName, RouterEvent.Return, out viewName))
                    {
                        btn.onClick.AddListener(Return);
                    }
                    else if (NamingRuleEngine.CheckRouterEventMatch(btnName, RouterEvent.Skip, out viewName))
                    {
                        btn.onClick.AddListener(() => Skip(currentViewName, viewName));
                    }
                }
                else if (uis[i].Item2 == UIType.Toggle || uis[i].Item2 == UIType.ToggleGroup)
                {
                    Toggle toggle = uis[i].Item1 as Toggle;
                    string toggleName = toggle.name;
                    string viewName;
                    if (NamingRuleEngine.CheckRouterEventMatch(toggleName, RouterEvent.Close, out viewName))
                    {
                        toggle.onValueChanged.AddListener((isOn) => { if (isOn) { Close(viewName); } });
                    }
                    else if (NamingRuleEngine.CheckRouterEventMatch(toggleName, RouterEvent.Open, out viewName))
                    {
                        toggle.onValueChanged.AddListener((isOn) => { if (isOn) {  Open(viewName); } });
                    }
                    else if (NamingRuleEngine.CheckRouterEventMatch(toggleName, RouterEvent.Return, out viewName))
                    {
                        toggle.onValueChanged.AddListener((isOn) => { if (isOn) { Return(); } });
                    }
                    else if (NamingRuleEngine.CheckRouterEventMatch(toggleName, RouterEvent.Skip, out viewName))
                    {
                        toggle.onValueChanged.AddListener((isOn) => { if (isOn) {  Skip(currentViewName, viewName); } });
                    }
                }
            }
        }

    }

}
