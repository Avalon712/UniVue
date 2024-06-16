using System;
using UnityEngine;
using UnityEngine.UI;
using UniVue.Input;
using UniVue.Tween;
using UniVue.Tween.Tweens;

namespace UniVue.View.Widgets
{
    /// <summary>
    /// 轮播图组件
    /// </summary>
    public sealed class Carousel : Widget
    {
        private int _currPage;                      //当前页
        private float _perPageScrollTime;           //滚动一页需要的时间
        private float _intervalTime;                //每隔多少秒切换一次
        private RectTransform _content;             //显示内容区域
        private RectTransform _viewport;            //视口区域
        private Vector2 _deltaPos;                  //右边Item减去左边的Item的位置差
        private INavigator _navigator;              //导航
        private TweenTask _scrollTween;             //滚动动画
        private TweenTask _timer;                   //定时缓动
        private Vector3[] _corners;                  //第一个Item的四个角的世界坐标
        private Vector3[] _viewportCorners;          //视口区域的四个角的世界坐标
        private int _count;

        /// <summary>
        /// 视口区域
        /// </summary>
        public RectTransform Viewport => _viewport;

        /// <summary>
        /// 显示内容区域
        /// </summary>
        public RectTransform Content => _content;


        /// <summary>
        /// 页面数量
        /// </summary>
        public int Count => _count;

        /// <summary>
        /// 手动创建组件
        /// </summary>
        /// <remarks>Content下的GameObject将作为每个滚动页，同时默认显示的是第零页</remarks>
        /// <param name="viewport">视口区域</param>
        /// <param name="content">装视图的区域</param>
        /// <param name="distance">rightItem.localPos.x - leftItem.localPos.x</param>
        /// <param name="intervalTime">每隔多少秒切换一次</param>
        public Carousel(RectTransform viewport, RectTransform content, float distance, float intervalTime, float perPageScrollTime = 0.1f)
        {
            _viewport = viewport;
            _content = content;
            _count = content.childCount;
            _deltaPos = new Vector3(distance, 0);
            _perPageScrollTime = Mathf.Clamp(perPageScrollTime, 0.1f, 5f);
            _intervalTime = Mathf.Clamp(intervalTime, 1f, 60f);
            _viewportCorners = new Vector3[4];
            _corners = new Vector3[4];
            _timer = TweenBehavior.Timer(() => ScrollTo((_currPage + 1) % Count))
                .Interval(_intervalTime).ExecuteNum(int.MaxValue).Delay(_intervalTime);
            _timer.Pause(); //设为暂停状态
        }

        /// <summary>
        /// 绑定轮播图导航栏
        /// </summary>
        /// <remarks>索引就代表其对应的页码</remarks>
        /// <param name="navigators">导航器</param>
        /// <param name="interactive">是否允许通过导航栏设置当前显示的页</param>
        public void UseNavigators(Toggle[] navigators, bool interactive = true)
        {
            CheckCount(navigators.Length);
            _navigator = new ToggleNavigator(ScrollTo, navigators, interactive);
        }

        /// <summary>
        /// 绑定轮播图导航栏
        /// </summary>
        /// <remarks>索引就代表其对应的页码</remarks>
        /// <param name="navigators">导航器</param>
        /// <param name="interactive">是否允许通过导航栏设置当前显示的页</param>
        /// <param name="active">处于当前页是高亮的颜色</param>
        /// <param name="disactive">没有处于当前页时的颜色</param>
        public void UseNavigators(Button[] navigators, Color active, Color disactive, bool interactive = true)
        {
            CheckCount(navigators.Length);
            _navigator = new ButtonNavigator(ScrollTo, navigators, interactive, active, disactive);
        }

        /// <summary>
        /// 绑定轮播图导航栏
        /// </summary>
        /// <remarks>索引就代表其对应的页码</remarks>
        /// <param name="nextPageBtn">点击显示下一页按钮</param>
        /// <param name="lastPageBtn">点击显示上一页按钮</param>
        public void UseNavigators(Button nextPageBtn, Button lastPageBtn)
        {
            nextPageBtn.onClick.AddListener(() => ScrollTo((_currPage + 1) % Count));
            lastPageBtn.onClick.AddListener(() => ScrollTo((_currPage + Count - 1) % Count));
        }

