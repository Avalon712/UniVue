using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniVue.Model;
using UniVue.Utils;
using UniVue.ViewModel;

namespace UniVue.View.Widgets
{
    public sealed class ClampList : Widget
    {
        private List<IBindableModel> _models;
        private IObservableList _observer;
        private Transform _content;

        /// <summary>
        /// 显示Item的父物体
        /// </summary>
        public Transform Content => _content;

        private IBindableModel this[int index]
        {
            get
            {
                if (_observer != null)
                    return _observer.Get<IBindableModel>(index);
                else
                    return _models[index];
            }
        }

        public int Count => _observer == null ? _models.Count : _observer.Count;

        public ClampList(Transform content)
        {
            if (content == null)
                throw new Exception($"参数content不能为null");

            if (content.childCount == 0)
                throw new Exception($"名字为{content.name}的GameObject必须有一个子物体作为预制体模板进行预制体克隆!");

#if UNITY_EDITOR
            if (content.GetComponent<LayoutGroup>() == null)
                LogUtil.Warning($"建议在显示Item的父物体{content.name}上挂载一个Unity自带的布局组件! ClampList组件不会自动进行布局!");
#endif

            _content = content;
        }

        /// <summary>
        /// 绑定集合
        /// </summary>
        /// <remarks>使用这个函数可以防止数据的冗余。同时能够在数据发生变化时自动更新响应变化</remarks>
        public void BindList(IObservableList observer)
        {
            if (_models != null || _observer != null) { return; }
            _observer = observer;
            BindListeners();
            //创建Item
            CreateItemViews();
        }

        /// <summary>
        /// 绑定集合
        /// </summary>
        /// <remarks>
        /// 使用这个函数可以防止数据的冗余。同时能够在数据发生变化时自动更新响应变化</remarks>
        public void RebindList(IObservableList observer)
        {
            if (_models != null) { return; }
            if (_observer == null)
                BindList(observer);
            else if (!ReferenceEquals(_observer, observer))
            {
                _observer.RemoveListeners(GetHashCode()); //移除上次绑定的数据
                _observer = observer;
                BindListeners();
                Refresh(); //刷新数据
            }
        }

        /// <summary>
        /// 重新绑定数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="newData">绑定的新数据，注意必须与旧数据的类型一致！</param>
        public void RebindList<T>(List<T> newData) where T : IBindableModel
        {
            if (_observer != null) return;

            if (_models == null)
            {
                BindList(newData);
            }
            else
            {
                ListUtil.Copy(_models, newData);
                Refresh(); //刷新数据
            }
        }

        /// <summary>
        /// 为Item绑定显示数据
        /// </summary>
        /// <param name="data">绑定的数据</param>
        public void BindList<T>(List<T> data) where T : IBindableModel
        {
            if (_observer != null) return;

            if (_models != null)
            {
#if UNITY_EDITOR
                LogUtil.Warning("ClampList已经绑定了List数据，不能再进行绑定");
#endif
                return;
            }
            _models = new List<IBindableModel>(data.Count);
            ListUtil.Copy(_models, data);

            //创建Item
            CreateItemViews();
        }

        /// <summary>
        /// 对列表进行排序，排序规则
        /// </summary>
        /// <param name="comparer">排序规则</param>
        public void Sort<T>(Comparison<T> comparer) where T : IBindableModel
        {
            if (_observer == null && _models != null)
            {
                _models.Sort((b1, b2) => comparer((T)b1, (T)b2));
            }
            Refresh(); //刷新
        }

        /// <summary>
        /// 清空数据
        /// </summary>
        public void Clear()
        {
            if (_models != null)
                _models.Clear();

            int count = _content.childCount;
            for (int i = 0; i < count; i++)
            {
                _content.GetChild(i).gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="newData">新加入的数据</param>
        public void AddData<T>(T newData) where T : IBindableModel
        {
            if (_models != null && !_models.Contains(newData))
            {
                _models.Add(newData);
            }

            bool instance = true;
            int count = _content.childCount;
            for (int i = 0; i < count; i++)
            {
                GameObject itemObj = _content.GetChild(i).gameObject;
                if (!itemObj.activeSelf)
                {
                    itemObj.SetActive(true);
                    Rebind(itemObj, newData);
                    instance = false;
                    break;
                }
            }

            if (instance)
            {
                GameObject itemViewObject = CreatItemView();
                itemViewObject.SetActive(true);
                Rebind(itemViewObject, newData);
            }
        }

        /// <summary>
        /// 移除数据
        /// </summary>
        public void RemoveData<T>(T remove) where T : IBindableModel
        {
            if (remove != null && _models != null && _models.Contains(remove))
                _models.Remove(remove);

            Refresh();
        }

        /// <summary>
        /// 刷新视图
        /// </summary>
        public void Refresh()
        {
            Transform content = _content;
            int len = content.childCount;
            int count = Count;
            for (int i = 0; i < len; i++)
            {
                GameObject itemObj = content.GetChild(i).gameObject;
                ViewUtil.SetActive(itemObj, i < count);
                if (i < count)
                {
                    Rebind(itemObj, this[i]);
                }
            }
        }

        private void CreateItemViews()
        {
            GameObject firstItem = _content.GetChild(0).gameObject;

            int count = Count;
            if (count > 0)
                ViewUtil.Patch3Pass(firstItem, this[0]);

            for (int i = 1; i < count; i++)
            {
                GameObject itemViewObject = PrefabCloneUtil.RectTransformClone(firstItem, _content);
                itemViewObject.name += i;
                ViewUtil.Patch3Pass(itemViewObject, this[i]);
            }
        }

        private GameObject CreatItemView()
        {
            GameObject firstItem = _content.GetChild(0).gameObject;
            GameObject itemViewObject = PrefabCloneUtil.RectTransformClone(firstItem, _content);
            itemViewObject.name += _content.childCount - 1;
            return itemViewObject;
        }

        private void Rebind(GameObject itemViewObject, IBindableModel model)
        {
            UIBundle bundle = UIQuerier.Query(itemViewObject.name, model);
            if (bundle == null)
                ViewUtil.Patch3Pass(itemViewObject, model);
            else
                Vue.Updater.Rebind(itemViewObject.name, model);

            model.NotifyAll();
        }

        private void BindListeners()
        {
            int observerId = GetHashCode();
            //注册事件
            _observer.AddListener_OnAdded<IBindableModel>(observerId, v => AddData(v));
            _observer.AddListener_OnRemoved<IBindableModel>(observerId, (v, index) => RemoveData(v));
            _observer.AddListener_OnReplaced<IBindableModel>(observerId, (index, r1, r2) => Refresh());
            _observer.AddListener_OnChanged(observerId, mode =>
            {
                if (mode == NotificationMode.Sort)
                    Refresh();
            });
        }

        public override void Destroy()
        {
            _models?.Clear();
            _observer?.RemoveListeners(GetHashCode()); //移除上次绑定的数据
            _observer = null;
            _models = null;
            _content = null;
        }
    }
}
