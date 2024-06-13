using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UniVue.Input;
using UniVue.Model;

namespace UniVue.View.Widgets
{
    /// <summary>
    /// 对GridWidget的功能进行增强，支持对元素的拖拽、交换；
    /// 以及两个或多个SuperGridWidget之间的元素互换、添加等操作（前提是绑定的数据类型是一致的）
    /// </summary>
    public sealed class SuperGrid : Widget
    {
        private LoopGrid _grid;
        private GridGroup _group;           //当前SuperGridWidget所属组
        private DraggableItem[] _items;     //当前SuperGridWidget所有的可拖拽元素

        public SuperGrid(LoopGrid grid)
        {
            _grid = grid;
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

        /// <summary>
        /// 获取当前所加入的组
        /// </summary>
        /// <returns>GridGroup</returns>
        public GridGroup GetGroup() => _group;

        /// <summary>
        /// 将replaced元素替换为target元素
        /// </summary>
        /// <remarks>注意：出来交换元素外还会将两个元素绑定的数据也会在两个SuperGridWidget之间进行交换</remarks>
        /// <param name="replaced">即将被替换的元素元素</param>
        /// <param name="target">替代replaced的目标元素</param>
        /// <param name="targetPos">target在没有进行拖拽前的目标位置</param>
        /// <param name="targetOfGrid">目标元素所属的SuperGridWidget</param>
        internal void Swap(Transform replaced, Transform target, Vector3 targetPos, SuperGrid targetOfGrid)
        {
            //如果当前交换的元素属于同一个SuperGridWidget --> 只交换数据然后重新渲染数据
            if (ReferenceEquals(this, targetOfGrid))
            {
                int rIndex = GetDataIndex(replaced);
                int tIndex = GetDataIndex(target);
                _grid.SwapData(rIndex, tIndex);
                Vue.Router.GetView(replaced.name).RebindModel(_grid.GetData(tIndex));
                Vue.Router.GetView(replaced.name).RebindModel(_grid.GetData(rIndex));
            }
            //即交换位置又要交换数据的清空
            else if (replaced != null)
            {
                Vector3 replacePos = replaced.localPosition;
                int rIndex = GetDataIndex(replaced);
                int tIndex = targetOfGrid.GetDataIndex(target);
                IBindableModel replacedModel = _grid.GetData(rIndex);
                IBindableModel targetModel = targetOfGrid.GetData(tIndex);

                //交换位置
                replaced.SetParent(targetOfGrid.ScrollRect.content);
                replaced.localPosition = targetPos;
                target.SetParent(ScrollRect.content);
                target.localPosition = replacePos;
                //交换数据
                Replace(rIndex, targetModel);
                targetOfGrid.Replace(tIndex, replacedModel);
            }
            //当replaced为null时说明是将一个Item数据放到当前GridWidget中
            else
            {
                int tIndex = targetOfGrid.GetDataIndex(target);
                IBindableModel targetModel = targetOfGrid.GetData(tIndex);
                targetOfGrid.RemoveData(targetModel);
                AddData(targetModel);
            }

            //刷新Item
            RefreshItems();
            if (!ReferenceEquals(this, targetOfGrid))
                targetOfGrid.RefreshItems();
        }

        /// <summary>
        /// 获取指定索引的数据
        /// </summary>
        internal IBindableModel GetData(int index) => _grid.GetData(index);

        /// <summary>
        /// 将指定的索引的数据替换为目标数据
        /// </summary>
        internal void Replace(int replacedIndex, IBindableModel newModel) => _grid.Replace(replacedIndex, newModel);

        /// <summary>
        /// 通过一个位置获得距离此pos位置最近的那个Item
        /// </summary>
        /// <param name="worldPos">被拖拽元素在世界空间下的位置</param>
        internal DraggableItem GetItem(Vector3 worldPos)
        {
            DraggableItem min = null;
            float distance = 99999;
            //float checkDistance = 10f; //设置一个最小检查的距离
            for (int i = 0; i < _items.Length; i++)
            {
                RectTransform trans = _items[i].Target.GetComponent<RectTransform>();
                float d = Vector3.Distance(worldPos, trans.position);
                if (d < distance) { min = _items[i]; distance = d; }
            }
            return min;
        }

        /// <summary>
        /// 刷新元素（当进行元素进行交换之后都需要调用此函数）
        /// </summary>
        internal void RefreshItems()
        {
            Transform content = _grid.ScrollRect.content;
            for (int i = 0; i < content.childCount; i++)
            {
                _items[i].Target = content.GetChild(i);
                _items[i].BelongsToGrid = this;
            }
        }

        /// <summary>
        /// 获取ScrollRect组件
        /// </summary>
        public ScrollRect ScrollRect => _grid.ScrollRect;

        /// <summary>
        /// 获取Item上绑定的数据的索引
        /// </summary>
        private int GetDataIndex(Transform item)
        {
            Transform content = _grid.ScrollRect.content;
            int headPtr = _grid.Head;
            for (int i = 0; i < content.childCount; i++)
            {
                if (item == content.GetChild(i)) { return headPtr; }
                headPtr++;
            }

            throw new Exception($"未找到{item.name}所绑定数据的索引");
        }

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
            _items = new DraggableItem[content.childCount];
            for (int i = 0; i < content.childCount; i++)
            {
                Transform item = content.GetChild(i);
                var input = item.gameObject.AddComponent<DragInput>();
                DraggableItem draggableItem = new() { Target = item, BelongsToGrid = this };
                input.onBeginDrag += draggableItem.OnBeginDrag;
                input.onDrag += draggableItem.OnDrag;
                input.onEndDrag += draggableItem.OnEndDrag;
                _items[i] = draggableItem;
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
        public void Refresh()
        {
            _grid.Refresh();
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
            for (int i = 0; i < _items.Length; i++)
            {
                _items[i] = null;
            }
            _items = null;
            _group?.Remove(this);
            _group = null;
        }
    }

    public sealed class GridGroup
    {
        private List<SuperGrid> _grids;
        private List<Vector3[]> _corners;

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
            _corners = new(memberCount);
        }

        internal void Check(Vector3 worldPos)
        {
            for (int i = 0; i < _grids.Count; i++)
            {
                ScrollRect scrollRect = _grids[i].ScrollRect;
                //顺时针方向，左下角为索引0的位置
                scrollRect.GetComponent<RectTransform>().GetWorldCorners(_corners[i]);
                if (_corners[i][0].y <= worldPos.y && worldPos.y <= _corners[i][1].y &&
                    _corners[i][0].x <= worldPos.x && worldPos.x <= _corners[i][3].x)
                {
                    CurrentEnteredGrid = _grids[i];
                    return;
                }
            }
        }

        internal void Add(SuperGrid grid)
        {
            _grids.Add(grid);
            if (_corners.Count < _grids.Count) { _corners.Add(new Vector3[4]); }
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
        /// 获取当前拖拽元素所属的
        /// </summary>
        public SuperGrid BelongsToGrid { get; set; }

        /// <summary>
        /// 当前可拖拽元素的Transform对象
        /// </summary>
        public Transform Target { get; set; }

        /// <summary>
        /// 当前被拖拽元素最初在SuperGridWidget这的位置
        /// </summary>
        public Vector3 TargetPos { get; set; }

        public void OnBeginDrag(PointerEventData eventData)
        {
            TargetPos = Target.localPosition;
            GridGroup group = BelongsToGrid.GetGroup();
            group.CurrentDraggedItem = this;
            group.CurrentEnteredGrid = BelongsToGrid;
        }

        public void OnDrag(PointerEventData eventData)
        {
            BelongsToGrid.GetGroup().Check(eventData.pointerCurrentRaycast.worldPosition);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            GridGroup group = BelongsToGrid.GetGroup();
            group.CurrentEnteredGrid.Swap(
                group.CurrentEnteredGrid.GetItem(Target.GetComponent<RectTransform>().position)?.Target,
                Target, TargetPos, BelongsToGrid);
            group.CurrentDraggedItem = null;
            group.CurrentEnteredGrid = null;
        }
    }
}
