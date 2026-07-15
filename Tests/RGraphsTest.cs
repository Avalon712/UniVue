using System;
using NUnit.Framework;
using UniVue.Common;
using UniVue.Event;
using UniVue.Model;
using UniVue.UI;

namespace UniVue.Test
{
    /// <summary>
    /// 针对 <see cref="RGraphs"/> 渲染依赖图的严格单元测试，覆盖：
    /// 构造/销毁、节点添加（模型/属性/事件）、启用禁用、脏标记、
    /// 强制渲染、属性/事件触发入队、各种 Clear、Rebind 以及边界与空参数场景。
    /// </summary>
    internal sealed class RGraphsTest
    {
        private RGraphs _rg;

        [SetUp]
        public void SetUp()
        {
            _rg = new RGraphs();
        }

        [TearDown]
        public void TearDown()
        {
            // Dispose 幂等，第二次调用会因为 Entry.g == null 直接返回
            _rg?.Dispose();
            _rg = null;
        }

        #region 构造与基本属性

        [Test]
        public void Constructor_Initializes_EntryAndDefaults()
        {
            Assert.That(_rg.Entry.g, Is.Not.Null, "构造后应创建入口节点 Entry");
            Assert.That(_rg.Entry.g.next.Count, Is.EqualTo(0), "初始入口不应有任何子节点");
            Assert.That(_rg.RenderInternal, Is.EqualTo(0.1f).Within(1e-6f), "默认渲染间隔应为 0.1 秒");
            Assert.That(_rg.WaitExecuteRenderingCount, Is.EqualTo(0));
            Assert.That(_rg.DisableGraphCount, Is.EqualTo(0));
        }

        [Test]
        public void RenderInternal_IsSettable()
        {
            _rg.RenderInternal = 0.5f;
            Assert.That(_rg.RenderInternal, Is.EqualTo(0.5f).Within(1e-6f));
        }

        #endregion

        #region AddNode - 模型（无属性）

        [Test]
        public void AddNode_Model_CreatesGraphAndBinding()
        {
            var model = new TestModel();
            int count = 0;
            Action render = () => count++;

            RGraph graph = default;
            _rg.AddNode(ref graph, model, render);

            Assert.That(graph.g, Is.Not.Null, "AddNode 应在 graph 为空时创建一个新的 RGraph");
            Assert.That(_rg.Entry.g.next.ContainsKey(model), Is.True, "入口应记录该模型");

            // 强制渲染应当调用绑定的渲染函数
            _rg.RenderIfDirtyOrForce(graph, force: true, enable: true);
            Assert.That(count, Is.EqualTo(1));
        }

        [Test]
        public void AddNode_Model_NullModel_IsNoOp()
        {
            RGraph graph = default;
            _rg.AddNode(ref graph, null, () => { });

            Assert.That(graph.g, Is.Null, "模型为空时不应创建图");
            Assert.That(_rg.Entry.g.next.Count, Is.EqualTo(0));
        }

        [Test]
        public void AddNode_Model_NullRenderFn_IsNoOp()
        {
            var model = new TestModel();
            RGraph graph = default;
            _rg.AddNode(ref graph, model, (Action)null);

            Assert.That(graph.g, Is.Null, "渲染函数为空时不应创建图");
            Assert.That(_rg.Entry.g.next.Count, Is.EqualTo(0));
        }

        [Test]
        public void AddNode_Model_AnyPropertyChange_Enqueues()
        {
            var model = new TestModel();
            RGraph graph = default;
            _rg.AddNode(ref graph, model, () => { });

            Assert.That(_rg.WaitExecuteRenderingCount, Is.EqualTo(0));

            model.Hp = 1; // 无属性绑定 -> 任意属性变化都会入队
            Assert.That(_rg.WaitExecuteRenderingCount, Is.EqualTo(1));
        }

        [Test]
        public void AddNode_Model_SameRenderFnTwice_NotDuplicated()
        {
            var model = new TestModel();
            int count = 0;
            Action render = () => count++;

            RGraph graph = default;
            _rg.AddNode(ref graph, model, render);
            _rg.AddNode(ref graph, model, render);

            _rg.RenderIfDirtyOrForce(graph, force: true, enable: true);
            Assert.That(count, Is.EqualTo(1), "同一渲染函数重复添加不应重复调用");
        }

