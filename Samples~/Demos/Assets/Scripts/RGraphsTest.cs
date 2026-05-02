using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UniVue.Common;
using UniVue.Event;
using UniVue.Model;
using UniVue.UI;
using Debug = UnityEngine.Debug;

/// <summary>
/// RGraphs 功能与性能压测。覆盖公共 API：<see cref="RGraphs.ClearAll" />、<c>Clear(BaseModel)</c>、
/// <c>Clear(ref RGraph, BaseModel)</c>、<c>Clear(..., params string[])</c>、<c>Clear(..., Params&lt;string&gt;)</c>、
/// 全局/按图 <c>Clear(EventKey)</c>、<c>Rebind</c> 两载；<c>AddNode(..., Params&lt;string&gt;)</c>、
/// <see cref="RGraphs.TrySetDirty" />/<see cref="RGraphs.GetEnable" />；Rebind 提前返回与全局并入/重复边消解。
/// </summary>
public sealed class RGraphsTest : MonoBehaviour
{
    [Header("性能压测参数")] [SerializeField] private int stressGraphCount = 200;
    [SerializeField] private int stressPropertyIterations = 5000;
    [SerializeField] private int stressAddRemoveIterations = 8000;
    private int _failed;
    private int _passed;

    private void Start()
    {
        StartCoroutine(RunAllTests());
    }

    /// <summary>
    /// EventMgr 在无任何该 Key 监听时不会触发全局 OnEvent；RGraphs 依赖 OnEvent 入队，故测试里派发前需挂占位监听。
    /// </summary>
    private static void EventDispatchNoopStub() { }

    [ContextMenu("运行 RGraphs 全部测试")]
    private void MenuRunAllTests()
    {
        StartCoroutine(RunAllTests());
    }

