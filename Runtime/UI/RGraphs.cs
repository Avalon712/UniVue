using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UniVue.Common;
using UniVue.Coroutine;
using UniVue.Event;
using UniVue.Internal;
using UniVue.Model;
using UniVue.Utils;

namespace UniVue.UI
{
    public sealed class RGraphs
    {
        private static readonly CoroutineYieldHandleContext _renderCtx =
            new(new List<YieldHandler>(1) { new InternalYieldWaitSecondsTime() });

        /// <summary>
        /// key = RGraph.g  value = 1-禁用 2-禁用并且禁用期间有更新
        /// </summary>
        private readonly Dictionary<RNode, int> _disableGraphs = new();

        private readonly HashSet<RNode> _renderQueue = new(16);
        private CoroutineID _coroutineId;

        public RGraphs()
        {
            Entry = RGraph.Create();
            EventMgr.OnEvent += OnTriggerEvent;
        }

        internal RGraph Entry { get; private set; }

        /// <summary>
        /// 渲染间隔（秒），默认0.1秒渲染一次（如果有渲染事件触发）
        /// </summary>
        public float RenderInternal { get; set; } = 0.1f;

        public int WaitExecuteRenderingCount => _renderQueue.Count;

        public int DisableGraphCount => _disableGraphs.Count;

        /// <summary>
        /// 销毁所有渲染绑定，同时RGraphs不可再用
        /// </summary>
        internal void Dispose()
        {
            if (Entry.g == null) return;
            _disableGraphs.Clear();
            ClearAll();
            RNode.ForceDispose(Entry.g);
            Entry = default;
            EventMgr.OnEvent -= OnTriggerEvent;
            CoroutineMgr.Kill(_coroutineId);
            _coroutineId = 0;
            _renderQueue.Clear();
        }

        private IEnumerator Render()
        {
            while (true)
            {
                yield return RenderInternal;
                if (_renderQueue.Count == 0) continue;

                using InternalTempCollection<HashSet<RNode>, RNode> queue = new(_renderQueue);
                _renderQueue.Clear();

                foreach (RNode rNode in queue)
                {
                    Action renderFn = rNode.Key.As<Action>();
                    if (rNode.Reachable && renderFn != null) renderFn.Invoke();
                }

                //清空那些已经被释放的RGraph
                queue.Collection.Clear();
                foreach (RNode g in _disableGraphs.Keys)
                {
                    if (!g.Reachable)
                        queue.Collection.Add(g);
                }

                foreach (RNode g in queue) _disableGraphs.Remove(g);
            }
        }

        private void OnTriggerEvent(EventKey eventKey)
        {
            if (Entry.g == null || !Entry.g.next.TryGetValue(eventKey, out RNode mGraphs)) return;
            using InternalTempCollection<Dictionary<RKey, RNode>, KeyValuePair<RKey, RNode>> graphs = new(mGraphs.next);
            foreach (RNode graph in graphs.Collection.Values)
            {
                graph.Visit(node =>
                {
                    if (node.Key.type != RKeyType.Event || !node.Key.Equals(eventKey))
                        return node.Key.type == RKeyType.Event || node.Key.type == RKeyType.Graph;

                    foreach (RKey key in node.next.Keys)
                    {
                        if (key.type != RKeyType.Rendering || key.As<Action>() == null) continue;

                        if (!GetEnable(graph.Key.As<RGraph>()))
                            _disableGraphs[graph] = 2;
                        else
                            _renderQueue.Add(node.next[key]);
                    }

                    return node.Key.type == RKeyType.Event || node.Key.type == RKeyType.Graph;
                });
            }

            if (_coroutineId == 0) _coroutineId = CoroutineMgr.Run(Render(), _renderCtx);
        }

