using System;
using UnityEngine;
using UniVue.Coroutine;

namespace UniVue.Internal
{
    internal sealed class InternalYieldWaitSecondsTime : YieldHandler
    {
        public override Type YieldType => typeof(float);

        protected override bool HandleYield(CoroutineMgr.CoroutineRecorder recorder)
        {
            float seconds = (float)recorder.Yield - Time.deltaTime;
            recorder.Yield = seconds;
            return seconds <= 0;
        }
    }
}