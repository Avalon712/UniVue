using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UniVue.Event;

namespace UniVue.Test
{
    /// <summary>
    /// <see cref="EventMgr" /> 的单元测试。覆盖：无参/带参注册与分发、各种 Off 注销语义、
    /// <see cref="EventKey" /> 的 int/string/enum 三种形态、全局 <c>OnEvent</c>、<c>CurrentTriggeredEvent</c>、
    /// 死循环链路检测，以及一系列健壮性场景（无回调分发、注销不存在回调、分发中取消订阅）。
    /// <para>
    /// <see cref="EventMgr" /> 是全局静态状态，且 <c>CheckMutiDelegate</c> 等断言仅在 <c>UNITY_EDITOR</c> 下生效，
    /// 因此全部用例以 EditMode（<see cref="TestAttribute" />）方式运行；每个用例结束后通过 <see cref="TearDown" />
    /// 反射清空静态字典与事件，保证用例间完全隔离（即便断言失败导致用例内清理未执行）。
    /// </para>
    /// </summary>
    internal sealed class EventTest
    {
        private static int _staticCallCount;

        /// <summary>清空 <see cref="EventMgr" /> 的全部全局静态状态，保证用例间互不影响。</summary>
        [TearDown]
        public void TearDown()
        {
            Type t = typeof(EventMgr);
            const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Static;

            (t.GetField("_callbacks", flags)?.GetValue(null) as IDictionary)?.Clear();
            (t.GetField("_dispatchPaths", flags)?.GetValue(null) as IList)?.Clear();
            t.GetField("OnEvent", flags)?.SetValue(null, null);
            t.GetField("OnDeadLoop", flags)?.SetValue(null, null);
        }

        #region 基本注册与分发

        [Test]
        public void OnAndDispatch_NoArg_InvokesEachDispatch()
        {
            EventKey key = "test_no_arg";
            int callCount = 0;
            Action cb = () => callCount++;

            EventMgr.On(key, cb);
            EventMgr.Dispatch(key);
            Assert.AreEqual(1, callCount, "回调应被调用一次");

            EventMgr.Dispatch(key);
            Assert.AreEqual(2, callCount, "再次分发后回调应被调用两次");
        }

        [Test]
        public void OnAndDispatch_WithArg_ReceivesArgument()
        {
            EventKey key = "test_with_arg";
            string received = null;
            Action<string> cb = s => received = s;

            EventMgr.On(key, cb);
            EventMgr.Dispatch(key, "hello");

            Assert.AreEqual("hello", received, "带参回调应收到正确的参数值");
        }

        [Test]
        public void DispatchWithArg_AlsoCallsNoArgListeners()
        {
            EventKey key = "test_arg_calls_noarg";
            bool noArgCalled = false;
            string argReceived = null;
            Action noArgCb = () => noArgCalled = true;
            Action<string> argCb = s => argReceived = s;

            EventMgr.On(key, noArgCb);
            EventMgr.On(key, argCb);
            EventMgr.Dispatch(key, "world");

            Assert.IsTrue(noArgCalled, "带参分发也应调用无参监听");
            Assert.AreEqual("world", argReceived, "带参回调应收到正确参数");
        }

        [Test]
        public void DispatchNoArg_DoesNotCallArgListeners()
        {
            EventKey key = "test_noarg_skip_arg";
            bool argCalled = false;
            Action<int> argCb = _ => argCalled = true;

            EventMgr.On(key, argCb);
            EventMgr.Dispatch(key);

            Assert.IsFalse(argCalled, "无参分发不应调用带参监听");
        }

        #endregion

        #region Off 注销语义

        [Test]
        public void Off_SpecificNoArgCallback_RemovesOnlyIt()
        {
            EventKey key = "test_off_specific";
            int callCount = 0;
            Action cb = () => callCount++;

            EventMgr.On(key, cb);
            EventMgr.Off(key, cb);
            EventMgr.Dispatch(key);

            Assert.AreEqual(0, callCount, "注销后无参回调不应被调用");
        }

