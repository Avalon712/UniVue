using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UniVue.Timer;
using Debug = UnityEngine.Debug;
using Random = System.Random;

public class TimerTest : MonoBehaviour
{
    private int _failed;
    private int _passed;

    private void Start()
    {
        StartCoroutine(RunAllTests());
    }

    private IEnumerator RunAllTests()
    {
        Debug.Log("========== TimerMgr 功能测试开始 ==========");

        yield return SafeRun(Test_BasicDelay());
        yield return SafeRun(Test_IntervalRepeat());
        yield return SafeRun(Test_RepeatOnce_Default());
        yield return SafeRun(Test_RepeatInfinite());
        yield return SafeRun(Test_DelayThenInterval());
        yield return SafeRun(Test_ExecuteCondition_Block());
        yield return SafeRun(Test_CancelCondition());
        yield return SafeRun(Test_Kill());
        yield return SafeRun(Test_Builder_FluentAPI());
        yield return SafeRun(Test_Builder_MultipleCallbacks());
        yield return SafeRun(Test_Kill_NonExistent());
        yield return SafeRun(Test_Random1000_DelayAccuracy());

        Debug.Log($"========== 测试完成: {_passed} 通过, {_failed} 失败 ==========");
        UnityEngine.Assertions.Assert.IsTrue(_failed == 0, $"TimerMgr 功能测试失败: {_failed} 个用例未通过");
    }

    private IEnumerator SafeRun(IEnumerator test)
    {
        while (true)
        {
            bool moved;
            try
            {
                moved = test.MoveNext();
            }
            catch (Exception e)
            {
                _failed++;
                Debug.LogError($"  [EXCEPTION] {e.Message}");
                yield break;
            }

            if (!moved) yield break;
            yield return test.Current;
        }
    }

    private void Assert(bool condition, string testName)
    {
        if (condition)
        {
            _passed++;
            Debug.Log($"  [PASS] {testName}");
        }
        else
        {
            _failed++;
            Debug.LogError($"  [FAIL] {testName}");
        }
    }

    // ================================================================
    //  基础延时触发
    // ================================================================
    private IEnumerator Test_BasicDelay()
    {
        Debug.Log("[Test] Test_BasicDelay");
        int count = 0;
        TimerMgr.AddTimer(0.15f, 0f, 1, () => count++);

        yield return new WaitForSeconds(0.05f);
        Assert(count == 0, "Test_BasicDelay: 延时未到不触发");

        yield return new WaitForSeconds(0.3f);
        Assert(count == 1, "Test_BasicDelay: 延时到达后触发一次");
    }

    // ================================================================
    //  间隔重复执行
    // ================================================================
    private IEnumerator Test_IntervalRepeat()
    {
        Debug.Log("[Test] Test_IntervalRepeat");
        int count = 0;
        TimerMgr.AddTimer(0f, 0.1f, 3, () => count++);

        yield return new WaitForSeconds(0.55f);
        Assert(count == 3, "Test_IntervalRepeat: 重复3次后停止");

        int countAfter = count;
        yield return new WaitForSeconds(0.3f);
        Assert(count == countAfter, "Test_IntervalRepeat: 达到次数后不再触发");
    }

    // ================================================================
    //  默认执行一次 (repeatCount=0 → 内部默认为1)
    // ================================================================
    private IEnumerator Test_RepeatOnce_Default()
    {
        Debug.Log("[Test] Test_RepeatOnce_Default");
        int count = 0;
        TimerMgr.AddTimer(0.1f, 0f, 0, () => count++);

        yield return new WaitForSeconds(0.4f);
        Assert(count == 1, "Test_RepeatOnce_Default: repeatCount=0时默认执行1次");
    }

    // ================================================================
    //  无限重复 (repeatCount < 0)
    // ================================================================
    private IEnumerator Test_RepeatInfinite()
    {
        Debug.Log("[Test] Test_RepeatInfinite");
        int count = 0;
        ulong id = TimerMgr.AddTimer(0f, 0.1f, -1, () => count++);

        yield return new WaitForSeconds(0.55f);
        Assert(count >= 3, "Test_RepeatInfinite: 无限重复持续触发");

        TimerMgr.Kill(id);
        int countAtKill = count;
        yield return new WaitForSeconds(0.3f);
        Assert(count == countAtKill, "Test_RepeatInfinite: Kill后停止触发");
    }

    // ================================================================
    //  先延时再按间隔重复
    // ================================================================
    private IEnumerator Test_DelayThenInterval()
    {
        Debug.Log("[Test] Test_DelayThenInterval");
        int count = 0;
        ulong id = TimerMgr.AddTimer(0.2f, 0.1f, 3, () => count++);

        yield return new WaitForSeconds(0.1f);
        Assert(count == 0, "Test_DelayThenInterval: 延时期间不触发");

        yield return new WaitForSeconds(0.8f);
        Assert(count == 3, "Test_DelayThenInterval: 延时后按间隔执行3次");
    }

