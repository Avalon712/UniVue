using System.Collections.Generic;
using UnityEngine;
using UniVue.Utils;

namespace UniVue.Tween
{
    public sealed class TweenTaskExecutor : MonoBehaviour
    {
        private static TweenTaskExecutor _executor;
        private List<ITweenTask> _tweens;

        public static TweenTaskExecutor GetExecutor()
        {
            if (_executor == null)
            {
                GameObject vueObject = new GameObject("UniVue_TweenTaskExecutor", typeof(TweenTaskExecutor));
                _executor = vueObject.GetComponent<TweenTaskExecutor>();
                _executor._tweens = new List<ITweenTask>();
            }
            return _executor;
        }

        internal void RemoveTween(ITweenTask tween)
        {
            for (int i = 0; i < _tweens.Count; i++)
            {
                if (_tweens[i] == tween)
                {
                    ListUtil.TrailDelete(_tweens, i--);
                    break;
                }
            }
        }

        internal void AddTween(ITweenTask tween)
        {
            _tweens.Add(tween);
        }

        private void Update()
        {
            if (_tweens.Count > 0)
            {
                //这一步是为了防止有动画在执行期间将另外一个动画给Kill()掉了从而导致这儿尾删除会出异常
                int version = _tweens.Count;
                for (int i = 0; i < _tweens.Count; i++)
                {
                    ITweenTask task = _tweens[i];
                    if (task.State == TweenState.Playing && task.Execute(Time.deltaTime))
                    {
                        task.Reset();
                        if (version == _tweens.Count)
                            ListUtil.TrailDelete(_tweens, i--);
                        else
                            _tweens.Remove(task);
                    }
                    version = _tweens.Count;
                }
            }
        }

        private void OnDestroy()
        {
            for (int i = 0; i < _tweens.Count; i++)
            {
                _tweens[i].Reset();
            }
            _tweens.Clear();
            _tweens = null;
            _executor = null;
        }

    }
}
