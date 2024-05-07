using System;

namespace UniVue.Tween.Tweens
{
    public abstract class TweenTask : ITweenTask
    {
        protected TweenState _state = TweenState.Idel; //动画状态
        protected TweenEase _ease = TweenEase.Linear; //缓动曲线
        protected float _time; //当前动画时间
        protected float _delay;//延迟时间
        protected int _loopNum;//循环次数
        protected int _pingPong; //回合次数
        protected float _duration; //持续时间
        protected Action _OnComplete; //回调函数

        public TweenState State => _state;

        /// <summary>
        /// 序列动画
        /// </summary>
        internal TweenTask next;

        /// <summary>
        /// 创建序列动画
        /// </summary>
        /// <returns>previous</returns>
        public static TweenTask operator +(TweenTask previous, TweenTask after)
        {
            if (previous.next == null)
            {
                previous.next = after;
                previous.Pause();
                after.Pause();
            }
            return previous;
        }

        protected TweenTask() { TweenTaskExecutor.GetExecutor().AddTween(this); }

        public TweenTask(float duration,TweenEase ease) : this()
        {
            _duration = duration; _ease = ease;
        }

        public TweenTask Call(Action callback)
        {
            _OnComplete += callback;
            return this;
        }

        public abstract bool Execute(float deltaTime);

        /// <summary>
        /// 设置延迟执行
        /// </summary>
        /// <param name="delay">延迟多少秒后开始执行</param>
        public TweenTask Delay(float delay)
        {
            _delay = delay;
            return this;
        }

        /// <summary>
        /// 杀死动画，不会恢复到动画的初始状态
        /// </summary>
        /// <param name="isExecuteCall">是否执行回调函数</param>
        public void Kill(bool isExecuteCall = false)
        {
            if (_OnComplete != null && isExecuteCall) { _OnComplete(); }

            if (_state == TweenState.Playing)
            {
                TweenTaskExecutor.GetExecutor().RemoveTween(this);
                Reset();
            }
        }

        /// <summary>
        /// 循环执行，每次都从起点重新开始
        /// 注: 如果即设置了 loop又设置了pingpong，那么loop设置将会先执行
        /// </summary>
        /// <param name="loopNum">循环次数</param>
        public TweenTask Loop(int loopNum)
        {
            _loopNum = loopNum - 1; //减一是因为默认就会执行一次
            return this;
        }

        /// <summary>
        /// 来回执行
        /// 注: 如果即设置了 loop又设置了pingpong，那么loop设置将会先执行
        /// </summary>
        /// <param name="num">执行的回合数</param>
        public TweenTask PingPong(int num = 1)
        {
            if (num > 1) { num += 1; }
            _pingPong = num;
            return this;
        }

        /// <summary>
        /// 暂停动画
        /// </summary>
        public void Pause()
        {
            if (_state == TweenState.Playing)
            {
                TweenTaskExecutor.GetExecutor().RemoveTween(this);
            }
        }

        /// <summary>
        /// 播放动画
        /// </summary>
        public void Play()
        {
            if (_state == TweenState.Paused)
            {
                TweenTaskExecutor.GetExecutor().AddTween(this);
            }
        }

        /// <summary>
        /// 重置
        /// </summary>
        public virtual void Reset()
        {
            _delay = _duration = _time = _loopNum = _pingPong = 0;
            _state = TweenState.Idel;
            _ease = TweenEase.Linear;
            next = null;
        }
    }
}
