using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UniVue.Utils;

namespace UniVue.View.Views
{
    public sealed class SEnsureTipView : ScriptableView
    {
        #region 配置数据
        [Header("取消按钮的名称")]
        [SerializeField] private string _cancelBtnName;

        [Header("确认按钮的名称")]
        [SerializeField] private string _sureBtnName;

        [Header("显示提示消息的TMP_Text组件的名称")]
        [SerializeField] private string _contentName;

        [Header("显示提示消息的标题的TMP_Text组件的名称,可空")]
        [SerializeField] private string _titleName;
        #endregion

        private EnsureTipWidget _comp;

        public override void OnLoad()
        {
            if (string.IsNullOrEmpty(_contentName))
                throw new System.Exception("必须指定用于显示提示消息的TMP_Text组件的名称!");

            if (string.IsNullOrEmpty(_sureBtnName))
                throw new System.Exception("必须指定确定按钮的名称!");

            if (string.IsNullOrEmpty(_cancelBtnName))
                throw new System.Exception("必须指定取消按钮的名称!");

            Button sureBtn=null, cancelBtn=null;
            TMP_Text message=null, title=null;

            using(var it = GameObjectFindUtil.BreadthFind(viewObject, _sureBtnName, _cancelBtnName, _contentName, _titleName).GetEnumerator())
            {
                while (it.MoveNext())
                {
                    GameObject gameObject = it.Current;
                    if(gameObject.name == _sureBtnName)
                    {
                        sureBtn = gameObject.GetComponent<Button>();
                    }else if(gameObject.name == _cancelBtnName)
                    {
                        cancelBtn = gameObject.GetComponent<Button>();
                    }else if(gameObject.name == _contentName)
                    {
                        message = gameObject.GetComponent<TMP_Text>();
                    }
                    else if(gameObject.name == _titleName)
                    {
                        title = gameObject.GetComponent<TMP_Text>();
                    }
                }
            }

            _comp = new(name, cancelBtn, sureBtn, message, title);
            base.OnLoad();
        }

        public override void OnUnload()
        {
            _comp.Destroy();
            _comp = null;
            base.OnUnload();
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
            _comp.Open(title, message, sure, cancel);
        }

        public override void Close()
        {
            base.Close();
            _comp.Close();
        }
    }
}