        #endregion

        #region AddNode - 属性

        [Test]
        public void AddNode_Property_OnlyBoundPropertyEnqueues()
        {
            var model = new TestModel();
            RGraph graph = default;
            _rg.AddNode(ref graph, model, () => { }, "Hp");

            model.Mp = 5; // 未绑定属性
            Assert.That(_rg.WaitExecuteRenderingCount, Is.EqualTo(0));

            model.Hp = 5; // 绑定属性
            Assert.That(_rg.WaitExecuteRenderingCount, Is.EqualTo(1));
        }

        [Test]
        public void AddNode_Property_EmptyPropertyNames_FallsBackToModelBinding()
        {
            var model = new TestModel();
            RGraph graph = default;
            _rg.AddNode(ref graph, model, () => { }, new string[0]);

            // 退化为模型级绑定，任意属性变化都会入队
            model.Mp = 3;
            Assert.That(_rg.WaitExecuteRenderingCount, Is.EqualTo(1));
        }

        [Test]
        public void AddNode_Property_MultipleProperties()
        {
            var model = new TestModel();
            RGraph graph = default;
            _rg.AddNode(ref graph, model, () => { }, "Hp", "Mp");

            model.Hp = 1;
            Assert.That(_rg.WaitExecuteRenderingCount, Is.EqualTo(1));

            model.Mp = 1;
            Assert.That(_rg.WaitExecuteRenderingCount, Is.EqualTo(2), "两个不同属性对应两个不同渲染节点");
        }

        [Test]
        public void AddNode_Property_UsingParamsStruct()
        {
            var model = new TestModel();
            RGraph graph = default;
            _rg.AddNode(ref graph, model, () => { }, new Params<string>(1, "Hp"));

            model.Mp = 2;
            Assert.That(_rg.WaitExecuteRenderingCount, Is.EqualTo(0));

            model.Hp = 2;
            Assert.That(_rg.WaitExecuteRenderingCount, Is.EqualTo(1));
        }

        [Test]
        public void AddNode_Property_EmptyParamsStruct_FallsBackToModelBinding()
        {
            var model = new TestModel();
            RGraph graph = default;
            _rg.AddNode(ref graph, model, () => { }, new Params<string>(0, null));

            model.Hp = 9;
            Assert.That(_rg.WaitExecuteRenderingCount, Is.EqualTo(1));
        }

        #endregion

        #region AddNode - 事件

        [Test]
        public void AddNode_Event_DispatchEnqueues()
        {
            EventKey ek = "click";
            int dummyCalls = 0;
            Action listener = () => dummyCalls++;
            EventMgr.On(ek, listener);
            try
            {
                RGraph graph = default;
                _rg.AddNode(ref graph, ek, () => { });

                Assert.That(_rg.WaitExecuteRenderingCount, Is.EqualTo(0));

                EventMgr.Dispatch(ek);

                Assert.That(dummyCalls, Is.EqualTo(1), "事件监听回调应被触发");
                Assert.That(_rg.WaitExecuteRenderingCount, Is.EqualTo(1), "事件触发应使渲染节点入队");
            }
            finally
            {
                EventMgr.Off(ek, listener);
            }
        }

        [Test]
        public void AddNode_Event_NotEventKey_IsNoOp()
        {
            RGraph graph = default;
            _rg.AddNode(ref graph, default(EventKey), () => { });

            Assert.That(graph.g, Is.Null, "非法事件键不应创建图");
            Assert.That(_rg.Entry.g.next.Count, Is.EqualTo(0));
        }

        [Test]
        public void AddNode_Event_NullRenderFn_IsNoOp()
        {
            RGraph graph = default;
            EventKey ek = "e";
            _rg.AddNode(ref graph, ek, null);

            Assert.That(graph.g, Is.Null);
            Assert.That(_rg.Entry.g.next.Count, Is.EqualTo(0));
        }

        #endregion

