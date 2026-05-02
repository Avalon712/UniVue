using System;
using UnityEngine;

namespace UniVue.Coroutine
{
    public sealed class YieldWaitWhile : YieldHandler
    {
        public override Type YieldType => typeof(WaitWhile);

        protected override bool HandleYield(CoroutineMgr.CoroutineRecorder recorder)
        {
            WaitWhile waitWhile = (WaitWhile)recorder.Yield;
            return !waitWhile.keepWaiting;
        }
    }
}