        private void OnNotifyPropertyChanged(BaseModel model, string propertyName, object _)
        {
            if (Entry.g == null || !Entry.g.next.TryGetValue(model, out RNode mGraphs)) return;
            using InternalTempCollection<Dictionary<RKey, RNode>, KeyValuePair<RKey, RNode>> graphs = new(mGraphs.next);

            foreach (RNode graph in graphs.Collection.Values)
            {
                graph.Visit(node =>
                {
                    if (node.Key.type != RKeyType.Model || !node.Key.Equals(model))
                        return node.Key.type == RKeyType.Model || node.Key.type == RKeyType.Graph;

                    if (node.next.TryGetValue(propertyName, out RNode pNode))
                    {
                        foreach (RKey key in pNode.next.Keys)
                        {
                            if (key.type != RKeyType.Rendering || key.As<Action>() == null) continue;

                            if (!GetEnable(graph.Key.As<RGraph>()))
                                _disableGraphs[graph] = 2;
                            else
                                _renderQueue.Add(pNode.next[key]);
                        }
                    }

                    foreach (RKey key in node.next.Keys)
                    {
                        if (key.type != RKeyType.Rendering || key.As<Action>() == null) continue;

                        if (!GetEnable(graph.Key.As<RGraph>()))
                            _disableGraphs[graph] = 2;
                        else
                            _renderQueue.Add(node.next[key]);
                    }

                    return node.Key.type == RKeyType.Model || node.Key.type == RKeyType.Graph;
                });
            }

            if (_coroutineId == 0) _coroutineId = CoroutineMgr.Run(Render(), _renderCtx);
        }

        internal void SetEnable(in RGraph graph, bool enable)
        {
            if (graph.g == null) return;
            if (enable)
                _disableGraphs.Remove(graph.g);
            else
                _disableGraphs.TryAdd(graph.g, 1);
        }

        internal bool GetEnable(in RGraph graph)
        {
            if (graph.g == null) return false;
            return !_disableGraphs.ContainsKey(graph.g);
        }

        internal bool TrySetDirty(in RGraph graph)
        {
            if (graph.g == null || GetEnable(graph)) return false;
            _disableGraphs[graph.g] = 2;
            return true;
        }

        internal void RenderIfDirtyOrForce(in RGraph graph, bool force, bool enable)
        {
            if (graph.g == null || !graph.g.Reachable) return;
            if ((_disableGraphs.TryGetValue(graph.g, out int flag) && flag == 2) || force)
            {
                graph.g.Visit(node =>
                {
                    if (node.Key.type == RKeyType.Rendering) node.Key.As<Action>()?.Invoke();
                    return true;
                });
                if (enable)
                    _disableGraphs.Remove(graph.g);
            }
        }

        [Conditional("UNITY_EDITOR")]
        private void CheckDispose()
        {
            ExceptionUtils.ThrowIfNull(Entry.g, "RGraphs is disposed!");
        }

        private RNode GetModelNode(ref RGraph graph, BaseModel model)
        {
            if (graph.g == null)
                graph = RGraph.Create();

            if (!Entry.g.next.TryGetValue(model, out RNode mGraphs))
            {
                mGraphs = RNode.Create();
                Entry.g.next[model] = mGraphs;
                mGraphs.Key = model;
                mGraphs.In = 1;
                model.OnPropertyChanged += OnNotifyPropertyChanged;
            }

            if (!mGraphs.next.ContainsKey(graph))
            {
                graph.g.In++;
                mGraphs.next[graph] = graph.g;
            }

            if (!graph.g.next.TryGetValue(model, out RNode mNode))
            {
                mNode = RNode.Create();
                mNode.Key = model;
                mNode.In = 1;
                graph.g.next[model] = mNode;
            }

            return mNode;
        }

        internal void AddNode(ref RGraph graph, BaseModel model, Action renderFn)
        {
            CheckDispose();
            if (model == null || renderFn == null) return;

            RNode mNode = GetModelNode(ref graph, model);
            if (!mNode.next.TryGetValue(renderFn, out RNode rNode))
            {
                rNode = RNode.Create();
                rNode.Key = renderFn;
                rNode.In++;
                mNode.next[renderFn] = rNode;
            }
        }

        internal void AddNode(ref RGraph graph, BaseModel model, Action renderFn, params string[] propertyNames)
        {
            CheckDispose();
            if (model == null || renderFn == null) return;
            if (graph.g == null)
                graph = RGraph.Create();

            if (propertyNames == null || propertyNames.Length == 0)
            {
                AddNode(ref graph, model, renderFn);
                return;
            }

            RNode mNode = GetModelNode(ref graph, model);
            foreach (string propertyName in propertyNames)
            {
                if (!mNode.next.TryGetValue(propertyName, out RNode pNode))
                {
                    pNode = RNode.Create();
                    pNode.Key = propertyName;
                    pNode.In = 1;
                    mNode.next[propertyName] = pNode;
                }

                if (!pNode.next.TryGetValue(renderFn, out RNode rNode))
                {
                    rNode = RNode.Create();
                    rNode.Key = renderFn;
                    pNode.next[renderFn] = rNode;
                }

                rNode.In++;
            }
        }

