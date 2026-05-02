using System;
using System.Collections;
using UnityEngine;
using UniVue.Coroutine;

public class CoroutineTest : MonoBehaviour
{
    private int _failed;
    private int _passed;

    private void Start()
    {
        StartCoroutine(RunAllTests());
    }

    private IEnumerator RunAllTests()
    {
        Debug.Log("========== CoroutineMgr 功能测试开始 ==========");

        yield return SafeRun(Test_Run_BasicCompletion());
        yield return SafeRun(Test_YieldNull_WaitsOneFrame());
        yield return SafeRun(Test_YieldPredicate());
        yield return SafeRun(Test_YieldNestedCoroutine());
        yield return SafeRun(Test_YieldCoroutineID_Dependency());
        yield return SafeRun(Test_Kill());
        yield return SafeRun(Test_Stop_Resume());
        yield return SafeRun(Test_CombineDependency());
        yield return SafeRun(Test_CombineDependency_Multi());
        yield return SafeRun(Test_MultipleConcurrentCoroutines());
        yield return SafeRun(Test_RunWithAliasName());
        yield return SafeRun(Test_RunAsCoroutine_Extension());
        yield return SafeRun(Test_YieldWaitUntil());
        yield return SafeRun(Test_YieldWaitWhile());
        yield return SafeRun(Test_Kill_NonExistent());
        yield return SafeRun(Test_Stop_NonRunning());

        Debug.Log($"========== 测试完成: {_passed} 通过, {_failed} 失败 ==========");
        UnityEngine.Assertions.Assert.IsTrue(_failed == 0, $"CoroutineMgr 功能测试失败: {_failed} 个用例未通过");
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

    private IEnumerator WaitFrames(int count)
    {
        for (int i = 0; i < count; i++)
            yield return null;
    }

    // ================================================================
    //  基础运行与完成
    // ================================================================
    private IEnumerator Test_Run_BasicCompletion()
    {
        Debug.Log("[Test] 基础运行与完成");
        bool step1 = false, step2 = false, step3 = false;

        IEnumerator Coroutine()
        {
            step1 = true;
            yield return null;
            step2 = true;
            yield return null;
            step3 = true;
        }

        CoroutineMgr.Run(Coroutine());
        yield return WaitFrames(5);

        Assert(step1, "步骤1已执行");
        Assert(step2, "步骤2已执行");
        Assert(step3, "步骤3已执行（协程完成）");
    }

    // ================================================================
    //  yield return null 帧间暂停
    // ================================================================
    private IEnumerator Test_YieldNull_WaitsOneFrame()
    {
        Debug.Log("[Test] yield return null 帧间暂停");
        int frameCounter = 0;
        int frameAtStep2 = -1;
        int frameAtStart = Time.frameCount;

        IEnumerator Coroutine()
        {
            frameCounter = Time.frameCount;
            yield return null;
            frameAtStep2 = Time.frameCount;
        }

        CoroutineMgr.Run(Coroutine());
        yield return WaitFrames(5);

        Assert(frameAtStep2 > frameCounter, "yield return null 后帧号递增");
    }

    // ================================================================
    //  yield return Func<bool> (Predicate)
    // ================================================================
    private IEnumerator Test_YieldPredicate()
    {
        Debug.Log("[Test] yield return Func<bool> 谓词等待");
        bool gate = false;
        bool afterGate = false;

        IEnumerator Coroutine()
        {
            yield return (Func<bool>)(() => gate);
            afterGate = true;
        }

        CoroutineMgr.Run(Coroutine());
        yield return WaitFrames(3);
        Assert(!afterGate, "谓词为false时协程阻塞");

        gate = true;
        yield return WaitFrames(3);
        Assert(afterGate, "谓词变true后协程继续执行");
    }

    // ================================================================
    //  yield return IEnumerator 嵌套协程
    // ================================================================
    private IEnumerator Test_YieldNestedCoroutine()
    {
        Debug.Log("[Test] yield return IEnumerator 嵌套协程");
        bool outerStart = false, innerDone = false, outerEnd = false;

        IEnumerator Inner()
        {
            yield return null;
            innerDone = true;
        }

        IEnumerator Outer()
        {
            outerStart = true;
            yield return Inner();
            outerEnd = true;
        }

        CoroutineMgr.Run(Outer());
        yield return WaitFrames(8);

        Assert(outerStart, "外层协程开始");
        Assert(innerDone, "内层协程完成");
        Assert(outerEnd, "内层完成后外层继续执行");
    }

    // ================================================================
    //  yield return CoroutineID 等待另一个协程完成
    // ================================================================
    private IEnumerator Test_YieldCoroutineID_Dependency()
    {
        Debug.Log("[Test] yield return CoroutineID 协程依赖");
        bool depDone = false, mainDone = false;

        IEnumerator DependencyCoroutine()
        {
            yield return null;
            yield return null;
            yield return null;
            depDone = true;
        }

        CoroutineID depId = CoroutineMgr.Run(DependencyCoroutine());

        IEnumerator MainCoroutine()
        {
            yield return depId;
            mainDone = true;
        }

        CoroutineMgr.Run(MainCoroutine());
        yield return WaitFrames(3);
        Assert(!mainDone, "依赖未完成时主协程阻塞");

        yield return WaitFrames(10);
        Assert(depDone, "依赖协程已完成");
        Assert(mainDone, "依赖完成后主协程继续执行");
    }

    // ================================================================
    //  Kill 杀死协程
    // ================================================================
    private IEnumerator Test_Kill()
    {
        Debug.Log("[Test] Kill 杀死协程");
        bool step1 = false, step2 = false;

        IEnumerator Coroutine()
        {
            step1 = true;
            yield return null;
            yield return null;
            yield return null;
            step2 = true;
        }

        CoroutineID id = CoroutineMgr.Run(Coroutine());
        yield return WaitFrames(3);
        Assert(step1, "Kill前步骤1已执行");

        CoroutineMgr.Kill(id);
        yield return WaitFrames(5);
        Assert(!step2, "Kill后步骤2未执行");
    }

    // ================================================================
    //  Stop / Resume 暂停与恢复
    // ================================================================
    private IEnumerator Test_Stop_Resume()
    {
        Debug.Log("[Test] Stop / Resume 暂停与恢复");
        int progress = 0;

        IEnumerator Coroutine()
        {
            progress = 1;
            yield return null;
            progress = 2;
            yield return null;
            progress = 3;
        }

        CoroutineID id = CoroutineMgr.Run(Coroutine());
        yield return WaitFrames(3);
        Assert(progress >= 1, "暂停前协程已开始");

        CoroutineMgr.Stop(id);
        int progressAtStop = progress;
        yield return WaitFrames(5);
        Assert(progress == progressAtStop, "暂停期间进度未变化");

        CoroutineMgr.Resume(id);
        yield return WaitFrames(5);
        Assert(progress == 3, "恢复后协程执行完成");
    }

    // ================================================================
    //  CombineDependency 依赖编排
    // ================================================================
    private IEnumerator Test_CombineDependency()
    {
        Debug.Log("[Test] CombineDependency 依赖编排");
        bool aDone = false, bDone = false;
        bool bStartedBeforeA = false;

        IEnumerator CoroutineA()
        {
            yield return null;
            yield return null;
            yield return null;
            aDone = true;
        }

        IEnumerator CoroutineB()
        {
            if (!aDone) bStartedBeforeA = true;
            bDone = true;
            yield break;
        }

        CoroutineID idA = CoroutineMgr.Run(CoroutineA());
        CoroutineID idB = CoroutineMgr.Run(CoroutineB());
        CoroutineMgr.CombineDependency(idB, idA);

        yield return WaitFrames(10);

        Assert(aDone, "协程A已完成");
        Assert(bDone, "协程B已完成");
        Assert(!bStartedBeforeA, "协程B在A完成后才开始执行");
    }

    // ================================================================
    //  CombineDependencies 多依赖
    // ================================================================
    private IEnumerator Test_CombineDependency_Multi()
    {
        Debug.Log("[Test] CombineDependencies 多依赖");
        bool dep1Done = false, dep2Done = false, mainDone = false;
        bool mainStartedEarly = false;

        IEnumerator Dep1()
        {
            yield return null;
            yield return null;
            dep1Done = true;
        }

        IEnumerator Dep2()
        {
            yield return null;
            yield return null;
            yield return null;
            dep2Done = true;
        }

        IEnumerator Main()
        {
            if (!dep1Done || !dep2Done) mainStartedEarly = true;
            mainDone = true;
            yield break;
        }

        CoroutineID id1 = CoroutineMgr.Run(Dep1());
        CoroutineID id2 = CoroutineMgr.Run(Dep2());
        CoroutineID idMain = CoroutineMgr.Run(Main());
        CoroutineMgr.CombineDependencies(idMain, true, id1, id2);

        yield return WaitFrames(12);

        Assert(dep1Done, "依赖1已完成");
        Assert(dep2Done, "依赖2已完成");
        Assert(mainDone, "主协程已完成");
        Assert(!mainStartedEarly, "主协程在所有依赖完成后才执行");
    }

    // ================================================================
    //  多协程并发
    // ================================================================
    private IEnumerator Test_MultipleConcurrentCoroutines()
    {
        Debug.Log("[Test] 多协程并发执行");
        int count1 = 0, count2 = 0, count3 = 0;

        IEnumerator C1()
        {
            count1++;
            yield return null;
            count1++;
        }

        IEnumerator C2()
        {
            count2++;
            yield return null;
            count2++;
        }

        IEnumerator C3()
        {
            count3++;
            yield return null;
            count3++;
        }

        CoroutineMgr.Run(C1());
        CoroutineMgr.Run(C2());
        CoroutineMgr.Run(C3());
        yield return WaitFrames(5);

        Assert(count1 == 2, "协程1完整执行");
        Assert(count2 == 2, "协程2完整执行");
        Assert(count3 == 2, "协程3完整执行");
    }

    // ================================================================
    //  带别名运行
    // ================================================================
    private IEnumerator Test_RunWithAliasName()
    {
        Debug.Log("[Test] 带别名运行协程");
        bool done = false;

        IEnumerator Coroutine()
        {
            done = true;
            yield break;
        }

        CoroutineID id = CoroutineMgr.Run(Coroutine(), "TestAlias");
        Assert(id.id != 0, "返回了有效的CoroutineID");

        yield return WaitFrames(5);
        Assert(done, "带别名协程正常执行完成");
    }

    // ================================================================
    //  RunAsCoroutine 扩展方法
    // ================================================================
    private IEnumerator Test_RunAsCoroutine_Extension()
    {
        Debug.Log("[Test] RunAsCoroutine 扩展方法");
        bool done = false;

        IEnumerator Coroutine()
        {
            done = true;
            yield break;
        }

        CoroutineID id = Coroutine().RunAsCoroutine();
        Assert(id.id != 0, "扩展方法返回有效CoroutineID");

        yield return WaitFrames(5);
        Assert(done, "扩展方法启动的协程正常执行");
    }

    // ================================================================
    //  yield return WaitUntil
    // ================================================================
    private IEnumerator Test_YieldWaitUntil()
    {
        Debug.Log("[Test] yield return WaitUntil");
        bool condition = false;
        bool afterWait = false;

        IEnumerator Coroutine()
        {
            yield return new WaitUntil(() => condition);
            afterWait = true;
        }

        CoroutineMgr.Run(Coroutine());
        yield return WaitFrames(3);
        Assert(!afterWait, "条件为false时协程阻塞");

        condition = true;
        yield return WaitFrames(3);
        Assert(afterWait, "条件变true后协程继续");
    }

    // ================================================================
    //  yield return WaitWhile
    // ================================================================
    private IEnumerator Test_YieldWaitWhile()
    {
        Debug.Log("[Test] yield return WaitWhile");
        bool blocking = true;
        bool afterWait = false;

        IEnumerator Coroutine()
        {
            yield return new WaitWhile(() => blocking);
            afterWait = true;
        }

        CoroutineMgr.Run(Coroutine());
        yield return WaitFrames(3);
        Assert(!afterWait, "条件为true时协程持续等待");

        blocking = false;
        yield return WaitFrames(3);
        Assert(afterWait, "条件变false后协程继续");
    }

    // ================================================================
    //  Kill 不存在的协程不报错
    // ================================================================
    private IEnumerator Test_Kill_NonExistent()
    {
        Debug.Log("[Test] Kill 不存在的协程不报错");
        bool noException = true;
        try
        {
            CoroutineMgr.Kill(999999);
        }
        catch (Exception)
        {
            noException = false;
        }

        Assert(noException, "Kill不存在的ID无异常");
        yield break;
    }

    // ================================================================
    //  Stop/Resume 对非运行中协程无效果
    // ================================================================
    private IEnumerator Test_Stop_NonRunning()
    {
        Debug.Log("[Test] Stop/Resume 对非运行协程无副作用");
        bool noException = true;
        try
        {
            CoroutineMgr.Stop(999999);
            CoroutineMgr.Resume(999999);
        }
        catch (Exception)
        {
            noException = false;
        }

        Assert(noException, "Stop/Resume不存在的ID无异常");
        yield break;
    }
}