        [Test]
        public void Off_SpecificGenericCallback_RemovesOnlyIt()
        {
            EventKey key = "test_off_generic";
            int callCount = 0;
            Action<int> cb = _ => callCount++;

            EventMgr.On(key, cb);
            EventMgr.Off(key, cb);
            EventMgr.Dispatch(key, 42);

            Assert.AreEqual(0, callCount, "注销后带参回调不应被调用");
        }

        [Test]
        public void Off_ByEventKey_RemovesAllListeners()
        {
            EventKey key = "test_off_all";
            int count1 = 0, count2 = 0;
            Action cb1 = () => count1++;
            Action<string> cb2 = _ => count2++;

            EventMgr.On(key, cb1);
            EventMgr.On(key, cb2);
            EventMgr.Off(key);
            EventMgr.Dispatch(key, "data");

            Assert.AreEqual(0, count1, "无参回调不应被调用");
            Assert.AreEqual(0, count2, "带参回调不应被调用");
        }

        [Test]
        public void Off_ByTargetOnSpecificEvent_RemovesTargetCallbacks()
        {
            EventKey key = "test_off_target_event";
            int count = 0;
            object target = new();
            Action cb = () => count++;

            EventMgr.On(key, target, cb);
            EventMgr.Off(key, target);
            EventMgr.Dispatch(key);

            Assert.AreEqual(0, count, "按目标注销后回调不应被调用");
        }

        [Test]
        public void Off_ByTargetOnAllEvents_RemovesTargetEverywhere()
        {
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

            Assert.AreEqual(0, count1, "事件1回调不应被调用");
            Assert.AreEqual(0, count2, "事件2回调不应被调用");
        }

        [Test]
        public void Off_NullTarget_IgnoredByDefault()
        {
            EventKey key = "test_off_null_default";
            int count = 0;
            object target = new();
            Action cb = () => count++;

            EventMgr.On(key, target, cb);
            EventMgr.Off((object)null);
            EventMgr.Dispatch(key);

            Assert.AreEqual(1, count, "null 目标默认被忽略，回调仍应生效");
        }

        [Test]
        public void Off_NullTarget_Allowed_RemovesStaticCallbacks()
        {
            EventKey key = "test_off_null_allowed";
            _staticCallCount = 0;

            EventMgr.On(key, StaticTestCallback);
            EventMgr.Off(null, true);
            EventMgr.Dispatch(key);

            Assert.AreEqual(0, _staticCallCount, "允许 null 目标后静态回调应被注销");
        }

        #endregion

        #region EventKey 形态

        [Test]
        public void EventKey_Int_Dispatches()
        {
            EventKey key = 100;
            int callCount = 0;
            Action cb = () => callCount++;

            EventMgr.On(key, cb);
            EventMgr.Dispatch(key);

            Assert.AreEqual(1, callCount, "int 类型 EventKey 应分发成功");
        }

        [Test]
        public void EventKey_String_Dispatches()
        {
            EventKey key = "my_string_event";
            int callCount = 0;
            Action cb = () => callCount++;

            EventMgr.On(key, cb);
            EventMgr.Dispatch(key);

            Assert.AreEqual(1, callCount, "string 类型 EventKey 应分发成功");
        }

        [Test]
        public void EventKey_Enum_Dispatches()
        {
            EventKey key = TestEvent.Login;
            int callCount = 0;
            Action cb = () => callCount++;

            EventMgr.On(key, cb);
            EventMgr.Dispatch(key);

            Assert.AreEqual(1, callCount, "enum 类型 EventKey 应分发成功");
        }

        [Test]
        public void EventKey_Enum_DifferentValuesIsolated()
        {
            EventKey loginKey = TestEvent.Login;
            EventKey logoutKey = TestEvent.Logout;
            int loginCount = 0, logoutCount = 0;
            Action loginCb = () => loginCount++;
            Action logoutCb = () => logoutCount++;

            EventMgr.On(loginKey, loginCb);
            EventMgr.On(logoutKey, logoutCb);
            EventMgr.Dispatch(loginKey);

            Assert.AreEqual(1, loginCount, "Login 回调应被调用");
            Assert.AreEqual(0, logoutCount, "Logout 回调不应被调用");
        }