        internal void AddNode(ref RGraph graph, BaseModel model, Action renderFn, in Params<string> propertyNames)
        {
            CheckDispose();
            if (model == null || renderFn == null) return;
            if (graph.g == null)
                graph = RGraph.Create();

            if (propertyNames.Length == 0)
            {
                AddNode(ref graph, model, renderFn);
                return;
            }

            RNode mNode = GetModelNode(ref graph, model);
            foreach (string propertyName in propertyNames)
            {
                if (!mNode.next.TryGetValue(propertyName, out RNode pNode))
                {
                    pNode = RNode.Create();
                    pNode.Key = propertyName;
                    pNode.In = 1;
                    mNode.next[propertyName] = pNode;
                }

                if (!pNode.next.TryGetValue(renderFn, out RNode rNode))
                {
                    rNode = RNode.Create();
                    rNode.Key = renderFn;
                    pNode.next[renderFn] = rNode;
                }

                rNode.In++;
            }
        }

        internal void AddNode(ref RGraph graph, in EventKey eventKey, Action renderFn)
        {
            CheckDispose();
            if (eventKey.Type == EventKeyType.NotEventKey || renderFn == null) return;
            if (graph.g == null)
                graph = RGraph.Create();

            if (!Entry.g.next.TryGetValue(eventKey, out RNode mGraphs))
            {
                mGraphs = RNode.Create();
                Entry.g.next[eventKey] = mGraphs;
                mGraphs.Key = eventKey;
                mGraphs.In = 1;
            }

            if (!mGraphs.next.ContainsKey(graph))
            {
                graph.g.In++;
                mGraphs.next[graph] = graph.g;
            }

            if (!graph.g.next.TryGetValue(eventKey, out RNode eNode))
            {
                eNode = RNode.Create();
                eNode.Key = eventKey;
                eNode.In = 1;
                graph.g.next[eventKey] = eNode;
            }

            if (!eNode.next.TryGetValue(renderFn, out RNode rNode))
            {
                rNode = RNode.Create();
                rNode.Key = renderFn;
                eNode.next[renderFn] = rNode;
            }

            rNode.In++;
        }

        internal void Remove(ref RGraph graph)
        {
            CheckDispose();
            if (graph.g == null) return;

            _disableGraphs.Remove(graph.g);

            using InternalTempCollection<List<RKey>, RKey> keys = new(null);

            //收集所有可达RGraph节点的RKey
            graph.CollectRKeysNoneAlloc(RKeyType.Model | RKeyType.Event, keys);
            foreach (RKey key in keys)
            {
                if (!Entry.g.next.TryGetValue(key, out RNode node)) continue;
                if (node.next.Remove(graph) && node.Out <= 0) node.In = 0; //标记为不可达
            }

            RNode.ForceDispose(graph.g);
            graph = default;

            //收集那些从Entry不可达的节点进行回收
            keys.Collection.Clear();
            foreach (RNode node in Entry.g.next.Values)
            {
                if (!node.Reachable)
                    keys.Collection.Add(node.Key);
            }

            foreach (RKey key in keys)
            {
                if (!Entry.g.next.Remove(key, out RNode node)) continue;
                if (key.type == RKeyType.Model)
                    key.As<BaseModel>().OnPropertyChanged -= OnNotifyPropertyChanged;
                RNode.ForceDispose(node);
            }
        }

        /// <summary>
        /// 清空所有渲染绑定
        /// </summary>
        public void ClearAll()
        {
            CheckDispose();
            foreach (RNode node in Entry.g.next.Values)
            {
                if (node.Key.type == RKeyType.Model)
                    node.Key.As<BaseModel>().OnPropertyChanged -= OnNotifyPropertyChanged;
                RNode.ForceDispose(node);
            }

            _disableGraphs.Clear();
            _renderQueue.Clear();
            Entry.g.next.Clear();
        }

