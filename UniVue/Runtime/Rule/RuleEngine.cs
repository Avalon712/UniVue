using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniVue.Utils;
using UniVue.View.Views;

namespace UniVue.Rule
{
    public sealed class RuleEngine
    {
        private readonly List<object> _results;

        internal RuleEngine()
        {
            _results = new List<object>(20);
        }

        public void Filter(GameObject gameObject, IRuleFilter filter)
        {
            using (var it = GlobalRule.Filter(gameObject).GetEnumerator())
            {
                while (it.MoveNext())
                {
                    ValueTuple<Component, UIType> comp = it.Current;
                    filter.Check(ref comp, _results);
                }
            }
            filter.OnComplete(_results);
            _results.Clear();
        }

        public void AsyncFilter(GameObject gameObject, IRuleFilter filter)
        {
            UnityTempObject tempObject = UnityTempObject.Instance;
            tempObject.StartCoroutine(DoAsyncFilter(gameObject, filter, tempObject.gameObject));
        }

        public void Filter(GameObject gameObject, IRuleFilter[] filters, bool parallel = false)
        {
            if (parallel)
                ParallelFilter(gameObject, filters);
            else
                SequenceFilter(gameObject, filters);
        }

        /// <summary>
        /// 对当前场景下所有根视图的所有UI组件进行过滤
        /// </summary>
        public void AsyncFilterAll(IRuleFilter filter, Action onComplete = null)
        {
            UnityTempObject tempObject = UnityTempObject.Instance;
            tempObject.StartCoroutine(DoAsyncFliterAll(filter, tempObject.gameObject, onComplete));
        }

        private IEnumerator DoAsyncFliterAll(IRuleFilter filter, GameObject tempObject, Action onComplete)
        {
            WaitForEndOfFrame waitOneFrame = new WaitForEndOfFrame();
            using (var it = Vue.Router.GetAllView().GetEnumerator())
            {
                while (it.MoveNext())
                {
                    IView view = it.Current;
                    if (string.IsNullOrEmpty(view.Parent))
                    {
                        using (var it2 = GlobalRule.Filter(view.ViewObject).GetEnumerator())
                        {
                            while (it2.MoveNext())
                            {
                                ValueTuple<Component, UIType> comp = it2.Current;
                                filter.Check(ref comp, _results);
                                yield return waitOneFrame;
                            }
                        }
                    }
                }
            }
            UnityEngine.Object.Destroy(tempObject);
            onComplete?.Invoke();
        }

        private IEnumerator DoAsyncFilter(GameObject gameObject, IRuleFilter filter, GameObject tempObject)
        {
            WaitForEndOfFrame waitOneFrame = new WaitForEndOfFrame();
            using (var it = GlobalRule.Filter(gameObject).GetEnumerator())
            {
                while (it.MoveNext())
                {
                    ValueTuple<Component, UIType> comp = it.Current;
                    filter.Check(ref comp, _results);
                    yield return waitOneFrame;
                }
            }
            filter.OnComplete(_results);
            _results.Clear();
            UnityEngine.Object.Destroy(tempObject);
        }

        private void SequenceFilter(GameObject gameObject, IRuleFilter[] filters)
        {
            for (int i = 0; i < filters.Length; i++)
            {
                Filter(gameObject, filters[i]);
            }
        }

        private void ParallelFilter(GameObject gameObject, IRuleFilter[] filters)
        {
            List<object>[] results = new List<object>[filters.Length];
            for (int i = 0; i < filters.Length; i++)
            {
                results[i] = new List<object>();
            }

            using (var it = GlobalRule.Filter(gameObject).GetEnumerator())
            {
                while (it.MoveNext())
                {
                    ValueTuple<Component, UIType> comp = it.Current;
                    for (int i = 0; i < filters.Length; i++)
                    {
                        filters[i].Check(ref comp, results[i]);
                    }
                }
            }

            for (int i = 0; i < filters.Length; i++)
            {
                filters[i].OnComplete(results[i]);
                results[i].Clear();
            }
        }
    }
}