        [Test]
        public void EventKey_Enum_WithArg_ReceivesValue()
        {
            EventKey key = TestEvent.Purchase;
            float received = 0f;
            Action<float> cb = v => received = v;

            EventMgr.On(key, cb);
            EventMgr.Dispatch(key, 99.5f);

            Assert.That(received, Is.EqualTo(99.5f).Within(1e-4f), "枚举 Key 带参分发应收到正确值");
        }

        [Test]
        public void EventKey_Enum_OffSpecificCallback()
        {
            EventKey key = TestEvent.Login;
            int count = 0;
            Action cb = () => count++;

            EventMgr.On(key, cb);
            EventMgr.Off(key, cb);
            EventMgr.Dispatch(key);

            Assert.AreEqual(0, count, "枚举 Key 注销后回调不应被调用");
        }

        [Test]
        public void EventKey_Enum_OffAll()
        {
            EventKey key = TestEvent.Logout;
            int count1 = 0, count2 = 0;
            Action cb1 = () => count1++;
            Action<string> cb2 = _ => count2++;

            EventMgr.On(key, cb1);
            EventMgr.On(key, cb2);
            EventMgr.Off(key);
            EventMgr.Dispatch(key, "data");

            Assert.AreEqual(0, count1, "无参回调不应被调用");
            Assert.AreEqual(0, count2, "带参回调不应被调用");
        }

        [Test]
        public void EventKey_Enum_IntAndSameNameStringAreIsolated()
        {
            EventKey enumKey = TestEvent.Login; // EventKeyType.Int
            EventKey stringKey = "Login"; // EventKeyType.String
            int count = 0;
            Action cb = () => count++;

            EventMgr.On(enumKey, cb);
            EventMgr.Dispatch(stringKey);

            Assert.AreEqual(0, count, "enum Key(Int) 与同名 string Key(String) 应互不干扰");
        }

        [Test]
        public void EventKey_Enum_SameUnderlyingIntSharesKey()
        {
            EventKey key1 = TestEvent.Login; // 底层 int: 0
            EventKey key2 = AnotherEvent.Login; // 底层 int: 0
            int count = 0;
            Action cb = () => count++;

            EventMgr.On(key1, cb);
            EventMgr.Dispatch(key2);

            Assert.AreEqual(1, count, "不同枚举类型但底层 int 值相同时应共享同一 EventKey");
        }

        [Test]
        public void EventKey_Enum_MultipleValues_RegisteredAndDispatchedIndependently()
        {
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

            Assert.AreEqual(1, loginCount, "Login 应触发 1 次");
            Assert.AreEqual(1, logoutCount, "Logout 应触发 1 次");
            Assert.AreEqual(2, purchaseCount, "Purchase 应触发 2 次");
        }

        [Test]
        public void EventKey_NotEventKey_DispatchSkipped()
        {
            EventKey key = new(); // EventKeyType.NotEventKey
            bool globalFired = false;
            Action<EventKey> globalCb = _ => globalFired = true;

            EventMgr.OnEvent += globalCb;
            EventMgr.Dispatch(key);

            Assert.IsFalse(globalFired, "NotEventKey 不应触发 OnEvent");
        }

        #endregion

        #region CurrentTriggeredEvent / OnEvent

        [Test]
        public void CurrentTriggeredEvent_SetDuringDispatch_ResetAfter()
        {
            EventKey key = "test_current_event";
            EventKey captured = new();
            Action cb = () => captured = EventMgr.CurrentTriggeredEvent;

            EventMgr.On(key, cb);
            EventMgr.Dispatch(key);

            Assert.IsTrue(captured.Equals(key), "分发过程中 CurrentTriggeredEvent 应与分发的 Key 一致");
            Assert.AreEqual(EventKeyType.NotEventKey, EventMgr.CurrentTriggeredEvent.Type,
                            "分发结束后 CurrentTriggeredEvent 应重置为 NotEventKey");
        }

