using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.TestTools;
using UniVue.UI;

namespace UniVue.Test
{
    /// <summary>
    /// <see cref="RedPointMgr" /> 的单元测试。覆盖四类行为：
    /// <list type="bullet">
    /// <item>静态 Key 几何编码 API（根/父/子/深度判断），与运行时无关，用 <see cref="TestAttribute" /> 运行。</item>
    /// <item>枚举初始化的红点树状态传播（叶子 → 父 → 根，帧末协程刷新）。</item>
    /// <item>监听订阅 / 取消订阅回调。</item>
    /// <item>动态树 API（创建、添加依赖、状态、删除）。</item>
    /// </list>
    /// <para>
    /// <see cref="RedPointMgr" /> 通过 <c>VCoroutine</c> 在帧末（<see cref="UnityEngine.WaitForEndOfFrame" />）统一刷新
    /// 被标记为脏的红点树，因此凡是涉及父/根节点状态传播的断言都必须以 PlayMode 协程测试
    /// （<see cref="UnityTestAttribute" />）方式运行，并在修改叶子后等待若干帧让帧末协程完成刷新。
    /// 而叶子自身的状态在 <c>SetActive</c> 后即时可读，无需等待。
    /// </para>
    /// </summary>
    public sealed class RedPointTreeTest
    {
        #region Helpers

        /// <summary>逐帧等待直到 <paramref name="predicate" /> 成立，超出最大帧数则判定失败。</summary>
        private static IEnumerator WaitUntil(Func<bool> predicate, string message)
        {
            const int maxFrames = 300;
            int frames = 0;
            while (!predicate())
            {
                if (++frames > maxFrames)
                {
                    Assert.Fail($"等待超时（{maxFrames} 帧）：{message}");
                    yield break;
                }

                yield return null;
            }
        }

        /// <summary>固定等待若干帧（用于「状态应保持不变」这类稳定性断言）。</summary>
        private static IEnumerator WaitFrames(int count)
        {
            for (int i = 0; i < count; i++)
                yield return null;
        }

        #endregion

        #region 静态 Key 几何编码

        [Test]
        public void StaticKeyGeometry_ParentChildDepthRoot()
        {
            ulong main = (ulong)RedPointKey.Test_Main;
            ulong t1 = (ulong)RedPointKey.Test_1;
            ulong t12 = (ulong)RedPointKey.Test_1_2;

            Assert.IsTrue(RedPointMgr.IsRoot(main), "Test_Main 应为根（低 48 位为 0）");
            Assert.IsFalse(RedPointMgr.IsRoot(t1), "Test_1 不应为根");
            Assert.AreEqual(0, RedPointMgr.GetDepth(main), "根深度应为 0");
            Assert.AreEqual(1, RedPointMgr.GetDepth(t1), "Test_1 深度应为 1");
            Assert.AreEqual(2, RedPointMgr.GetDepth(t12), "Test_1_2 深度应为 2");
            Assert.AreEqual(main, RedPointMgr.GetRootKey(t12), "Test_1_2 的根应为 Test_Main");
            Assert.AreEqual(main, RedPointMgr.GetParentKey(t1), "Test_1 的父应为 Test_Main");
            Assert.AreEqual(t1, RedPointMgr.GetParentKey(t12), "Test_1_2 的父应为 Test_1");
            Assert.IsTrue(RedPointMgr.IsParentOf(t1, t12), "IsParentOf(Test_1, Test_1_2) 应为真");
            Assert.AreNotEqual(main, RedPointMgr.GetRootKey((ulong)RedPointKey.Test2_Main),
                               "Test2 树根应与 Test_Main 不同");
        }

        #endregion

        #region 枚举树状态传播

        [UnityTest]
        public IEnumerator EnumTree_StatusPropagatesToParentAndRoot()
        {
            RedPointMgr mgr = new(typeof(RedPointKey));
            Assert.IsTrue(mgr.IsRedPointKey((ulong)RedPointKey.Test_Main), "RedPointMgr 应从枚举成功初始化");

            ulong main = (ulong)RedPointKey.Test_Main;
            ulong t1 = (ulong)RedPointKey.Test_1;
            ulong t12 = (ulong)RedPointKey.Test_1_2;
            ulong t2 = (ulong)RedPointKey.Test_2;

            // 叶子的状态在 SetActive 后立即可读。
            mgr.SetActive(t12, true);
            Assert.IsTrue(mgr.GetStatus(t12), "叶子 SetActive(true) 后应立即可读为 true");

            // 父/根需等待帧末协程刷新后才会传播。
            yield return WaitUntil(() => mgr.GetStatus(t1), "帧末后父节点 Test_1 应更新为 true");
            Assert.IsTrue(mgr.GetStatus(main), "帧末后根 Test_Main 应传播为 true");

            mgr.SetActive(t12, false);
            yield return WaitUntil(() => !mgr.GetStatus(main), "Test_1_2 关闭后根应回到 false");

            // Or 规则：另一个兄弟叶子点亮，根依旧为 true。
            mgr.SetActive(t2, true);
            yield return WaitUntil(() => mgr.GetStatus(main), "兄弟叶子 Test_2 点亮后根应为 true (Or)");

            mgr.SetActive(t2, false);
            mgr.SetActive(t12, false);
            yield return WaitUntil(() => !mgr.GetStatus(main), "Test_1_2 / Test_2 均为 false 后根应为 false");
        }

        #endregion

        #region 监听订阅

