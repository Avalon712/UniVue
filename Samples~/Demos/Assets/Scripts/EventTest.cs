using System;
using UnityEngine;
using UniVue.Event;

public class EventTest : MonoBehaviour
{
    private static int _staticCallCount;
    private int _failed;
    private int _passed;

    private void Start()
    {
        Debug.Log("========== EventMgr 功能测试开始 ==========");

        Run(Test_OnAndDispatch_NoArg);
        Run(Test_OnAndDispatch_WithArg);
        Run(Test_Dispatch_WithArg_AlsoCallsNoArgListeners);
        Run(Test_Dispatch_NoArg_DoesNotCallArgListeners);
        Run(Test_Off_SpecificCallback);
        Run(Test_Off_GenericCallback);
        Run(Test_Off_AllByEventKey);
        Run(Test_Off_ByTargetOnSpecificEvent);
        Run(Test_Off_ByTargetOnAllEvents);
        Run(Test_Off_NullTargetIgnoredByDefault);
        Run(Test_Off_NullTargetAllowed);
        Run(Test_EventKey_Int);
        Run(Test_EventKey_String);
        Run(Test_EventKey_Enum);
        Run(Test_EventKey_Enum_DifferentValuesIsolated);
        Run(Test_EventKey_Enum_WithArg);
        Run(Test_EventKey_Enum_OffSpecificCallback);
        Run(Test_EventKey_Enum_OffAll);
        Run(Test_EventKey_Enum_EqualsStringName);
        Run(Test_EventKey_Enum_SameNameDifferentEnumSharesKey);
        Run(Test_EventKey_Enum_MultipleValues);
        Run(Test_EventKey_NotEventKey_DispatchSkipped);
        Run(Test_CurrentTriggeredEvent_DuringDispatch);
        Run(Test_Dispatch_DeadLoopChain_Detection);
        Run(Test_OnEvent_GlobalListener);
        Run(Test_MultipleCallbacksSameEvent);
        Run(Test_On_WithExplicitTarget_NoArg);
        Run(Test_On_WithExplicitTarget_WithArg);
        Run(Test_Dispatch_NoCallbacksRegistered);
        Run(Test_Off_NonExistentCallback);
        Run(Test_UnsubscribeDuringDispatch);

        Debug.Log($"========== 测试完成: {_passed} 通过, {_failed} 失败 ==========");
        UnityEngine.Assertions.Assert.IsTrue(_failed == 0, $"EventMgr 功能测试失败: {_failed} 个用例未通过");
    }

