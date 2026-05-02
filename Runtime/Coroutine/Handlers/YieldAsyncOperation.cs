using System;
using UnityEngine;

namespace UniVue.Coroutine
{
    public sealed class YieldAsyncOperation : YieldHandler
    {
        public override Type YieldType => typeof(AsyncOperation);

        protected override bool HandleYield(CoroutineMgr.CoroutineRecorder recorder)
        {
            AsyncOperation asyncOperation = (AsyncOperation)recorder.Yield;
            return asyncOperation.isDone;
        }
    }
}