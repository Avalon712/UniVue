using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniVue.Evt;
using UniVue.Model;
using UniVue.Utils;
using UniVue.View.Views;
using UniVue.View.Views.Flexible;

namespace UniVue.Runtime.View.Views
{
    /// <summary>
    /// 不能滚动的列表视图，可以方便实现显示和隐藏子物体
    /// </summary>
    public sealed class ClampListView : BaseView
    {
        private struct RuntimeData
        {
            public List<IBindableModel> models;
            public int hash; //RebindData()时判断新绑定的数据和之前绑定的数据是否是同一个对象
            public Transform content;
        }

        private RuntimeData _runtime;

        #region 配置信息
        /// <summary>
        /// 显示所有子物体的父物体
        /// </summary>
        [Header("所有子物体的父物体的名称")]
        [SerializeField] private string content;

        #endregion


        public override void OnLoad()
        {
            _runtime.content = GameObjectFindUtil.BreadthFind(content, viewObject)?.transform;

            if (_runtime.content == null)
                throw new Exception($"未能再ViewObject身上找到名为{content}的GameObject!");
            
            if (_runtime.content.childCount == 0)
                throw new Exception($"名字为{content}的GameObject必须有一个子物体作为预制体模板进行预制体克隆!");

#if UNITY_EDITOR
            if (_runtime.content.GetComponent<LayoutGroup>() == null)
                LogUtil.Warning("建议在显示Item的父物体上挂载一个Unity自带的布局组件!");
#endif

            base.OnLoad();
        }

        protected override void AutoBindEvent()
        {
            //获取所有的ui组件
            var uis = ComponentFindUtil.FindAllSpecialUIComponents(viewObject, this, _runtime.content.gameObject);
            //构建UIEvent
            UIEventBuilder.Build(name, uis);
            //处理路由事件
            Vue.Router.BindRouteEvt(name, uis);
        }

        public override void OnUnload()
        {
            _runtime = default;
            base.OnUnload();
        }

        /// <summary>
        /// 重新绑定数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="newData">绑定的新数据，注意必须与旧数据的类型一致！</param>
        public void RebindData<T>(List<T> newData) where T : IBindableModel
        {
            if (_runtime.models == null)
            {
                BindData(newData);
            }
            else if (_runtime.hash != newData.GetHashCode())
            {
                ListUtil.Copy(_runtime.models, newData);
                Refresh(); //刷新数据
                _runtime.hash = newData.GetHashCode();
            }
        }

        /// <summary>
        /// 为Item绑定显示数据
        /// </summary>
        /// <param name="data">绑定的数据</param>
        public void BindData<T>(List<T> data) where T : IBindableModel
        {
            if (_runtime.models != null)
            {
#if UNITY_EDITOR
                LogUtil.Warning("ClampListView已经绑定了List数据，不能再进行绑定");
#endif
                return;
            }
            _runtime.hash = data.GetHashCode();
            _runtime.models = new List<IBindableModel>(data.Count);
           
            //创建Item
            CreateItemViews();

            for (int i = 0; i < data.Count; i++) { AddData(data[i]); }
        }

        /// <summary>
        /// 对列表进行排序，排序规则
        /// </summary>
        /// <param name="comparer">排序规则</param>
        public void Sort(Comparison<IBindableModel> comparer)
        {
            _runtime.models.Sort(comparer);
            Refresh(); //刷新
        }

        /// <summary>
        /// 清空数据
        /// </summary>
        public void Clear()
        {
            int count = _runtime.content.childCount;
            for (int i = 0; i < count; i++)
            {
                _runtime.content.GetChild(i).gameObject.SetActive(false);
            }
            _runtime.models.Clear();
        }

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="newData">新加入的数据</param>
        public void AddData<T>(T newData) where T : IBindableModel
        {
            if (!_runtime.models.Contains(newData))
            {
                _runtime.models.Add(newData);

                bool instance = true;
                int count = _runtime.content.childCount;
                for (int i = 0; i < count; i++)
                {
                    GameObject itemObj = _runtime.content.GetChild(i).gameObject;
                    if (!itemObj.activeSelf)
                    {
                        itemObj.SetActive(true);
                        Rebind(itemObj.name, newData);
                        instance = false;
                        break;
                    }
                }

                if (instance)
                {
                    GameObject itemObj = CreatItemView();
                    itemObj.SetActive(true);
                    Rebind(itemObj.name, newData);
                }
            }
            
        }

        /// <summary>
        /// 移除数据
        /// </summary>
        public void RemoveData<T>(T remove) where T : IBindableModel
        {
            if (remove != null && _runtime.models.Contains(remove))
            {
                ListUtil.TrailDelete(_runtime.models, _runtime.models.IndexOf(remove));
                Refresh();
            }
        }

        /// <summary>
        /// 刷新视图
        /// </summary>
        public void Refresh()
        {
            Transform content = _runtime.content;
            int len = content.childCount;
            for (int i = 0; i < len; i++)
            {
                GameObject itemObj = content.GetChild(i).gameObject;
                if (i < _runtime.models.Count)
                {
                    if (!itemObj.activeSelf) { itemObj.SetActive(true); }
                    Rebind(itemObj.name, _runtime.models[i]);
                }
                else
                {
                    itemObj.SetActive(false);
                }
            }
        }

        private void CreateItemViews()
        {
            GameObject firstItem = _runtime.content.GetChild(0).gameObject;
           
            new FlexibleView(firstItem);

            //创建Item
            int count = _runtime.models.Count;
            for (int i = 1; i < count; i++)
            {
                GameObject itemViewObject = PrefabCloneUtil.RectTransformClone(firstItem, _runtime.content);
                itemViewObject.name += i;
                new FlexibleView(itemViewObject);
            }
        }

        private GameObject CreatItemView()
        {
            GameObject firstItem = _runtime.content.GetChild(0).gameObject;
            GameObject itemViewObject = PrefabCloneUtil.RectTransformClone(firstItem, _runtime.content);
            itemViewObject.name += (_runtime.content.childCount-1);
            new FlexibleView(itemViewObject);
            return itemViewObject;
        }
    
        private void Rebind(string itemName, IBindableModel model)
        {
            Vue.Router.GetView(itemName).BindModel(model, true, null, true);
        }
    }
}
