using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniVue.Internal;
using Object = UnityEngine.Object;

namespace UniVue.Coroutine
{
    /// <summary>
    /// 非Unity托管的协程实现，内部的协程调用具有队列的执行顺序
    /// </summary>
    public static class CoroutineMgr
    {
        public enum CoroutineRunEnvironment
        {
            Update,
            LateUpdate,
            FixedUpdate
        }

        public enum CoroutineStatus
        {
            Run,
            WaitDependencyCompleted,
            Stop,
            Kill
        }

        private static ulong _coroutineId = 1;
        private static VCoroutineRunner _instance;

        /// <summary>
        /// 后添加的CoroutineId更大，SortedDictionary按升序遍历，从而保证队列式的执行顺序
        /// </summary>
        private static readonly SortedDictionary<ulong, CoroutineRecorder> _coroutines = new();

        private static readonly Dictionary<ulong, CoroutineRecorder> _appendingQueue = new(16);
        private static readonly HashSet<CoroutineRecorder> _disposeQueue = new(16);

        /// <summary>
        /// 依赖拓扑图，key=被依赖的协程Id value=依赖key的协程
        /// </summary>
        private static readonly Dictionary<ulong, List<CoroutineRecorder>> _dependencyGraph = new(32);

        public static CoroutineRunEnvironment Environment { get; private set; }

        private static void InitializeIfNecessary()
        {
            if (_instance == null)
            {
                GameObject runner = new("VCoroutine Runner");
                runner.hideFlags = HideFlags.HideAndDontSave;
                _instance = runner.AddComponent<VCoroutineRunner>();
                Object.DontDestroyOnLoad(runner);
            }
        }

        private static CoroutineRecorder GetRecorder(ulong coroutineId)
        {
            if (_coroutines.TryGetValue(coroutineId, out CoroutineRecorder recorder) ||
                _appendingQueue.TryGetValue(coroutineId, out recorder))
                return recorder;
            return null;
        }

        private static List<CoroutineRecorder> GetListFromPool()
        {
            return InternalObjectPool<List<CoroutineRecorder>>.Shared.Rent();
        }

        private static void RecycleList(List<CoroutineRecorder> list)
        {
            list.Clear();
            InternalObjectPool<List<CoroutineRecorder>>.Shared.Return(ref list);
        }

        private static CoroutineRecorder Rent()
        {
            return InternalObjectPool<CoroutineRecorder>.Shared.Rent();
        }

        private static void Return(CoroutineRecorder recorder)
        {
            recorder.status = CoroutineStatus.Run;
            recorder.stack.Clear();
            recorder.Context = null;
            recorder.Yield = null;
            recorder.CoroutineId = 0;
            recorder.dependencies.Clear();
            recorder.CoroutineName = null;
            InternalObjectPool<CoroutineRecorder>.Shared.Return(ref recorder);
        }

        private static bool HandleYield(CoroutineRecorder recorder)
        {
            if (recorder.Yield != null)
            {
                YieldHandler handler = recorder.Context.GetHandler(recorder.Yield.GetType());
                if (handler != null && !handler.HandleYieldInternal(recorder)) return false;
                recorder.Yield = null;
            }

            return true;
        }

        private static void Tick(CoroutineRunEnvironment environment)
        {
            Environment = environment;

            if (environment == CoroutineRunEnvironment.LateUpdate)
            {
                if (_appendingQueue.Count > 0)
                {
                    foreach (KeyValuePair<ulong, CoroutineRecorder> kv in _appendingQueue)
                        _coroutines.Add(kv.Key, kv.Value);
                    _appendingQueue.Clear();
                }

                if (_disposeQueue.Count > 0)
                {
                    foreach (CoroutineRecorder dispose in _disposeQueue) DisposeCoroutine(dispose);
                    _disposeQueue.Clear();
                }
            }

            if (_coroutines.Count <= 0) return;

            foreach (KeyValuePair<ulong, CoroutineRecorder> kv in _coroutines)
            {
                ulong id = kv.Key;
                CoroutineRecorder recorder = kv.Value;

                if (recorder.environment != environment) continue;

                switch (recorder.status)
                {
                    case CoroutineStatus.Run:
                    {
                        if (!HandleYield(recorder)) continue;

                        IEnumerator coroutine = recorder.stack.Peek();
                        if (coroutine.MoveNext())
                        {
                            recorder.Yield = coroutine.Current;
                            HandleYield(recorder);
                        }
                        else
                        {
                            recorder.stack.Pop();
                            if (recorder.stack.Count <= 0)
                            {
                                _disposeQueue.Add(recorder);
                                recorder.status = CoroutineStatus.Kill;
                            }
                        }
                    }
                        break;
                    case CoroutineStatus.Stop:
                        break;
                    case CoroutineStatus.WaitDependencyCompleted:
                        if (recorder.dependencies.Count <= 0) recorder.status = CoroutineStatus.Run;
                        break;
                    case CoroutineStatus.Kill:
                        _disposeQueue.Add(recorder);
                        break;
                }
            }
        }

