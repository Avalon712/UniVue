using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UniVue.Utils;

namespace UniVue.Input
{
    public sealed class DragInput : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private bool _draggable = true;             //是否可以进行拖拽
        [SerializeField] private bool _realtimeCalculateLimitArea;   //是否实时计算限制区域
        [SerializeField] private RectTransform _limitArea;           //限制区域

        private RectTransform _mover;                                //移动者
        private Vector3 _offset;                                     //开始拖拽是鼠标与UI位置的偏移量
        private float _minX, _maxX, _minY, _maxY;                    //被限制的区域的四个角的坐标
        private Vector3[] _corners;                                  //用于计算限制区域的四个角

        /// <summary>
        /// 对外提供可注册事件
        /// </summary>
        public event UnityAction<PointerEventData> onBeginDrag, onDrag, onEndDrag;

        /// <summary>
        /// 是否能够使得UI进行拖拽，默认为false
        /// </summary>
        public bool Draggable { get => _draggable; set => _draggable = value; }

        /// <summary>
        /// 拖拽限制区域
        /// </summary>
        public RectTransform LimitArea
        {
            get => _limitArea;
            set => _limitArea = value;
        }

        /// <summary>
        /// 是否实时计算限制区域
        /// </summary>
        /// <remarks>
        /// 如果一个UI元素（称为A）被限制在Canvas下进行拖拽，A的子元素B被限制在A中进行拖拽时，此时需要
        /// B元素身上挂载的DragInput脚本需要开启实现计算，否则会出现不正确的限制区域。即两个具有父子或
        /// 祖孙关系的元素拥有不同的限制区域时需要开启此属性。
        /// </remarks>
        public bool RealtimeCalculateLimitArea
        {
            get => _realtimeCalculateLimitArea;
            set => _realtimeCalculateLimitArea = value;
        }

        private void Start()
        {
            _mover = GetComponent<RectTransform>();

            if (_limitArea == null)
                _limitArea = ComponentFindUtil.LookUpFindComponent<Canvas>(_mover.gameObject).GetComponent<RectTransform>();

            CalculateMovementArea();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (onBeginDrag != null) { onBeginDrag(eventData); }

            if (Draggable)
            {
                //将鼠标的屏幕坐标转换成_rectTransform平面下的世界坐标
                if (RectTransformUtility.ScreenPointToWorldPointInRectangle(_mover, eventData.position, eventData.enterEventCamera, out Vector3 mousePos))
                {
                    //计算UI和指针之间的位置偏移量
                    _offset = _mover.position - mousePos;
                }
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (onDrag != null) { onDrag(eventData); }

            if (Draggable)
            {
                if (RectTransformUtility.ScreenPointToWorldPointInRectangle(_mover, eventData.position, eventData.enterEventCamera, out Vector3 mousePos))
                {
                    if (RealtimeCalculateLimitArea)
                    {
                        CalculateMovementArea();
                    }
                    Vector3 currPos = _offset + new Vector3(mousePos.x, mousePos.y, 0);
                    currPos.x = Mathf.Clamp(currPos.x, _minX, _maxX);
                    currPos.y = Mathf.Clamp(currPos.y, _minY, _maxY);
                    currPos.z = 0;
                    _mover.position = currPos;
                }
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (onEndDrag != null) { onEndDrag(eventData); }
        }


        /// <summary>
        /// 计算可移动区域
        /// </summary>
        private void CalculateMovementArea()
        {
            _corners = new Vector3[4];
            _limitArea.GetComponent<RectTransform>().GetWorldCorners(_corners);
            _minX = _corners[0].x;
            _maxX = _corners[3].x;
            _minY = _corners[0].y;
            _maxY = _corners[1].y;

            _mover.GetWorldCorners(_corners);
            float halfWidth = Mathf.Abs(_corners[0].x - _corners[3].x) / 2;
            float halfHeight = Mathf.Abs(_corners[0].y - _corners[1].y) / 2;

            _minX += halfWidth;
            _maxX -= halfWidth;
            _minY += halfHeight;
            _maxY -= halfHeight;
        }
    }

    [Serializable]
    public sealed class DragInputConfig
    {
        [Header("是否可以进行拖拽移动")]
        [SerializeField] private bool _draggable = true;
        [Header("拖拽限制区域的GameObject的名字")]
        [SerializeField] private string _limitArea;
        [Header("被拖拽的GameObject的名字")]
        [SerializeField] private string _mover;
        [Header("是否实时计算拖拽区域")]
        [Tooltip("如果其祖先或父物体也可以拖拽请设置为true")]
        [SerializeField] private bool _realtimeCalculateLimitArea;

        public bool Draggable
        {
            get => _draggable;
            set => _draggable = value;
        }

        public string LimitArea => _limitArea;

        public string Mover => _mover;

        public bool RealtimeCalculateLimitArea => _realtimeCalculateLimitArea;
    }

}

