using System.Collections;
using System.Collections.Generic;
using Game.UI;
using UnityEngine;
using UniVue.Coroutine;
using UniVue.UI;

/// <summary>
/// 手动验证 <see cref="RedPointMgr" />：静态 Key 编码 API、枚举树状态传播（帧末协程刷新父/根）、监听订阅、动态树 API。
/// 帧末等待使用与 <see cref="RedPointMgr" /> 相同的 <see cref="YieldWaitForEndOfFrame" />，并由 <see cref="CoroutineMgr" /> 驱动协程，
/// 避免 Unity 内置协程与 CoroutineMgr 的 LateUpdate 顺序不确定。
/// 不使用 Unity Test Framework。
/// </summary>
public sealed class RedPointTest : MonoBehaviour
{
    /// <summary>
    /// 等待一帧
    /// </summary>
    private static readonly object WaitOneFrame = null;

    private int _failed;
    private int _passed;

    private void Start()
    {
        CoroutineMgr.Run(RunAll());
    }

    private IEnumerator RunAll()
    {
        RunStaticKeyGeometryTests();

        RedPointMgr mgr = new(typeof(RedPointKey));
        if (!mgr.IsRedPointKey((ulong)RedPointKey.Test_Main))
        {
            Debug.LogError("[RedPointTest] RedPointMgr 未从枚举初始化成功，跳过后续运行时测试。");
            Summary();
            yield break;
        }

        yield return RunEnumTreePropagationTests(mgr);
        yield return RunListenerTests(mgr);

        // 动态树 CreateRedPointTree 会分配形如 (2|rule)<<48 的根；若与已有枚举根同 Key 会覆盖节点。
        // 使用仅含一个占位根、且 Key 为 1<<48 的独立枚举初始化，避免与 2<<48 冲突。
        RedPointMgr dynMgr = new(typeof(DynamicTreeMountEnum));
        if (!dynMgr.IsRedPointKey((ulong)DynamicTreeMountEnum.MountOnly))
        {
            Debug.LogWarning("[RedPointTest] 独立动态树 Mgr 初始化失败，跳过动态树测试。");
        }
        else
        {
            yield return RunDynamicTreeTests(dynMgr);
        }

        Summary();
    }

    private void Summary()
    {
        Debug.Log($"[RedPointTest] 完成: {_passed} 通过, {_failed} 失败");
    }

    /// <summary>
    /// 与运行时无关：Key 的父子、深度、根判断。
    /// </summary>
    private void RunStaticKeyGeometryTests()
    {
        ulong main = (ulong)RedPointKey.Test_Main;
        ulong t1 = (ulong)RedPointKey.Test_1;
        ulong t12 = (ulong)RedPointKey.Test_1_2;

        Assert(RedPointMgr.IsRoot(main), "静态: Test_Main 为根 (低 48 位为 0)");
        Assert(!RedPointMgr.IsRoot(t1), "静态: Test_1 非根");
        Assert(RedPointMgr.GetDepth(main) == 0, "静态: 根深度为 0");
        Assert(RedPointMgr.GetDepth(t1) == 1, "静态: Test_1 深度为 1");
        Assert(RedPointMgr.GetDepth(t12) == 2, "静态: Test_1_2 深度为 2");
        Assert(RedPointMgr.GetRootKey(t12) == main, "静态: Test_1_2 的根为 Test_Main");
        Assert(RedPointMgr.GetParentKey(t1) == main, "静态: Test_1 的父为 Test_Main");
        Assert(RedPointMgr.GetParentKey(t12) == t1, "静态: Test_1_2 的父为 Test_1");
        Assert(RedPointMgr.IsParentOf(t1, t12), "静态: IsParentOf(Test_1, Test_1_2)");
        Assert(RedPointMgr.GetRootKey((ulong)RedPointKey.Test2_Main) != main, "静态: Test2 树根与 Test_Main 不同");
    }