        private static bool ExistLoopDependency(CoroutineRecorder coroutine, CoroutineRecorder dependency)
        {
            HashSet<CoroutineRecorder> dependencies = dependency.dependencies;
            if (dependencies.Contains(coroutine)) return true;
            foreach (CoroutineRecorder recorder in dependencies)
            {
                if (ExistLoopDependency(coroutine, recorder))
                    return true;
            }

            return false;
        }

        private static void DisposeCoroutine(CoroutineRecorder dispose)
        {
            _coroutines.Remove(dispose.CoroutineId);
            _appendingQueue.Remove(dispose.CoroutineId);
            if (!_dependencyGraph.Remove(dispose.CoroutineId, out List<CoroutineRecorder> waitQueue)) return;
            foreach (CoroutineRecorder wait in waitQueue) wait.dependencies.Remove(dispose);
            RecycleList(waitQueue);
        }

        /// <summary>
        /// 运行协程（使用默认的yield处理上下文）
        /// </summary>
        /// <param name="coroutine">协程</param>
        /// <returns>协程唯一id</returns>
        public static CoroutineID Run(IEnumerator coroutine)
        {
            return Run(coroutine, null, CoroutineYieldHandleContext.Default);
        }

        /// <summary>
        /// 运行协程
        /// </summary>
        /// <param name="coroutine">协程</param>
        /// <param name="context">协程运行时对yield结果处理的上下文对象</param>
        /// <returns>协程唯一id</returns>
        public static CoroutineID Run(IEnumerator coroutine, CoroutineYieldHandleContext context)
        {
            return Run(coroutine, null, context);
        }

        /// <summary>
        /// 运行协程
        /// </summary>
        /// <param name="coroutine">协程</param>
        /// <param name="aliasName">协程别名（取别名方便调试）</param>
        /// <returns>协程唯一id</returns>
        public static CoroutineID Run(IEnumerator coroutine, string aliasName)
        {
            return Run(coroutine, aliasName, CoroutineYieldHandleContext.Default);
        }

        /// <summary>
        /// 运行协程
        /// </summary>
        /// <param name="coroutine">协程</param>
        /// <param name="aliasName">协程别名（取别名方便调试）</param>
        /// <param name="context">协程运行时对yield结果处理的上下文对象</param>
        /// <returns>协程唯一id</returns>
        public static CoroutineID Run(IEnumerator coroutine, string aliasName, CoroutineYieldHandleContext context)
        {
            InitializeIfNecessary();
            ulong id = _coroutineId++;
            CoroutineRecorder recorder = Rent();
            recorder.Context = context ?? CoroutineYieldHandleContext.Default;
            recorder.stack.Push(coroutine);
            recorder.CoroutineId = id;
            recorder.CoroutineName = aliasName;
            _appendingQueue.Add(id, recorder);
            return id;
        }

        /// <summary>
        /// 组合协程的依赖，只有依赖项都运行完成后才运行协程
        /// </summary>
        /// <remarks>如果内部有循环依赖将会抛出异常</remarks>
        /// <param name="coroutineId">协程</param>
        /// <param name="checkLoopDependency">是否检测循环依赖，默认为true，如果你能保证一定不会出现循环依赖，可设置为false以减少循环依赖检测的开销</param>
        /// <param name="dependencies">被依赖的协程</param>
        public static void CombineDependencies(in CoroutineID coroutineId, bool checkLoopDependency = true,
                                               params CoroutineID[] dependencies)
        {
            if (dependencies != null && dependencies.Length > 0)
            {
                foreach (CoroutineID id in dependencies)
                    CombineDependency(coroutineId, id, checkLoopDependency);
            }
        }

