using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniVue.Common;
using UniVue.Extensions;
using UniVue.Model;
using UniVue.View;
using UniVue.ViewModel;

namespace UniVue.Widgets
{
    [DefaultExecutionOrder(int.MinValue)] //保证Vue被先初始化
    public abstract class Loopable : MonoBehaviour
    {
        [SerializeField]
        protected ScrollRect _scrollRect;
        [SerializeField]
        protected ScrollDirection _direction;

        protected Vector3[] _itemCorners;
        protected Vector3[] _viewportCorners;
        private bool _initialized;                    //是否已经初始化
        protected IObservableList _data;              //绑定的数据
        protected int _tail;                          //数据尾指针，指向的是最后一个Item的位置所渲染的数据的索引
        protected int _head;                          //数据头指针，指向的是第一个Item的位置所渲染的数据的索引

        public ScrollRect scrollRect => _scrollRect;

        public IObservableList Data => _data;

        /// <summary>
        /// 可见的Item的最大数量
        /// </summary>
        public abstract int viewCount { get; }

        #region 视图实现
        private sealed class ItemView : IView
        {
            public bool state { get; private set; } = false;

            public ViewLevel Level => ViewLevel.Unmanaged;

            public string Name { get; }

            public ItemView(string name)
            {
                Name = name;
            }

            public void Close()
            {
                if (state)
                {
                    state = false;
                    this.GetViewObject().SetActive(false);
                }
            }

            public void OnUnload() { }

            public void Open()
            {
                if (!state)
                {
                    state = true;
                    this.GetViewObject().SetActive(true);
                }
            }
        }
        #endregion

        private void Start()
        {
            if (!_initialized)
            {
                Initialize();
            }
        }

        private void Initialize()
        {
            _initialized = true;
            //创建视图
            Transform content = _scrollRect.content;
            int childCount = content.childCount;
            for (int i = 0; i < childCount; i++)
            {
                GameObject itemViewObject = content.GetChild(i).gameObject;
                ItemView view = new ItemView(itemViewObject.name);
                Vue.Router.AddView(view);
                GameObjectUtil.SetActive(itemViewObject, false);
                ThrowUtil.ThrowWarnIfTrue(!Vue.IsViewObject(itemViewObject), "LoopList/LoopGrid组件中ScrollRect.content下的每个Item必须是一个ViewObject!");
            }

            //绑定滚动事件
            _itemCorners = new Vector3[4];
            _viewportCorners = new Vector3[4];
            _scrollRect.onValueChanged.AddListener(OnScroll);
        }

        /// <summary>
        /// 判断指定数据索引是否能被看见
        /// </summary>
        /// <param name="index">数据索引</param>
        /// <returns>true:可见</returns>
        public bool IsVisible(int index)
        {
            List<ModelUI> modelUIs = UIQuerier.QueryAllModelUI(_scrollRect.content.GetChild(0).name);
            int startIndex = _data.IndexOf(modelUIs[0].Model);
            return index >= startIndex && index <= startIndex + viewCount;
        }

        /// <summary>
        /// 判断指定的数据是否能被看见
        /// </summary>
        /// <param name="model">模型数据</param>
        /// <returns>true:可见</returns>
        public bool IsVisible(IBindableModel model)
        {
            return IsVisible(_data.IndexOf(model));
        }

        /// <summary>
        /// 绑定数据
        /// </summary>
        /// <remarks>如果已经绑定了数据将会进行重新绑定</remarks>
        public void BindData(IObservableList data)
        {
            //保证初始化 
            if (!_initialized)
            {
                Initialize();
            }
            if (data != null && _data != data)
            {
                if (_data != null)
                    _data.OnChanged -= OnDataChanged;
                _data = data;
                data.OnChanged += OnDataChanged;

                Transform content = _scrollRect.content;
                int childCount = content.childCount;
                ViewRouter router = Vue.Router;
                for (int i = 0; i < childCount; i++)
                {
                    IView view = router.GetView(content.GetChild(i).name);
                    if (_tail < data.Count)
                        WillBindModel(view, _tail++);
                    else
                        view.Close();
                }
                --_tail;//指向可视域的最后一个位置
                Resize();
            }
        }

        private void OnDataChanged(ChangedMode mode, int index)
        {
            switch (mode)
            {
                case ChangedMode.Clear:
                case ChangedMode.Sort:
                    Refresh(true);
                    break;
                case ChangedMode.Remove:
                    Refresh(index < _tail);
                    break;
                case ChangedMode.Add:
                    Refresh(index <= viewCount);
                    break;
                case ChangedMode.Replace:
                    Refresh(index < _tail);
                    break;
            }
        }

        /// <summary>
        /// 将要重新渲染模型
        /// </summary>
        /// <param name="view">要重新绑定模型的视图</param>
        /// <param name="dataIndex">数据索引</param>
        protected void WillBindModel(IView view, int dataIndex)
        {
            IBindableModel model = _data[dataIndex] as IBindableModel;
            view.BindModel(model, true, null, true);
            view.Open();
        }

        /// <summary>
        /// 只刷新可以被看见的区域的数据
        /// </summary>
        protected void RefreshViewArea()
        {
            Transform content = _scrollRect.content;
            int len = content.childCount;
            int count = _data.Count;
            _tail = _head;
            ViewRouter router = Vue.Router;
            for (int i = 0; i < len; i++)
            {
                IView view = router.GetView(content.GetChild(i).name);
                if (_tail < count)
                    WillBindModel(view, _tail++);
                else
                    view.Close();
            }
            --_tail;
        }

        /// <summary>
        /// 强制刷新
        /// </summary>
        protected void ForceRefresh()
        {
            _head = 0;
            _scrollRect.normalizedPosition = _direction == ScrollDirection.Vertical ? Vector2.up : Vector2.zero;
            ResetItemPos(Vector3.zero);
            RefreshViewArea();
        }

        /// <summary>
        /// 根据第一个Item的位置重新计算每个Item的位置
        /// </summary>
        /// <param name="firstItemPos">第一个Item的位置</param>
        protected abstract void ResetItemPos(Vector2 firstItemPos);

        /// <summary>
        /// 刷新视图
        /// </summary>
        /// <param name="force">是否为强制刷新</param>
        public abstract void Refresh(bool force = false);

        /// <summary>
        /// 重新计算ScrollRect的内容区域大小
        /// </summary>
        protected abstract void Resize();

        /// <summary>
        /// ScrollRect滚动时回调
        /// </summary>
        /// <param name="pos">ScrollRect的正则化的位置</param>
        protected abstract void OnScroll(Vector2 pos);

    }
}
