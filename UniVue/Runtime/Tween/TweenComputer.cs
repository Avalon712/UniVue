using UnityEngine;

namespace UniVue.Tween
{
    /// <summary>
    /// 利用多核CPU并行计算
    /// </summary>
    public sealed class TweenComputer
    {
        private TweenComputer() { }

        /// <summary>
        /// 单值计算
        /// <para>【time】时间</para>
        /// <para>【duration】动画持续时间</para>
        /// <para>【start】起始值</para>
        /// <para>【distance】变化值=end-start (对于Punch，该值为力的大小)</para>
        /// </summary>
        public static float Compute(TweenEase ease, float time, float duration, float start, float distance)
        {
            switch (ease)
            {
                case TweenEase.Linear:
                    return Linear(time, duration, start, distance);
                case TweenEase.InQuad:
                    return InQuad(time, duration, start, duration);
                case TweenEase.OutQuad:
                    return OutQuad(time, duration, start, distance);
                case TweenEase.InOutQuad:
                    return InOutQuad(time, duration, start, distance);
                case TweenEase.InSin:
                    return InSin(time, duration, start, distance);
                case TweenEase.OutSin:
                    return OutSin(time, duration, start, distance);
                case TweenEase.InOutSin:
                    return InOutSin(time, duration, start, distance);
                case TweenEase.InExp:
                    return InExp(time, duration, start, distance);
                case TweenEase.OutExp:
                    return OutExp(time, duration, start, distance);
                case TweenEase.InOutExp:
                    return InOutExp(time, duration, start, distance);
                case TweenEase.InElastic:
                    return InElastic(time, duration, start, distance);
                case TweenEase.OutElastic:
                    return OutElastic(time, duration, start, distance);
                case TweenEase.InOutElastic:
                    return InOutElastic(time, duration, start, distance);
                case TweenEase.InBounce:
                    return InBounce(time, duration, start, distance);
                case TweenEase.OutBounce:
                    return OutBounce(time, duration, start, distance);
                case TweenEase.InOutBounce:
                    return InOutBounce(time, duration, start, distance);
                case TweenEase.InCirc:
                    return InCirc(time, duration, start, distance);
                case TweenEase.OutCirc:
                    return OutCirc(time, duration, start, duration);
                case TweenEase.InOutCirc:
                    return InOutCirc(time, duration, start, duration);
                case TweenEase.Vibrate:
                    return Vibrate(time, duration, start, distance);
                case TweenEase.OnePunch:
                    return OnePunch(time, duration, start, distance);
            }

            return start;
        }

        #region 内部函数实现

        public static float OnePunch(float time, float duration, float start, float distance)
        {
            if (distance > 0)
            {
                float x = time / duration;
                float y = -1.76f * x * x + 2.76f * x;
                return y * distance + start;
            }
            else
            {
                float x = 1 - time / duration;
                float y = -1.76f * x * x + 2.76f * x;
                return y * start;
            }
        }

        public static float Vibrate(float time, float duration, float start,float force)
        {
            float x = time / duration;
            if (x == 1) return start;
            return start*Mathf.Exp(-5*x)*Mathf.Sin(force*x)+start;
        }

        public static float Linear(float time, float duration, float start, float distance)
        {
            return distance * time / duration + start;
        }

        public static float InQuad(float time, float duration, float start, float distance)
        {
            return distance * (time /= duration) * time + start;
        }

        public static float OutQuad(float time, float duration, float start, float distance)
        {
            return -distance * (time /= duration) * (time - 2) + start;
        }

        public static float InOutQuad(float time, float duration, float start, float distance)
        {
            if ((time /= duration / 2) < 1) return distance / 2 * time * time + start;
            return -distance / 2 * (--time * (time - 2) - 1) + start;
        }

        public static float InSin(float time, float duration, float start, float distance)
        {
            return -distance * Mathf.Cos(time / duration * 1.5708f) + distance + start;
        }

        public static float OutSin(float time, float duration, float start, float distance)
        {
            return distance * Mathf.Sin(time / duration * 1.5708f) + start;
        }

        public static float InOutSin(float time, float duration, float start, float distance)
        {
            return -distance / 2 * (Mathf.Cos(Mathf.PI * time / duration) - 1) + start;
        }

