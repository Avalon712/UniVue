using UnityEngine.EventSystems;
using UnityEngine;
using System;

namespace UniVue.Input
{
    public sealed class MouseInput : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler,
        IPointerDownHandler,  IPointerMoveHandler, IPointerUpHandler
    {

        /// <summary>
        /// 注册鼠标事件
        /// </summary>
        public event Action<PointerEventData> onPointerClick,
                                              onPointerDown,
                                              onPointerEnter,
                                              onPointerExit,
                                              onPointerMove,
                                              onPointerUp;


        public static MouseInput ReceiveInput(GameObject receiver)
        {
            if (receiver != null)
            {
                MouseInput mouseInput = receiver.GetComponent<MouseInput>();
                if (mouseInput != null) { return mouseInput; }

                RectTransform rectTransform = receiver.GetComponent<RectTransform>();
                if (rectTransform == null) { return null; }

                mouseInput = receiver.AddComponent<MouseInput>();

                return mouseInput;
            }
            return null;
        }


        public void OnPointerEnter(PointerEventData eventData)
        {
            if (onPointerEnter != null) { onPointerEnter(eventData); }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (onPointerDown != null) { onPointerDown(eventData); }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (onPointerClick != null) { onPointerClick(eventData); }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (onPointerUp != null) { onPointerUp(eventData); }
        }

        public void OnPointerMove(PointerEventData eventData)
        {
            if (onPointerMove != null) { onPointerMove(eventData); }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (onPointerExit != null) { onPointerExit(eventData); }
        }


    }
}
