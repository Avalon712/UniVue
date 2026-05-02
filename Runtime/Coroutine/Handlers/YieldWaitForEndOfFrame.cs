using System;
using UnityEngine;

namespace UniVue.Coroutine
{
    public sealed class YieldWaitForEndOfFrame : YieldHandler
    {
        public override Type YieldType => typeof(WaitForEndOfFrame);

        protected override bool HandleYield(CoroutineMgr.CoroutineRecorder recorder)
        {
            if (recorder.environment == CoroutineMgr.CoroutineRunEnvironment.Update)
            {
                recorder.environment = CoroutineMgr.CoroutineRunEnvironment.LateUpdate;
                return false;
            }

            recorder.environment = CoroutineMgr.CoroutineRunEnvironment.Update;
            return true;
        }
    }
}