    // ================================================================
    //  ExecuteCondition 阻塞执行但不移除
    // ================================================================
    private IEnumerator Test_ExecuteCondition_Block()
    {
        Debug.Log("[Test] Test_ExecuteCondition_Block");
        bool allow = false;
        int count = 0;
        ulong id = TimerMgr.AddTimer(0f, 0.1f, 2, () => count++, () => allow);

        yield return new WaitForSeconds(0.35f);
        Assert(count == 0, "Test_ExecuteCondition_Block: 条件不满足时不执行");

        allow = true;
        yield return new WaitForSeconds(0.35f);
        Assert(count >= 1, "Test_ExecuteCondition_Block: 条件满足后开始执行");

        TimerMgr.Kill(id);
    }

    // ================================================================
    //  CancelCondition 满足时移除定时器
    // ================================================================
    private IEnumerator Test_CancelCondition()
    {
        Debug.Log("[Test] Test_CancelCondition");
        bool shouldCancel = false;
        int count = 0;
        TimerMgr.AddTimer(0f, 0.1f, -1, () => count++, null, () => shouldCancel);

        yield return new WaitForSeconds(0.35f);
        Assert(count >= 1, "Test_CancelCondition: 取消前正常执行");

        shouldCancel = true;
        int countAtCancel = count;
        yield return new WaitForSeconds(0.35f);
        Assert(count == countAtCancel, "Test_CancelCondition: 取消条件满足后停止执行");
    }

    // ================================================================
    //  Kill 手动移除定时器
    // ================================================================
    private IEnumerator Test_Kill()
    {
        Debug.Log("[Test] Test_Kill");
        int count = 0;
        ulong id = TimerMgr.AddTimer(0f, 0.1f, -1, () => count++);

        yield return new WaitForSeconds(0.25f);
        Assert(count >= 1, "Test_Kill: Kill前有执行");

        TimerMgr.Kill(id);
        int countAtKill = count;
        yield return new WaitForSeconds(0.3f);
        Assert(count == countAtKill, "Test_Kill: Kill后不再执行");
    }

    // ================================================================
    //  TimerBuilder 流式API
    // ================================================================
    private IEnumerator Test_Builder_FluentAPI()
    {
        Debug.Log("[Test] Test_Builder_FluentAPI");
        int count = 0;
        ulong id = TimerMgr.Create()
                           .OfDelay(0.1f)
                           .OfInterval(0.1f)
                           .OfCount(2)
                           .OfCallback(() => count++)
                           .Build();

        Assert(id != 0, "Test_Builder_FluentAPI: Builder返回有效ID");

        yield return new WaitForSeconds(0.6f);
        Assert(count == 2, "Test_Builder_FluentAPI: 按配置执行2次");
    }

    // ================================================================
    //  OfCallback 多次调用追加回调
    // ================================================================
    private IEnumerator Test_Builder_MultipleCallbacks()
    {
        Debug.Log("[Test] Test_Builder_MultipleCallbacks");
        int countA = 0, countB = 0;
        TimerMgr.Create()
                .OfDelay(0.1f)
                .OfCount(1)
                .OfCallback(() => countA++)
                .OfCallback(() => countB++)
                .Build();

        yield return new WaitForSeconds(0.4f);
        Assert(countA == 1, "Test_Builder_MultipleCallbacks: 回调A被调用");
        Assert(countB == 1, "Test_Builder_MultipleCallbacks: 回调B被调用");
    }

    // ================================================================
    //  Kill 不存在的定时器不报错
    // ================================================================
    private IEnumerator Test_Kill_NonExistent()
    {
        Debug.Log("[Test] Test_Kill_NonExistent");
        bool noException = true;
        try
        {
            TimerMgr.Kill(0);
            TimerMgr.Kill(999999);
        }
        catch (Exception)
        {
            noException = false;
        }

        Assert(noException, "Test_Kill_NonExistent: Kill不存在的ID无异常");
        yield break;
    }

    // ================================================================
    //  1000 个随机延时任务：回调时刻与期望间隔对比（毫秒），误差 ≤ 100ms 为通过
    // ================================================================
    private IEnumerator Test_Random1000_DelayAccuracy()
    {
        Debug.Log("[Test] Test_Random1000_DelayAccuracy（约 30s+，请稍候）");
        const int total = 1000;
        const double toleranceMs = 100.0; // 0.1s
        int completed = 0;
        int timingFailures = 0;
        Random rnd = new();

        for (int i = 0; i < total; i++)
        {
            float delaySec = (float)(rnd.NextDouble() * (30.0 - 0.5) + 0.5);
            long createStamp = Stopwatch.GetTimestamp();
            TimerMgr.AddTimer(delaySec, 0f, 1, () =>
            {
                double elapsedMs = (Stopwatch.GetTimestamp() - createStamp) * 1000.0 /
                                   Stopwatch.Frequency;
                double expectedMs = delaySec * 1000.0;
                if (Math.Abs(elapsedMs - expectedMs) > toleranceMs)
                    timingFailures++;
                completed++;
            });
        }

        float waitStart = Time.realtimeSinceStartup;
        yield return new WaitUntil(() => completed >= total || Time.realtimeSinceStartup - waitStart > 38f);

        Assert(completed == total,
               $"Test_Random1000_DelayAccuracy: 应完成 {total} 次回调（实际 {completed}）");
        Assert(timingFailures == 0,
               $"Test_Random1000_DelayAccuracy: 与期望延时的偏差应 ≤ {toleranceMs}ms（超标次数 {timingFailures}）");
    }
}