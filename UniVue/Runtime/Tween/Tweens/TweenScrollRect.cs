using UnityEngine;
using UnityEngine.UI;

namespace UniVue.Tween.Tweens
{
    public sealed class TweenScrollRect : TweenTask
    {
        private ScrollRect _scrollRect;
        private Vector2 _start, _end;

        public TweenScrollRect(float duration, TweenEase ease) : base(duration, ease)
        {
        }

        public void Scroll(ScrollRect scrollRect,Vector2 end)
        {
            _scrollRect = scrollRect;
            _start = scrollRect.normalizedPosition;
            _end = end;
        }

        public override bool Execute(float deltaTime)
        {
            //计算时间 保证最后一帧的位置完全匹配
            _time = (_time += deltaTime) >= _duration ? _duration : _time;

            //是否执行完
            bool executed = _time >= _duration;

            Vector2 pos = _scrollRect.normalizedPosition;
            pos.x = TweenComputer.Linear(_time, _duration, _start.x, _end.x - _start.x);
            pos.y = TweenComputer.Linear( _time, _duration, _start.y, _end.y - _start.y);
            _scrollRect.normalizedPosition = pos;
            _scrollRect.onValueChanged.Invoke(pos);

            if(_OnComplete!=null && executed) { _OnComplete(); }

            //将下一个序列动画进行播放
            if (next != null && executed) { next.Play(); }

            return executed;
        }

        public override void Reset()
        {
            base.Reset();
            _scrollRect = null;
        }
    }
}
