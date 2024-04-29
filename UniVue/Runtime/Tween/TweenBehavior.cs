using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UniVue.Tween.Tweens;

namespace UniVue.Tween
{
    /// <summary>
    /// 每个GameObject的动画行为由这个进行关联,这个对象可被重复使用，改变transform或其它关联的动画对象即可
    /// </summary>
    public sealed class TweenBehavior
    {
        private TweenBehavior(){  }

        /// <summary>
        /// 实现定时调用功能
        /// </summary>
        /// <param name="delay">延时</param>
        /// <param name="callback">回调函数</param>
        /// <param name="executeNum">执行次数(<=0则无限调用)</param>
        public static TweenTimer Timer(Action timerTask)
        {
            return TweenTimer.CreateTimer(timerTask);
        }

        public static TweenTask DoMove(Transform transform, float duration, Vector3 end, TweenEase ease = TweenEase.Linear)
        {
            TweenMove tween = new TweenMove(duration,ease);
            tween.Move(transform, end);
            return tween;
        }

        public static TweenTask DoMoveX(Transform transform,float duration, float end, TweenEase ease = TweenEase.Linear)
        {
            return DoMove(transform,duration, new Vector3(end, transform.position.y, transform.position.z),ease);
        }

        public static TweenTask DoMoveY(Transform transform, float duration, float end, TweenEase ease = TweenEase.Linear)
        {
            return DoMove(transform, duration, new Vector3(transform.position.x, end, transform.position.z), ease);
        }

        public static TweenTask DoMoveZ(Transform transform,float duration, float end, TweenEase ease = TweenEase.Linear)
        {
            return DoMove(transform, duration, new Vector3(transform.position.x, transform.position.y, end), ease);
        }

        public static TweenTask DoLocalMove(Transform transform,float duration, Vector3 end, TweenEase ease = TweenEase.Linear)
        {
            TweenMove tween = new TweenMove(duration, ease);
            tween.LocalMove(transform, end);
            return tween;
        }

        public static TweenTask DoLocalMoveX(Transform transform,float duration, float end, TweenEase ease = TweenEase.Linear)
        {
            return DoLocalMove(transform,duration, new Vector3(end, transform.position.y, transform.position.z), ease);
        }

        public static TweenTask DoLocalMoveY(Transform transform, float duration, float end, TweenEase ease = TweenEase.Linear)
        {
            return DoLocalMove(transform, duration, new Vector3(transform.position.x, end, transform.position.z), ease);
        }

        public static TweenTask DoLocalMoveZ(Transform transform, float duration, float end, TweenEase ease = TweenEase.Linear)
        {
            return DoLocalMove(transform, duration, new Vector3(transform.position.x, transform.position.y, end), ease);
        }

        public static TweenTask DoLocalScale(Transform transform, float duration, Vector3 end, TweenEase ease = TweenEase.Linear)
        {
            TweenScale tween = new TweenScale(duration, ease);
            tween.Scale(transform, end);
            return tween;
        }

        public static TweenTask DoLocalScaleX(Transform transform, float duration, float end, TweenEase ease = TweenEase.Linear)
        {
            return DoLocalScale(transform,duration, new Vector3(end, transform.localScale.y, transform.localScale.z),ease);
        }

        public static TweenTask DoLocalScaleY(Transform transform, float duration, float end, TweenEase ease = TweenEase.Linear)
        {
            return DoLocalScale(transform, duration, new Vector3(transform.localScale.x, end, transform.localScale.z), ease);
        }

        public static TweenTask DoLocalScaleZ(Transform transform, float duration, float end, TweenEase ease = TweenEase.Linear)
        {
            return DoLocalScale(transform, duration, new Vector3(transform.localScale.x, transform.localScale.y, end), ease);
        }

        public static TweenTask DoRotation(Transform transform, float duration, Quaternion end, TweenEase ease = TweenEase.Linear)
        {
            TweenRotation tween = new TweenRotation(duration, ease);
            tween.Rotation(transform, end.eulerAngles);
            return tween;
        }