        /// <summary>
        /// 绑定用于显示当前是位于第几页的状态栏
        /// </summary>
        /// <remarks>索引就代表其对应的页码</remarks>
        /// <param name="navigators">显示状态的图片</param>
        /// <param name="active">处于当前页是高亮的颜色</param>
        /// <param name="disactive">没有处于当前页时的颜色</param>
        public void UseNavigators(Image[] navigators, Color active, Color disactive)
        {
            CheckCount(navigators.Length);
            _navigator = new ImageNavigator(navigators, active, disactive);
        }

        /// <summary>
        /// 滚动到指定页
        /// </summary>
        /// <param name="pageNumber">页数</param>
        public void ScrollTo(int pageNumber)
        {
            //必须等待动画完成
            if (_scrollTween != null || pageNumber == _currPage) return;

            //暂停住定时器
            _timer.Pause();

            Vector3 startPos = _content.anchoredPosition;
            startPos.x = ((int)(startPos.x / _deltaPos.x)) * _deltaPos.x;

            Vector3 sumDeltaPos;
            //从最后一页滚动到第一页或从第一页滚动到最后一页时需要特殊处理
            if (_currPage == 0 && pageNumber == Count - 1)
                sumDeltaPos = _deltaPos;
            else if (_currPage == Count - 1 && pageNumber == 0)
                sumDeltaPos = -1 * _deltaPos;
            else
                sumDeltaPos = (_currPage - pageNumber) * _deltaPos;

            Vector3 endPos = startPos + sumDeltaPos;

            float gap = Mathf.Abs(sumDeltaPos.x / _deltaPos.x);
            float duration = gap * _perPageScrollTime;

            //Debug.Log($"sumDeltaPos ={sumDeltaPos} currPage = {_currPage} pageNumber={pageNumber} duration = {duration}, gap = {gap}");

            _currPage = pageNumber;

            //不需要太高的刷新频率
            float interval = 0.1f; //每隔多少秒执行一次
            int executeNum = 10 * (int)duration; //执行次数

            //如果是向左滚动则调用频率取决于下一页到当前页的距离
            if (sumDeltaPos.x < 0)
            {
                interval = duration / gap;
                executeNum = Mathf.FloorToInt(gap + 1);
            }

            TweenTask onPosChanged = TweenBehavior.Timer(OnPositionChanged).Interval(interval).ExecuteNum(executeNum);
            _scrollTween = TweenBehavior.DoAnchorMove(_content, duration, endPos)
                .Call(() =>
                {
                    onPosChanged.Kill(); //杀死位置改变函数
                    _timer.Play(); //恢复定时器
                    _scrollTween = null;
                    _navigator?.Active(_currPage);
                });
        }

        /// <summary>
        /// 监听屏幕输入
        /// </summary>
        /// <remarks>监听之后可以通过手指或鼠标进行左右滑动切换上一页或下一页</remarks>
        public void ListenScreenInput()
        {
            DragInput input = _viewport.GetComponent<DragInput>();
            if (input == null)
                input = _viewport.gameObject.AddComponent<DragInput>();

            input.Draggable = false; //不允许拖拽
            Vector2 start = Vector2.zero;
            input.onBeginDrag += p => start = p.position;
            input.onEndDrag += p =>
            {
                //右滑动
                if (p.position.x > start.x)
                    ScrollTo((_currPage + Count - 1) % Count);
                //左滑动
                else if (p.position.x < start.x)
                    ScrollTo((_currPage + 1) % Count);
            };
        }

        /// <summary>
        /// 暂停缓动定时器
        /// </summary>
        /// <remarks>当关闭视图时你应该调用此函数，以此减少没有必要的计算开销</remarks>
        public void PauseTimer()
        {
            _timer.Pause();
        }

        /// <summary>
        /// 开始缓动定时器
        /// </summary>
        /// <remarks>当打开视图时你应该调用此函数</remarks>
        public void StartTimer()
        {
            _timer.Play();
        }

        public override void Destroy()
        {
            _content = _viewport = null;
            _navigator?.Destroy();
            _navigator = null;
        }

        private void CheckCount(int count)
        {
            if (count != Count)
                throw new Exception($"导航状态栏的数量{count}必须与轮播图的数量{Count}一致");
        }

        #region 算法实现


        private void OnPositionChanged()
        {
            //计算出视口区域的四个角
            _viewport.GetWorldCorners(_viewportCorners);
            (_content.GetChild(0) as RectTransform).GetWorldCorners(_corners);
            //只需监听第一个
            HorizontalListenCorners(_content, _viewportCorners, _corners);
        }

