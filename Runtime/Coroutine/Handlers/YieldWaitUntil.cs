using System;
using UnityEngine;

namespace UniVue.Coroutine
{
    public sealed class YieldWaitUntil : YieldHandler
    {
        public override Type YieldType => typeof(WaitUntil);

        protected override bool HandleYield(CoroutineMgr.CoroutineRecorder recorder)
        {
            WaitUntil waitUntil = (WaitUntil)recorder.Yield;
            return !waitUntil.keepWaiting;
        }
    }
}