        public static TweenTask DoRotationX(Transform transform, float duration, float end, TweenEase ease = TweenEase.Linear)
        {
            Quaternion r = transform.rotation;  r.x = end;
            return DoRotation(transform,duration,r,ease);
        }
        public static TweenTask DoRotationY(Transform transform, float duration, float end, TweenEase ease = TweenEase.Linear)
        {
            Quaternion r = transform.rotation; r.y = end;
            return DoRotation(transform, duration, r, ease);
        }

        public static TweenTask DoRotationZ(Transform transform, float duration, float end, TweenEase ease = TweenEase.Linear)
        {
            Quaternion r = transform.rotation; r.z = end;
            return DoRotation(transform, duration, r, ease);
        }
        public static TweenTask DoLocalRotation(Transform transform, float duration, Quaternion end, TweenEase ease = TweenEase.Linear)
        {
            TweenRotation tween = new TweenRotation(duration, ease);
            tween.LocalRotation(transform, end.eulerAngles);
            return tween;
        }
        public static TweenTask DoLocalRotationX(Transform transform, float duration, float end, TweenEase ease = TweenEase.Linear)
        {
            Quaternion r = transform.localRotation;  r.x = end;
            return DoLocalRotation(transform,duration,r,ease);
        }
        public static TweenTask DoLocalRotationY(Transform transform, float duration, float end, TweenEase ease = TweenEase.Linear)
        {
            Quaternion r = transform.localRotation; r.y = end;
            return DoLocalRotation(transform, duration, r, ease);
        }

        public static TweenTask DoLocalRotationZ(Transform transform, float duration, float end, TweenEase ease = TweenEase.Linear)
        {
            Quaternion r = transform.localRotation; r.z = end;
            return DoLocalRotation(transform, duration, r, ease);
        }

        /// <summary>
        /// 对缩放进行"击打动画"缩放为1，localScale
        /// </summary>
        /// <param name="force">施加的力的大小[10,100]</param>
        public static TweenTask DoPunch(Transform transform, float duration, float force = 30)
        {
            Action<float,int,int> customTween = (time,loopNum,pingPong) =>
            {
                float v = TweenComputer.Vibrate(time, duration, 1, force);
                transform.localScale = new Vector3(v, v, 1);
                if(loopNum>0 && time == duration) { transform.localScale = Vector3.one; }
            };
            return new TweenCustom(customTween,duration,TweenEase.Linear);
        }

        /// <summary>
        /// 对位置进行晃动
        /// </summary>
        /// <param name="offset">晃动的最大偏移距，默认25</param>
        /// <param name="force">晃动的力的大小[10,100]</param>
        /// <returns></returns>
        public static TweenTask DoShake(Transform transform, float duration, float offset = 25, float force = 40)
        {
            Vector3 start = transform.localPosition;

            Action<float, int, int> customTween = (time, loopNum, pingPong) =>
            {
                Vector3 localPos = transform.localPosition;
                float v = TweenComputer.Vibrate(time, duration, offset, force) - offset;
                localPos.x = start.x + v;
                localPos.y = start.y + v;
                localPos.z = start.z + v;
                transform.localPosition = localPos;
                if (loopNum > 0 && time == duration) { transform.localPosition = start; }
            };

            return new TweenCustom(customTween,duration, TweenEase.Linear);
        }

        public static TweenTask Typewriter(TMP_Text tmpText,string text, float duration, TweenEase ease = TweenEase.Linear)
        {
            TweenText tween = new TweenText(duration, ease);
            tween.Text(tmpText, text);
            return tween;
        }

        public static TweenTask DoFade(Image image,float duration,float end, TweenEase ease = TweenEase.Linear)
        {
            Color endColor = image.color; endColor.a = end;
            return DoColor(image, duration, endColor, ease);
        }

        public static TweenTask DoColor(Image image,float duration, Color end, TweenEase ease = TweenEase.Linear)
        {
            TweenImage tween = new TweenImage(duration, ease);
            tween.Color(image, end);
            return tween;
        }

        /// <summary>
        /// ScrollRect滚动到指定位置
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static TweenTask DoScroll(ScrollRect scrollRect, float duration, Vector2 end)
        {
            TweenScrollRect tween = new TweenScrollRect(duration, TweenEase.Linear);
            tween.Scroll(scrollRect, end);
            return tween;
        }

    }
}