        /// <summary>
        /// 组合协程的依赖，只有依赖项都运行完成后才运行协程
        /// </summary>
        /// <remarks>如果内部有循环依赖将会抛出异常</remarks>
        /// <param name="coroutineId">协程</param>
        /// <param name="dependencyCoroutineId">被依赖的协程</param>
        /// <param name="checkLoopDependency">是否检测循环依赖，默认为true，如果你能保证一定不会出现循环依赖，可设置为false以减少循环依赖检测的开销</param>
        public static void CombineDependency(CoroutineID coroutineId, ulong dependencyCoroutineId,
                                             bool checkLoopDependency = true)
        {
            CoroutineRecorder recorder = GetRecorder(coroutineId);
            CoroutineRecorder dependencyRecorder = GetRecorder(dependencyCoroutineId);
            if (recorder != null && dependencyRecorder != null && !recorder.dependencies.Contains(dependencyRecorder))
            {
                if (checkLoopDependency && ExistLoopDependency(recorder, dependencyRecorder))
                    throw new Exception("duplicate loop dependency");

                if (_dependencyGraph.TryGetValue(dependencyCoroutineId, out List<CoroutineRecorder> waitList))
                {
                    waitList.Add(recorder);
                }
                else
                {
                    List<CoroutineRecorder> list = GetListFromPool();
                    list.Add(recorder);
                    _dependencyGraph.Add(dependencyCoroutineId, list);
                }

                recorder.dependencies.Add(dependencyRecorder);
                recorder.status = CoroutineStatus.WaitDependencyCompleted;
            }
        }

        /// <summary>
        /// 杀死一个协程
        /// </summary>
        /// <param name="coroutineId">协程唯一Id</param>
        public static void Kill(ulong coroutineId)
        {
            CoroutineRecorder recorder = GetRecorder(coroutineId);
            if (recorder != null && recorder.status != CoroutineStatus.Kill && !_disposeQueue.Contains(recorder))
            {
                recorder.status = CoroutineStatus.Kill;
                _disposeQueue.Add(recorder);
            }
        }

        /// <summary>
        /// 暂停一个处于运行中的协程（如果不是处于运行中则将无法生效）
        /// </summary>
        /// <param name="coroutineId">协程唯一Id</param>
        public static void Stop(ulong coroutineId)
        {
            CoroutineRecorder recorder = GetRecorder(coroutineId);
            if (recorder != null && recorder.status == CoroutineStatus.Run) recorder.status = CoroutineStatus.Stop;
        }

        /// <summary>
        /// 恢复一个处于暂停中的协程（如果不是处于暂停中则将无法生效）
        /// </summary>
        /// <param name="coroutineId">协程唯一Id</param>
        public static void Resume(ulong coroutineId)
        {
            CoroutineRecorder recorder = GetRecorder(coroutineId);
            if (recorder != null && recorder.status == CoroutineStatus.Stop) recorder.status = CoroutineStatus.Run;
        }

        public sealed class CoroutineRecorder
        {
            internal readonly HashSet<CoroutineRecorder> dependencies = new(8);
            internal readonly Stack<IEnumerator> stack = new(8);
            public CoroutineRunEnvironment environment = CoroutineRunEnvironment.Update;
            public CoroutineStatus status = CoroutineStatus.Run;
            public CoroutineYieldHandleContext Context { get; internal set; }

            public string CoroutineName { get; internal set; }
            public ulong CoroutineId { get; internal set; }

            /// <summary>
            /// 协程的yield 结果
            /// </summary>
            public object Yield { get; set; }
        }

        private sealed class VCoroutineRunner : MonoBehaviour
        {
            private void Update()
            {
                Tick(CoroutineRunEnvironment.Update);
            }

            private void FixedUpdate()
            {
                Tick(CoroutineRunEnvironment.FixedUpdate);
            }

            private void LateUpdate()
            {
                Tick(CoroutineRunEnvironment.LateUpdate);
            }

            private void OnDestroy()
            {
                _instance = null;
                foreach (KeyValuePair<ulong, CoroutineRecorder> kv in _coroutines) Return(kv.Value);

                foreach (KeyValuePair<ulong, CoroutineRecorder> kv in _appendingQueue) Return(kv.Value);

                foreach (KeyValuePair<ulong, List<CoroutineRecorder>> kv in _dependencyGraph) RecycleList(kv.Value);

                _dependencyGraph.Clear();
                _coroutines.Clear();
                _appendingQueue.Clear();
                _disposeQueue.Clear();
            }
        }
    }
}