        [Test]
        public void OnEvent_GlobalListener_ReceivesDispatchedKey()
        {
            EventKey key = "test_on_event";
            EventKey receivedKey = new();
            Action<EventKey> globalCb = k => receivedKey = k;

            EventMgr.On(key, () => { });
            EventMgr.OnEvent += globalCb;
            EventMgr.Dispatch(key);

            Assert.IsTrue(receivedKey.Equals(key), "OnEvent 应收到正确的 EventKey");
        }

        #endregion

        #region 死循环检测

        /// <summary>
        /// 多种嵌套环：长链、三角、自环、菱形汇交、桥接跳回。重复进入已在栈上的 Key 时应被拦截（控制台 Warn），
        /// 链路上每个监听体各执行一次，不会卡死或重复执行。
        /// </summary>
        [Test]
        public void Dispatch_DeadLoopChains_DetectedAndEachListenerRunsOnce()
        {
            // 1) 长链：A→B→C→D→E→F→B
            EventKey kA = "A", kB = "B", kC = "C", kD = "D", kE = "E", kF = "F";
            int cA = 0, cB = 0, cC = 0, cD = 0, cE = 0, cF = 0;

            EventMgr.On(kA, () => { cA++; EventMgr.Dispatch(kB); });
            EventMgr.On(kB, () => { cB++; EventMgr.Dispatch(kC); });
            EventMgr.On(kC, () => { cC++; EventMgr.Dispatch(kD); });
            EventMgr.On(kD, () => { cD++; EventMgr.Dispatch(kE); });
            EventMgr.On(kE, () => { cE++; EventMgr.Dispatch(kF); });
            EventMgr.On(kF, () => { cF++; EventMgr.Dispatch(kB); });

            EventMgr.Dispatch(kA);

            Assert.IsTrue(cA == 1 && cB == 1 && cC == 1 && cD == 1 && cE == 1 && cF == 1,
                          "长链上各监听各执行一次，第二次 Dispatch(B) 不重复执行");

            ResetCallbacks();

            // 2) 三角：A→B→C→D→B
            cA = cB = cC = cD = 0;
            EventMgr.On(kA, () => { cA++; EventMgr.Dispatch(kB); });
            EventMgr.On(kB, () => { cB++; EventMgr.Dispatch(kC); });
            EventMgr.On(kC, () => { cC++; EventMgr.Dispatch(kD); });
            EventMgr.On(kD, () => { cD++; EventMgr.Dispatch(kB); });

            EventMgr.Dispatch(kA);

            Assert.IsTrue(cA == 1 && cB == 1 && cC == 1 && cD == 1,
                          "三角链上各监听各执行一次，第二次 Dispatch(B) 不重复执行");

            ResetCallbacks();

            // 3) 自环：A→A
            cA = 0;
            EventMgr.On(kA, () => { cA++; EventMgr.Dispatch(kA); });
            EventMgr.Dispatch(kA);
            Assert.AreEqual(1, cA, "自环只触发一次监听体");

            ResetCallbacks();

            // 4) 菱形汇交：A→B→C→D→E→F→D
            cA = cB = cC = cD = cE = cF = 0;
            EventMgr.On(kA, () => { cA++; EventMgr.Dispatch(kB); });
            EventMgr.On(kB, () => { cB++; EventMgr.Dispatch(kC); });
            EventMgr.On(kC, () => { cC++; EventMgr.Dispatch(kD); });
            EventMgr.On(kD, () => { cD++; EventMgr.Dispatch(kE); });
            EventMgr.On(kE, () => { cE++; EventMgr.Dispatch(kF); });
            EventMgr.On(kF, () => { cF++; EventMgr.Dispatch(kD); });

            EventMgr.Dispatch(kA);

            Assert.IsTrue(cA == 1 && cB == 1 && cC == 1 && cD == 1 && cE == 1 && cF == 1,
                          "汇交菱形链上各监听各执行一次，第二次 Dispatch(D) 不重复");

            ResetCallbacks();

            // 5) 桥接跳回：A→B→C→D→E→F→B
            cA = cB = cC = cD = cE = cF = 0;
            EventMgr.On(kA, () => { cA++; EventMgr.Dispatch(kB); });
            EventMgr.On(kB, () => { cB++; EventMgr.Dispatch(kC); });
            EventMgr.On(kC, () => { cC++; EventMgr.Dispatch(kD); });
            EventMgr.On(kD, () => { cD++; EventMgr.Dispatch(kE); });
            EventMgr.On(kE, () => { cE++; EventMgr.Dispatch(kF); });
            EventMgr.On(kF, () => { cF++; EventMgr.Dispatch(kB); });

            EventMgr.Dispatch(kA);

            Assert.IsTrue(cA == 1 && cB == 1 && cC == 1 && cD == 1 && cE == 1 && cF == 1,
                          "交叉跳回链上各监听各执行一次，经 F 的第二次 Dispatch(B) 不重复");
        }