        #region 启用/禁用 与 脏标记

        [Test]
        public void GetEnable_DefaultGraph_ReturnsFalse()
        {
            RGraph graph = default;
            Assert.That(_rg.GetEnable(graph), Is.False);
        }

        [Test]
        public void GetEnable_NewlyBoundGraph_ReturnsTrue()
        {
            RGraph graph = MakeModelGraph(out _, out _);
            Assert.That(_rg.GetEnable(graph), Is.True);
            Assert.That(_rg.DisableGraphCount, Is.EqualTo(0));
        }

        [Test]
        public void SetEnable_TogglesEnableState()
        {
            RGraph graph = MakeModelGraph(out _, out _);

            _rg.SetEnable(graph, false);
            Assert.That(_rg.GetEnable(graph), Is.False);
            Assert.That(_rg.DisableGraphCount, Is.EqualTo(1));

            _rg.SetEnable(graph, true);
            Assert.That(_rg.GetEnable(graph), Is.True);
            Assert.That(_rg.DisableGraphCount, Is.EqualTo(0));
        }

        [Test]
        public void SetEnable_DefaultGraph_IsNoOp()
        {
            RGraph graph = default;
            _rg.SetEnable(graph, false);
            Assert.That(_rg.DisableGraphCount, Is.EqualTo(0));
        }

        [Test]
        public void DisabledGraph_PropertyChange_MarksDirty_NoEnqueue()
        {
            RGraph graph = MakeModelGraph(out TestModel model, out _, "Hp");
            _rg.SetEnable(graph, false);

            model.Hp = 1;

            Assert.That(_rg.WaitExecuteRenderingCount, Is.EqualTo(0), "禁用状态下不应入队渲染");
            Assert.That(_rg.DisableGraphCount, Is.EqualTo(1), "禁用图仍在禁用集中，只是被标脏");
        }

        [Test]
        public void TrySetDirty_EnabledGraph_ReturnsFalse()
        {
            RGraph graph = MakeModelGraph(out _, out _);
            Assert.That(_rg.TrySetDirty(graph), Is.False, "启用中的图无法被标脏");
        }

        [Test]
        public void TrySetDirty_DisabledGraph_ReturnsTrue()
        {
            RGraph graph = MakeModelGraph(out _, out _);
            _rg.SetEnable(graph, false);

            Assert.That(_rg.TrySetDirty(graph), Is.True);
        }

        [Test]
        public void TrySetDirty_DefaultGraph_ReturnsFalse()
        {
            RGraph graph = default;
            Assert.That(_rg.TrySetDirty(graph), Is.False);
        }

        #endregion

        #region RenderIfDirtyOrForce

        [Test]
        public void RenderIfDirtyOrForce_NotDirtyNotForced_DoesNotRender()
        {
            RGraph graph = MakeModelGraph(out _, out RenderCounter counter);

            _rg.RenderIfDirtyOrForce(graph, force: false, enable: true);
            Assert.That(counter.Count, Is.EqualTo(0));
        }

        [Test]
        public void RenderIfDirtyOrForce_Forced_Renders()
        {
            RGraph graph = MakeModelGraph(out _, out RenderCounter counter);

            _rg.RenderIfDirtyOrForce(graph, force: true, enable: true);
            Assert.That(counter.Count, Is.EqualTo(1));
        }

        [Test]
        public void RenderIfDirtyOrForce_DirtyDisabled_RendersAndClearsDirtyWhenEnable()
        {
            RGraph graph = MakeModelGraph(out TestModel model, out RenderCounter counter, "Hp");
            _rg.SetEnable(graph, false);
            model.Hp = 1; // 标记为脏（flag == 2）

            _rg.RenderIfDirtyOrForce(graph, force: false, enable: true);

            Assert.That(counter.Count, Is.EqualTo(1), "脏图应被渲染");
            Assert.That(_rg.DisableGraphCount, Is.EqualTo(0), "enable 为 true 时应从禁用集中移除");
        }