        public static float InExp(float time, float duration, float start, float distance)
        {
            return time == 0 ? start : distance * Mathf.Pow(2, 10 * (time / duration - 1)) + start;
        }

        public static float OutExp(float time, float duration, float start, float distance)
        {
            return time == duration ? start + distance : distance * (-Mathf.Pow(2, -10 * time / duration) + 1) + start;
        }

        public static float InOutExp(float time, float duration, float start, float distance)
        {
            if (time == 0) return start;
            if (time == duration) return start + distance;
            if ((time /= duration / 2) < 1) return distance / 2 * Mathf.Pow(2, 10 * (time - 1)) + start;
            return distance / 2 * (-Mathf.Pow(2, -10 * --time) + 2) + start;
        }

        public static float InElastic(float time, float duration, float start, float distance)
        {
            if (time == 0 || distance == 0) return start;
            float s, p;
            float a = distance;
            if ((time /= duration) == 1) return start + distance;
            p = duration * 0.3f;
            if (a < Mathf.Abs(distance))
            {
                a = distance;
                s = p / 4;
            }
            else s = p / 6.28319f * Mathf.Asin(distance / a);
            return -(a * Mathf.Pow(2, 10 * (time -= 1)) * Mathf.Sin((time * duration - s) * 6.28319f / p)) + start;
        }

        public static float OutElastic(float time, float duration, float start, float distance)
        {
            if (time == 0 || distance == 0) return start;
            float s;
            float p = duration * 0.3f;
            float a = distance;
            if ((time /= duration) == 1) return start + distance;
            if (a < Mathf.Abs(distance))
            {
                a = distance;
                s = p / 4;
            }
            else s = p / 6.28319f * Mathf.Asin(distance / a);
            return (float)(a * Mathf.Pow(2, -10 * time) * Mathf.Sin((time * duration - s) * 6.28319f / p) + distance + start);
        }

        public static float InOutElastic(float time, float duration, float start, float distance)
        {
            if (time == 0 || distance == 0) return start;
            float s;
            float p = duration * (0.3f * 1.5f);
            float a = distance;
            if ((time /= duration / 2) == 2) return start + distance;
            if (a < Mathf.Abs(distance))
            {
                a = distance;
                s = p / 4;
            }
            else s = (float)(p / 6.28319f * Mathf.Asin(distance / a));
            if (time < 1) return -0.5f * (a * Mathf.Pow(2, 10 * (time -= 1)) * Mathf.Sin((time * duration - s) * 6.28319f / p)) + start;
            return (float)(a * Mathf.Pow(2, -10 * (time -= 1)) * Mathf.Sin((time * duration - s) * 6.283189f / p) * .5 + distance + start);
        }

        public static float InCirc(float time, float duration, float start, float distance)
        {
            return -distance * (Mathf.Sqrt(1 - (time /= duration) * time) - 1) + start;
        }

        public static float OutCirc(float time, float duration, float start, float distance)
        {
            return distance * Mathf.Sqrt(1 - (time = time / duration - 1) * time) + start;
        }

        public static float InOutCirc(float time, float duration, float start, float distance)
        {
            if ((time /= duration / 2) < 1) return -distance / 2 * (Mathf.Sqrt(1 - time * time) - 1) + start;
            return distance / 2 * (Mathf.Sqrt(1 - (time -= 2) * time) + 1) + start;
        }

        public static float InOutBounce(float time, float duration, float start, float distance)
        {
            if (time < duration / 2) return InBounce(time * 2, duration, 0, distance) * 0.5f + start;
            return OutBounce(time * 2 - duration, duration, 0, distance) * 0.5f + distance * 0.5f + start;
        }

        public static float OutBounce(float time, float duration, float start, float distance)
        {
            if ((time /= duration) < 0.363636)
            {
                return distance * (7.5625f * time * time) + start;
            }
            else if (time < 0.727273)
            {
                return distance * (7.5625f * (time -= 0.545455f) * time + 0.75f) + start;
            }
            else if (time < 0.909091)
            {
                return distance * (7.5625f * (time -= 0.818182f) * time + 0.9375f) + start;
            }
            else
            {
                return distance * (7.5625f * (time -= 0.954545f) * time + 0.984375f) + start;
            }

        }

        public static float InBounce(float time, float duration, float start, float distance)
        {
            return distance - OutBounce(duration - time, duration, 0, distance) + start;
        }

        #endregion
    }
}
