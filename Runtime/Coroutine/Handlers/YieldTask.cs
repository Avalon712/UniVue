using System;
using System.Threading.Tasks;

namespace UniVue.Coroutine
{
    public sealed class YieldTask : YieldHandler
    {
        public override Type YieldType => typeof(Task);

        protected override bool HandleYield(CoroutineMgr.CoroutineRecorder recorder)
        {
            Task task = (Task)recorder.Yield;
            return task.IsCompleted;
        }
    }
}