        [Test]
        public void RenderIfDirtyOrForce_DirtyDisabled_KeepsDirtyWhenNotEnable()
        {
            RGraph graph = MakeModelGraph(out TestModel model, out RenderCounter counter, "Hp");
            _rg.SetEnable(graph, false);
            model.Hp = 1;

            _rg.RenderIfDirtyOrForce(graph, force: false, enable: false);

            Assert.That(counter.Count, Is.EqualTo(1));
            Assert.That(_rg.DisableGraphCount, Is.EqualTo(1), "enable 为 false 时应保留禁用状态");
        }

        [Test]
        public void RenderIfDirtyOrForce_DefaultGraph_IsNoOp()
        {
            RGraph graph = default;
            Assert.DoesNotThrow(() => _rg.RenderIfDirtyOrForce(graph, true, true));
        }

        #endregion

        #region ClearAll / Clear(model)

        [Test]
        public void ClearAll_RemovesEverythingAndUnsubscribes()
        {
            RGraph graph = MakeModelGraph(out TestModel model, out _, "Hp");
            _rg.SetEnable(graph, false);

            _rg.ClearAll();

            Assert.That(_rg.Entry.g.next.Count, Is.EqualTo(0));
            Assert.That(_rg.DisableGraphCount, Is.EqualTo(0));
            Assert.That(_rg.WaitExecuteRenderingCount, Is.EqualTo(0));

            model.Hp = 99; // 已解除订阅，不应再入队
            Assert.That(_rg.WaitExecuteRenderingCount, Is.EqualTo(0));
        }

        [Test]
        public void Clear_Model_RemovesOnlyThatModel()
        {
            var modelA = new TestModel();
            var modelB = new TestModel();
            RGraph graph = default;
            _rg.AddNode(ref graph, modelA, () => { });
            _rg.AddNode(ref graph, modelB, () => { });

            _rg.Clear(modelA);

            Assert.That(_rg.Entry.g.next.ContainsKey(modelA), Is.False);
            Assert.That(_rg.Entry.g.next.ContainsKey(modelB), Is.True);

            modelA.Hp = 1;
            Assert.That(_rg.WaitExecuteRenderingCount, Is.EqualTo(0), "已清理的模型不再触发渲染");

            modelB.Hp = 1;
            Assert.That(_rg.WaitExecuteRenderingCount, Is.EqualTo(1), "未清理的模型仍可触发渲染");
        }

        [Test]
        public void Clear_Model_Null_IsNoOp()
        {
            RGraph graph = MakeModelGraph(out _, out _);
            int before = _rg.Entry.g.next.Count;

            _rg.Clear((BaseModel)null);
            Assert.That(_rg.Entry.g.next.Count, Is.EqualTo(before));
        }

        [Test]
        public void Clear_Model_Unregistered_IsNoOp()
        {
            MakeModelGraph(out _, out _);
            var other = new TestModel();

            Assert.DoesNotThrow(() => _rg.Clear(other));
        }

        #endregion

        #region Clear(ref graph, model[, properties])

        [Test]
        public void Clear_GraphModel_RemovesBindingAndResetsGraph()
        {
            RGraph graph = MakeModelGraph(out TestModel model, out _);

            _rg.Clear(ref graph, model);

            Assert.That(graph.g, Is.Null, "唯一绑定被清理后图应被回收为默认值");
            Assert.That(_rg.Entry.g.next.ContainsKey(model), Is.False);
        }

        [Test]
        public void Clear_GraphModel_NullArgs_IsNoOp()
        {
            RGraph graph = MakeModelGraph(out TestModel model, out _);

            RGraph defaultGraph = default;
            Assert.DoesNotThrow(() => _rg.Clear(ref defaultGraph, model));
            Assert.DoesNotThrow(() => _rg.Clear(ref graph, null));
            Assert.That(_rg.Entry.g.next.ContainsKey(model), Is.True, "无效参数不应改变绑定");
        }

        [Test]
        public void Clear_GraphModelProperties_RemovesSelectedProperty()
        {
            RGraph graph = MakeModelGraph(out TestModel model, out _, "Hp", "Mp");

            _rg.Clear(ref graph, model, "Hp");

            model.Hp = 1;
            Assert.That(_rg.WaitExecuteRenderingCount, Is.EqualTo(0), "已清理属性不再入队");

            model.Mp = 1;
            Assert.That(_rg.WaitExecuteRenderingCount, Is.EqualTo(1), "剩余属性仍生效");
        }

