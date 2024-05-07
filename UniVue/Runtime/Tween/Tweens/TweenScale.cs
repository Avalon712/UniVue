using UnityEngine;

namespace UniVue.Tween.Tweens
{
    public sealed class TweenScale : TweenTask
    {
        private Transform _transform;
        private Vector3 _startScale, _endScale;

        public TweenScale(float duration, TweenEase ease) : base(duration, ease)
        {
        }

        public void Scale(Transform transform, Vector3 endScale)
        {
            _transform = transform;
            _startScale = transform.localScale;
            _endScale = endScale;
        }

        public override bool Execute(float deltaTime)
        {
            //计算延迟
            if (_delay > 0) { _delay -= deltaTime; return false; }

            //计算时间 保证最后一帧的位置完全匹配
            _time = (_time += deltaTime) >= _duration ? _duration : _time;

            //是否执行完
            bool executed = _time >= _duration;

            Vector3 localScale = _transform.localScale;
            localScale.x = TweenComputer.Compute(_ease, _time, _duration, _startScale.x, _endScale.x - _startScale.x);
            localScale.y = TweenComputer.Compute(_ease, _time, _duration, _startScale.y, _endScale.y - _startScale.y);
            localScale.z = TweenComputer.Compute(_ease, _time, _duration, _startScale.z, _endScale.z - _startScale.z);
            _transform.localScale = localScale;

            //检查loop 和 pingpong
            if (executed)
            {
                if (_loopNum > 0 && _pingPong <= 0)
                {
                    --_loopNum;
                    _transform.localScale = _startScale;
                    executed = false;
                }
                else if (_pingPong > 0 && _loopNum <= 0)
                {
                    --_pingPong;
                    Vector3 tmp = _startScale;
                    _startScale = _endScale;
                    _endScale = tmp;
                    executed = false;
                }

                if (!executed) { _time = 0; }
            }

            //执行回调
            if(_OnComplete!=null && executed) { _OnComplete(); }

            //将下一个序列动画进行播放
            if (next != null && executed) { next.Play(); }

            return executed;
        }

        public override void Reset()
        {
            base.Reset();
            _transform = null;
        }
    }
}