        /// <summary>
        /// 清空所有有关指定model的渲染
        /// </summary>
        /// <param name="model"></param>
        public void Clear(BaseModel model)
        {
            CheckDispose();
            if (model == null || !Entry.g.next.Remove(model, out RNode mGraphs)) return;

            model.OnPropertyChanged -= OnNotifyPropertyChanged;
            foreach (RNode graph in mGraphs.next.Values)
            {
                --graph.In;
                if (!graph.Reachable)
                    RNode.ForceDispose(graph);
                else
                    RNode.SafeDispose(graph.next[model]); //说明此Graph还可通过其他节点可达，不能强制释放        
            }

            mGraphs.next.Clear();
            RNode.ForceDispose(mGraphs);
        }


        /// <summary>
        /// 清空目标RGraph上所有model的渲染
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="model"></param>
        public void Clear(ref RGraph graph, BaseModel model)
        {
            CheckDispose();
            if (graph.g == null || model == null) return;
            if (!Entry.g.next.TryGetValue(model, out RNode mGraphs)) return;

            if (mGraphs.next.Remove(graph))
            {
                if (mGraphs.Out <= 0)
                {
                    model.OnPropertyChanged -= OnNotifyPropertyChanged;
                    Entry.g.next.Remove(model);
                    RNode.ForceDispose(mGraphs);
                }

                graph.g.In--;
                if (!graph.g.Reachable)
                {
                    RNode.ForceDispose(graph.g);
                    graph = default;
                }
                else
                {
                    if (graph.g.next.Remove(model, out RNode mNode)) RNode.SafeDispose(mNode);
                }
            }
        }

        /// <summary>
        /// 清空目标RGraph上model指定属性的渲染
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="model"></param>
        /// <param name="propertyNames">属性名称</param>
        public void Clear(ref RGraph graph, BaseModel model, params string[] propertyNames)
        {
            CheckDispose();
            if (graph.g == null || model == null) return;
            if (propertyNames == null || propertyNames.Length == 0)
            {
                Clear(ref graph, model);
            }
            else
            {
                if (!Entry.g.next.TryGetValue(model, out _)) return;

                if (!graph.g.next.TryGetValue(model, out RNode mNode)) return;

                foreach (string propertyName in propertyNames)
                {
                    if (mNode.next.Remove(propertyName, out RNode pNode))
                        RNode.SafeDispose(pNode);
                }

                if (mNode.Out <= 0)
                    Clear(ref graph, model);
            }
        }

        /// <summary>
        /// 清空目标RGraph上model指定属性的渲染
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="model"></param>
        /// <param name="propertyNames">属性名称</param>
        public void Clear(ref RGraph graph, BaseModel model, in Params<string> propertyNames)
        {
            CheckDispose();
            if (graph.g == null || model == null) return;
            if (propertyNames.Length == 0)
            {
                Clear(ref graph, model);
            }
            else
            {
                if (!Entry.g.next.TryGetValue(model, out _)) return;

                if (!graph.g.next.TryGetValue(model, out RNode mNode)) return;

                foreach (string propertyName in propertyNames)
                {
                    if (mNode.next.Remove(propertyName, out RNode pNode))
                        RNode.SafeDispose(pNode);
                }

                if (mNode.Out <= 0)
                    Clear(ref graph, model);
            }
        }

        /// <summary>
        /// 清空所有监听了事件的渲染
        /// </summary>
        /// <param name="eventKey"></param>
        public void Clear(in EventKey eventKey)
        {
            CheckDispose();
            if (eventKey.Type == EventKeyType.NotEventKey || !Entry.g.next.Remove(eventKey, out RNode node)) return;

            foreach (RNode graph in node.next.Values)
            {
                --graph.In;
                if (!graph.Reachable)
                    RNode.ForceDispose(graph);
                else
                    RNode.SafeDispose(graph.next[eventKey]); //说明此Graph还可通过其他节点可达，不能强制释放        
            }

            node.next.Clear();
            RNode.ForceDispose(node);
        }

