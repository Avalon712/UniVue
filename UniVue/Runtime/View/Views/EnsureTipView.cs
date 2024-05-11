using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UniVue.Utils;

namespace UniVue.View.Views
{
    public sealed class EnsureTipView : BaseView
    {
        #region 配置数据
        [Header("取消按钮的名称")]
        [SerializeField] private string cancelBtnName;

        [Header("确认按钮的名称")]
        [SerializeField] private string sureBtnName;

        [Header("显示提示消息的TMP_Text组件的名称")]
        [SerializeField] private string contentName;

        [Header("显示提示消息的标题的TMP_Text组件的名称,可空")]
        [SerializeField] private string titleName;
        #endregion

        private struct RuntimeData
        {
            public Button cancelBtn;
            public Button sureBtn;
            public TMP_Text message;
            public TMP_Text title;
        }

        private RuntimeData _runtime;

        public override void OnLoad()
        {
            if (string.IsNullOrEmpty(contentName))
                throw new System.Exception("必须指定用于显示提示消息的TMP_Text组件的名称!");

            if (string.IsNullOrEmpty(sureBtnName))
                throw new System.Exception("必须指定确定按钮的名称!");

            if (string.IsNullOrEmpty(cancelBtnName))
                throw new System.Exception("必须指定取消按钮的名称!");

            using(var it = GameObjectFindUtil.BreadthFind(viewObject, sureBtnName, cancelBtnName, contentName, titleName).GetEnumerator())
            {
                while (it.MoveNext())
                {
                    GameObject gameObject = it.Current;
                    if(gameObject.name == sureBtnName)
                    {
                        _runtime.sureBtn = gameObject.GetComponent<Button>();
                    }else if(gameObject.name == cancelBtnName)
                    {
                        _runtime.cancelBtn = gameObject.GetComponent<Button>();
                    }else if(gameObject.name == contentName)
                    {
                        _runtime.message = gameObject.GetComponent<TMP_Text>();
                    }
                    else if(gameObject.name == titleName)
                    {
                        _runtime.title = gameObject.GetComponent<TMP_Text>();
                    }
                }
            }

            if (_runtime.sureBtn == null)
                throw new System.Exception($"未能根据名称{sureBtnName}找到确定按钮!");

            if (_runtime.cancelBtn == null)
                throw new System.Exception($"未能根据名称{cancelBtnName}找到取消按钮!");

            if (_runtime.message == null)
                throw new System.Exception($"未能根据名称{contentName}找到用于显示消息的TMP_Text组件!");

            base.OnLoad();
        }

        public override void OnUnload()
        {
            base.OnUnload();
            _runtime = default;
        }

        /// <summary>
        /// 打开此视图
        /// </summary>
        /// <remarks>注：事件绑定是暂时的，当视图关闭后会自动注销回调事件</remarks>
        /// <param name="title">消息的标题,可以为null</param>
        /// <param name="message">要显示的消息</param>
        /// <param name="sure">点击"确定"按钮时回调函数</param>
        /// <param name="cancel">点击"取消"按钮时回调函数</param>
        public void Open(string title,string message,UnityAction sure,UnityAction cancel)
        {
            if (_runtime.title != null) { _runtime.title.text = title; }
            _runtime.message.text = message;
            if(sure != null) { _runtime.sureBtn.onClick.AddListener(sure); }
            if(cancel != null) { _runtime.cancelBtn.onClick.AddListener(cancel); }
            Vue.Router.Open(name);
        }

        public override void Close()
        {
            base.Close();
            _runtime.sureBtn.onClick.RemoveAllListeners();
            _runtime.cancelBtn.onClick.RemoveAllListeners();
        }
    }
}
