using System;
using System.Collections;

namespace UniVue.Coroutine
{
    public sealed class YieldEnumerator : YieldHandler
    {
        public override Type YieldType => typeof(IEnumerator);

        protected override bool HandleYield(CoroutineMgr.CoroutineRecorder recorder)
        {
            recorder.stack.Push((IEnumerator)recorder.Yield);
            return true;
        }
    }
}