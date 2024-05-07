using UnityEngine;

namespace UniVue.Tween.Tweens
{
    public sealed class TweenRotation : TweenTask
    {
        private Transform _transform;
        private Vector3 _startRotation, _endRotation,_rotation;
        private bool _isLocal;

        public TweenRotation(float duration, TweenEase ease) : base(duration, ease)
        {
        }

        public void Rotation(Transform transform,Vector3 end)
        {
            _transform = transform;
            _startRotation = transform.rotation.eulerAngles;
            _endRotation = end;
            _isLocal = false;
        }

        public void LocalRotation(Transform transform, Vector3 end)
        {
            _transform = transform;
            _startRotation = transform.localRotation.eulerAngles;
            _endRotation = end;
            _isLocal = true;
        }


        public override bool Execute(float deltaTime)
        {            //计算延迟
            if (_delay > 0) { _delay -= deltaTime; return false; }

            //计算时间 保证最后一帧的位置完全匹配
            _time = (_time += deltaTime) >= _duration ? _duration : _time;

            //是否执行完
            bool executed = _time >= _duration;

            _rotation.x = TweenComputer.Compute(_ease, _time, _duration, _startRotation.x, _endRotation.x - _startRotation.x);
            _rotation.y = TweenComputer.Compute(_ease, _time, _duration, _startRotation.y, _endRotation.y - _startRotation.y);
            _rotation.z = TweenComputer.Compute(_ease, _time, _duration, _startRotation.z, _endRotation.z - _startRotation.z);
            
            if (_isLocal) { _transform.localRotation = Quaternion.Euler(_rotation); }
            else{ _transform.rotation = Quaternion.Euler(_rotation); }

            //检查loop 和 pingpong
            if (executed)
            {
                if (_loopNum > 0 && _pingPong <= 0)
                {
                    --_loopNum;
                    if (_isLocal) { _transform.localRotation = Quaternion.Euler(_startRotation); }
                    else { _transform.rotation = Quaternion.Euler(_startRotation); }
                    executed = false;
                }
                else if (_pingPong > 0 && _loopNum <= 0)
                {
                    --_pingPong;
                    Vector3 tmp = _startRotation;
                    _startRotation = _endRotation;
                    _endRotation = tmp;
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
