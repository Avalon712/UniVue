using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UniVue.Timer;

namespace UniVue.Test
{
    /// <summary>
    /// <see cref="TimerMgr"/> 的行为单元测试。
    /// <para>
    /// <see cref="TimerMgr"/> 依赖 <c>VCoroutine</c> 的 MonoBehaviour 更新循环来推进时间，
    /// 因此所有用例都必须以 PlayMode 协程测试（<see cref="UnityTestAttribute"/>）方式运行，
    /// 只有在 Play 模式下逐帧驱动 <c>Update</c> 时定时器回调才会被触发。
    /// </para>
    /// <para>
    /// 由于 <see cref="TimerMgr"/> 是全局静态状态，所有断言都通过回调（可观察行为）进行，
    /// 并在 <see cref="TearDown"/> 中回收本用例创建的所有定时器，避免用例间相互污染。
    /// </para>
    /// </summary>
    internal sealed class TimerTest
    {
        /// <summary>等待条件成立的最大真实时间（秒），给帧调度留足冗余。</summary>
        private const float Timeout = 5f;

        private readonly List<ulong> _createdTimers = new();

        [SetUp]
        public void SetUp()
        {
            // 定时器基于 Time.deltaTime（受 timeScale 影响），确保时间正常流逝。
            Time.timeScale = 1f;
            _createdTimers.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (ulong timerId in _createdTimers)
                TimerMgr.Kill(timerId);
            _createdTimers.Clear();
        }

        #region Helpers

        private ulong Track(ulong timerId)
        {
            _createdTimers.Add(timerId);
            return timerId;
        }

        /// <summary>逐帧等待直到 <paramref name="predicate"/> 为 true，超时则失败。</summary>
        private static IEnumerator WaitUntil(Func<bool> predicate, string message)
        {
            float start = Time.realtimeSinceStartup;
            while (!predicate())
            {
                if (Time.realtimeSinceStartup - start > Timeout)
                {
                    Assert.Fail($"等待超时（{Timeout}s）：{message}");
                    yield break;
                }

                yield return null;
            }
        }

        /// <summary>基于真实时间等待固定秒数（不受 timeScale 影响）。</summary>
        private static IEnumerator WaitRealSeconds(float seconds)
        {
            float start = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - start < seconds)
                yield return null;
        }

        #endregion

        #region 基础延时 / 间隔

        [UnityTest]
        public IEnumerator Delay_SingleShot_InvokesExactlyOnce()
        {
            int fireCount = 0;
            Track(TimerMgr.Create()
                          .OfDelay(0.1f)
                          .OfCallback(() => fireCount++)
                          .Build());

            // 构建后、推进帧之前不应触发。
            Assert.That(fireCount, Is.EqualTo(0), "定时器不应在构建时同步触发");

            yield return WaitUntil(() => fireCount >= 1, "延时回调应被触发一次");
            Assert.That(fireCount, Is.EqualTo(1));

            // 默认执行次数为 1，之后不应再触发。
            yield return WaitRealSeconds(0.3f);
            Assert.That(fireCount, Is.EqualTo(1), "单次定时器不应重复触发");
        }

        [UnityTest]
        public IEnumerator Interval_WithRepeatCount_InvokesExactCount()
        {
            const int repeat = 3;
            int fireCount = 0;
            Track(TimerMgr.Create()
                          .OfInterval(0.05f)
                          .OfCount(repeat)
                          .OfCallback(() => fireCount++)
                          .Build());

            yield return WaitUntil(() => fireCount >= repeat, "间隔定时器应触发指定次数");

            // 严格校验：达到次数后必须停止，不能多触发。
            yield return WaitRealSeconds(0.3f);
            Assert.That(fireCount, Is.EqualTo(repeat), "达到执行次数后不应继续触发");
        }

        [UnityTest]
        public IEnumerator DelayThenInterval_FirstWaitUsesDelay_SubsequentUseInterval()
        {
            const float delay = 0.5f;
            int fireCount = 0;
            float firstFireElapsed = -1f;
            float start = Time.realtimeSinceStartup;

            Track(TimerMgr.Create()
                          .OfDelay(delay)
                          .OfInterval(0.05f)
                          .OfCount(3)
                          .OfCallback(() =>
                          {
                              if (fireCount == 0)
                                  firstFireElapsed = Time.realtimeSinceStartup - start;
                              fireCount++;
                          })
                          .Build());

            yield return WaitUntil(() => fireCount >= 3, "延时+间隔定时器应触发 3 次");

            // 首次触发应等待 delay（给足冗余以避免帧抖动导致的偶发失败）。
            Assert.That(firstFireElapsed, Is.GreaterThanOrEqualTo(delay * 0.5f),
                "首次回调应至少等待 delay 时间");

            yield return WaitRealSeconds(0.3f);
            Assert.That(fireCount, Is.EqualTo(3));
        }

        #endregion

        #region 无限重复 / Kill

        [UnityTest]
        public IEnumerator InfiniteRepeat_KeepsFiring_UntilKilled()
        {
            int fireCount = 0;
            ulong timerId = Track(TimerMgr.Create()
                                          .OfInterval(0.05f)
                                          .OfCount(-1) // 负数表示无限执行
                                          .OfCallback(() => fireCount++)
                                          .Build());

            yield return WaitUntil(() => fireCount >= 2, "无限定时器应持续触发");

            TimerMgr.Kill(timerId);
            int countAtKill = fireCount;

            // Kill 之后不应再触发。
            yield return WaitRealSeconds(0.4f);
            Assert.That(fireCount, Is.EqualTo(countAtKill), "Kill 之后定时器不应再触发");
        }

