using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UniVue.Utils;
using UniVue.View.Views;
using UniVue.View.Widgets;

namespace UniVue.View.Config
{
    public sealed class EnsureTipViewConfig : ViewConfig
    {
        [Header("取消按钮的名称")]
        public string _cancelBtnName;

        [Header("确认按钮的名称")]
        public string _sureBtnName;

        [Header("显示提示消息的TMP_Text组件的名称")]
        public string _contentName;

        [Header("显示提示消息的标题的TMP_Text组件的名称,可空")]
        public string _titleName;

        internal override IView CreateView(GameObject viewObject)
        {
            if (string.IsNullOrEmpty(_contentName))
                throw new System.Exception("必须指定用于显示提示消息的TMP_Text组件的名称!");

            if (string.IsNullOrEmpty(_sureBtnName))
                throw new System.Exception("必须指定确定按钮的名称!");

            if (string.IsNullOrEmpty(_cancelBtnName))
                throw new System.Exception("必须指定取消按钮的名称!");

            Button sureBtn = null, cancelBtn = null;
            TMP_Text message = null, title = null;

            using (var it = GameObjectFindUtil.BreadthFind(viewObject, _sureBtnName, _cancelBtnName, _contentName, _titleName).GetEnumerator())
            {
                while (it.MoveNext())
                {
                    GameObject gameObject = it.Current;
                    if (gameObject.name == _sureBtnName)
                    {
                        sureBtn = gameObject.GetComponent<Button>();
                    }
                    else if (gameObject.name == _cancelBtnName)
                    {
                        cancelBtn = gameObject.GetComponent<Button>();
                    }
                    else if (gameObject.name == _contentName)
                    {
                        message = gameObject.GetComponent<TMP_Text>();
                    }
                    else if (gameObject.name == _titleName)
                    {
                        title = gameObject.GetComponent<TMP_Text>();
                    }
                }
            }

            EnsureTip comp = new(name, cancelBtn, sureBtn, message, title);
            EnsureTipView view = new(comp, viewObject, viewName, level);
            BaseSettings(view);
            return view;
        }
    }
}