        /// <summary>
        /// 清空目标RGraph上所有eventKey的渲染
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="eventKey"></param>
        public void Clear(ref RGraph graph, in EventKey eventKey)
        {
            CheckDispose();
            if (graph.g == null || eventKey.Type == EventKeyType.NotEventKey) return;

            if (!Entry.g.next.TryGetValue(eventKey, out RNode eGraphs)) return;

            if (eGraphs.next.Remove(graph))
            {
                if (eGraphs.Out <= 0)
                {
                    Entry.g.next.Remove(eventKey);
                    RNode.ForceDispose(eGraphs);
                }

                graph.g.In--;
                if (graph.g.Reachable)
                {
                    if (!graph.g.next.Remove(eventKey, out RNode eNode)) return;
                    RNode.SafeDispose(eNode);
                }
                else
                {
                    RNode.ForceDispose(graph.g);
                    graph = default;
                }
            }
        }

        /// <summary>
        /// 将所有绑定了模型oldModel替换为newModel
        /// </summary>
        /// <param name="oldModel"></param>
        /// <param name="newModel"></param>
        /// <param name="refreshRightNow">重新绑定完成后是否立即渲染一次，默认为true</param>
        /// <typeparam name="T">BaseModel</typeparam>
        public void Rebind<T>(T oldModel, T newModel, bool refreshRightNow = true) where T : BaseModel
        {
            if (oldModel == null || newModel == null || oldModel == newModel) return;
            if (!Entry.g.next.TryGetValue(newModel, out RNode newGraphs))
            {
                newGraphs = RNode.Create();
                newGraphs.In = 1;
                newGraphs.Key = newModel;
                Entry.g.next[newModel] = newGraphs;
                newModel.OnPropertyChanged += OnNotifyPropertyChanged;
            }

            if (!Entry.g.next.Remove(oldModel, out RNode mGraphs)) return;

            oldModel.OnPropertyChanged -= OnNotifyPropertyChanged;
            foreach (RNode graph in mGraphs.next.Values)
            {
                if (!graph.next.Remove(oldModel, out RNode mNode)) continue;
                if (graph.next.ContainsKey(newModel))
                {
                    RNode.SafeDispose(mNode);
                }
                else
                {
                    mNode.Key = newModel;
                    graph.next[newModel] = mNode;
                    if (newGraphs.next.TryAdd(graph.Key, graph) && refreshRightNow)
                        RenderIfDirtyOrForce(graph.Key.As<RGraph>(), true, GetEnable(graph.Key.As<RGraph>()));
                }
            }

            mGraphs.next.Clear();
            RNode.ForceDispose(mGraphs);
        }

        /// <summary>
        /// 将目标RGraph身上绑定了oldModel替换为newModel
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="oldModel"></param>
        /// <param name="newModel"></param>
        /// <param name="refreshRightNow">重新绑定完成后是否立即渲染一次，默认为true</param>
        /// <typeparam name="T">BaseModel</typeparam>
        public void Rebind<T>(in RGraph graph, T oldModel, T newModel, bool refreshRightNow = true) where T : BaseModel
        {
            if (graph.g == null || oldModel == null || newModel == null || oldModel == newModel) return;
            if (!graph.g.next.ContainsKey(oldModel) || graph.g.next.ContainsKey(newModel)) return;
            if (!graph.g.next.Remove(oldModel, out RNode mNode)) return;

            if (Entry.g.next.TryGetValue(oldModel, out RNode mGraphs))
            {
                if (mGraphs.next.Remove(graph) && mGraphs.Out <= 0)
                {
                    Entry.g.next.Remove(oldModel);
                    oldModel.OnPropertyChanged -= OnNotifyPropertyChanged;
                    RNode.ForceDispose(mGraphs);
                }
            }

            if (!Entry.g.next.TryGetValue(newModel, out mGraphs))
            {
                mGraphs = RNode.Create();
                mGraphs.In = 1;
                mGraphs.Key = newModel;
                Entry.g.next[newModel] = mGraphs;
                newModel.OnPropertyChanged += OnNotifyPropertyChanged;
            }

            mGraphs.next[graph] = graph.g;

            mNode.Key = newModel;
            graph.g.next[newModel] = mNode;

            if (refreshRightNow) RenderIfDirtyOrForce(graph, true, GetEnable(graph));
        }
    }
}