using System.Collections.Generic;
using UniVue.Utils;
using UniVue.View.Views;

namespace UniVue.View
{
    /// <summary>
    /// 管理所有视图的打开、关闭逻辑
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
        /// 当前视图的总数量
        /// </summary>
        public int ViewCount => _views.Count;

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
            IView view = GetView(viewName);
            return view != null ? (T)view : default;
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
            if (string.IsNullOrEmpty(viewName)) { return null; }
            if (_views.ContainsKey(viewName)) { return _views[viewName]; }
            return null;
        }

        internal void AddView(IView view)
        {
            if (_views.ContainsKey(view.Name))
            {
#if UNITY_EDITOR
                LogUtil.Warning($"当前场景下的视图名称{view.Name}存在重复!");
#endif
                return;
            }

            _views.Add(view.Name, view);

            //如果当前视图的初始状态处于打开状态
            if (view.State && view.Level != ViewLevel.Permanent)
            {
                ListUtil.AddButNoOutOfCapacity(_histories, view.Name);
            }
        }


        #region 视图动作相关
        /// <summary>
        /// 关闭当前视图，跳转打开指定名称的视图
        /// </summary>
        /// <param name="viewName">待打开的视图</param>
        public void Skip(string currentViewName, string viewName)
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
        public void Open(string viewName, bool top = true)
        {
            IView opening = GetView(viewName);
            if (opening == null)
            {
#if UNITY_EDITOR
                LogUtil.Warning($"未找到名称为{viewName}的视图进行打开操作，不存在这个名称的视图！");
#endif
                return;
            }

            //1.检查当前视图是否以及处于打开状态
            //2.检查是否为Permanent级别
            if (opening.State || opening.Level == ViewLevel.Permanent) { return; }

            //3.检验当前是否有一个Modal视图被打开 
            IView opened = GetView(CurrentOpenedView()); //当前被打开的视图
            if (opened != null && opened.Level == ViewLevel.Modal)
            {
#if UNITY_EDITOR
                LogUtil.Warning($"当前有一个模态Modal视图被打开[viewName={opened.Name}],无法对视图viewName={opening.Name}执行打开操作，无法再打开其它视图，除非关闭它");
#endif
                return;
            }

            //4.检查其父视图是否被打开
            IView parent = GetView(opening.Parent);
            if (parent != null && !parent.State)
            {
#if UNITY_EDITOR
                LogUtil.Warning($"当前正在执行打开操作的视图{viewName}的父视图{parent.Name}尚未被打开，只有先打开其父视图才允许其被打开！");
#endif
                return;
            }

            //5. System级别的视图打开逻辑：同级互斥，关闭上一个与当前正在打开的视图同一级的已经打开了的System级的视图
            if (opening.Level == ViewLevel.System)
            {
                for (int i = 0; i < _histories.Count; i++)
                {
                    IView view = GetView(_histories[i]);
                    if (view.State && view.Level == ViewLevel.System && view.Parent == opening.Parent)
                    {
                        Close(view.Name);
                        break; //跳出的原因：同级中永远只有一个System级的视图被打开
                    }
                }
            }

            //6.将其设置为最后一个子物体，保证被打开的视图能被显示，只对根视图有效
            if (top && string.IsNullOrEmpty(opening.Parent))
            {
                opening.ViewObject.transform.SetAsLastSibling();
            }

            //7.播放音效
            _audioEffectCtr?.PlayAudioEffect(viewName);

            //8.设置状态
            IsRouterCtrl = true;

            //9.打开视图
            opening.Open();

            //10.执行回调
            _audioEffectCtr?.AfterOpen(viewName);

            //11.加入历史记录中，只有不为瞬态的视图才加入
            if (opening.Level != ViewLevel.Transient)
            {
                //如果当前历史记录里面已经有过其记录，则先移除再添加
                if (_histories.Contains(viewName)) { _histories.Remove(viewName); }
                ListUtil.AddButNoOutOfCapacity(_histories, viewName);
            }

            //12.设置状态
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
            if (!closing.State) { return; }

            //2.检查是否为Permanent级别
            if (closing.Level == ViewLevel.Permanent)
            {
#if UNITY_EDITOR
                LogUtil.Warning($"不能关闭一个视图级别为{closing.Level}的视图!");
#endif
                return;
            }

            //3.设置状态
            IsRouterCtrl = true;

            //4.关闭视图
            closing.Close();

            //5.执行回调
            _audioEffectCtr?.AfterClose(viewName);

            //6.设置状态
            IsRouterCtrl = false;
        }

        #endregion

        /// <summary>
        /// 获取当前最新被打开的视图
        /// </summary>
        /// <returns>最新被打开的视图名称</returns>
        private string CurrentOpenedView()
        {
            if (_histories.Count > 0)
            {
                for (int i = _histories.Count - 1; i >= 0; i--)
                {
                    if (GetView(_histories[i]).State) { return _histories[i]; }
                }
            }
            return null;
        }

        /// <summary>
        /// 当前最新被关闭的视图
        /// </summary>
        /// <returns>最新被关闭的视图</returns>
        private string CurrentClosedView()
        {
            if (_histories.Count > 0)
            {
                for (int i = _histories.Count - 1; i >= 0; i--)
                {
                    if (!GetView(_histories[i]).State) { return _histories[i]; }
                }
            }
            return null;
        }

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

    }

}