    private void Run(Action test)
    {
        try
        {
            test();
        }
        catch (Exception e)
        {
            _failed++;
            Debug.LogError($"  [EXCEPTION] {test.Method.Name}: {e.Message}");
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

    private void CleanupEvent(params EventKey[] keys)
    {
        foreach (EventKey key in keys)
            EventMgr.Off(key);
    }

    // ---- 基本注册与无参分发 ----
    private void Test_OnAndDispatch_NoArg()
    {
        Debug.Log("[Test] On + Dispatch 无参");
        EventKey key = "test_no_arg";
        int callCount = 0;
        Action cb = () => callCount++;

        EventMgr.On(key, cb);
        EventMgr.Dispatch(key);

        Assert(callCount == 1, "回调被调用一次");

        EventMgr.Dispatch(key);
        Assert(callCount == 2, "再次分发后回调被调用两次");

        EventMgr.Off(key, cb);
        CleanupEvent(key);
    }

    // ---- 带参注册与分发 ----
    private void Test_OnAndDispatch_WithArg()
    {
        Debug.Log("[Test] On<T> + Dispatch<T> 带参");
        EventKey key = "test_with_arg";
        string received = null;
        Action<string> cb = s => received = s;

        EventMgr.On(key, cb);
        EventMgr.Dispatch(key, "hello");

        Assert(received == "hello", "收到正确的参数值");

        EventMgr.Off(key, cb);
        CleanupEvent(key);
    }

    // ---- 带参分发也会触发无参监听 ----
    private void Test_Dispatch_WithArg_AlsoCallsNoArgListeners()
    {
        Debug.Log("[Test] Dispatch<T> 也会调用无参监听");
        EventKey key = "test_arg_calls_noarg";
        bool noArgCalled = false;
        string argReceived = null;
        Action noArgCb = () => noArgCalled = true;
        Action<string> argCb = s => argReceived = s;

        EventMgr.On(key, noArgCb);
        EventMgr.On(key, argCb);
        EventMgr.Dispatch(key, "world");

        Assert(noArgCalled, "无参回调被调用");
        Assert(argReceived == "world", "带参回调收到正确参数");

        EventMgr.Off(key, noArgCb);
        EventMgr.Off(key, argCb);
        CleanupEvent(key);
    }

    // ---- 无参分发不会触发带参监听 ----
    private void Test_Dispatch_NoArg_DoesNotCallArgListeners()
    {
        Debug.Log("[Test] Dispatch 无参不会调用带参监听");
        EventKey key = "test_noarg_skip_arg";
        bool argCalled = false;
        Action<int> argCb = _ => argCalled = true;

        EventMgr.On(key, argCb);
        EventMgr.Dispatch(key);

        Assert(!argCalled, "带参回调未被调用");

        EventMgr.Off(key, argCb);
        CleanupEvent(key);
    }

    // ---- Off 注销特定无参回调 ----
    private void Test_Off_SpecificCallback()
    {
        Debug.Log("[Test] Off 注销特定无参回调");
        EventKey key = "test_off_specific";
        int callCount = 0;
        Action cb = () => callCount++;

        EventMgr.On(key, cb);
        EventMgr.Off(key, cb);
        EventMgr.Dispatch(key);

        Assert(callCount == 0, "注销后回调未被调用");
        CleanupEvent(key);
    }

    // ---- Off<T> 注销特定带参回调 ----
    private void Test_Off_GenericCallback()
    {
        Debug.Log("[Test] Off<T> 注销特定带参回调");
        EventKey key = "test_off_generic";
        int callCount = 0;
        Action<int> cb = _ => callCount++;

        EventMgr.On(key, cb);
        EventMgr.Off(key, cb);
        EventMgr.Dispatch(key, 42);

        Assert(callCount == 0, "注销后带参回调未被调用");
        CleanupEvent(key);
    }

    // ---- Off(EventKey) 注销该Key下所有监听 ----
    private void Test_Off_AllByEventKey()
    {
        Debug.Log("[Test] Off(EventKey) 注销该Key全部监听");
        EventKey key = "test_off_all";
        int count1 = 0, count2 = 0;
        Action cb1 = () => count1++;
        Action<string> cb2 = _ => count2++;

        EventMgr.On(key, cb1);
        EventMgr.On(key, cb2);
        EventMgr.Off(key);
        EventMgr.Dispatch(key, "data");

        Assert(count1 == 0, "无参回调未被调用");
        Assert(count2 == 0, "带参回调未被调用");
    }

    // ---- Off(EventKey, target) 按目标对象注销指定事件 ----
    private void Test_Off_ByTargetOnSpecificEvent()
    {
        Debug.Log("[Test] Off(EventKey, target) 按目标注销");
        EventKey key = "test_off_target_event";
        int count = 0;
        object target = new();
        Action cb = () => count++;

        EventMgr.On(key, target, cb);
        EventMgr.Off(key, target);
        EventMgr.Dispatch(key);

        Assert(count == 0, "目标注销后回调未被调用");
        CleanupEvent(key);
    }

    // ---- Off(target) 注销目标在所有事件上的监听 ----
    private void Test_Off_ByTargetOnAllEvents()
    {
        Debug.Log("[Test] Off(target) 注销目标所有事件监听");
        EventKey key1 = "test_off_target_all_1";
        EventKey key2 = "test_off_target_all_2";
        int count1 = 0, count2 = 0;
        object target = new();
        Action cb1 = () => count1++;
        Action cb2 = () => count2++;

        EventMgr.On(key1, target, cb1);
        EventMgr.On(key2, target, cb2);
        EventMgr.Off(target);
        EventMgr.Dispatch(key1);
        EventMgr.Dispatch(key2);

        Assert(count1 == 0, "事件1回调未被调用");
        Assert(count2 == 0, "事件2回调未被调用");
        CleanupEvent(key1, key2);
    }

    // ---- Off(null) 默认不允许null target ----
    private void Test_Off_NullTargetIgnoredByDefault()
    {
        Debug.Log("[Test] Off(null) 默认忽略null目标");
        EventKey key = "test_off_null_default";
        int count = 0;
        object target = new();
        Action cb = () => count++;

        EventMgr.On(key, target, cb);
        EventMgr.Off((object)null);
        EventMgr.Dispatch(key);

        Assert(count == 1, "null目标默认被忽略，回调仍然生效");

        EventMgr.Off(key, target);
        CleanupEvent(key);
    }

    // ---- Off(null, allowTargetIsNull:true) 注销静态回调 ----
    private static void StaticTestCallback()
    {
        _staticCallCount++;
    }

    private void Test_Off_NullTargetAllowed()
    {
        Debug.Log("[Test] Off(null, allowTargetIsNull:true) 注销null目标回调");
        EventKey key = "test_off_null_allowed";
        _staticCallCount = 0;

        EventMgr.On(key, StaticTestCallback);
        EventMgr.Off(null, true);
        EventMgr.Dispatch(key);

        Assert(_staticCallCount == 0, "允许null目标后静态回调被注销");
        CleanupEvent(key);
    }

    // ---- EventKey: int 类型 ----
    private void Test_EventKey_Int()
    {
        Debug.Log("[Test] EventKey int 类型");
        EventKey key = 100;
        int callCount = 0;
        Action cb = () => callCount++;

        EventMgr.On(key, cb);
        EventMgr.Dispatch(key);

        Assert(callCount == 1, "int类型EventKey分发成功");

        EventMgr.Off(key, cb);
        CleanupEvent(key);
    }

    // ---- EventKey: string 类型 ----
    private void Test_EventKey_String()
    {
        Debug.Log("[Test] EventKey string 类型");
        EventKey key = "my_string_event";
        int callCount = 0;
        Action cb = () => callCount++;

        EventMgr.On(key, cb);
        EventMgr.Dispatch(key);

        Assert(callCount == 1, "string类型EventKey分发成功");

        EventMgr.Off(key, cb);
        CleanupEvent(key);
    }

    // ---- EventKey: enum 类型 ----
    private void Test_EventKey_Enum()
    {
        Debug.Log("[Test] EventKey enum 类型");
        EventKey key = TestEvent.Login;
        int callCount = 0;
        Action cb = () => callCount++;

        EventMgr.On(key, cb);
        EventMgr.Dispatch(key);

        Assert(callCount == 1, "enum类型EventKey分发成功");

        EventMgr.Off(key, cb);
        CleanupEvent(key);
    }

    // ---- 不同枚举值是独立事件 ----
    private void Test_EventKey_Enum_DifferentValuesIsolated()
    {
        Debug.Log("[Test] Enum 不同枚举值事件隔离");
        EventKey loginKey = TestEvent.Login;
        EventKey logoutKey = TestEvent.Logout;
        int loginCount = 0, logoutCount = 0;
        Action loginCb = () => loginCount++;
        Action logoutCb = () => logoutCount++;

        EventMgr.On(loginKey, loginCb);
        EventMgr.On(logoutKey, logoutCb);
        EventMgr.Dispatch(loginKey);

        Assert(loginCount == 1, "Login回调被调用");
        Assert(logoutCount == 0, "Logout回调未被调用");

        EventMgr.Off(loginKey, loginCb);
        EventMgr.Off(logoutKey, logoutCb);
        CleanupEvent(loginKey, logoutKey);
    }

    // ---- 枚举Key带参分发 ----
    private void Test_EventKey_Enum_WithArg()
    {
        Debug.Log("[Test] Enum Key 带参分发");
        EventKey key = TestEvent.Purchase;
        float received = 0f;
        Action<float> cb = v => received = v;

        EventMgr.On(key, cb);
        EventMgr.Dispatch(key, 99.5f);

        Assert(Mathf.Approximately(received, 99.5f), "枚举Key带参分发收到正确值");

        EventMgr.Off(key, cb);
        CleanupEvent(key);
    }

    // ---- 枚举Key Off特定回调 ----
    private void Test_EventKey_Enum_OffSpecificCallback()
    {
        Debug.Log("[Test] Enum Key Off特定回调");
        EventKey key = TestEvent.Login;
        int count = 0;
        Action cb = () => count++;

        EventMgr.On(key, cb);
        EventMgr.Off(key, cb);
        EventMgr.Dispatch(key);

        Assert(count == 0, "枚举Key注销后回调未被调用");
        CleanupEvent(key);
    }

    // ---- 枚举Key Off全部回调 ----
    private void Test_EventKey_Enum_OffAll()
    {
        Debug.Log("[Test] Enum Key Off全部回调");
        EventKey key = TestEvent.Logout;
        int count1 = 0, count2 = 0;
        Action cb1 = () => count1++;
        Action<string> cb2 = _ => count2++;

        EventMgr.On(key, cb1);
        EventMgr.On(key, cb2);
        EventMgr.Off(key);
        EventMgr.Dispatch(key, "data");

        Assert(count1 == 0, "无参回调未被调用");
        Assert(count2 == 0, "带参回调未被调用");
    }

    // ---- 枚举Key(Int) 与同名字符串Key(String) 互不干扰 ----
    private void Test_EventKey_Enum_EqualsStringName()
    {
        Debug.Log("[Test] Enum Key(Int) 与同名 string Key(String) 互不干扰");
        EventKey enumKey = TestEvent.Login;
        EventKey stringKey = "Login";
        int count = 0;
        Action cb = () => count++;

        EventMgr.On(enumKey, cb);
        EventMgr.Dispatch(stringKey);

        Assert(count == 0, "enum Key(Int) 与同名 string Key(String) 互不干扰");

        EventMgr.Off(enumKey, cb);
        CleanupEvent(enumKey);
    }

    // ---- 不同枚举类型相同底层int值共享同一Key ----
    private void Test_EventKey_Enum_SameNameDifferentEnumSharesKey()
    {
        Debug.Log("[Test] 不同枚举相同int值共享Key");
        EventKey key1 = TestEvent.Login; // int: 0
        EventKey key2 = AnotherEvent.Login; // int: 0
        int count = 0;
        Action cb = () => count++;

        EventMgr.On(key1, cb);
        EventMgr.Dispatch(key2);

        Assert(count == 1, "不同枚举类型但底层int值相同时共享EventKey");

        EventMgr.Off(key1, cb);
        CleanupEvent(key1);
    }

    // ---- 同一枚举多个值同时注册 ----
    private void Test_EventKey_Enum_MultipleValues()
    {
        Debug.Log("[Test] 同一枚举多个值同时注册与分发");
        EventKey loginKey = TestEvent.Login;
        EventKey logoutKey = TestEvent.Logout;
        EventKey purchaseKey = TestEvent.Purchase;
        int loginCount = 0, logoutCount = 0, purchaseCount = 0;
        Action loginCb = () => loginCount++;
        Action logoutCb = () => logoutCount++;
        Action purchaseCb = () => purchaseCount++;

        EventMgr.On(loginKey, loginCb);
        EventMgr.On(logoutKey, logoutCb);
        EventMgr.On(purchaseKey, purchaseCb);

        EventMgr.Dispatch(loginKey);
        EventMgr.Dispatch(logoutKey);
        EventMgr.Dispatch(purchaseKey);
        EventMgr.Dispatch(purchaseKey);

        Assert(loginCount == 1, "Login触发1次");
        Assert(logoutCount == 1, "Logout触发1次");
        Assert(purchaseCount == 2, "Purchase触发2次");

        EventMgr.Off(loginKey, loginCb);
        EventMgr.Off(logoutKey, logoutCb);
        EventMgr.Off(purchaseKey, purchaseCb);
        CleanupEvent(loginKey, logoutKey, purchaseKey);
    }

    // ---- NotEventKey 类型的 Dispatch 直接跳过 ----
    private void Test_EventKey_NotEventKey_DispatchSkipped()
    {
        Debug.Log("[Test] NotEventKey 分发被跳过");
        EventKey key = new();
        bool globalFired = false;
        Action<EventKey> globalCb = _ => globalFired = true;

        EventMgr.OnEvent += globalCb;
        EventMgr.Dispatch(key);

        Assert(!globalFired, "NotEventKey不会触发OnEvent");

        EventMgr.OnEvent -= globalCb;
    }

    // ---- CurrentTriggeredEvent 在 Dispatch 过程中正确设置 ----
    private void Test_CurrentTriggeredEvent_DuringDispatch()
    {
        Debug.Log("[Test] CurrentTriggeredEvent 在分发过程中正确");
        EventKey key = "test_current_event";
        EventKey captured = new();
        Action cb = () => captured = EventMgr.CurrentTriggeredEvent;

        EventMgr.On(key, cb);
        EventMgr.Dispatch(key);

        Assert(captured.Equals(key), "分发过程中 CurrentTriggeredEvent 与分发的Key一致");

        EventKey afterDispatch = EventMgr.CurrentTriggeredEvent;
        Assert(afterDispatch.Type == EventKeyType.NotEventKey, "分发结束后 CurrentTriggeredEvent 重置为 NotEventKey");

        EventMgr.Off(key, cb);
        CleanupEvent(key);
    }

    /// <summary>
    /// 多种嵌套环：长链、三角、自环，以及多链路汇交（菱形）与经桥接跳回前驱的交叉死循环；重复进入已在栈上的 Key 时应 Warn，不卡死。
    /// </summary>
    private void Test_Dispatch_DeadLoopChain_Detection()
    {
        // 1) 长链：A→B→C→D→E→F→B（经多跳后再次进入 B）
        Debug.Log("[Test] 死循环 ① 长链 A→B→C→D→E→F→B（第二次 Dispatch(B) 应被拦截，控制台 Warn）");
        EventKey kA = "A", kB = "B", kC = "C", kD = "D", kE = "E", kF = "F";
        int cA = 0, cB = 0, cC = 0, cD = 0, cE = 0, cF = 0;

        EventMgr.On(kA, () =>
        {
            cA++;
            EventMgr.Dispatch(kB);
        });
        EventMgr.On(kB, () =>
        {
            cB++;
            EventMgr.Dispatch(kC);
        });
        EventMgr.On(kC, () =>
        {
            cC++;
            EventMgr.Dispatch(kD);
        });
        EventMgr.On(kD, () =>
        {
            cD++;
            EventMgr.Dispatch(kE);
        });
        EventMgr.On(kE, () =>
        {
            cE++;
            EventMgr.Dispatch(kF);
        });
        EventMgr.On(kF, () =>
        {
            cF++;
            EventMgr.Dispatch(kB);
        });

        EventMgr.Dispatch(kA);

        Assert(cA == 1 && cB == 1 && cC == 1 && cD == 1 && cE == 1 && cF == 1,
               "长链上各监听各执行一次，第二次 Dispatch(B) 不重复跑 B 的监听");

        CleanupEvent(kA, kB, kC, kD, kE, kF);

        // 2) 三角：A→B→C→D→B（入口 A，经 B、C、D 后回到已在栈上的 B）
        Debug.Log("[Test] 死循环 ② 三角 A→B→C→D→B（第二次 Dispatch(B) 应被拦截）");
        kA = "A";
        kB = "B";
        kC = "C";
        kD = "D";
        cA = 0;
        cB = 0;
        cC = 0;
        cD = 0;

        EventMgr.On(kA, () =>
        {
            cA++;
            EventMgr.Dispatch(kB);
        });
        EventMgr.On(kB, () =>
        {
            cB++;
            EventMgr.Dispatch(kC);
        });
        EventMgr.On(kC, () =>
        {
            cC++;
            EventMgr.Dispatch(kD);
        });
        EventMgr.On(kD, () =>
        {
            cD++;
            EventMgr.Dispatch(kB);
        });

        EventMgr.Dispatch(kA);

        Assert(cA == 1 && cB == 1 && cC == 1 && cD == 1,
               "三角链上各监听各执行一次，第二次 Dispatch(B) 不重复执行");

        CleanupEvent(kA, kB, kC, kD);

        // 3) 自环：A→A
        Debug.Log("[Test] 死循环 ③ 监听内 Dispatch(A)（自环，应被拦截）");
        kA = "A";
        cA = 0;
        EventMgr.On(kA, () =>
        {
            cA++;
            EventMgr.Dispatch(kA);
        });
        EventMgr.Dispatch(kA);
        Assert(cA == 1, "自环只触发一次监听体");

        CleanupEvent(kA);

        // 4) 菱形汇交：A→B→C→D→E→F→D（左支到 D，右支离开后再入 D）
        Debug.Log("[Test] 死循环 ④ 双支汇交 A→B→C→D→E→F→D（第二次 Dispatch(D) 应被拦截）");
        kA = "A";
        kB = "B";
        kC = "C";
        kD = "D";
        kE = "E";
        kF = "F";
        cA = 0;
        cB = 0;
        cC = 0;
        cD = 0;
        cE = 0;
        cF = 0;

        EventMgr.On(kA, () =>
        {
            cA++;
            EventMgr.Dispatch(kB);
        });
        EventMgr.On(kB, () =>
        {
            cB++;
            EventMgr.Dispatch(kC);
        });
        EventMgr.On(kC, () =>
        {
            cC++;
            EventMgr.Dispatch(kD);
        });
        EventMgr.On(kD, () =>
        {
            cD++;
            EventMgr.Dispatch(kE);
        });
        EventMgr.On(kE, () =>
        {
            cE++;
            EventMgr.Dispatch(kF);
        });
        EventMgr.On(kF, () =>
        {
            cF++;
            EventMgr.Dispatch(kD);
        });

        EventMgr.Dispatch(kA);

        Assert(cA == 1 && cB == 1 && cC == 1 && cD == 1 && cE == 1 && cF == 1,
               "汇交菱形链上各监听各执行一次，第二次 Dispatch(D) 不重复");

        CleanupEvent(kA, kB, kC, kD, kE, kF);

        // 5) 桥接跳回：A→B→C→D→E→F→B（F 为桥，回到较早的 B）
        Debug.Log("[Test] 死循环 ⑤ 交叉跳回 A→B→C→D→E→F→B（第二次 Dispatch(B) 应被拦截）");
        kA = "A";
        kB = "B";
        kC = "C";
        kD = "D";
        kE = "E";
        kF = "F";
        cA = 0;
        cB = 0;
        cC = 0;
        cD = 0;
        cE = 0;
        cF = 0;

        EventMgr.On(kA, () =>
        {
            cA++;
            EventMgr.Dispatch(kB);
        });
        EventMgr.On(kB, () =>
        {
            cB++;
            EventMgr.Dispatch(kC);
        });
        EventMgr.On(kC, () =>
        {
            cC++;
            EventMgr.Dispatch(kD);
        });
        EventMgr.On(kD, () =>
        {
            cD++;
            EventMgr.Dispatch(kE);
        });
        EventMgr.On(kE, () =>
        {
            cE++;
            EventMgr.Dispatch(kF);
        });
        EventMgr.On(kF, () =>
        {
            cF++;
            EventMgr.Dispatch(kB);
        });

        EventMgr.Dispatch(kA);

        Assert(cA == 1 && cB == 1 && cC == 1 && cD == 1 && cE == 1 && cF == 1,
               "交叉跳回链上各监听各执行一次，经 F 的第二次 Dispatch(B) 不重复");

        CleanupEvent(kA, kB, kC, kD, kE, kF);
    }

    // ---- OnEvent 全局监听 ----
    private void Test_OnEvent_GlobalListener()
    {
        Debug.Log("[Test] OnEvent 全局监听器");
        EventKey key = "test_on_event";
        EventKey receivedKey = new();
        Action<EventKey> globalCb = k => receivedKey = k;

        EventMgr.On(key, () => { });
        EventMgr.OnEvent += globalCb;
        EventMgr.Dispatch(key);

        Assert(receivedKey.Equals(key), "OnEvent 收到正确的EventKey");

        EventMgr.OnEvent -= globalCb;
        CleanupEvent(key);
    }

    // ---- 同一事件多个回调都被调用 ----
    private void Test_MultipleCallbacksSameEvent()
    {
        Debug.Log("[Test] 同一事件多个回调");
        EventKey key = "test_multi_cb";
        int count1 = 0, count2 = 0, count3 = 0;
        Action cb1 = () => count1++;
        Action cb2 = () => count2++;
        Action<int> cb3 = _ => count3++;

        EventMgr.On(key, cb1);
        EventMgr.On(key, cb2);
        EventMgr.On(key, cb3);
        EventMgr.Dispatch(key, 99);

        Assert(count1 == 1, "回调1被调用");
        Assert(count2 == 1, "回调2被调用");
        Assert(count3 == 1, "带参回调3被调用");

        EventMgr.Off(key, cb1);
        EventMgr.Off(key, cb2);
        EventMgr.Off(key, cb3);
        CleanupEvent(key);
    }

    // ---- On(EventKey, target, Action) 显式目标 ----
    private void Test_On_WithExplicitTarget_NoArg()
    {
        Debug.Log("[Test] On(EventKey, target, Action) 显式目标无参");
        EventKey key = "test_explicit_target_noarg";
        int callCount = 0;
        object target = new();
        Action cb = () => callCount++;

        EventMgr.On(key, target, cb);
        EventMgr.Dispatch(key);

        Assert(callCount == 1, "显式目标无参回调被调用");

        EventMgr.Off(key, target);
        CleanupEvent(key);
    }

    // ---- On<T>(EventKey, target, Action<T>) 显式目标 ----
    private void Test_On_WithExplicitTarget_WithArg()
    {
        Debug.Log("[Test] On<T>(EventKey, target, Action<T>) 显式目标带参");
        EventKey key = "test_explicit_target_arg";
        float received = 0f;
        object target = new();
        Action<float> cb = v => received = v;

        EventMgr.On(key, target, cb);
        EventMgr.Dispatch(key, 3.14f);

        Assert(Mathf.Approximately(received, 3.14f), "显式目标带参回调收到正确值");

        EventMgr.Off(key, target);
        CleanupEvent(key);
    }

    // ---- 没有回调注册时 Dispatch 不报错 ----
    private void Test_Dispatch_NoCallbacksRegistered()
    {
        Debug.Log("[Test] 无回调注册时 Dispatch 不报错");
        EventKey key = "test_dispatch_empty";
        bool noException = true;
        try
        {
            EventMgr.Dispatch(key);
            EventMgr.Dispatch(key, 42);
        }
        catch (Exception)
        {
            noException = false;
        }

        Assert(noException, "无回调时 Dispatch 正常执行");
    }

    // ---- Off 一个不存在的回调不报错 ----
    private void Test_Off_NonExistentCallback()
    {
        Debug.Log("[Test] Off 不存在的回调不报错");
        EventKey key = "test_off_nonexist";
        bool noException = true;
        try
        {
            EventMgr.Off(key, () => { });
            EventMgr.Off<int>(key, _ => { });
            EventMgr.Off(key);
            EventMgr.Off(key, new object());
        }
        catch (Exception)
        {
            noException = false;
        }

        Assert(noException, "注销不存在的回调无异常");
    }

    // ---- 分发过程中取消订阅不会崩溃 ----
    private void Test_UnsubscribeDuringDispatch()
    {
        Debug.Log("[Test] 分发过程中取消订阅");
        EventKey key = "test_unsub_during_dispatch";
        int count = 0;
        Action cb2 = () => count++;
        Action cb1 = null;
        cb1 = () =>
        {
            count++;
            EventMgr.Off(key, cb2);
        };

        EventMgr.On(key, cb1);
        EventMgr.On(key, cb2);

        bool noException = true;
        try
        {
            EventMgr.Dispatch(key);
        }
        catch (Exception)
        {
            noException = false;
        }

        Assert(noException, "分发中取消订阅不崩溃");
        Assert(count >= 1, "至少一个回调被执行");

        EventMgr.Off(key, cb1);
        CleanupEvent(key);
    }

    private enum TestEvent
    {
        Login,
        Logout,
        Purchase
    }

    private enum AnotherEvent
    {
        Login,
        Shutdown
    }
}