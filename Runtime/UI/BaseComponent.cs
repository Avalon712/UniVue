using UnityEngine;

namespace UniVue.UI
{
    [DisallowMultipleComponent]
    public abstract class BaseComponent : BaseUI
    {
        /// <summary>
        /// 当前组件所属的界面
        /// </summary>
        public BaseView View
        {
            get
            {
                BaseUI parent = Parent;
                while (parent)
                {
                    if(parent is BaseView view)
                        return view;
                    parent = parent.Parent;
                }
                return null;
            }
        }
    }
}