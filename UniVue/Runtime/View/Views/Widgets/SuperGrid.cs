using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UniVue.Input;
using UniVue.Model;
using UniVue.Utils;

namespace UniVue.View.Widgets
{
    /// <summary>
    /// 对GridWidget的功能进行增强，支持对元素的拖拽、交换；
    /// 以及两个或多个SuperGridWidget之间的元素互换、添加等操作（前提是绑定的数据类型是一致的）
    /// </summary>
    public sealed class SuperGrid : Widget
    {
        private LoopGrid _grid;
        private GridGroup _group;                                    //当前SuperGrid所属组
        private Dictionary<RectTransform, DraggableItem> _items;     //key=Item、value=此时绑定的数据的索引

        public SuperGrid(LoopGrid grid)
        {
            _grid = grid;
            grid.OnRebind += OnRebind;
        }

        internal LoopGrid Grid => _grid;

        /// <summary>
        /// 获取当前所加入的组
        /// </summary>
        /// <returns>GridGroup</returns>
        public GridGroup Group => _group;

        /// <summary>
        /// 获取ScrollRect组件
        /// </summary>
        public ScrollRect ScrollRect => _grid.ScrollRect;


        /// <summary>
        /// 更新Item绑定的模型数据的索引
        /// </summary>
        internal void OnRebind(RectTransform item, int dataIndex)
        {
            _items[item].DataIndex = dataIndex;
        }

        /// <summary>
        /// 获得距离target位置最近的那个Item
        /// </summary>
        internal DraggableItem GetItem(RectTransform target)
        {
            DraggableItem min = null;
            float distance = 99999;

            Vector3 worldPos = target.position;
            foreach (var item in _items.Keys)
            {
                if (item == target) { continue; }

                float d = Vector3.Distance(worldPos, item.position);
                if (d < distance)
                {
                    min = _items[item];
                    distance = d;
                }
            }
            return min;
        }