    private IEnumerator RunAllTests()
    {
        _passed = 0;
        _failed = 0;

        Debug.Log("========== RGraphs 测试开始 ==========");

        yield return SafeRun(RunFunctionalTests());
        yield return SafeRun(RunStressTest());

        Debug.Log($"========== RGraphs 测试完成: {_passed} 通过, {_failed} 失败 ==========");
        UnityEngine.Assertions.Assert.IsTrue(_failed == 0, $"RGraphs 测试失败: {_failed} 个断言未通过");
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
                Debug.LogError($"  [EXCEPTION] {e.Message}\n{e.StackTrace}");
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

    private static IEnumerator WaitRenderPump(int frames = 4)
    {
        for (int i = 0; i < frames; i++)
            yield return null;
    }

    private IEnumerator RunFunctionalTests()
    {
        RGraphs graphs = new() { RenderInternal = 0f };
        try
        {
            yield return Functional_ModelBind_RenderOnProperty(graphs);
            yield return Functional_PropertyFilter_OnlyListedProperty(graphs);
            yield return Functional_AddNode_ParamsString_MultiProperty(graphs);
            yield return Functional_TwoBindings_SameModel_BothDispatch(graphs);
            yield return Functional_EventDispatch_TriggersRender(graphs);

            // —— 交叉 / 汇聚（DAG 多入边）——
            yield return Cross_TwoModels_PropertyPaths_ConvergeOneDelegate(graphs);
            yield return Cross_SameFrame_TwoModels_BothEnqueued(graphs);
            yield return Cross_TwoEvents_OneRenderDelegate(graphs);
            yield return Cross_ModelProperty_And_Event_SameRenderDelegate(graphs);
            yield return Cross_TwoRGraphs_SameModel_BothRefresh(graphs);
            yield return Cross_ClearOneModel_OtherModelUnaffected(graphs);
            yield return Cross_TwoPropertyFilters_SameModel_DistinctRenderNodes(graphs);

            // —— 多属性在同一张 RGraph 上的交叉（分枝、重叠、与全量并存）——
            yield return Cross_MultiProp_OneAddNode_MultiPropertyParams(graphs);
            yield return Cross_FullModelBind_Plus_PropertyFilter_SameModel(graphs);
            yield return Cross_OverlappingPropertyEdges_TwoDelegates(graphs);
            yield return Cross_ThreeModels_DistinctProperties_ConvergeOneDelegate(graphs);
            yield return Cross_SameDelegate_SeparatePropertyAddNodes(graphs);
            yield return Cross_Event_Plus_TwoProperties_SameDelegate(graphs);

            // —— RGraphs 公共 API：按属性清理、Params 重载、事件清理、Rebind ——
            yield return Api_Clear_BaseModel_AllGraphs(graphs);
            yield return Api_Clear_RefGraph_Model_SpecificPropertyParams(graphs);
            yield return Api_Clear_RefGraph_Model_ParamsStringStruct(graphs);
            yield return Api_Clear_RefGraph_Model_EmptyParamsArray_FullUnbind(graphs);
            yield return Api_Clear_Global_EventKey(graphs);
            yield return Api_Clear_RefGraph_SingleEventKey(graphs);
            yield return Api_Rebind_ReplaceModel_Global(graphs);
            yield return Api_Rebind_ReplaceModel_SingleGraph(graphs);
            yield return Api_Rebind_SingleGraph_NewModelAlreadyOnGraph_NoOp(graphs);
            yield return Api_Rebind_Global_MergeIntoExistingNewModel(graphs);
            yield return Api_Rebind_Global_DisposeDuplicateWhenSameGraphHasBothModels(graphs);

            yield return Functional_Clear_UnsubscribesModel(graphs);
            yield return Functional_Disable_DirtyThenRenderIfDirty(graphs);
            yield return Api_TrySetDirty_GetEnable_Lifecycle(graphs);
            yield return Functional_ClearAll_ResetsEntry(graphs);

            Debug.Log("[RGraphsTest] 功能测试子流程全部跑完。");
        }
        finally
        {
            graphs.Dispose();
        }
    }

    private IEnumerator Functional_ModelBind_RenderOnProperty(RGraphs graphs)
    {
        Debug.Log("[RGraphsTest] Functional_ModelBind_RenderOnProperty");

        TestCountModel model = new();
        RGraph rg = default;
        int renders = 0;
        graphs.AddNode(ref rg, model, () => renders++);

        model.Counter = 1;
        yield return WaitRenderPump();
        Assert(renders == 1, "模型绑定后首次属性变更应触发一次渲染");

        graphs.Remove(ref rg);
        model.Counter = 2;
        yield return WaitRenderPump();
        Assert(renders == 1, "Remove 后不应再响应属性变更");
    }

    private IEnumerator Functional_PropertyFilter_OnlyListedProperty(RGraphs graphs)
    {
        Debug.Log("[RGraphsTest] Functional_PropertyFilter_OnlyListedProperty");

        TestCountModel model = new();
        RGraph rg = default;
        int renders = 0;
        graphs.AddNode(ref rg, model, () => renders++, nameof(TestCountModel.A));

        model.B = 1;
        yield return WaitRenderPump();
        Assert(renders == 0, "未监听的属性 B 不应触发渲染");

        model.A = 1;
        yield return WaitRenderPump();
        Assert(renders == 1, "监听的属性 A 应触发渲染");

        graphs.Remove(ref rg);
    }

    /// <summary>
    /// <see cref="RGraphs.AddNode(ref RGraph, BaseModel, Action, in Params{string})" />：与多字符串 params 语义一致，属性过滤入边正确。
    /// </summary>
    private IEnumerator Functional_AddNode_ParamsString_MultiProperty(RGraphs graphs)
    {
        Debug.Log("[RGraphsTest] Functional_AddNode_ParamsString_MultiProperty");

        CrossMultiModel m = new();
        RGraph rg = default;
        int r = 0;
        Params<string> p1p2 = Params<string>._(nameof(CrossMultiModel.P1), nameof(CrossMultiModel.P2));
        graphs.AddNode(ref rg, m, () => r++, p1p2);

        m.P4 = 1;
        yield return WaitRenderPump();
        Assert(r == 0, "Params AddNode: P4 未列出不应触发");

        m.P1 = 1;
        yield return WaitRenderPump();
        Assert(r == 1, "Params AddNode: P1");

        m.P2 = 2;
        yield return WaitRenderPump();
        Assert(r == 2, "Params AddNode: P2");

        graphs.Remove(ref rg);
    }

    private IEnumerator Functional_TwoBindings_SameModel_BothDispatch(RGraphs graphs)
    {
        Debug.Log("[RGraphsTest] Functional_TwoBindings_SameModel_BothDispatch");

        TestCountModel model = new();
        RGraph rg = default;
        int a = 0, b = 0;
        graphs.AddNode(ref rg, model, () => a++);
        graphs.AddNode(ref rg, model, () => b++);

        model.Counter = 1;
        yield return WaitRenderPump();
        Assert(a == 1 && b == 1, "同一 Model 上两个绑定应各执行一次");

        graphs.Remove(ref rg);
    }

    private IEnumerator Functional_EventDispatch_TriggersRender(RGraphs graphs)
    {
        Debug.Log("[RGraphsTest] Functional_EventDispatch_TriggersRender");

        EventKey key = new("RGraphsTest.Event.A");
        RGraph rg = default;
        int renders = 0;

        EventMgr.On(key, EventDispatchNoopStub);
        try
        {
            graphs.AddNode(ref rg, key, () => renders++);

            EventMgr.Dispatch(key);
            yield return WaitRenderPump();
            Assert(renders == 1, "事件派发应入队并执行渲染");

            graphs.Remove(ref rg);
            EventMgr.Dispatch(key);
            yield return WaitRenderPump();
            Assert(renders == 1, "Remove 后 RGraph 已不再绑定该事件，渲染次数不变");
        }
        finally
        {
            EventMgr.Off(key, EventDispatchNoopStub);
        }
    }

    /// <summary>
    /// Model A 的 P1 与 Model B 的 P2 通过各自属性边汇聚到同一渲染委托 R1。
    /// </summary>
    private IEnumerator Cross_TwoModels_PropertyPaths_ConvergeOneDelegate(RGraphs graphs)
    {
        Debug.Log("[RGraphsTest] Cross_TwoModels_PropertyPaths_ConvergeOneDelegate");

        CrossPairModel ma = new();
        CrossPairModel mb = new();
        RGraph rg = default;
        int r1 = 0;
        Action renderR1 = () => r1++;

        graphs.AddNode(ref rg, ma, renderR1, nameof(CrossPairModel.P1));
        graphs.AddNode(ref rg, mb, renderR1, nameof(CrossPairModel.P2));

        ma.P1 = 1;
        yield return WaitRenderPump();
        Assert(r1 == 1, "仅 A.P1 变更应触发 R1 一次");

        mb.P2 = 1;
        yield return WaitRenderPump();
        Assert(r1 == 2, "B.P2 变更应再触发 R1 一次");

        graphs.Remove(ref rg);
    }

    /// <summary>
    /// 同一帧内（一次 yield 前）连续修改 A.P1 与 B.P2，队列应包含两个渲染节点，批处理中 R1 执行两次。
    /// </summary>
    private IEnumerator Cross_SameFrame_TwoModels_BothEnqueued(RGraphs graphs)
    {
        Debug.Log("[RGraphsTest] Cross_SameFrame_TwoModels_BothEnqueued");

        CrossPairModel ma = new();
        CrossPairModel mb = new();
        RGraph rg = default;
        int r1 = 0;
        Action renderR1 = () => r1++;

        graphs.AddNode(ref rg, ma, renderR1, nameof(CrossPairModel.P1));
        graphs.AddNode(ref rg, mb, renderR1, nameof(CrossPairModel.P2));

        ma.P1 = 7;
        mb.P2 = 8;
        yield return WaitRenderPump();
        Assert(r1 == 2, "同帧多源入队后 R1 应执行两次（两路 RNode 各一）");

        graphs.Remove(ref rg);
    }

    /// <summary>
    /// 两个不同 EventKey 绑定同一渲染委托，分别 Dispatch 各触发一次。
    /// </summary>
    private IEnumerator Cross_TwoEvents_OneRenderDelegate(RGraphs graphs)
    {
        Debug.Log("[RGraphsTest] Cross_TwoEvents_OneRenderDelegate");

        EventKey e1 = new("RGraphsTest.Cross.E1");
        EventKey e2 = new("RGraphsTest.Cross.E2");
        RGraph rg = default;
        int r1 = 0;
        Action renderR1 = () => r1++;

        EventMgr.On(e1, EventDispatchNoopStub);
        EventMgr.On(e2, EventDispatchNoopStub);
        try
        {
            graphs.AddNode(ref rg, e1, renderR1);
            graphs.AddNode(ref rg, e2, renderR1);

            EventMgr.Dispatch(e1);
            yield return WaitRenderPump();
            Assert(r1 == 1, "Dispatch(E1) 应触发 R1");

            EventMgr.Dispatch(e2);
            yield return WaitRenderPump();
            Assert(r1 == 2, "Dispatch(E2) 应再触发 R1");
        }
        finally
        {
            EventMgr.Off(e1, EventDispatchNoopStub);
            EventMgr.Off(e2, EventDispatchNoopStub);
        }

        graphs.Remove(ref rg);
    }

    /// <summary>
    /// 事件与模型属性混合：同一 R1 既挂 A.P1 又挂 Event，各自独立入队。
    /// </summary>
    private IEnumerator Cross_ModelProperty_And_Event_SameRenderDelegate(RGraphs graphs)
    {
        Debug.Log("[RGraphsTest] Cross_ModelProperty_And_Event_SameRenderDelegate");

        EventKey evt = new("RGraphsTest.Cross.MixEvt");
        CrossPairModel ma = new();
        RGraph rg = default;
        int r1 = 0;
        Action renderR1 = () => r1++;

        EventMgr.On(evt, EventDispatchNoopStub);
        try
        {
            graphs.AddNode(ref rg, ma, renderR1, nameof(CrossPairModel.P1));
            graphs.AddNode(ref rg, evt, renderR1);

            ma.P1 = 1;
            yield return WaitRenderPump();
            Assert(r1 == 1, "A.P1 应触发 R1");

            EventMgr.Dispatch(evt);
            yield return WaitRenderPump();
            Assert(r1 == 2, "事件应再触发 R1");
        }
        finally
        {
            EventMgr.Off(evt, EventDispatchNoopStub);
        }

        graphs.Remove(ref rg);
    }

    /// <summary>
    /// 两个 RGraph 挂同一 Model：一次属性变更应对两图各刷新一次。
    /// </summary>
    private IEnumerator Cross_TwoRGraphs_SameModel_BothRefresh(RGraphs graphs)
    {
        Debug.Log("[RGraphsTest] Cross_TwoRGraphs_SameModel_BothRefresh");

        TestCountModel model = new();
        RGraph rg1 = default;
        RGraph rg2 = default;
        int c1 = 0, c2 = 0;

        graphs.AddNode(ref rg1, model, () => c1++);
        graphs.AddNode(ref rg2, model, () => c2++);

        model.Counter = 1;
        yield return WaitRenderPump();
        Assert(c1 == 1 && c2 == 1, "同一 Model 应对两个 RGraph 各触发一次渲染");

        graphs.Remove(ref rg1);
        graphs.Remove(ref rg2);
    }

    /// <summary>
    /// 只 Clear 掉 Model A 的绑定后，B 侧属性仍应驱动 R1。
    /// </summary>
    private IEnumerator Cross_ClearOneModel_OtherModelUnaffected(RGraphs graphs)
    {
        Debug.Log("[RGraphsTest] Cross_ClearOneModel_OtherModelUnaffected");

        CrossPairModel ma = new();
        CrossPairModel mb = new();
        RGraph rg = default;
        int r1 = 0;
        Action renderR1 = () => r1++;

        graphs.AddNode(ref rg, ma, renderR1, nameof(CrossPairModel.P1));
        graphs.AddNode(ref rg, mb, renderR1, nameof(CrossPairModel.P2));

        graphs.Clear(ref rg, ma);

        ma.P1 = 100;
        yield return WaitRenderPump();
        Assert(r1 == 0, "Clear(A) 后 A.P1 不应再触发");

        mb.P2 = 1;
        yield return WaitRenderPump();
        Assert(r1 == 1, "B.P2 仍应触发 R1");

        graphs.Remove(ref rg);
    }

    /// <summary>
    /// 同一 Model 上 P1、P2 各绑定独立渲染委托——一次 Counter 变更（全模型路径）会先走属性分支再走全量 Rendering 分支，行为与框架一致此处只验证双流独立存在时的计数隔离。
    /// 改为分开测：仅用属性过滤绑定 Rp1/Rp2，分别改 P1、P2 各触发对应委托。
    /// </summary>
    private IEnumerator Cross_TwoPropertyFilters_SameModel_DistinctRenderNodes(RGraphs graphs)
    {
        Debug.Log("[RGraphsTest] Cross_TwoPropertyFilters_SameModel_DistinctRenderNodes");

        CrossPairModel m = new();
        RGraph rg = default;
        int rp1 = 0, rp2 = 0;

        graphs.AddNode(ref rg, m, () => rp1++, nameof(CrossPairModel.P1));
        graphs.AddNode(ref rg, m, () => rp2++, nameof(CrossPairModel.P2));

        m.P2 = 1;
        yield return WaitRenderPump();
        Assert(rp1 == 0 && rp2 == 1, "仅 P2 变更只应触发 P2 绑定");

        m.P1 = 1;
        yield return WaitRenderPump();
        Assert(rp1 == 1 && rp2 == 1, "再改 P1 只应追加 P1 绑定一次");

        graphs.Remove(ref rg);
    }

    /// <summary>
    /// 一次 <c>AddNode(..., "P1","P2","P3")</c>：三列属性边挂载同一渲染委托；改任一列入队一次；未列名的 P4 不应触发。
    /// </summary>
    private IEnumerator Cross_MultiProp_OneAddNode_MultiPropertyParams(RGraphs graphs)
    {
        Debug.Log("[RGraphsTest] Cross_MultiProp_OneAddNode_MultiPropertyParams");

        CrossMultiModel m = new();
        RGraph rg = default;
        int r = 0;
        graphs.AddNode(ref rg, m, () => r++,
                       nameof(CrossMultiModel.P1), nameof(CrossMultiModel.P2), nameof(CrossMultiModel.P3));

        m.P1 = 1;
        yield return WaitRenderPump();
        Assert(r == 1, "P1 在列表内应触发");

        m.P2 = 1;
        yield return WaitRenderPump();
        Assert(r == 2, "P2 在列表内应再触发");

        m.P4 = 1;
        yield return WaitRenderPump();
        Assert(r == 2, "P4 未注册仍不应触发");

        m.P3 = 1;
        yield return WaitRenderPump();
        Assert(r == 3, "P3 应触发第三次");

        graphs.Remove(ref rg);
    }

    /// <summary>
    /// 全模型绑定（mNode 下直接 Rendering）与 P1 属性分枝同时存在：<c>P1</c> 变更走属性枝 + 全量枝；其它已声明属性若无单独过滤则再走全量枝。
    /// </summary>
    private IEnumerator Cross_FullModelBind_Plus_PropertyFilter_SameModel(RGraphs graphs)
    {
        Debug.Log("[RGraphsTest] Cross_FullModelBind_Plus_PropertyFilter_SameModel");

        CrossMultiModel m = new();
        RGraph rg = default;
        int full = 0, onlyP1 = 0;
        graphs.AddNode(ref rg, m, () => full++);
        graphs.AddNode(ref rg, m, () => onlyP1++, nameof(CrossMultiModel.P1));

        m.P1 = 10;
        yield return WaitRenderPump();
        Assert(full == 1 && onlyP1 == 1, "P1 变更应同时走全模型与 P1 分枝");

        m.P3 = 20;
        yield return WaitRenderPump();
        Assert(full == 2 && onlyP1 == 1, "仅全模型监听 P3，分支 P1 不应再累加");

        graphs.Remove(ref rg);
    }

    /// <summary>
    /// <c>Rσ</c>：委托甲挂 P1+P2；委托乙挂 P2+P3——<c>P2</c> 为<strong>重叠边</strong>，改 P2 时两委托都应执行。
    /// </summary>
    private IEnumerator Cross_OverlappingPropertyEdges_TwoDelegates(RGraphs graphs)
    {
        Debug.Log("[RGraphsTest] Cross_OverlappingPropertyEdges_TwoDelegates");

        CrossMultiModel m = new();
        RGraph rg = default;
        int a = 0, b = 0;
        graphs.AddNode(ref rg, m, () => a++, nameof(CrossMultiModel.P1), nameof(CrossMultiModel.P2));
        graphs.AddNode(ref rg, m, () => b++, nameof(CrossMultiModel.P2), nameof(CrossMultiModel.P3));

        m.P2 = 1;
        yield return WaitRenderPump();
        Assert(a == 1 && b == 1, "P2 同时为两委托的属性边应交叠触发");

        m.P1 = 1;
        yield return WaitRenderPump();
        Assert(a == 2 && b == 1, "仅甲监听 P1");

        m.P3 = 2;
        yield return WaitRenderPump();
        Assert(a == 2 && b == 2, "仅乙监听 P3");

        graphs.Remove(ref rg);
    }

    /// <summary>
    /// 三实例、三属性名（各挂一列属性边）汇入同一渲染委托——与「双实例双属性」同属多源汇聚，粒度更细。
    /// </summary>
    private IEnumerator Cross_ThreeModels_DistinctProperties_ConvergeOneDelegate(RGraphs graphs)
    {
        Debug.Log("[RGraphsTest] Cross_ThreeModels_DistinctProperties_ConvergeOneDelegate");

        CrossMultiModel m1 = new();
        CrossMultiModel m2 = new();
        CrossMultiModel m3 = new();
        RGraph rg = default;
        int r = 0;
        Action all = () => r++;
        graphs.AddNode(ref rg, m1, all, nameof(CrossMultiModel.P1));
        graphs.AddNode(ref rg, m2, all, nameof(CrossMultiModel.P2));
        graphs.AddNode(ref rg, m3, all, nameof(CrossMultiModel.P3));

        m3.P3 = 1;
        yield return WaitRenderPump();
        Assert(r == 1, "m3.P3");

        m1.P1 = 1;
        yield return WaitRenderPump();
        Assert(r == 2, "m1.P1");

        m2.P2 = 2;
        yield return WaitRenderPump();
        Assert(r == 3, "m2.P2");

        graphs.Remove(ref rg);
    }

    /// <summary>
    /// 同一委托实例分两次 <c>AddNode(..., Property)</c> 挂到 P1 与 P2——两粒不同属性边结点，同帧修改应各计一次。
    /// </summary>
    private IEnumerator Cross_SameDelegate_SeparatePropertyAddNodes(RGraphs graphs)
    {
        Debug.Log("[RGraphsTest] Cross_SameDelegate_SeparatePropertyAddNodes");

        CrossMultiModel m = new();
        RGraph rg = default;
        int x = 0;
        Action one = () => x++;
        graphs.AddNode(ref rg, m, one, nameof(CrossMultiModel.P1));
        graphs.AddNode(ref rg, m, one, nameof(CrossMultiModel.P2));

        m.P2 = 1;
        m.P1 = 2;
        yield return WaitRenderPump();
        Assert(x == 2, "同帧 P1+P2，两路径各一 RNode，应执行两次（非合并为单次）");

        graphs.Remove(ref rg);
    }

    /// <summary>
    /// Event + Model 上 P1、P2（一次 params）三者绑定同一渲染委托——事件一次、两属性各一次。
    /// </summary>
    private IEnumerator Cross_Event_Plus_TwoProperties_SameDelegate(RGraphs graphs)
    {
        Debug.Log("[RGraphsTest] Cross_Event_Plus_TwoProperties_SameDelegate");

        EventKey evt = new("RGraphsTest.Cross.Evt_MultiProp");
        CrossMultiModel m = new();
        RGraph rg = default;
        int k = 0;
        Action r = () => k++;

        EventMgr.On(evt, EventDispatchNoopStub);
        try
        {
            graphs.AddNode(ref rg, m, r, nameof(CrossMultiModel.P1), nameof(CrossMultiModel.P2));
            graphs.AddNode(ref rg, evt, r);

            EventMgr.Dispatch(evt);
            yield return WaitRenderPump();
            Assert(k == 1, "事件 Dispatch 应触发一次渲染");

            k = 0;
            m.P1 = 1;
            yield return WaitRenderPump();
            Assert(k == 1, "P1");

            k = 0;
            m.P2 = 2;
            yield return WaitRenderPump();
            Assert(k == 1, "P2");
        }
        finally
        {
            EventMgr.Off(evt, EventDispatchNoopStub);
        }

        graphs.Remove(ref rg);
    }

    private IEnumerator Functional_Clear_UnsubscribesModel(RGraphs graphs)
    {
        Debug.Log("[RGraphsTest] Functional_Clear_UnsubscribesModel");

        TestCountModel model = new();
        RGraph rg = default;
        int renders = 0;
        graphs.AddNode(ref rg, model, () => renders++);

        graphs.Clear(ref rg, model);
        model.Counter = 3;
        yield return WaitRenderPump();
        Assert(renders == 0, "Clear(ref graph, model) 后属性变更不应触发渲染");
    }

    private IEnumerator Functional_Disable_DirtyThenRenderIfDirty(RGraphs graphs)
    {
        Debug.Log("[RGraphsTest] Functional_Disable_DirtyThenRenderIfDirty");

        TestCountModel model = new();
        RGraph rg = default;
        int renders = 0;
        graphs.AddNode(ref rg, model, () => renders++);

        graphs.SetEnable(rg, false);
        model.Counter = 7;
        yield return WaitRenderPump();
        Assert(renders == 0, "禁用时属性变更不应经队列执行渲染委托");
        Assert(graphs.DisableGraphCount > 0, "禁用图应记入 DisableGraphCount");

        graphs.RenderIfDirtyOrForce(rg, false, true);
        Assert(renders == 1, "RenderIfDirtyOrForce 应在有脏标记时同步刷新");

        graphs.SetEnable(rg, true);
        model.Counter = 8;
        yield return WaitRenderPump();
        Assert(renders == 2, "重新启用后属性变更应再次经队列渲染");

        graphs.Remove(ref rg);
    }

    /// <summary>
    /// <see cref="RGraphs.TrySetDirty" /> / <see cref="RGraphs.GetEnable" />：仅在禁用时记脏；启用后 TrySetDirty 无效。
    /// </summary>
    private IEnumerator Api_TrySetDirty_GetEnable_Lifecycle(RGraphs graphs)
    {
        Debug.Log("[RGraphsTest] Api_TrySetDirty_GetEnable_Lifecycle");

        TestCountModel model = new();
        RGraph rg = default;
        int renders = 0;
        graphs.AddNode(ref rg, model, () => renders++);

        Assert(graphs.GetEnable(rg), "初始应为启用");

        graphs.SetEnable(rg, false);
        Assert(!graphs.GetEnable(rg), "SetEnable(false) 后应为禁用");

        Assert(graphs.TrySetDirty(rg), "禁用状态下 TrySetDirty 应返回 true");

        graphs.RenderIfDirtyOrForce(rg, false, true);
        Assert(renders == 1, "脏图应同步渲染一次");
        Assert(graphs.GetEnable(rg), "RenderIfDirtyOrForce(..., enable:true) 后应恢复启用");

        Assert(!graphs.TrySetDirty(rg), "启用状态下 TrySetDirty 应返回 false");

        model.Counter = 3;
        yield return WaitRenderPump();
        Assert(renders == 2, "重新启用后属性变更应再走异步队列");

        graphs.Remove(ref rg);
    }

    private IEnumerator Functional_ClearAll_ResetsEntry(RGraphs graphs)
    {
        Debug.Log("[RGraphsTest] Functional_ClearAll_ResetsEntry");

        TestCountModel model = new();
        RGraph rg = default;
        graphs.AddNode(ref rg, model, () => { });

        graphs.ClearAll();
        Assert(graphs.WaitExecuteRenderingCount == 0, "ClearAll 后渲染队列应为空");

        yield break;
    }

    /// <summary>
    /// <see cref="RGraphs.Clear(BaseModel)" />：从全局入口移除指定 Model 对所有 RGraph 的挂载。
    /// </summary>
    private IEnumerator Api_Clear_BaseModel_AllGraphs(RGraphs graphs)
    {
        Debug.Log("[RGraphsTest] Api_Clear_BaseModel_AllGraphs");

        TestCountModel m = new();
        RGraph rg1 = default;
        RGraph rg2 = default;
        int n1 = 0, n2 = 0;
        graphs.AddNode(ref rg1, m, () => n1++);
        graphs.AddNode(ref rg2, m, () => n2++);

        graphs.Clear(m);

        m.Counter = 99;
        yield return WaitRenderPump();
        Assert(n1 == 0 && n2 == 0, "Clear(BaseModel) 后两图均不应再收到该模型的通知驱动");
    }

    /// <summary>
    /// <see cref="RGraphs.Clear(ref RGraph, BaseModel, string[])" />：只卸 P1、P2 属性枝，P3 仍在。
    /// </summary>
    private IEnumerator Api_Clear_RefGraph_Model_SpecificPropertyParams(RGraphs graphs)
    {
        Debug.Log("[RGraphsTest] Api_Clear_RefGraph_Model_SpecificPropertyParams");

        CrossMultiModel m = new();
        RGraph rg = default;
        int c1 = 0, c2 = 0, c3 = 0;
        graphs.AddNode(ref rg, m, () => c1++, nameof(CrossMultiModel.P1));
        graphs.AddNode(ref rg, m, () => c2++, nameof(CrossMultiModel.P2));
        graphs.AddNode(ref rg, m, () => c3++, nameof(CrossMultiModel.P3));

        graphs.Clear(ref rg, m, nameof(CrossMultiModel.P1), nameof(CrossMultiModel.P2));

        m.P1 = 1;
        m.P2 = 2;
        yield return WaitRenderPump();
        Assert(c1 == 0 && c2 == 0, "已 Clear 的 P1/P2 不应再触发");

        m.P3 = 3;
        yield return WaitRenderPump();
        Assert(c3 == 1, "未 Clear 的 P3 绑定仍应触发");

        graphs.Remove(ref rg);
    }

    /// <summary>
    /// <see cref="RGraphs.Clear(ref RGraph, BaseModel, Params{string})" /> 与 params 数组语义对齐。
    /// </summary>
    private IEnumerator Api_Clear_RefGraph_Model_ParamsStringStruct(RGraphs graphs)
    {
        Debug.Log("[RGraphsTest] Api_Clear_RefGraph_Model_ParamsStringStruct");

        CrossMultiModel m = new();
        RGraph rg = default;
        int c1 = 0, c2 = 0;
        graphs.AddNode(ref rg, m, () => c1++, nameof(CrossMultiModel.P1));
        graphs.AddNode(ref rg, m, () => c2++, nameof(CrossMultiModel.P2));

        Params<string> onlyP1 = Params<string>._(nameof(CrossMultiModel.P1));
        graphs.Clear(ref rg, m, onlyP1);

        m.P1 = 1;
        yield return WaitRenderPump();
        Assert(c1 == 0, "Params: P1 应已卸下");

        m.P2 = 1;
        yield return WaitRenderPump();
        Assert(c2 == 1, "Params: P2 仍绑定");

        graphs.Remove(ref rg);
    }

    /// <summary>
    /// propertyNames 为空数组时走「整图对该 Model 卸载」分支，与同参 <see cref="RGraphs.Clear(ref RGraph, BaseModel)" /> 一致。
    /// </summary>
    private IEnumerator Api_Clear_RefGraph_Model_EmptyParamsArray_FullUnbind(RGraphs graphs)
    {
        Debug.Log("[RGraphsTest] Api_Clear_RefGraph_Model_EmptyParamsArray_FullUnbind");

        CrossMultiModel m = new();
        RGraph rg = default;
        int c = 0;
        graphs.AddNode(ref rg, m, () => c++, nameof(CrossMultiModel.P1));

        graphs.Clear(ref rg, m, Array.Empty<string>());

        m.P1 = 1;
        yield return WaitRenderPump();
        Assert(c == 0, "空 params 等价于整条 Model 边上该图卸载");

        graphs.Remove(ref rg);
    }

    /// <summary>
    /// <see cref="RGraphs.Clear(EventKey)" />：从全局入口摘掉该 EventKey（多图挂载同一 Key 一并清）。
    /// </summary>
    private IEnumerator Api_Clear_Global_EventKey(RGraphs graphs)
    {
        Debug.Log("[RGraphsTest] Api_Clear_Global_EventKey");

        EventKey ek = new("RGraphsTest.Api_ClearGlobalEvt");
        RGraph rg1 = default;
        RGraph rg2 = default;
        int a = 0, b = 0;

        EventMgr.On(ek, EventDispatchNoopStub);
        try
        {
            graphs.AddNode(ref rg1, ek, () => a++);
            graphs.AddNode(ref rg2, ek, () => b++);

            graphs.Clear(ek);

            EventMgr.Dispatch(ek);
            yield return WaitRenderPump();
            Assert(a == 0 && b == 0, "全局 Clear(EventKey) 后不应再有该 Key 绑定的渲染");
        }
        finally
        {
            EventMgr.Off(ek, EventDispatchNoopStub);
        }
    }

    /// <summary>
    /// <see cref="RGraphs.Clear(ref RGraph, EventKey)" />：只摘掉当前图上某一 EventKey。
    /// </summary>
    private IEnumerator Api_Clear_RefGraph_SingleEventKey(RGraphs graphs)
    {
        Debug.Log("[RGraphsTest] Api_Clear_RefGraph_SingleEventKey");

        EventKey e1 = new("RGraphsTest.Api_E1");
        EventKey e2 = new("RGraphsTest.Api_E2");
        RGraph rg = default;
        int x1 = 0, x2 = 0;

        EventMgr.On(e1, EventDispatchNoopStub);
        EventMgr.On(e2, EventDispatchNoopStub);
        try
        {
            graphs.AddNode(ref rg, e1, () => x1++);
            graphs.AddNode(ref rg, e2, () => x2++);

            graphs.Clear(ref rg, e1);

            EventMgr.Dispatch(e1);
            yield return WaitRenderPump();
            Assert(x1 == 0, "应从本图摘掉 E1");

            EventMgr.Dispatch(e2);
            yield return WaitRenderPump();
            Assert(x2 == 1, "E2 仍在本图");

            graphs.Remove(ref rg);
        }
        finally
        {
            EventMgr.Off(e1, EventDispatchNoopStub);
            EventMgr.Off(e2, EventDispatchNoopStub);
        }
    }

    /// <summary>
    /// <see cref="RGraphs.Rebind{T}(T, T, bool)" />：跨 Entry 把 oldModel 所有图边迁到 newModel。
    /// </summary>
    private IEnumerator Api_Rebind_ReplaceModel_Global(RGraphs graphs)
    {
        Debug.Log("[RGraphsTest] Api_Rebind_ReplaceModel_Global");

        TestCountModel oldM = new();
        TestCountModel newM = new();
        RGraph rg = default;
        int n = 0;
        graphs.AddNode(ref rg, oldM, () => n++);

        graphs.Rebind(oldM, newM);
        yield return WaitRenderPump();
        int afterRebind = n;

        oldM.Counter = 11;
        yield return WaitRenderPump();
        Assert(n == afterRebind, "全局 Rebind 后 oldModel 不应再驱动");

        newM.Counter = 1;
        yield return WaitRenderPump();
        Assert(n > afterRebind, "newModel 应接手绑定");

        graphs.Remove(ref rg);
    }

    /// <summary>
    /// <see cref="RGraphs.Rebind{T}(in RGraph, T, T, bool)" />：仅切一条图上的 Model 引用。
    /// </summary>
    private IEnumerator Api_Rebind_ReplaceModel_SingleGraph(RGraphs graphs)
    {
        Debug.Log("[RGraphsTest] Api_Rebind_ReplaceModel_SingleGraph");

        TestCountModel oldM = new();
        TestCountModel newM = new();
        RGraph rg = default;
        int v = 0;
        graphs.AddNode(ref rg, oldM, () => v++);

        graphs.Rebind(rg, oldM, newM);
        yield return WaitRenderPump();
        int afterRebind = v;

        oldM.Counter = 22;
        yield return WaitRenderPump();
        Assert(v == afterRebind, "单图 Rebind 后 old 不应触发");

        newM.Counter = 1;
        yield return WaitRenderPump();
        Assert(v > afterRebind, "单图 Rebind 后 new 应触发");

        graphs.Remove(ref rg);
    }

    /// <summary>
    /// <see cref="RGraphs.Rebind{T}(in RGraph, T, T, bool)" />：目标图上已存在 <paramref name="newModel" /> 时直接返回，不改变现有绑定。
    /// </summary>
    private IEnumerator Api_Rebind_SingleGraph_NewModelAlreadyOnGraph_NoOp(RGraphs graphs)
    {
        Debug.Log("[RGraphsTest] Api_Rebind_SingleGraph_NewModelAlreadyOnGraph_NoOp");

        TestCountModel oldM = new();
        TestCountModel newM = new();
        RGraph rg = default;
        int a = 0, b = 0;
        graphs.AddNode(ref rg, oldM, () => a++);
        graphs.AddNode(ref rg, newM, () => b++);

        graphs.Rebind(rg, oldM, newM, false);

        oldM.Counter = 1;
        yield return WaitRenderPump();
        Assert(a == 1 && b == 0, "提前返回时 old 侧绑定仍生效");

        newM.Counter = 1;
        yield return WaitRenderPump();
        Assert(b == 1, "new 侧绑定仍独立");

        graphs.Remove(ref rg);
    }

    /// <summary>
    /// 全局 <see cref="RGraphs.Rebind{T}(T, T, bool)" />：目标 Model 已在入口注册（另一张图）时，将 old 的图边并入 existing new。
    /// </summary>
    private IEnumerator Api_Rebind_Global_MergeIntoExistingNewModel(RGraphs graphs)
    {
        Debug.Log("[RGraphsTest] Api_Rebind_Global_MergeIntoExistingNewModel");

        TestCountModel oldM = new();
        TestCountModel newM = new();
        RGraph rg1 = default;
        RGraph rg2 = default;
        int c1 = 0, c2 = 0;
        graphs.AddNode(ref rg1, oldM, () => c1++);
        graphs.AddNode(ref rg2, newM, () => c2++);

        graphs.Rebind(oldM, newM, false);

        oldM.Counter = 77;
        yield return WaitRenderPump();
        Assert(c1 == 0 && c2 == 0, "全局 Rebind 后 oldModel 不应再驱动");

        newM.Counter = 1;
        yield return WaitRenderPump();
        Assert(c1 == 1 && c2 == 1, "newModel 属性变更应对并入的两图各渲染一次");

        graphs.Remove(ref rg1);
        graphs.Remove(ref rg2);
    }

    /// <summary>
    /// 全局 Rebind：同一 <see cref="RGraph" /> 上同时已有 old、new 两条模型边时走 SafeDispose 丢弃重复枝。
    /// </summary>
    private IEnumerator Api_Rebind_Global_DisposeDuplicateWhenSameGraphHasBothModels(RGraphs graphs)
    {
        Debug.Log("[RGraphsTest] Api_Rebind_Global_DisposeDuplicateWhenSameGraphHasBothModels");

        TestCountModel oldM = new();
        TestCountModel newM = new();
        RGraph rg = default;
        int a = 0, b = 0;
        graphs.AddNode(ref rg, oldM, () => a++);
        graphs.AddNode(ref rg, newM, () => b++);

        graphs.Rebind(oldM, newM, false);

        newM.Counter = 1;
        yield return WaitRenderPump();
        Assert(a == 0 && b == 1, "重复边上仅保留已有 new 委托");

        oldM.Counter = 99;
        yield return WaitRenderPump();
        Assert(a == 0 && b == 1, "oldModel 已从全局入口卸下");

        graphs.Remove(ref rg);
    }

    private IEnumerator RunStressTest()
    {
        Debug.Log(
                  $"[RGraphsTest][Stress] 开始 graphs={stressGraphCount}, propIter={stressPropertyIterations}, addRemove={stressAddRemoveIterations}");

        yield return Stress_AddRemoveLoop_Verified(stressAddRemoveIterations);
        yield return StressManyGraphsPropertyBurst(stressGraphCount, stressPropertyIterations);
        yield return StressCross_ManyModelsOneGraphStress();

        Debug.Log("[RGraphsTest][Stress] 结束（耗时见上文）");
    }

    /// <summary>
    /// AddNode/Remove 压测：轮询多实例 Model，避免单 Model 一条边掩盖引用问题；循环末断言
    /// <see cref="RGraphs.DisableGraphCount" />、<see cref="RGraphs.WaitExecuteRenderingCount" />，再
    /// <see cref="RGraphs.ClearAll" /> 二次校验。
    /// </summary>
    private IEnumerator Stress_AddRemoveLoop_Verified(int iterations)
    {
        const int poolSize = 32;
        RGraphs graphs = new() { RenderInternal = 0f };
        TestCountModel[] pool = new TestCountModel[poolSize];
        for (int p = 0; p < poolSize; p++)
            pool[p] = new TestCountModel();

        try
        {
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                RGraph rg = default;
                graphs.AddNode(ref rg, pool[i % poolSize], () => { });
                graphs.Remove(ref rg);
                if ((i & 0x3FF) == 0 && i != 0)
                    yield return null;
            }

            sw.Stop();

            Assert(graphs.DisableGraphCount == 0, "Stress AddRemove: 禁用图计数应为 0");
            Assert(graphs.WaitExecuteRenderingCount == 0, "Stress AddRemove: 待渲染队列应为空");

            graphs.ClearAll();
            Assert(graphs.WaitExecuteRenderingCount == 0, "Stress AddRemove: ClearAll 后待渲染队列应为空");

            Debug.Log(
                      $"[RGraphsTest][Stress] AddNode+Remove x{iterations}, pool={poolSize}: {sw.ElapsedMilliseconds} ms ({sw.Elapsed.TotalMilliseconds / iterations:F4} ms/次)");
        }
        finally
        {
            graphs.Dispose();
        }
    }

