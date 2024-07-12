using System;
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
            _results = new List<object>();
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

        public void Filter(GameObject gameObject, IRuleFilter[] filters, bool parallel = true)
        {
            if (parallel)
                ParallelFilter(gameObject, filters);
            else
                SequenceFilter(gameObject, filters);
        }

        /// <summary>
        /// 对当前场景下视图的所有UI组件进行过滤
        /// </summary>
        public void FilterAll(IRuleFilter filter)
        {
            using (var it = Vue.Router.GetAllView().GetEnumerator())
            {
                while (it.MoveNext())
                {
                    IView view = it.Current;
                    Filter(view.ViewObject, filter);
                }
            }
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