        /// <summary>
        /// 水平滚动
        /// </summary>
        private void HorizontalListenCorners(Transform content, Vector3[] viewportCorners, Vector3[] corners)
        {
            int childCount = content.childCount;
            if (corners[3].x - 0.05f < viewportCorners[0].x) //向左移动了一个Item的距离
            {
                RectTransform page = content.GetChild(0) as RectTransform;
                page.anchoredPosition = (content.GetChild(childCount - 1) as RectTransform).anchoredPosition + _deltaPos;
                page.SetAsLastSibling(); //设置为最后一个位置
            }
            else if (corners[3].x > viewportCorners[3].x) //正在在向右移动
            {
                RectTransform page = content.GetChild(childCount - 1) as RectTransform;
                page.anchoredPosition = (content.GetChild(0) as RectTransform).anchoredPosition - _deltaPos;
                page.SetAsFirstSibling(); //最后一个设置为第一个位置
            }
        }

        #endregion
    }

    internal interface INavigator
    {
        void Active(int pageNumber);

        void Destroy();
    }

    internal sealed class ToggleNavigator : INavigator
    {
        private Toggle[] _states;
        private Toggle _last;
        Action<int> _scrollTo;
        private bool _flag;

        public ToggleNavigator(Action<int> scrollTo, Toggle[] states, bool interactive)
        {
            CheckToggleGroup(states);
            _states = states;
            _scrollTo = scrollTo;
            _last = _states[0];
            if (interactive)
            {
                for (int i = 0; i < states.Length; i++)
                {
                    states[i].onValueChanged.AddListener(ScrollTo);
                }
            }
        }

        private void ScrollTo(bool on)
        {
            if (on && _flag)
            {
                for (int i = 0; i < _states.Length; i++)
                {
                    if (_states[i].isOn)
                    {
                        _scrollTo.Invoke(i);
                        break;
                    }
                }
            }
            else if (on)
                _flag = true;
        }

        public void Active(int pageNumber)
        {
            Toggle active = _states[pageNumber];
            _flag = false;
            active.isOn = true;
            _last = active;
        }

        public void Destroy()
        {
            for (int i = 0; i < _states.Length; i++)
            {
                _states[i].onValueChanged.RemoveListener(ScrollTo);
            }
            _states = null;
            _last = null;
        }

        private void CheckToggleGroup(Toggle[] states)
        {
            ToggleGroup group = states[0].group;
            bool threw = group == null;

            for (int i = 0; i < states.Length && !threw; i++)
                threw = ReferenceEquals(states[i], group);

            if (threw)
                throw new Exception("绑定的导航栏Toggle必须关联一个相同的ToggleGroup组件");
        }
    }

    internal sealed class ButtonNavigator : INavigator
    {
        private Button[] _states;
        private ImageNavigator _navigatorImg;
        private Action<int> _scrollTo;

        public ButtonNavigator(Action<int> scrollTo, Button[] states, bool interactive, Color active = default, Color disactive = default)
        {
            _states = states;
            _scrollTo = scrollTo;
            Image[] images = new Image[states.Length];
            if (interactive)
            {
                for (int i = 0; i < states.Length; i++)
                {
                    int pageNumber = i;
                    states[i].onClick.AddListener(() => _scrollTo.Invoke(pageNumber));
                    if (states[i].TryGetComponent(out Image image))
                        images[i] = image;
                }
            }
            if (images[0] != null)
                _navigatorImg = new ImageNavigator(images, active, disactive);
        }


        public void Active(int pageNumber)
        {
            Button active = _states[pageNumber];
            _navigatorImg?.Active(pageNumber);
        }

        public void Destroy()
        {
            for (int i = 0; i < _states.Length; i++)
                _states[i].onClick.RemoveAllListeners();

            _navigatorImg?.Destroy();
            _navigatorImg = null;
            _states = null;
        }
    }

    internal sealed class ImageNavigator : INavigator
    {
        private Image[] _states;
        private Color _activeColor, _disactiveColor;
        private Image _last;

        public ImageNavigator(Image[] states, Color active = default, Color disactive = default)
        {
            _states = states;
            _last = _states[0];
            _activeColor = active;
            _disactiveColor = disactive;
        }


        public void Active(int pageNumber)
        {
            Image active = _states[pageNumber];
            active.color = _activeColor;
            _last.color = _disactiveColor;
            _last = active;
        }

        public void Destroy()
        {
            _states = null;
            _last = null;
        }
    }
}
