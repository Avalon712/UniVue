using System;

namespace UniVue.Tween.Tweens
{
    public sealed class TweenCustom : TweenTask
    {
        private Action<float, int, int> _tween;

        /// <summary>
        /// 构造函数：
        /// 参数1是动画已经播放了多长时间
        /// 参数2是当前循环次数
        /// 参数3是当前pingpong次数
        /// </summary>
        /// <param name="tween">参数是动画已经播放了多长时间</param>
        public TweenCustom(Action<float,int,int> tween,float duration,TweenEase ease) :base(duration,ease){
            _tween = tween;
        }

        public override bool Execute(float deltaTime)
        {
            //计算延迟
            if (_delay > 0) { _delay -= deltaTime; return false; }

            //计算时间 保证最后一帧的位置完全匹配
            _time = (_time += deltaTime) >= _duration ? _duration : _time;

            bool executed = _time >= _duration;
            _tween(_time,_loopNum,_pingPong);

            //检查loop 和 pingpong
            if (executed)
            {
                if (_loopNum > 0 && _pingPong <= 0)
                {
                    --_loopNum;
                    executed = false;
                }
                else if (_pingPong > 0 && _loopNum <= 0)
                {
                    --_pingPong;
                    executed = false;
                }

                if (!executed) { _time = 0; }
            }

            //执行回调
            if (_OnComplete != null && executed) { _OnComplete(); }

            //将下一个序列动画进行播放
            if (next != null && executed) { next.Play(); }

            return executed;
        }

        public override void Reset()
        {
            _tween = null;
        }
    }
}