    private IEnumerator RunEnumTreePropagationTests(RedPointMgr mgr)
    {
        ulong main = (ulong)RedPointKey.Test_Main;
        ulong t1 = (ulong)RedPointKey.Test_1;
        ulong t12 = (ulong)RedPointKey.Test_1_2;
        ulong t2 = (ulong)RedPointKey.Test_2;

        mgr.SetActive(t12, false);
        yield return WaitOneFrame;

        mgr.SetActive(t12, true);
        Assert(mgr.GetStatus(t12), "枚举树: 叶子 SetActive 后立即可读为 true");
        yield return WaitOneFrame;
        Assert(mgr.GetStatus(t1), "枚举树: 帧末后父节点 Test_1 应更新");
        Assert(mgr.GetStatus(main), "枚举树: 帧末后根 Test_Main 应传播为 true");

        mgr.SetActive(t12, false);
        yield return WaitOneFrame;

        mgr.SetActive(t2, true);
        yield return WaitOneFrame;
        Assert(mgr.GetStatus(main), "枚举树: 兄弟叶子 Test_2 点亮后根仍为 true (Or)");

        mgr.SetActive(t2, false);
        mgr.SetActive(t12, false);
        yield return WaitOneFrame;
        Assert(!mgr.GetStatus(main), "枚举树: Test_1_2 / Test_2 均 false 后根为 false");
    }

    private IEnumerator RunListenerTests(RedPointMgr mgr)
    {
        ulong main = (ulong)RedPointKey.Test_Main;
        int rootChanges = 0;

        void OnRoot(bool _)
        {
            rootChanges++;
        }

        mgr.ListenerRedPointStatus(main, OnRoot);
        mgr.SetActive((ulong)RedPointKey.Test_1_2, true);
        yield return WaitOneFrame;
        Assert(rootChanges >= 1, "监听: 根状态变化应触发回调");

        mgr.UnListenerRedPointStatus(main, OnRoot);
        int before = rootChanges;
        mgr.SetActive((ulong)RedPointKey.Test_1_2, false);
        yield return WaitOneFrame;
        Assert(rootChanges == before, "监听: 取消订阅后计数不变");
    }

    private IEnumerator RunDynamicTreeTests(RedPointMgr mgr)
    {
        ulong mount = (ulong)DynamicTreeMountEnum.MountOnly;
        Assert(!mgr.GetStatus(mount), "动态: 占位根初始为 false");

        ulong dynRoot = mgr.CreateRedPointTree(RedPointRule.Or, "DynRoot");
        Assert(dynRoot != 0, "动态: CreateRedPointTree 非 0");
        Assert(dynRoot != mount, "动态: 新根 Key 与占位枚举根不同");
        Assert(mgr.IsDynamicDependency(dynRoot), "动态: 新根标记为动态树");
        Assert(RedPointMgr.IsRoot(dynRoot), "动态: IsRoot");

        ulong leafA = mgr.AddDependency(dynRoot);
        ulong leafB = mgr.AddDependency(dynRoot);
        Assert(leafA != 0 && leafB != 0 && leafA != leafB, "动态: 两子 key 有效且互异");

        mgr.SetActive(leafA, true);
        yield return WaitOneFrame;
        Assert(mgr.GetStatus(dynRoot), "动态 Or: 一叶子 true 则根 true");

        mgr.SetActive(leafA, false);
        yield return WaitOneFrame;
        Assert(!mgr.GetStatus(dynRoot), "动态 Or: 全 false 则根 false");

        mgr.SetActive(leafA, true);
        mgr.SetActive(leafB, true);
        yield return WaitOneFrame;
        Assert(mgr.GetStatus(dynRoot), "动态 Or: 双子 true 根 true");

        HashSet<ulong> children = new();
        mgr.GetChildrenKeysNoneAlloc(dynRoot, children);
        Assert(children.Contains(leafA) && children.Contains(leafB), "动态: GetChildrenKeysNoneAlloc");

        mgr.DeleteDependency(leafB);
        Assert(mgr.IsRedPointKey(leafA) && !mgr.IsRedPointKey(leafB), "动态: 删除其一子");

        mgr.DeleteDependency(dynRoot);
        Assert(!mgr.IsRedPointKey(dynRoot), "动态: 删根后 key 不存在");
    }

    private void Assert(bool condition, string message)
    {
        if (condition)
        {
            _passed++;
            Debug.Log($"  [PASS] {message}");
        }
        else
        {
            _failed++;
            Debug.LogError($"  [FAIL] {message}");
        }
    }

    /// <summary>仅占位根 (1&lt;&lt;48)，无子节点枚举项，供动态 AddDependency 挂载。</summary>
    private enum DynamicTreeMountEnum : ulong
    {
        MountOnly = (ulong)1 << 48
    }
}