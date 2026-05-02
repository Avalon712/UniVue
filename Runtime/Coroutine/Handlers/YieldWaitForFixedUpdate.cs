using System;
using UnityEngine;

namespace UniVue.Coroutine
{
    public sealed class YieldWaitForFixedUpdate : YieldHandler
    {
        public override Type YieldType => typeof(WaitForFixedUpdate);

        protected override bool HandleYield(CoroutineMgr.CoroutineRecorder recorder)
        {
            if (recorder.environment == CoroutineMgr.CoroutineRunEnvironment.Update)
            {
                recorder.environment = CoroutineMgr.CoroutineRunEnvironment.FixedUpdate;
                return false;
            }

            recorder.environment = CoroutineMgr.CoroutineRunEnvironment.Update;
            return true;
        }
    }
}