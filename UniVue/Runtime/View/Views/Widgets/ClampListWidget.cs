using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniVue.Model;
using UniVue.Utils;

namespace UniVue.View.Views
{
    [Serializable]
    public sealed class ClampListWidget : IWidget
    {
        private List<IBindableModel> _models;
        [SerializeField]
        private Transform _content;

        public ClampListWidget() { }

        public ClampListWidget(Transform content)
        {
            if (content == null)
                throw new Exception($"参数content不能为null");

            if (content.childCount == 0)
                throw new Exception($"名字为{content.name}的GameObject必须有一个子物体作为预制体模板进行预制体克隆!");

#if UNITY_EDITOR
            if (content.GetComponent<LayoutGroup>() == null)
                LogUtil.Warning("建议在显示Item的父物体上挂载一个Unity自带的布局组件!");
#endif

            _content = content;
        }

        /// <summary>
        /// 重新绑定数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="newData">绑定的新数据，注意必须与旧数据的类型一致！</param>
        public void RebindData<T>(List<T> newData) where T : IBindableModel
        {
            if (_models == null)
            {
                BindData(newData);
            }
            else if (!ReferenceEquals(_models,newData))
            {
                ListUtil.Copy(_models, newData);
                Refresh(); //刷新数据
            }
        }

        /// <summary>
        /// 为Item绑定显示数据
        /// </summary>
        /// <param name="data">绑定的数据</param>
        public void BindData<T>(List<T> data) where T : IBindableModel
        {
            if (_models != null)
            {
#if UNITY_EDITOR
                LogUtil.Warning("ClampListView已经绑定了List数据，不能再进行绑定");
#endif
                return;
            }
            _models = new List<IBindableModel>(data.Count);

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
            _models.Sort(comparer);
            Refresh(); //刷新
        }

        /// <summary>
        /// 清空数据
        /// </summary>
        public void Clear()
        {
            int count = _content.childCount;
            for (int i = 0; i < count; i++)
            {
                _content.GetChild(i).gameObject.SetActive(false);
            }
            _models.Clear();
        }

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="newData">新加入的数据</param>
        public void AddData<T>(T newData) where T : IBindableModel
        {
            if (!_models.Contains(newData))
            {
                _models.Add(newData);

                bool instance = true;
                int count = _content.childCount;
                for (int i = 0; i < count; i++)
                {
                    GameObject itemObj = _content.GetChild(i).gameObject;
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
            if (remove != null && _models.Contains(remove))
            {
                ListUtil.TrailDelete(_models, _models.IndexOf(remove));
                Refresh();
            }
        }

        /// <summary>
        /// 刷新视图
        /// </summary>
        public void Refresh()
        {
            Transform content = _content;
            int len = content.childCount;
            for (int i = 0; i < len; i++)
            {
                GameObject itemObj = content.GetChild(i).gameObject;
                if (i < _models.Count)
                {
                    if (!itemObj.activeSelf) { itemObj.SetActive(true); }
                    Rebind(itemObj.name, _models[i]);
                }
                else
                {
                    itemObj.SetActive(false);
                }
            }
        }

        private void CreateItemViews()
        {
            GameObject firstItem = _content.GetChild(0).gameObject;

            new FlexibleView(firstItem);

            //创建Item
            int count = _models.Count;
            for (int i = 1; i < count; i++)
            {
                GameObject itemViewObject = PrefabCloneUtil.RectTransformClone(firstItem, _content);
                itemViewObject.name += i;
                new FlexibleView(itemViewObject);
            }
        }

        private GameObject CreatItemView()
        {
            GameObject firstItem = _content.GetChild(0).gameObject;
            GameObject itemViewObject = PrefabCloneUtil.RectTransformClone(firstItem, _content);
            itemViewObject.name += (_content.childCount - 1);
            new FlexibleView(itemViewObject);
            return itemViewObject;
        }

        private void Rebind(string itemName, IBindableModel model)
        {
            Vue.Router.GetView(itemName).BindModel(model, true, null, true);
        }
        public void Destroy()
        {
            _models?.Clear();
            _models = null;
            _content = null;
        }
    }
}
