using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UniVue.Utils;

namespace UniVue.View.Views
{
    public sealed class FEnsureTipView : FlexibleView
    {
        private EnsureTipWidget _comp;

        /// <summary>
        /// 自动查找组件
        /// <para>显示标题的TMP_Text组件的名称为"Title"</para>
        /// <para>显示消息内容的TMP_Text组件的名称为"Content"</para>
        /// <para>确认按钮的名称为"Sure_Btn"</para>
        /// <para>取消按钮的名称为"Cancel_Btn"</para>
        /// </summary>
        public FEnsureTipView(GameObject viewObject, string viewName = null, ViewLevel level = ViewLevel.Common)
            : this("Title", "Content", "Sure_Btn", "Cancel_Btn", viewObject, viewName, level) { }

        public FEnsureTipView(string titleName, string contentName, string sureBtnName, string cancelBtnName,
            GameObject viewObject, string viewName = null, ViewLevel level = ViewLevel.Common)
            : base(viewObject, viewName, level)
        {
            if (string.IsNullOrEmpty(contentName))
                throw new System.Exception("必须指定用于显示提示消息的TMP_Text组件的名称!");

            if (string.IsNullOrEmpty(sureBtnName))
                throw new System.Exception("必须指定确定按钮的名称!");

            if (string.IsNullOrEmpty(cancelBtnName))
                throw new System.Exception("必须指定取消按钮的名称!");
           
            Button sureBtn = null, cancelBtn = null;
            TMP_Text message = null, title = null;
            
            using (var it = GameObjectFindUtil.BreadthFind(viewObject, sureBtnName, cancelBtnName, contentName, titleName).GetEnumerator())
            {
                while (it.MoveNext())
                {
                    GameObject gameObject = it.Current;
                    if (gameObject.name == sureBtnName)
                    {
                        sureBtn = gameObject.GetComponent<Button>();
                    }
                    else if (gameObject.name == cancelBtnName)
                    {
                        cancelBtn = gameObject.GetComponent<Button>();
                    }
                    else if (gameObject.name == contentName)
                    {
                        message = gameObject.GetComponent<TMP_Text>();
                    }
                    else if (gameObject.name == titleName)
                    {
                        title = gameObject.GetComponent<TMP_Text>();
                    }
                }
            }
            
            _comp = new (name,cancelBtn,sureBtn,message,title);
        }

        public FEnsureTipView(TMP_Text title, TMP_Text content, Button sureBtn, Button cancelBtn,
            GameObject viewObject, string viewName = null, ViewLevel level = ViewLevel.Common)
            : base(viewObject, viewName, level)
        {
            _comp = new(name, cancelBtn, sureBtn, content, title);
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
        public void Open(string title, string message, UnityAction sure, UnityAction cancel)
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