        internal int GetSiblingIndex(RectTransform item)
        {
            Transform content = ScrollRect.content;
            for (int i = 0; i < content.childCount; i++)
            {
                if (content.GetChild(i) == item)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// 加入一个组
        /// </summary>
        public void JoinGroup(GridGroup group)
        {
            _group = group;
            group.Add(this);
        }

        /// <summary>
        /// 退出当前组
        /// </summary>
        public void QuitGroup()
        {
            if (_group == null)
                throw new Exception("当前尚未加入任何组!");
            _group.Remove(this);
            _group = null;
        }

        #region 对LoopGrid中的方法进行装饰

        /// <summary>
        /// 重新绑定数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="newData">绑定的新数据，注意必须与旧数据的类型一致！</param>
        public void RebindData<T>(List<T> newData) where T : IBindableModel
        {
            _grid.RebindList(newData);
        }

        /// <summary>
        /// 绑定item数据
        /// <para>若为引用绑定，则无需使用AddData/RemoveData函数进行对数据的增删</para>
        /// </summary>
        /// <param name="data">绑定数据</param>
        public void BindData<T>(List<T> data) where T : IBindableModel
        {
            _grid.BindList(data);

            //为每个Item挂载上DragInput组件
            Transform content = _grid.ScrollRect.content;
            _items = new Dictionary<RectTransform, DraggableItem>(content.childCount);
            for (int i = 0; i < content.childCount; i++)
            {
                RectTransform item = content.GetChild(i) as RectTransform;
                DragInput input = item.gameObject.AddComponent<DragInput>();
                DraggableItem draggableItem = new() { Target = item, Owner = this };
                input.onBeginDrag += draggableItem.OnBeginDrag;
                input.onEndDrag += draggableItem.OnEndDrag;
                draggableItem.DataIndex = i;
                _items.Add(item, draggableItem);
            }
        }


        /// <summary>
        /// 排序，本质上是对数据进行排序
        /// </summary>
        public void Sort(Comparison<IBindableModel> comparer)
        {
            _grid.Sort(comparer);
        }

        /// <summary>
        /// 添加数据(需要先绑定数据)
        /// </summary>
        /// <param name="newData">新加入的数据</param>
        public void AddData<T>(T newData) where T : IBindableModel
        {
            _grid.AddData(newData);
        }

        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="remove">要移除的数据[如果知道索引则不用传递改参数]</param>
        public void RemoveData<T>(T remove) where T : IBindableModel
        {
            _grid.RemoveData(remove);
        }

        /// <summary>
        /// 视图刷新
        /// </summary>
        public void Refresh(bool force = false)
        {
            _grid.Refresh(force);
        }

        /// <summary>
        /// 清空数据
        /// </summary>
        public void Clear()
        {
            _grid.Clear();
        }

        public override void Destroy()
        {
            _grid.Destroy();
            _grid = null;
            _items.Clear();
            _items = null;
            _group?.Remove(this);
            _group = null;
        }
        #endregion
    }

    /// <summary>
    /// 在一组的格子应该保证其数据类型是一致的
    /// </summary>
    /// <remarks>拖拽交换时的本质其实只有数据在交换</remarks>
    public sealed class GridGroup
    {
        private List<SuperGrid> _grids;

        /// <summary>
        /// 当前正在被拖拽的元素
        /// </summary>
        internal DraggableItem CurrentDraggedItem { get; set; }

        /// <summary>
        /// 当前被拖拽元素进入的网格
        /// </summary>
        internal SuperGrid CurrentEnteredGrid { get; set; }

        public GridGroup(int memberCount = 2)
        {
            _grids = new(memberCount);
        }

        /// <summary>
        /// 将拖拽元素与当前网格的元素进行互换
        /// </summary>
        /// <remarks>注意：出来交换元素外还会将两个元素绑定的数据也会在两个SuperGrid之间进行交换</remarks>
        /// <param name="noDragged">当前网格中没有被拖拽的元素</param>
        /// <param name="dragged">正在被拖拽的元素</param>
        internal void Swap(DraggableItem noDragged, DraggableItem dragged)
        {
            SuperGrid noDraggedGrid = noDragged.Owner;
            SuperGrid draggedGrid = dragged.Owner;
            IBindableModel noDraggedModel = noDraggedGrid.Grid.GetData(noDragged.DataIndex);
            IBindableModel draggedModel = draggedGrid.Grid.GetData(dragged.DataIndex);

            //Debug.Log($"Dragged=(index={dragged.DataIndex} value={((AtomModel<int>)draggedModel).Value}], noDragged=[index={noDragged.DataIndex}, value={((AtomModel<int>)noDraggedModel).Value}]");

            //当放置拖拽元素的地方是一个空位，则只需要传递数据而不做交换
            if (!noDragged.Target.gameObject.activeSelf)
            {
                //直接在尾巴位置放入数据
                draggedGrid.Grid.RemoveData(dragged.DataIndex);
                noDraggedGrid.AddData(draggedModel);
                noDragged.Target.gameObject.SetActive(true);
                Vue.Router.GetView(noDragged.Target.name).BindModel(draggedModel, true, dragged.Target.name, true);
                dragged.Target.gameObject.SetActive(false);
            }
            else
            {
                //交换数据
                noDraggedGrid.Grid.SetData(noDragged.DataIndex, draggedModel);
                draggedGrid.Grid.SetData(dragged.DataIndex, noDraggedModel);

                //重新绑定数据
                Vue.Router.GetView(dragged.Target.name).BindModel(noDraggedModel, true, noDragged.Target.name, true);
                Vue.Router.GetView(noDragged.Target.name).BindModel(draggedModel, true, dragged.Target.name, true);

                //刷新数据
                noDraggedModel.NotifyAll();
                draggedModel.NotifyAll();
            }
        }

        /// <summary>
        /// 要求Item必须完全进入到格子中去
        /// </summary>
        internal bool DropWhere(RectTransform item)
        {
            Vector3[] gridCorners = new Vector3[4];
            Vector3[] itemCorners = new Vector3[4];

            item.GetWorldCorners(itemCorners);

            for (int i = 0; i < _grids.Count; i++)
            {
                ScrollRect scrollRect = _grids[i].ScrollRect;
                //顺时针方向，左下角为索引0的位置
                scrollRect.GetComponent<RectTransform>().GetWorldCorners(gridCorners);
                if (gridCorners[0].x < itemCorners[0].x && gridCorners[0].y < itemCorners[0].y
                    && gridCorners[2].x > itemCorners[2].x && gridCorners[2].y > itemCorners[2].y)
                {
                    CurrentEnteredGrid = _grids[i];
                    return true;
                }
            }
            return false;
        }

        internal void Add(SuperGrid grid)
        {
            _grids.Add(grid);
        }

        internal void Remove(SuperGrid remove)
        {
            _grids.Remove(remove);
        }

        internal bool Contains(SuperGrid check)
        {
            return _grids.Contains(check);
        }

        /// <summary>
        /// 解散此组
        /// </summary>
        public void Dissolve()
        {
            if (_grids == null)
                throw new Exception("不合法的访问，你正在访问一个已经解散不存在的组！");

            for (int i = 0; i < _grids.Count; i++)
            {
                _grids[i].QuitGroup();
            }
            _grids = null;
        }
    }


    internal class DraggableItem
    {
        /// <summary>
        /// 获取当前拖拽元素所属的格子
        /// </summary>
        public SuperGrid Owner { get; set; }

        /// <summary>
        /// 当前可拖拽元素的Transform对象
        /// </summary>
        public RectTransform Target { get; set; }

        /// <summary>
        /// 当前被拖拽元素最初在SuperGridWidget这的位置
        /// </summary>
        public Vector3 OriginalPos { get; private set; }

        /// <summary>
        /// 当前元素在Content下的第几个
        /// </summary>
        public int SiblingIndex { get; set; }

        /// <summary>
        /// 绑定的数据的索引
        /// </summary>
        public int DataIndex { get; set; }

        public void OnBeginDrag(PointerEventData eventData)
        {
            OriginalPos = Target.anchoredPosition;
            GridGroup group = Owner.Group;
            group.CurrentDraggedItem = this;
            group.CurrentEnteredGrid = Owner;
            SiblingIndex = Owner.GetSiblingIndex(Target);
            //将当前被拖拽的元素设置为Canvas下的物体，这样总能被看见
            RectTransform canvas = ComponentFindUtil.LookUpFindComponent<Canvas>(Target.gameObject).transform as RectTransform;
            Target.SetParent(canvas);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            GridGroup group = Owner.Group;

            //计算当前拖拽元素落入了哪个格子中
            if (group.DropWhere(Target))
            {
                DraggableItem noDraggedItem = group.CurrentEnteredGrid.GetItem(Target);
                group.Swap(noDraggedItem, this);
            }
            ToOriginalPos();
        }

        /// <summary>
        /// 回到开始的位置
        /// </summary>
        public void ToOriginalPos()
        {
            Target.SetParent(Owner.ScrollRect.content);
            Target.SetSiblingIndex(SiblingIndex);
            Target.anchoredPosition = OriginalPos;
        }


    }
}
