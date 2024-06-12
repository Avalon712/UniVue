using System;
using UnityEngine;
using UnityEngine.UI;
using UniVue.Tween;
using UniVue.Utils;
using UniVue.View.Views;
using UniVue.View.Widgets;

namespace UniVue.View.Config
{
    public sealed class GridViewConfig : ViewConfig
    {
        [Header("滚动方向 Horizontal|Vercital")]
        public Direction scrollDir = Direction.Vertical;

        [Header("可见行数")]
        public int rows;

        [Header("可见列数")]
        public int cols;

        [Header("leftItemPos.x+x=rightItemPos.x")]
        public float x;

        [Header("upItemPos.y+y=downItemPos.y")]
        public float y;

        [Header("当刷新视图时是否播放滚动效果")]
        public bool _playScrollEffectOnRefresh;

        internal override IView CreateView(GameObject viewObject)
        {
            ViewUtil.SetActive(viewObject, initState);
            var scrollRect = ComponentFindUtil.BreadthFind<ScrollRect>(viewObject);

            if (scrollRect == null)
            {
                throw new Exception("viewObject身上未包含一个ScrollRect组件，该功能依赖该组件！");
            }

            LoopGrid gridComp = new(scrollRect, rows, cols, x, y, scrollDir)
            {
                PlayScrollEffectOnRefresh = _playScrollEffectOnRefresh
            };

            GridView view = new(gridComp, viewObject, viewName, level);
            BaseSettings(view);
            return view;
        }
    }
}