        [Test]
        public void Clear_GraphModelProperties_LastProperty_ClearsModel()
        {
            RGraph graph = MakeModelGraph(out TestModel model, out _, "Hp");

            _rg.Clear(ref graph, model, "Hp");

            Assert.That(_rg.Entry.g.next.ContainsKey(model), Is.False, "清空全部属性后应移除模型绑定");
        }

        [Test]
        public void Clear_GraphModelProperties_EmptyNames_ClearsWholeModel()
        {
            RGraph graph = MakeModelGraph(out TestModel model, out _, "Hp", "Mp");

            _rg.Clear(ref graph, model, new string[0]);

            Assert.That(_rg.Entry.g.next.ContainsKey(model), Is.False);
        }

        [Test]
        public void Clear_GraphModelProperties_ParamsStruct()
        {
            RGraph graph = MakeModelGraph(out TestModel model, out _, "Hp", "Mp");

            _rg.Clear(ref graph, model, new Params<string>(1, "Hp"));

            model.Hp = 1;
            Assert.That(_rg.WaitExecuteRenderingCount, Is.EqualTo(0));
            model.Mp = 1;
            Assert.That(_rg.WaitExecuteRenderingCount, Is.EqualTo(1));
        }

        #endregion

        #region Clear(event)

        [Test]
        public void Clear_Event_RemovesEventBinding()
        {
            EventKey ek = "evt";
            Action listener = () => { };
            EventMgr.On(ek, listener);
            try
            {
                RGraph graph = default;
                _rg.AddNode(ref graph, ek, () => { });

                _rg.Clear(ek);
                Assert.That(_rg.Entry.g.next.ContainsKey(ek), Is.False);

                EventMgr.Dispatch(ek);
                Assert.That(_rg.WaitExecuteRenderingCount, Is.EqualTo(0));
            }
            finally
            {
                EventMgr.Off(ek, listener);
            }
        }

        [Test]
        public void Clear_Event_NotEventKey_IsNoOp()
        {
            Assert.DoesNotThrow(() => _rg.Clear(default(EventKey)));
        }

        [Test]
        public void Clear_GraphEvent_RemovesBindingAndResetsGraph()
        {
            EventKey ek = "evt";
            RGraph graph = default;
            _rg.AddNode(ref graph, ek, () => { });

            _rg.Clear(ref graph, ek);

            Assert.That(graph.g, Is.Null, "唯一事件绑定被清理后图应被回收");
            Assert.That(_rg.Entry.g.next.ContainsKey(ek), Is.False);
        }

        #endregion

        #region Rebind

        [Test]
        public void Rebind_Model_MigratesBindingAndRefreshes()
        {
            var oldModel = new TestModel();
            var newModel = new TestModel();
            var counter = new RenderCounter();
            RGraph graph = default;
            _rg.AddNode(ref graph, oldModel, counter.Render, "Hp");

            _rg.Rebind(oldModel, newModel); // refreshRightNow 默认 true

            Assert.That(counter.Count, Is.EqualTo(1), "重绑定后应立即渲染一次");
            Assert.That(_rg.Entry.g.next.ContainsKey(oldModel), Is.False);
            Assert.That(_rg.Entry.g.next.ContainsKey(newModel), Is.True);

            oldModel.Hp = 1;
            Assert.That(_rg.WaitExecuteRenderingCount, Is.EqualTo(0), "旧模型不再触发");

            newModel.Hp = 1;
            Assert.That(_rg.WaitExecuteRenderingCount, Is.EqualTo(1), "新模型接管触发");
        }

        [Test]
        public void Rebind_Model_NoRefresh_DoesNotRender()
        {
            var oldModel = new TestModel();
            var newModel = new TestModel();
            var counter = new RenderCounter();
            RGraph graph = default;
            _rg.AddNode(ref graph, oldModel, counter.Render, "Hp");

            _rg.Rebind(oldModel, newModel, refreshRightNow: false);

            Assert.That(counter.Count, Is.EqualTo(0));
            Assert.That(_rg.Entry.g.next.ContainsKey(newModel), Is.True);
        }

