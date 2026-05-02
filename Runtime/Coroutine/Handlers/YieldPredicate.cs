using System;

namespace UniVue.Coroutine
{
    public sealed class YieldPredicate : YieldHandler
    {
        public override Type YieldType => typeof(Func<bool>);

        protected override bool HandleYield(CoroutineMgr.CoroutineRecorder recorder)
        {
            Func<bool> predicate = (Func<bool>)recorder.Yield;
            return predicate.Invoke();
        }
    }
}