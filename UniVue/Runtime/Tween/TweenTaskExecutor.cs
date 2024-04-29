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
                for (int i = 0; i < _tweens.Count; i++)
                {
                    if (_tweens[i].Execute(Time.deltaTime))
                    {
                        _tweens[i].Reset();
                        ListUtil.TrailDelete(_tweens, i--);
                    }
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
