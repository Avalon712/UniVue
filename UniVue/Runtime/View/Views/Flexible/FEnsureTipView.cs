using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UniVue.Utils;

namespace UniVue.View.Views.Flexible
{
    public sealed class FEnsureTipView : FlexibleView
    {
        private TMP_Text _title;
        private TMP_Text _content;
        private Button _sureBtn;
        private Button _cancelBtn;

        /// <summary>
        /// 自动查找组件
        /// 显示标题的TMP_Text组件的名称为"~Title"
        /// 显示消息内容的TMP_Text组件的名称为"~Content"
        /// 确认按钮的名称为"~Sure_Btn"
        /// 取消按钮的名称为"~Cancel_Btn"
        /// </summary>
        public FEnsureTipView(GameObject viewObject, string viewName = null, ViewLevel level = ViewLevel.Common)
            : this("~Title","~Content","~Sure_Btn","~Cancel_Btn",viewObject, viewName, level){ }

        public FEnsureTipView(string titleName,string contentName,string sureBtnName, string cancelBtnName,
            GameObject viewObject, string viewName = null, ViewLevel level = ViewLevel.Common) 
            : base(viewObject, viewName, level)
        {
            if (string.IsNullOrEmpty(contentName))
                throw new System.Exception("必须指定用于显示提示消息的TMP_Text组件的名称!");

            if (string.IsNullOrEmpty(sureBtnName))
                throw new System.Exception("必须指定确定按钮的名称!");

            if (string.IsNullOrEmpty(cancelBtnName))
                throw new System.Exception("必须指定取消按钮的名称!");

            using (var it = GameObjectFindUtil.BreadthFind(viewObject, sureBtnName, cancelBtnName, contentName, titleName).GetEnumerator())
            {
                while (it.MoveNext())
                {
                    GameObject gameObject = it.Current;
                    if (gameObject.name == sureBtnName)
                    {
                        _sureBtn = gameObject.GetComponent<Button>();
                    }
                    else if (gameObject.name == cancelBtnName)
                    {
                        _cancelBtn = gameObject.GetComponent<Button>();
                    }
                    else if (gameObject.name == contentName)
                    {
                        _content = gameObject.GetComponent<TMP_Text>();
                    }
                    else if (gameObject.name == titleName)
                    {
                        _title = gameObject.GetComponent<TMP_Text>();
                    }
                }
            }

            if (_sureBtn == null)
                throw new System.Exception($"未能根据名称{sureBtnName}找到确定按钮!");

            if (_cancelBtn == null)
                throw new System.Exception($"未能根据名称{cancelBtnName}找到取消按钮!");

            if (_content == null)
                throw new System.Exception($"未能根据名称{contentName}找到用于显示消息的TMP_Text组件!");

        }

        public FEnsureTipView(TMP_Text title,TMP_Text content,Button sureBtn,Button cancelBtn,
            GameObject viewObject, string viewName = null, ViewLevel level = ViewLevel.Common) 
            : base(viewObject, viewName, level)
        {
            if (sureBtn == null)
                throw new System.Exception($"必须指定确认按钮");

            if (cancelBtn == null)
                throw new System.Exception($"必须指定取消按钮");

            if (content == null)
                throw new System.Exception($"必须指定用于显示消息的TMP_Text组件!");

            _title = title; _content = content; _sureBtn = sureBtn; _cancelBtn = cancelBtn;
        }

        /// <summary>
        /// 打开此视图
        /// </summary>
        /// <remarks>注：事件绑定是暂时的，当视图关闭后会自动注销回调事件</remarks>
        /// <param name="title">消息的标题,可以为null</param>
        /// <param name="message">要显示的消息</param>
        /// <param name="sure">点击"确定"按钮时回调函数</param>
        /// <param name="cancel">点击"取消"按钮时回调函数</param>
        public void Open(string title, string message, UnityAction sure, UnityAction cancel)
        {
            if (_title != null) { _title.text = title; }
            _content.text = message;
            if (sure != null) { _sureBtn.onClick.AddListener(sure); }
            if (cancel != null) { _cancelBtn.onClick.AddListener(cancel); }
            Vue.Router.Open(name);
        }

        public override void Close()
        {
            base.Close();
            _sureBtn.onClick.RemoveAllListeners();
            _cancelBtn.onClick.RemoveAllListeners();
        }
    }
}
