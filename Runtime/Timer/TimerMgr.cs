using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniVue.Common;
using UniVue.Coroutine;
using UniVue.Internal;

namespace UniVue.Timer
{
    public static class TimerMgr
    {
        private static readonly Dictionary<ulong, TimerTask> _timerTasks = new(64);
        private static readonly HashSet<ulong> _timers = new(64);
        private static readonly Heap<float> _timerHeap = new(HeapType.MinHeap, 64);
        private static float _waitTime;
        private static bool _initialized;

        private static IEnumerator Tick()
        {
            while (true)
            {
                if (_timerTasks.Count > 0)
                {
                    _waitTime += Time.deltaTime;
                    if (_waitTime >= _timerHeap.Peek())
                        UpdateWaitTime();
                }

                yield return null;
            }
        }

        private static void UpdateWaitTime()
        {
            if (_waitTime <= 0 || _timerTasks.Count <= 0) return;
            _timerHeap.Clear();
            int missCount = 0;
            foreach (ulong timerId in _timers)
            {
                if (!_timerTasks.TryGetValue(timerId, out TimerTask timerTask))
                {
                    missCount++;
                    continue;
                }

                timerTask.surplusWaitTime -= _waitTime;
                if (timerTask.surplusWaitTime <= 0 && timerTask.Invoke())
                {
                    Recycle(timerId);
                    missCount++;
                }
                else
                {
                    _timerHeap.Add(timerTask.surplusWaitTime);
                }
            }

            _waitTime = 0;

            // 批量移除
            if (missCount > _timers.Count / 2 || missCount > 32)
                _timers.RemoveWhere(timerId => !_timerTasks.ContainsKey(timerId));
        }

        private static void AddTimer(ulong timerId, TimerTask timerTask)
        {
            if (!_initialized)
            {
                CoroutineMgr.Run(Tick());
                _initialized = true;
            }

            UpdateWaitTime();
            _timers.Add(timerId);
            _timerTasks.Add(timerId, timerTask);
            _timerHeap.Add(timerTask.surplusWaitTime);
        }

        private static void Recycle(ulong timerId)
        {
            if (!_timerTasks.Remove(timerId, out TimerTask timerTask))
                return;
            timerTask.repeatCount = 0;
            timerTask.callback = null;
            timerTask.executeCondition = null;
            timerTask.cancelCondition = null;
            timerTask.surplusWaitTime = 0f;
            timerTask.delay = 0f;
            timerTask.interval = 0f;
            InternalObjectPool<TimerTask>.Shared.Return(ref timerTask);
        }

        public static TimerBuilder Create()
        {
            return new TimerBuilder();
        }

        /// <summary>
        /// 添加定时器任务
        /// </summary>
        /// <param name="delay">延时指定时间后开始执行</param>
        /// <param name="interval">每隔指定时间后执行一次</param>
        /// <param name="repeat">执行次数，小于0表示无限执行</param>
        /// <param name="callback">回调函数</param>
        /// <param name="executeCondition">每次执行时需要满足的条件（true表示满足条件），一旦不满足条件不会移除定时器而是会继续等待下一次执行</param>
        /// <param name="cancelCondition">定时任务取消条件（true表示满足条件），一旦满足条件会移除定时器</param>
        /// <returns>定时器唯一id</returns>
        public static ulong AddTimer(float delay, float interval, int repeat, Action callback,
                                     Func<bool> executeCondition = null, Func<bool> cancelCondition = null)
        {
            return Create()
                  .OfDelay(delay)
                  .OfInterval(interval)
                  .OfCount(repeat)
                  .OfCallback(callback)
                  .OfExecuteCondition(executeCondition)
                  .OfCancelCondition(cancelCondition)
                  .Build();
        }

        public static void Kill(ulong timerId)
        {
            Recycle(timerId);
        }

