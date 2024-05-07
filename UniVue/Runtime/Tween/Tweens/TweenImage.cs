using UnityEngine;
using UnityEngine.UI;

namespace UniVue.Tween.Tweens
{
    public sealed class TweenImage : TweenTask
    {
        private Image _image;
        private Color _startColor, _endColor;

        public TweenImage(float duration, TweenEase ease) : base(duration, ease)
        {
        }

        public void Color(Image image,Color endColor)
        {
            _image = image;
            _startColor = image.color;
            _endColor = endColor;
        }

        public override bool Execute(float deltaTime)
        {
            //计算延迟
            if (_delay > 0) { _delay -= deltaTime; return false; }

            //计算时间 保证最后一帧的位置完全匹配
            _time = (_time += deltaTime) >= _duration ? _duration : _time;

            //是否执行完
            bool executed = _time >= _duration;

            Color color = _image.color;
            color.a = TweenComputer.Compute(_ease, _time, _duration, _startColor.a, _endColor.a - _startColor.a);
            color.r = TweenComputer.Compute(_ease, _time, _duration, _startColor.r, _endColor.r - _startColor.r);
            color.g = TweenComputer.Compute(_ease, _time, _duration, _startColor.g, _endColor.g - _startColor.g);
            color.b = TweenComputer.Compute(_ease, _time, _duration, _startColor.b, _endColor.b - _startColor.b);
            _image.color = color;

            //检查loop 和 pingpong
            if (executed)
            {
                if (_loopNum > 0 && _pingPong <= 0)
                {
                    --_loopNum;
                    _image.color = _startColor;
                    executed = false;
                }
                else if (_pingPong > 0 && _loopNum <= 0)
                {
                    --_pingPong;
                    Color tmp = _startColor;
                    _startColor = _endColor;
                    _endColor = tmp;
                    executed = false;
                }

                if (!executed) { _time = 0; }
            }

            //执行回调
            if (_OnComplete != null && executed) { _OnComplete(); }

            //将下一个序列动画进行播放
            if (next != null && executed){ next.Play();}

            return executed;
        }

        public override void Reset()
        {
            base.Reset();
            _image = null;
        }
    }
}