    private IEnumerator StressManyGraphsPropertyBurst(int graphCount, int propertyIterations)
    {
        RGraphs graphs = new() { RenderInternal = 0f };
        List<TestCountModel> models = new(graphCount);
        try
        {
            for (int i = 0; i < graphCount; i++)
            {
                models.Add(new TestCountModel());
                RGraph rg = default;
                TestCountModel m = models[i];
                graphs.AddNode(ref rg, m, () => { });
            }

            Stopwatch sw = Stopwatch.StartNew();
            for (int k = 0; k < propertyIterations; k++)
            {
                for (int i = 0; i < graphCount; i++)
                    models[i].Counter = k + i;
            }

            sw.Stop();
            Debug.Log(
                      $"[RGraphsTest][Stress] 属性赋值 {propertyIterations}x{graphCount} 次（仅同步路径）: {sw.ElapsedMilliseconds} ms");

            yield return WaitRenderPump(8);
            Debug.Log($"[RGraphsTest][Stress] WaitExecuteRenderingCount={graphs.WaitExecuteRenderingCount}");
        }
        finally
        {
            graphs.Dispose();
        }
    }

    /// <summary>
    /// 大量 Model 各占一条独立的绑定边（各槽独立计数）。不因「取样瞬间」误判：
    /// 脚本协程、<see cref="CoroutineMgr" /> 与队列批处理<strong>不具有与 MonoBehaviour.Update 对齐的瞬时序</strong>，
    /// 下一轮赋值前只靠固定帧数有时会剩一批未排空。结束主循环后对<strong>再等若干帧直至各槽计数达到 rounds 或超时</strong>，
    /// 再给结论；断言针对稳定态，不苛求与某一帧边沿对齐。
    /// </summary>
    private IEnumerator StressCross_ManyModelsOneGraphStress()
    {
        const int n = 64;
        const int rounds = 50;
        const int waitAfterEachRoundFrames = 8;
        const int stabilizeDeadlineFrames = 240;

        RGraphs graphs = new() { RenderInternal = 0f };
        CrossPairModel[] arr = new CrossPairModel[n];
        RGraph rg = default;
        int[] renderCountBySlot = new int[n];

        for (int i = 0; i < n; i++)
            arr[i] = new CrossPairModel();

        for (int i = 0; i < n; i++)
        {
            int slot = i;
            graphs.AddNode(ref rg, arr[i], () => renderCountBySlot[slot]++, nameof(CrossPairModel.P1));
        }

        try
        {
            Stopwatch swFull = Stopwatch.StartNew();
            long syncAssignTicks = 0;
            for (int round = 0; round < rounds; round++)
            {
                Stopwatch swRound = Stopwatch.StartNew();
                for (int i = 0; i < n; i++)
                {
                    // 不可使用 round==0,i==0 时仍为 0 的公式——与字段初值相等会跳过 CheckPropertyChanged，槽位 0 会一直少一觉。
                    arr[i].P1 = round * 100 + i + 1;
                }

                swRound.Stop();
                syncAssignTicks += swRound.ElapsedTicks;

                yield return WaitRenderPump(waitAfterEachRoundFrames);
            }

            int stabilizeFrames = 0;
            while (stabilizeFrames < stabilizeDeadlineFrames &&
                   !StressCross_AllSlotsAtTarget(renderCountBySlot, n, rounds))
            {
                yield return null;
                stabilizeFrames++;
            }

            swFull.Stop();
            double syncMs = syncAssignTicks * (1000.0 / Stopwatch.Frequency);

            int mn = renderCountBySlot[0];
            int mx = renderCountBySlot[0];
            for (int j = 1; j < n; j++)
            {
                if (renderCountBySlot[j] < mn) mn = renderCountBySlot[j];
                if (renderCountBySlot[j] > mx) mx = renderCountBySlot[j];
            }

            Debug.Log(
                      $"[RGraphsTest][Stress][Cross] {n} 槽 × {rounds} 轮：稳定化额外帧 {stabilizeFrames}，min={mn} max={mx}，赋值同步≈ {syncMs:F2} ms，墙钟 {swFull.ElapsedMilliseconds} ms");

            if (mn != rounds || mx != rounds)
            {
                Debug.LogWarning(
                                 $"[RGraphsTest][Stress][Cross] 首槽不匹配索引={StressCross_FirstIndexNotEqual(renderCountBySlot, n, rounds)}, 期望值 {rounds}");
            }

            Assert(mn == mx && mn == rounds,
                   $"稳定化 {stabilizeFrames} 帧后各槽应为 {rounds} 次；min={mn} max={mx}。首槽未配对时先检查赋值是否曾等于属性初值而未产生通知");
        }
        finally
        {
            graphs.Dispose();
        }
    }

