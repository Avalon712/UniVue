using System;

namespace UniVue.Tween.Tweens
{
    public sealed class TweenTimer : TweenTask
    {
        private float _interval;// 每多少秒执行一次
        private int _executeNum = 1;// 执行多少次，默认执行一次
        private bool _perFrameExecute; //是否每帧执行

        private event Action _tasks; //定时任务

        private TweenTimer() :base(){ }

        public static TweenTimer CreateTimer(Action task)
        {
            if(task == null) { return null; }
            TweenTimer timer = new TweenTimer();
            timer._tasks = task;
            return timer;
        }

        /// <summary>
        /// 是否每帧执行
        /// </summary>
        /// <param name="perFrameExecute"></param>
        /// <returns></returns>
        public TweenTimer PerFrameExecute(bool perFrameExecute)
        {
            _perFrameExecute = perFrameExecute;
            return this;
        }

        /// <summary>
        /// 执行多少次
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public TweenTimer ExecuteNum(int num)
        {
            _executeNum = num>1 ? num : 1 ;
            return this;
        }

        /// <summary>
        /// 设置间隔时间
        /// </summary>
        /// <param name="interval">每隔多少秒执行一次</param>
        /// <returns></returns>
        public TweenTimer Interval(float interval)
        {
            _interval = interval;
            return this;
        }


        /// <summary>
        /// 添加定时任务
        /// </summary>
        public TweenTimer AddTimerTask(Action task)
        {
            _tasks += task;
            return this;
        }

        /// <summary>
        /// 移除一个定时任务
        /// 注意当没有定时任务时，当前定时器将会被删除
        /// </summary>
        public void RemoveTimerTask(Action task)
        {
            _tasks -= task;
        }


        public override bool Execute(float deltaTime)
        {
            //计算延迟 或 间隔
            if (_delay > 0) { _delay -= deltaTime; return false; }

            if (!_perFrameExecute){ _delay = _interval; }

            _tasks();
            if (--_executeNum == 0)
            {
                //将下一个序列动画进行播放
                if (next != null) { next.Play(); }
                return true;
            }

            return false;
        }

        public override void Reset()
        {
            _tasks = null;
            _interval = _delay = 0;
            _executeNum = 1;
            _perFrameExecute = false;
        }
    }
}