        [Test]
        public void Rebind_Model_SameOrNull_IsNoOp()
        {
            var model = new TestModel();
            RGraph graph = default;
            _rg.AddNode(ref graph, model, () => { });

            Assert.DoesNotThrow(() => _rg.Rebind(model, model));
            Assert.DoesNotThrow(() => _rg.Rebind<TestModel>(null, model));
            Assert.DoesNotThrow(() => _rg.Rebind<TestModel>(model, null));
            Assert.That(_rg.Entry.g.next.ContainsKey(model), Is.True);
        }

        [Test]
        public void Rebind_GraphModel_MigratesBinding()
        {
            var oldModel = new TestModel();
            var newModel = new TestModel();
            var counter = new RenderCounter();
            RGraph graph = default;
            _rg.AddNode(ref graph, oldModel, counter.Render, "Hp");

            _rg.Rebind(graph, oldModel, newModel);

            Assert.That(counter.Count, Is.EqualTo(1));
            Assert.That(_rg.Entry.g.next.ContainsKey(oldModel), Is.False);
            Assert.That(_rg.Entry.g.next.ContainsKey(newModel), Is.True);

            newModel.Hp = 1;
            Assert.That(_rg.WaitExecuteRenderingCount, Is.EqualTo(1));
        }

        [Test]
        public void Rebind_GraphModel_NewModelAlreadyBound_IsNoOp()
        {
            var oldModel = new TestModel();
            var newModel = new TestModel();
            RGraph graph = default;
            _rg.AddNode(ref graph, oldModel, () => { }, "Hp");
            _rg.AddNode(ref graph, newModel, () => { }, "Hp");

            Assert.DoesNotThrow(() => _rg.Rebind(graph, oldModel, newModel));
            Assert.That(_rg.Entry.g.next.ContainsKey(oldModel), Is.True, "目标模型已存在时不应迁移");
        }

        #endregion

        #region Dispose

        [Test]
        public void Dispose_ClearsEverything()
        {
            RGraph graph = MakeModelGraph(out _, out _, "Hp");
            _rg.SetEnable(graph, false);

            _rg.Dispose();

            Assert.That(_rg.Entry.g, Is.Null, "销毁后入口应为空");
            Assert.That(_rg.DisableGraphCount, Is.EqualTo(0));
            Assert.That(_rg.WaitExecuteRenderingCount, Is.EqualTo(0));
        }

        [Test]
        public void Dispose_IsIdempotent()
        {
            _rg.Dispose();
            Assert.DoesNotThrow(() => _rg.Dispose());
        }

        [Test]
        public void Dispose_UnsubscribesFromEvents()
        {
            EventKey ek = "evt";
            Action listener = () => { };
            EventMgr.On(ek, listener);
            try
            {
                RGraph graph = default;
                _rg.AddNode(ref graph, ek, () => { });
                _rg.Dispose();

                // 销毁后事件触发不应抛出（已从 EventMgr.OnEvent 注销）
                Assert.DoesNotThrow(() => EventMgr.Dispatch(ek));
            }
            finally
            {
                EventMgr.Off(ek, listener);
            }
        }

        #endregion

        #region 辅助工具

        /// <summary>
        /// 快速构建一个绑定了单一模型渲染函数的图。
        /// </summary>
        private RGraph MakeModelGraph(out TestModel model, out RenderCounter counter, params string[] properties)
        {
            model = new TestModel();
            counter = new RenderCounter();
            RGraph graph = default;
            if (properties == null || properties.Length == 0)
                _rg.AddNode(ref graph, model, counter.Render);
            else
                _rg.AddNode(ref graph, model, counter.Render, properties);
            return graph;
        }

        private sealed class RenderCounter
        {
            public int Count { get; private set; }
            public void Render() => Count++;
        }

        private sealed class TestModel : BaseModel
        {
            public int Hp { get; set; }
            public int Mp { get; set; }
        }

        #endregion
    }
}
