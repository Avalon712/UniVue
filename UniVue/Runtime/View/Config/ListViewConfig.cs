using System;
using UnityEngine;
using UnityEngine.UI;
using UniVue.Tween;
using UniVue.Utils;
using UniVue.View.Views;
using UniVue.View.Widgets;

namespace UniVue.View.Config
{
    public sealed class ListViewConfig : ViewConfig
    {
        [Header("滚动方向 Horizontal|Vercital")]
        public Direction _scrollDir = Direction.Vertical;

        [Header("循环滚动")]
        public bool _loop;

        [Header("可见数量")]
        public int _viewNum;

        [Header("相连两个item在滚动方向上的位置差")]
        [Tooltip("垂直方向滚动: upItem.localPos.y-downItem.localPos.y 水平方向滚动:rightItem.localPos.x-leftItem.localPos.x")]
        public float _distance;

        [Header("当刷新视图时是否播放滚动效果")]
        public bool _playScrollEffectOnRefresh;

        [Header("是否总是显示最新的数据")]
        public bool _alwaysShowNewestData;

        internal override IView CreateView(GameObject viewObject)
        {
            ViewUtil.SetActive(viewObject, initState);
            ScrollRect scrollRect = ComponentFindUtil.BreadthFind<ScrollRect>(viewObject);

            if (scrollRect == null)
            {
                throw new Exception("viewObject身上未包含一个ScrollRect组件，该功能依赖该组件！");
            }

            LoopList listComp = new(scrollRect, _distance, _viewNum, _scrollDir, _loop)
            {
                AlwaysShowNewestData = _alwaysShowNewestData,
                PlayScrollEffectOnRefresh = _playScrollEffectOnRefresh
            };

            ListView view = new ListView(listComp, viewObject, viewName, level);
            BaseSettings(view);
            return view;
        }
    }
}