        [UnityTest]
        public IEnumerator EnumTree_Listener_InvokedOnRootChangeAndStopsAfterUnListen()
        {
            RedPointMgr mgr = new(typeof(RedPointKey));
            Assert.IsTrue(mgr.IsRedPointKey((ulong)RedPointKey.Test_Main), "RedPointMgr 应从枚举成功初始化");

            ulong main = (ulong)RedPointKey.Test_Main;
            ulong t12 = (ulong)RedPointKey.Test_1_2;
            int rootChanges = 0;

            void OnRoot(bool _)
            {
                rootChanges++;
            }

            mgr.ListenerRedPointStatus(main, OnRoot);
            mgr.SetActive(t12, true);
            yield return WaitUntil(() => rootChanges >= 1, "根状态变化应触发监听回调");

            mgr.UnListenerRedPointStatus(main, OnRoot);
            int before = rootChanges;
            mgr.SetActive(t12, false);
            yield return WaitFrames(3);
            Assert.AreEqual(before, rootChanges, "取消订阅后回调计数应保持不变");
        }

        #endregion

        #region 动态树 API

        [UnityTest]
        public IEnumerator DynamicTree_CreateAddSetDelete_Works()
        {
            // 动态树 CreateRedPointTree 会分配形如 (2|rule)<<48 的根；为避免与之冲突，
            // 使用仅含一个占位根（Key 为 1<<48）的独立枚举来初始化 Mgr。
            RedPointMgr mgr = new(typeof(DynamicTreeMountEnum));
            ulong mount = (ulong)DynamicTreeMountEnum.MountOnly;
            Assert.IsTrue(mgr.IsRedPointKey(mount), "独立占位枚举应初始化成功");
            Assert.IsFalse(mgr.GetStatus(mount), "占位根初始应为 false");

            ulong dynRoot = mgr.CreateRedPointTree(RedPointRule.Or, "DynRoot");
            Assert.AreNotEqual(0UL, dynRoot, "CreateRedPointTree 应返回非 0 根 key");
            Assert.AreNotEqual(mount, dynRoot, "动态根应与占位枚举根不同");
            Assert.IsTrue(mgr.IsDynamicDependency(dynRoot), "新根应标记为动态树");
            Assert.IsTrue(RedPointMgr.IsRoot(dynRoot), "动态根应满足 IsRoot");

            ulong leafA = mgr.AddDependency(dynRoot);
            ulong leafB = mgr.AddDependency(dynRoot);
            Assert.IsTrue(leafA != 0 && leafB != 0 && leafA != leafB, "两个子节点 key 应有效且互异");

            mgr.SetActive(leafA, true);
            yield return WaitUntil(() => mgr.GetStatus(dynRoot), "Or 规则：一个叶子 true 则根为 true");

            mgr.SetActive(leafA, false);
            yield return WaitUntil(() => !mgr.GetStatus(dynRoot), "Or 规则：全部 false 则根为 false");

            mgr.SetActive(leafA, true);
            mgr.SetActive(leafB, true);
            yield return WaitUntil(() => mgr.GetStatus(dynRoot), "Or 规则：两个叶子 true 则根为 true");

            HashSet<ulong> children = new();
            mgr.GetChildrenKeysNoneAlloc(dynRoot, children);
            Assert.IsTrue(children.Contains(leafA) && children.Contains(leafB),
                          "GetChildrenKeysNoneAlloc 应返回全部子 key");

            mgr.DeleteDependency(leafB);
            Assert.IsTrue(mgr.IsRedPointKey(leafA) && !mgr.IsRedPointKey(leafB), "删除单个子节点应仅移除该节点");

            mgr.DeleteDependency(dynRoot);
            Assert.IsFalse(mgr.IsRedPointKey(dynRoot), "删除动态根后其 key 不应再存在");
        }

        #endregion

        #region 测试用红点 Key 枚举

        /// <summary>
        /// 与红点树编辑器导出的 <c>RedPointKey.g.cs</c> 编码规则一致的测试树：
        /// 根占高 16 位，其后每层各占 1 字节，末位为该节点的激活规则（0=Or，1=And）。
        /// </summary>
        private enum RedPointKey : ulong
        {
            Test_Main = 0b0000000000000010_00000000_00000000_00000000_00000000_00000000_00000000UL,

            Test_1 = 0b0000000000000010_00000010_00000000_00000000_00000000_00000000_00000000UL,

            Test_1_2 = 0b0000000000000010_00000010_00000010_00000000_00000000_00000000_00000000UL,

            Test_1_3 = 0b0000000000000010_00000010_00000100_00000000_00000000_00000000_00000000UL,

            Test_2 = 0b0000000000000010_00000100_00000000_00000000_00000000_00000000_00000000UL,


            Test2_Main = 0b0000000000000100_00000000_00000000_00000000_00000000_00000000_00000000UL,

            Test2_1 = 0b0000000000000100_00000011_00000000_00000000_00000000_00000000_00000000UL,

            Test2_1_1 = 0b0000000000000100_00000011_00000010_00000000_00000000_00000000_00000000UL,

            Test2_1_2 = 0b0000000000000100_00000011_00000100_00000000_00000000_00000000_00000000UL,

            Test2_2 = 0b0000000000000100_00000100_00000000_00000000_00000000_00000000_00000000UL,

            Test2_3 = 0b0000000000000100_00000110_00000000_00000000_00000000_00000000_00000000UL,

            Test2_3_1 = 0b0000000000000100_00000110_00000010_00000000_00000000_00000000_00000000UL
        }

        /// <summary>仅占位根（1&lt;&lt;48），无子节点，供动态 <c>AddDependency</c> 之外的独立 Mgr 挂载，避免与动态根冲突。</summary>
        private enum DynamicTreeMountEnum : ulong
        {
            MountOnly = 1UL << 48
        }

        #endregion
    }
}