        #endregion

        #region 多回调 / 显式目标

        [Test]
        public void MultipleCallbacksSameEvent_AllInvoked()
        {
            EventKey key = "test_multi_cb";
            int count1 = 0, count2 = 0, count3 = 0;
            Action cb1 = () => count1++;
            Action cb2 = () => count2++;
            Action<int> cb3 = _ => count3++;

            EventMgr.On(key, cb1);
            EventMgr.On(key, cb2);
            EventMgr.On(key, cb3);
            EventMgr.Dispatch(key, 99);

            Assert.AreEqual(1, count1, "回调1应被调用");
            Assert.AreEqual(1, count2, "回调2应被调用");
            Assert.AreEqual(1, count3, "带参回调3应被调用");
        }

        [Test]
        public void On_WithExplicitTarget_NoArg()
        {
            EventKey key = "test_explicit_target_noarg";
            int callCount = 0;
            object target = new();
            Action cb = () => callCount++;

            EventMgr.On(key, target, cb);
            EventMgr.Dispatch(key);

            Assert.AreEqual(1, callCount, "显式目标无参回调应被调用");
        }

        [Test]
        public void On_WithExplicitTarget_WithArg()
        {
            EventKey key = "test_explicit_target_arg";
            float received = 0f;
            object target = new();
            Action<float> cb = v => received = v;

            EventMgr.On(key, target, cb);
            EventMgr.Dispatch(key, 3.14f);

            Assert.That(received, Is.EqualTo(3.14f).Within(1e-4f), "显式目标带参回调应收到正确值");
        }

        #endregion

        #region 健壮性

        [Test]
        public void Dispatch_NoCallbacksRegistered_DoesNotThrow()
        {
            EventKey key = "test_dispatch_empty";
            Assert.DoesNotThrow(() =>
            {
                EventMgr.Dispatch(key);
                EventMgr.Dispatch(key, 42);
            }, "无回调时 Dispatch 应正常执行");
        }

        [Test]
        public void Off_NonExistentCallback_DoesNotThrow()
        {
            EventKey key = "test_off_nonexist";
            Assert.DoesNotThrow(() =>
            {
                EventMgr.Off(key, () => { });
                EventMgr.Off<int>(key, _ => { });
                EventMgr.Off(key);
                EventMgr.Off(key, new object());
            }, "注销不存在的回调不应抛异常");
        }

        [Test]
        public void UnsubscribeDuringDispatch_DoesNotCrash()
        {
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

            Assert.DoesNotThrow(() => EventMgr.Dispatch(key), "分发中取消订阅不应崩溃");
            Assert.GreaterOrEqual(count, 1, "至少一个回调应被执行");
        }

        #endregion

        #region Helpers / 测试用类型

        private static void StaticTestCallback()
        {
            _staticCallCount++;
        }

        /// <summary>清空 <see cref="EventMgr" /> 已注册的回调（用于单个用例内多段子场景之间的重置）。</summary>
        private static void ResetCallbacks()
        {
            const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Static;
            (typeof(EventMgr).GetField("_callbacks", flags)?.GetValue(null) as IDictionary)?.Clear();
            (typeof(EventMgr).GetField("_dispatchPaths", flags)?.GetValue(null) as IList)?.Clear();
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

        #endregion
    }
}
