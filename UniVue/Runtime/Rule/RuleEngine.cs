using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniVue.Common;

namespace UniVue.Rule
{
    public sealed class RuleEngine
    {
        private readonly char _ignoreSymbol, _skipSymbol;
        private readonly string _ruleSeparator;
        private readonly ArrayPool _pool;

        private sealed class ArrayPool
        {
            private IRule[] _pool;

            public ArrayPool(int capacity)
            {
                _pool = new IRule[capacity];
            }

            public Span<IRule> Rent(int length)
            {
                int len = 0;
                for (int i = 0; i < _pool.Length; i++)
                {
                    if (_pool[i] == null)
                    {
                        int startIndex = i;
                        for (int j = i; j < _pool.Length; j++)
                        {
                            if (_pool[j] == null)
                            {
                                len++;
                                if (len == length)
                                {
                                    //找到足够长的
                                    return _pool.AsSpan().Slice(startIndex, length);
                                }
                            }
                            else
                            {
                                len = 0;
                                i = j + 1;
                                break;
                            }
                        }
                    }
                }
                return new IRule[length];
            }

            public void Return(in Span<IRule> rent)
            {
                rent.Fill(null);
            }
        }

        internal RuleEngine()
        {
            _pool = new ArrayPool(10);
            _ruleSeparator = Vue.Config.RuleSeparator;
            _ignoreSymbol = Vue.Config.IgnoreSymbol;
            _skipSymbol = Vue.Config.SkipSymbol;
        }

        /// <summary>
        /// 当前正在被执行规则的GameObject
        /// </summary>
        /// <remarks>仅在IRule.Check()和IRule.OnCompleted()方法中可以访问到此对象</remarks>
        public GameObject TargetObject { get; private set; }

        /// <summary>
        /// 从池中获取一块指定长度的数组
        /// </summary>
        /// <remarks>当规则执行完后会自动对申请的这块数组进行回收</remarks>
        public Span<IRule> GetArray(int length)
        {
            return _pool.Rent(length);
        }

        public void Execute(GameObject gameObject, IRule rule)
        {
            Span<IRule> rules = _pool.Rent(1);
            rules.Fill(rule);
            Execute(gameObject, rules);
        }

        public void ExecuteAsync(GameObject gameObject, IRule rule)
        {
            ExecuteAsync(gameObject, new IRule[1] { rule });
        }

        public void Execute(GameObject gameObject, in Span<IRule> rules)
        {
            DepthSearch(gameObject, rules); //第一个GameObject不进行ViewObject检查操作
            for (int i = 0; i < rules.Length; i++)
            {
                TargetObject = gameObject;
                rules[i].OnComplete();
            }
            _pool.Return(rules);
            TargetObject = null;
        }

        public void ExecuteAsync(GameObject gameObject, IRule[] rules)
        {
            var temp = UnityTempObject.Temp;
            temp.StartCoroutine(DepthSearchAsync(gameObject, rules, temp.gameObject));//第一个GameObject不进行ViewObject检查操作
        }

        /// <summary>
        /// 对指定的所有ViewObject执行视图加载规则
        /// </summary>
        /// <param name="viewObjects">视图对象</param>
        internal void ExecuteLoadViewRule(IEnumerable<GameObject> viewObjects)
        {
            RouteRule routeRule = new RouteRule(null);
            EventRule eventRule = new EventRule(null);
            Span<IRule> rules = _pool.Rent(2);
            rules[0] = routeRule;
            rules[1] = eventRule;
            using (var it = viewObjects.GetEnumerator())
            {
                while (it.MoveNext())
                {
                    GameObject gameObject = it.Current;
                    string viewName = gameObject.name;
                    routeRule.ViewName = viewName;
                    eventRule.ViewName = viewName;
                    TargetObject = gameObject;
                    DepthSearch(gameObject, rules); //第一个GameObject不进行ViewObject检查操作
                    routeRule.OnComplete();
                    eventRule.OnComplete();
                    TargetObject = null;
                }
            }
            _pool.Return(rules);
        }


        private void DepthSearch(GameObject gameObject, in Span<IRule> rules)
        {
            //忽略此节点的条件：名称以忽略符号开头
            if (gameObject == null || gameObject.name.StartsWith(_ignoreSymbol)) return;

            Transform transform = gameObject.transform;
            if (UITypeUtil.TryGetUI(gameObject, out UIType type, out Component ui))
            {
                TargetObject = gameObject;
                string[] checkRules = gameObject.name.Split(_ruleSeparator);
                for (int i = 0; i < rules.Length; i++)
                {
                    for (int j = 0; j < checkRules.Length; j++)
                    {
                        rules[i].Check(checkRules[j], type, ui);
                    }
                }
            }
            int childNum = transform.childCount;
            for (int i = 0; i < childNum; i++)
            {
                GameObject childObj = transform.GetChild(i).gameObject;
                if (!Vue.IsViewObject(childObj))
                    DepthSearch(childObj, rules);
            }
        }

        private IEnumerator DepthSearchAsync(GameObject gameObject, IRule[] rules, GameObject temp)
        {
            if (gameObject != null && !gameObject.name.StartsWith(_ignoreSymbol))
            {
                Transform transform = gameObject.transform;
                if (UITypeUtil.TryGetUI(gameObject, out UIType type, out Component ui))
                {
                    TargetObject = gameObject;
                    string[] checkRules = gameObject.name.Split(_ruleSeparator);
                    for (int i = 0; i < rules.Length; i++)
                    {
                        for (int j = 0; j < checkRules.Length; j++)
                        {
                            rules[i].Check(checkRules[j], type, ui);
                            yield return null;
                        }
                    }
                }

                int childNum = transform.childCount;
                for (int i = 0; i < childNum; i++)
                {
                    GameObject childObj = transform.GetChild(i).gameObject;
                    if (!Vue.IsViewObject(childObj))
                        yield return DepthSearchAsync(childObj, rules, temp);
                }

                for (int i = 0; i < rules.Length; i++)
                {
                    TargetObject = gameObject;
                    rules[i].OnComplete();
                }

                TargetObject = null;
                UnityEngine.Object.Destroy(temp);
                _pool.Return(rules);
            }
        }
    }
}