        [UnityTest]
        public IEnumerator Kill_BeforeFirstFire_PreventsInvocation()
        {
            int fireCount = 0;
            ulong timerId = Track(TimerMgr.Create()
                                          .OfDelay(0.5f)
                                          .OfCallback(() => fireCount++)
                                          .Build());

            // 在延时到达之前立即杀死。
            yield return null;
            TimerMgr.Kill(timerId);

            yield return WaitRealSeconds(0.7f);
            Assert.That(fireCount, Is.EqualTo(0), "在触发前被 Kill 的定时器不应执行回调");
        }

        #endregion

        #region 执行条件 / 取消条件

        [UnityTest]
        public IEnumerator ExecuteCondition_False_SkipsInvocationButKeepsTimer()
        {
            bool allowExecute = false;
            int fireCount = 0;

            Track(TimerMgr.Create()
                          .OfInterval(0.05f)
                          .OfCount(1)
                          .OfExecuteCondition(() => allowExecute)
                          .OfCallback(() => fireCount++)
                          .Build());

            // 条件为 false：应跳过回调，但定时器不被移除（继续等待）。
            yield return WaitRealSeconds(0.4f);
            Assert.That(fireCount, Is.EqualTo(0), "执行条件为 false 时不应触发回调");

            // 满足条件后应立即执行。
            allowExecute = true;
            yield return WaitUntil(() => fireCount >= 1, "执行条件为 true 后应触发回调");
            Assert.That(fireCount, Is.EqualTo(1));

            // 已满足执行次数（1），之后不再触发。
            yield return WaitRealSeconds(0.3f);
            Assert.That(fireCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator CancelCondition_True_RemovesTimerWithoutInvoking()
        {
            int fireCount = 0;

            Track(TimerMgr.Create()
                          .OfDelay(0.1f)
                          .OfCancelCondition(() => true) // 一开始就满足取消条件
                          .OfCallback(() => fireCount++)
                          .Build());

            yield return WaitRealSeconds(0.4f);
            Assert.That(fireCount, Is.EqualTo(0), "满足取消条件时不应执行回调且定时器被移除");
        }

        [UnityTest]
        public IEnumerator CancelCondition_BecomesTrue_StopsFurtherInvocations()
        {
            bool cancel = false;
            int fireCount = 0;

            Track(TimerMgr.Create()
                          .OfInterval(0.05f)
                          .OfCount(-1)
                          .OfCancelCondition(() => cancel)
                          .OfCallback(() => fireCount++)
                          .Build());

            yield return WaitUntil(() => fireCount >= 1, "取消条件为 false 时应正常触发");

            cancel = true;
            // 取消条件在下一次判定时生效，需等待越过一个间隔以完成移除。
            yield return WaitRealSeconds(0.1f);
            int countAtCancel = fireCount;

            yield return WaitRealSeconds(0.4f);
            Assert.That(fireCount, Is.EqualTo(countAtCancel), "取消条件满足后不应再触发");
        }

        #endregion

        #region 便捷 API / 组合行为

        [UnityTest]
        public IEnumerator StaticAddTimer_Convenience_Works()
        {
            int fireCount = 0;
            Track(TimerMgr.AddTimer(0.1f, 0f, 1, () => fireCount++));

            yield return WaitUntil(() => fireCount >= 1, "静态 AddTimer 应正常触发");
            Assert.That(fireCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator OfCallback_CalledMultipleTimes_AppendsAllCallbacks()
        {
            int a = 0;
            int b = 0;

            Track(TimerMgr.Create()
                          .OfDelay(0.1f)
                          .OfCallback(() => a++)
                          .OfCallback(() => b++) // 追加而非覆盖
                          .Build());

            yield return WaitUntil(() => a >= 1, "第一个回调应被触发");
            Assert.That(a, Is.EqualTo(1));
            Assert.That(b, Is.EqualTo(1), "多次 OfCallback 应追加，所有回调都应被触发");
        }

        [UnityTest]
        public IEnumerator MultipleTimers_RunConcurrently_WithIndependentCounts()
        {
            int fast = 0;
            int slow = 0;

            Track(TimerMgr.Create()
                          .OfInterval(0.05f)
                          .OfCount(4)
                          .OfCallback(() => fast++)
                          .Build());

            Track(TimerMgr.Create()
                          .OfInterval(0.1f)
                          .OfCount(2)
                          .OfCallback(() => slow++)
                          .Build());

            yield return WaitUntil(() => fast >= 4 && slow >= 2, "两个定时器应各自独立完成");

            yield return WaitRealSeconds(0.3f);
            Assert.That(fast, Is.EqualTo(4), "第一个定时器应恰好触发 4 次");
            Assert.That(slow, Is.EqualTo(2), "第二个定时器应恰好触发 2 次");
        }

        #endregion

        #region 参数校验

#if UNITY_EDITOR
        [UnityTest]
        public IEnumerator Build_WithZeroDelayAndInterval_LogsAssertion()
        {
            // Build 内部使用 LogUtil.Assert（Debug.Assert），当 delay/interval 均无效时会记录断言日志。
            // 该断言仅在 UNITY_EDITOR 下编译生效。
            LogAssert.Expect(LogType.Assert, new Regex("Callback or delay/interval must be set"));

            int fireCount = 0;
            // 提供合法回调但 delay/interval 均为 0，触发断言日志；避免空回调导致的 NRE。
            Track(TimerMgr.Create()
                          .OfDelay(0f)
                          .OfInterval(0f)
                          .OfCallback(() => fireCount++)
                          .Build());

            // 让全局定时循环推进若干帧以处理（并回收）该非法任务，保持后续用例状态干净。
            yield return WaitRealSeconds(0.2f);
        }
#endif

        #endregion
    }
}