    private static bool StressCross_AllSlotsAtTarget(IReadOnlyList<int> slots, int n, int target)
    {
        for (int j = 0; j < n; j++)
        {
            if (slots[j] != target)
                return false;
        }

        return true;
    }

    /// <returns>首个 <paramref name="target" /> 不等的位置，否则 -1。</returns>
    private static int StressCross_FirstIndexNotEqual(IReadOnlyList<int> slots, int n, int target)
    {
        for (int j = 0; j < n; j++)
        {
            if (slots[j] != target)
                return j;
        }

        return -1;
    }

    /// <summary>P1–P4 四列独立通知属性，用于多属性名交叉绑定的 DAG 场景。</summary>
    private sealed class CrossMultiModel : BaseModel
    {
        private int _p1;
        private int _p2;
        private int _p3;
        private int _p4;

        public int P1
        {
            get => _p1;
            set
            {
                if (_p1 == value) return;
                int o = _p1;
                _p1 = value;
                CheckPropertyChanged(nameof(P1), o, value);
            }
        }

        public int P2
        {
            get => _p2;
            set
            {
                if (_p2 == value) return;
                int o = _p2;
                _p2 = value;
                CheckPropertyChanged(nameof(P2), o, value);
            }
        }

        public int P3
        {
            get => _p3;
            set
            {
                if (_p3 == value) return;
                int o = _p3;
                _p3 = value;
                CheckPropertyChanged(nameof(P3), o, value);
            }
        }

