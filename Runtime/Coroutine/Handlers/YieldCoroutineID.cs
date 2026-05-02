using System;

namespace UniVue.Coroutine
{
    public sealed class YieldCoroutineID : YieldHandler
    {
        public override Type YieldType => typeof(CoroutineID);

        protected override bool HandleYield(CoroutineMgr.CoroutineRecorder recorder)
        {
            CoroutineID dependency = (CoroutineID)recorder.Yield;
            CoroutineMgr.CombineDependency(recorder.CoroutineId, dependency.id);
            return true;
        }
    }
}