        private sealed class TimerTask
        {
            public Action callback;
            public Func<bool> cancelCondition;
            public float delay;
            public Func<bool> executeCondition;
            public float interval;
            public int repeatCount;

            /// <summary>
            /// 距离下一次回调还需要等待的时间，单位：秒
            /// </summary>
            public float surplusWaitTime;

            public float TotalTime => delay > 0 ? delay : interval;

            public bool Invoke()
            {
                bool completed = cancelCondition != null && cancelCondition.Invoke();
                if (!completed && (executeCondition == null || executeCondition.Invoke()))
                {
                    callback.Invoke();
                    delay = 0;
                    completed = repeatCount > 0 && --repeatCount <= 0;
                }

                surplusWaitTime = TotalTime;
                return completed;
            }
        }

        public ref struct TimerBuilder
        {
            private static ulong _timerId = ulong.MaxValue;
            private float delay;
            private float interval;
            private int repeatCount;
            private Action callback;
            private Func<bool> executeCondition;
            private Func<bool> cancelCondition;

            /// <summary>
            /// 设置回调函数（重复调用将进行追加）
            /// </summary>
            /// <param name="callback">回调函数</param>
            /// <returns></returns>
            public TimerBuilder OfCallback(Action callback)
            {
                if (callback != null)
                    this.callback += callback;
                return this;
            }

            /// <summary>
            /// 设置间隔时间
            /// </summary>
            /// <param name="interval">间隔时间，单位：秒</param>
            /// <returns></returns>
            public TimerBuilder OfInterval(float interval)
            {
                this.interval = interval;
                return this;
            }

            /// <summary>
            /// 设置延迟时间
            /// </summary>
            /// <param name="delay">延迟时间，单位：秒</param>
            /// <returns></returns>
            public TimerBuilder OfDelay(float delay)
            {
                this.delay = delay;
                return this;
            }

            /// <summary>
            /// 设置执行次数
            /// </summary>
            /// <param name="repeatCount">执行次数，小于0表示无限执行，默认执行1次</param>
            /// <returns></returns>
            public TimerBuilder OfCount(int repeatCount)
            {
                this.repeatCount = repeatCount;
                return this;
            }

            /// <summary>
            /// 设置执行条件（重复调用将进行覆盖）
            /// </summary>
            /// <param name="executeCondition">执行条件，返回true表示满足执行条件</param>
            /// <returns></returns>
            public TimerBuilder OfExecuteCondition(Func<bool> executeCondition)
            {
                if (executeCondition != null)
                    this.executeCondition = executeCondition;
                return this;
            }

            /// <summary>
            /// 设置取消条件（重复调用将进行覆盖）
            /// </summary>
            /// <param name="cancelCondition">取消条件，返回true表示满足取消条件，定时器会立即停止</param>
            /// <returns></returns>
            public TimerBuilder OfCancelCondition(Func<bool> cancelCondition)
            {
                if (cancelCondition != null)
                    this.cancelCondition = cancelCondition;
                return this;
            }

            /// <summary>
            /// 构建定时任务
            /// </summary>
            /// <returns>唯一的定时器ID</returns>
            /// <exception cref="Exception"></exception>
            public ulong Build()
            {
                bool dontCreateTimer = callback == null || (delay <= 0 && interval <= 0);
                LogUtil.Assert(!dontCreateTimer, "Callback or delay/interval must be set");
                ulong timerId = _timerId--;
                TimerTask timerTask = InternalObjectPool<TimerTask>.Shared.Rent();
                timerTask.delay = delay;
                timerTask.interval = interval;
                timerTask.repeatCount = repeatCount == 0 ? 1 : repeatCount;
                timerTask.callback = callback;
                timerTask.executeCondition = executeCondition;
                timerTask.cancelCondition = cancelCondition;
                timerTask.surplusWaitTime = timerTask.TotalTime;
                AddTimer(timerId, timerTask);
                return timerId;
            }
        }
    }
}