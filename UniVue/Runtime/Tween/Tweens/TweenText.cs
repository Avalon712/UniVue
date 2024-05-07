using TMPro;
using UnityEngine;

namespace UniVue.Tween.Tweens
{
    public sealed class TweenText : TweenTask
    {
        private TMP_Text _text;

        public TweenText(float duration, TweenEase ease) : base(duration, ease)
        {
        }

        public void Text(TMP_Text text,string str)
        {
            _text = text;
            text.maxVisibleCharacters = 0;
            text.text = str;
        }

        public override bool Execute(float deltaTime)
        {
            //计算延迟
            if (_delay > 0) { _delay -= deltaTime; return false; }

            //计算时间 保证最后一帧的位置完全匹配
            _time = (_time += deltaTime) >= _duration ? _duration : _time;

            //是否执行完
            bool executed = _time >= _duration;

            float f = TweenComputer.Compute(_ease, _time, _duration, 0, _text.text.Length);
            _text.maxVisibleCharacters = Mathf.RoundToInt(f);

            //检查loop
            if (executed)
            {
                if (_loopNum > 0 && _pingPong <= 0)
                {
                    --_loopNum;
                    _text.maxVisibleCharacters = 0;
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
            _text = null;
        }
    }
}
