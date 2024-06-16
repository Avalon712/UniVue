using UnityEngine;

namespace UniVue.Tween.Tweens
{
    public sealed class TweenAnchorMove : TweenTask
    {
        private RectTransform _transform;
        private Vector3 _startPos, _endPos, _pos;

        public TweenAnchorMove(float duration, TweenEase ease) : base(duration, ease)
        {
        }

        public void Move(RectTransform transform, Vector3 endPos)
        {
            _transform = transform;
            _startPos = transform.anchoredPosition;
            _endPos = endPos;
        }


        public override bool Execute(float deltaTime)
        {
            //计算延迟
            if (_delay > 0) { _delay -= deltaTime; return false; }

            //计算时间 保证最后一帧的位置完全匹配
            _time = (_time += deltaTime) >= _duration ? _duration : _time;

            //是否执行完
            bool executed = _time >= _duration;

            _pos.x = TweenComputer.Compute(_ease, _time, _duration, _startPos.x, _endPos.x - _startPos.x);
            _pos.y = TweenComputer.Compute(_ease, _time, _duration, _startPos.y, _endPos.y - _startPos.y);
            _pos.z = TweenComputer.Compute(_ease, _time, _duration, _startPos.z, _endPos.z - _startPos.z);

            _transform.anchoredPosition = _pos;

            //检查loop 和 pingpong
            if (executed)
            {
                if (_loopNum > 0 && _pingPong <= 0)
                {
                    --_loopNum;
                    _transform.anchoredPosition = _startPos;
                    executed = false;
                }
                else if (_pingPong > 0 && _loopNum <= 0)
                {
                    --_pingPong;
                    Vector3 tmp = _startPos;
                    _startPos = _endPos;
                    _endPos = tmp;
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
            base.Reset();
            _transform = null;
        }
    }
}
