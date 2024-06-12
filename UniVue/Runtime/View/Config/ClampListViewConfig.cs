using UnityEngine;
using UniVue.Utils;
using UniVue.View.Views;
using UniVue.View.Widgets;

namespace UniVue.View.Config
{
    public sealed class ClampListViewConfig : ViewConfig
    {
        [Header("所有子物体的父物体的名称")]
        public string _content;

        internal override IView CreateView(GameObject viewObject)
        {
            var content = GameObjectFindUtil.BreadthFind(_content, viewObject)?.transform;
            ClampList comp = new(content);
            ClampListView view = new(comp, viewObject, viewName, level);
            BaseSettings(view);
            return view;
        }
    }
}
