using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace UniVue.Coroutine
{
    public sealed class YieldWaitForSeconds : YieldHandler
    {
        public override Type YieldType => typeof(WaitForSeconds);

        protected override bool HandleYield(CoroutineMgr.CoroutineRecorder recorder)
        {
            unsafe
            {
                GCHandle handle = GCHandle.Alloc(recorder.Yield, GCHandleType.Pinned);
                try
                {
                    IntPtr ptr = handle.AddrOfPinnedObject();
                    float* fieldPtr = (float*)(ptr + 0);
                    float seconds = *fieldPtr - Time.deltaTime;
                    *fieldPtr = seconds;
                    return seconds <= 0;
                }
                finally
                {
                    handle.Free();
                }
            }
        }
    }
}