        public int P4
        {
            get => _p4;
            set
            {
                if (_p4 == value) return;
                int o = _p4;
                _p4 = value;
                CheckPropertyChanged(nameof(P4), o, value);
            }
        }
    }

    private sealed class CrossPairModel : BaseModel
    {
        private int _p1;
        private int _p2;

        public int P1
        {
            get => _p1;
            set
            {
                if (_p1 == value) return;
                int o = _p1;
                _p1 = value;
                CheckPropertyChanged(nameof(P1), o, value);
            }
        }

        public int P2
        {
            get => _p2;
            set
            {
                if (_p2 == value) return;
                int o = _p2;
                _p2 = value;
                CheckPropertyChanged(nameof(P2), o, value);
            }
        }
    }

    private sealed class TestCountModel : BaseModel
    {
        private int _a;
        private int _b;
        private int _counter;

        public int Counter
        {
            get => _counter;
            set
            {
                if (_counter == value) return;
                int oldValue = _counter;
                _counter = value;
                CheckPropertyChanged(nameof(Counter), oldValue, value);
            }
        }

        public int A
        {
            get => _a;
            set
            {
                if (_a == value) return;
                int oldValue = _a;
                _a = value;
                CheckPropertyChanged(nameof(A), oldValue, value);
            }
        }

        public int B
        {
            get => _b;
            set
            {
                if (_b == value) return;
                int oldValue = _b;
                _b = value;
                CheckPropertyChanged(nameof(B), oldValue, value);
            }
        }
    }
}