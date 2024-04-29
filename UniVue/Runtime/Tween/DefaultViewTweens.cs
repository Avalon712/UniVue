using UnityEngine;
using UniVue.Tween.Tweens;
using UniVue.Utils;

namespace UniVue.Tween
{
    /// <summary>
    /// 默认的一些动画效果
    /// </summary>
    public sealed class DefaultViewTweens
    {
        private DefaultViewTweens() { }

        /// <summary>
        /// 执行指定效果的缓动效果
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="tween"></param>
        /// <param name="duration">若小于0则使用默认的持续时间</param>
        /// <returns>TweenTask</returns>
        public static TweenTask ExecuteTween(Transform transform,DefaultTween tween,float duration)
        {
            switch (tween)
            {
                case DefaultTween.PunchOpen:
                    duration = duration < 0 ? 1.6f : duration;
                    PunchOpen(transform, duration);
                    break;
                case DefaultTween.QuickClose:
                    duration = duration < 0 ? 0.3f : duration;
                    QuickClose(transform, duration);
                    break;
                case DefaultTween.QuickOpen:
                    duration = duration < 0 ? 0.3f : duration;
                    QuickOpen(transform, duration);
                    break;
                case DefaultTween.FromOffScreenLeftOpen:
                    duration = duration < 0 ? 1.1f : duration;
                    FromOffScreenLeftOpen(transform, duration);
                    break;
                case DefaultTween.FromOffScreenRightOpen:
                    duration = duration < 0 ? 1.1f : duration;
                    FromOffScreenRightOpen(transform, duration);
                    break;
                case DefaultTween.FromOffScreenUpOpen:
                    duration = duration < 0 ? 1.1f : duration;
                    FromOffScreenUpOpen(transform, duration);
                    break;
                case DefaultTween.FromOffScreenDownOpen:
                    duration = duration < 0 ? 1.1f : duration;
                    FromOffScreenDownOpen(transform, duration);
                    break;
                case DefaultTween.ToOffScreenLeftClose:
                    duration = duration < 0 ? 1.1f : duration;
                    ToOffScreenLeftClose(transform, duration);
                    break;
                case DefaultTween.ToOffScreenRightClose:
                    duration = duration < 0 ? 1.1f : duration;
                    ToOffScreenRightClose(transform, duration);
                    break;
                case DefaultTween.ToOffScreenUpClose:
                    duration = duration < 0 ? 1.1f : duration;
                    ToOffScreenUpClose(transform, duration);
                    break;
                case DefaultTween.ToOffScreenDownClose:
                    duration = duration < 0 ? 1.1f : duration;
                    ToOffScreenDownClose(transform, duration);
                    break;
            }
            return null;
        }

        /// <summary>
        /// 像弹簧一样的效果打开UI视图
        /// </summary>
        public static TweenTask PunchOpen(Transform transform, float duration = 1.6f)
        {
            transform.localScale = new Vector3(0.5f, 0.5f, 1);
            SetGameObjectActive(transform.gameObject, true);
            return TweenBehavior.DoLocalScale(transform,duration, Vector3.one, TweenEase.OutElastic);
        }

        /// <summary>
        /// 快速打开UI视图
        /// </summary>
        public static TweenTask QuickOpen(Transform transform, float duration = 0.3f)
        {
            transform.localScale = new Vector3(0.5f, 0.5f, 1);
            SetGameObjectActive(transform.gameObject, true);
            return TweenBehavior.DoLocalScale(transform, duration, Vector2.one, TweenEase.OnePunch);
        }

        /// <summary>
        /// 快速关闭UI视图
        /// </summary>
        public static TweenTask QuickClose(Transform transform, float duration = 0.3f)
        {
            SetGameObjectActive(transform.gameObject, true);
            return TweenBehavior.DoLocalScale(transform, duration, new Vector3(0.5f, 0.5f, 1), TweenEase.OnePunch).Call(() =>
            {
                SetGameObjectActive(transform.gameObject, false); transform.localScale = Vector3.one;
            });
        }

        /// <summary>
        /// 从屏幕外(左)方向移动到UI视图位置进行打开
        /// </summary>
        public static TweenTask FromOffScreenLeftOpen(Transform transform, float duration = 1.1f)
        {
            return FromOffScreenOpen(transform, Direction.Left, duration);
        }

        /// <summary>
        /// 从屏幕外(右)方向移动到UI视图位置进行打开
        /// </summary>
        public static TweenTask FromOffScreenRightOpen(Transform transform, float duration = 1.1f)
        {
            return FromOffScreenOpen(transform, Direction.Right,duration);
        }

        /// <summary>
        /// 从屏幕外(上)方向移动到UI视图位置进行打开
        /// </summary>
        public static TweenTask FromOffScreenUpOpen(Transform transform, float duration = 1.1f)
        {
            return FromOffScreenOpen(transform, Direction.Up, duration);
        }

        /// <summary>
        /// 从屏幕外(下)方向移动到UI视图位置进行打开
        /// </summary>
        public static TweenTask FromOffScreenDownOpen(Transform transform, float duration = 1.1f)
        {
            return FromOffScreenOpen(transform, Direction.Down,duration);
        }

        /// <summary>
        /// 从UI视图位置移动到屏幕外(左)进行关闭
        /// </summary>
        public static TweenTask ToOffScreenLeftClose(Transform transform, float duration = 1.1f)
        {
            return ToOffScreenClose(transform, Direction.Left,duration);
        }

        /// <summary>
        /// 从UI视图位置移动到屏幕外(右)进行关闭
        /// </summary>
        public static TweenTask ToOffScreenRightClose(Transform transform, float duration = 1.1f)
        {
            return ToOffScreenClose(transform, Direction.Right,duration);
        }

        /// <summary>
        /// 从UI视图位置移动到屏幕外(上)进行关闭
        /// </summary>
        public static TweenTask ToOffScreenUpClose(Transform transform, float duration = 1.1f)
        {
            return ToOffScreenClose(transform, Direction.Up, duration);
        }

        /// <summary>
        /// 从UI视图位置移动到屏幕外(下)进行关闭
        /// </summary>
        public static TweenTask ToOffScreenDownClose(Transform transform, float duration = 1.1f)
        {
            return ToOffScreenClose(transform, Direction.Down,duration);
        }

        /// <summary>
        /// 从屏幕外的指定位置落到当前视图展示位置进行显示
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="dir">从屏幕外的那个位置（以UI原始位置外参考，上、下、左、右四个方向）</param>
        /// <param name="duration">动画时间</param>
        /// <returns>TweenTask</returns>
        private static TweenTask FromOffScreenOpen(Transform transform,Direction dir, float duration)
        {
            RectTransform rectTransform = transform.GetComponent<RectTransform>();
            Vector3 targetWorldPos = CalculateScreenEdgeWorldPos(rectTransform, dir);
            SetGameObjectActive(transform.gameObject, true);

            if (dir == Direction.Left || dir == Direction.Right)
            {
                float end = rectTransform.position.x;
                rectTransform.position = targetWorldPos;
                return TweenBehavior.DoMoveX(transform, duration, end, TweenEase.OutExp);
            }
            else
            {
                float end = rectTransform.position.y;
                rectTransform.position = targetWorldPos;
                return TweenBehavior.DoMoveY(transform, duration, end, TweenEase.OutExp);
            }
        }

        /// <summary>
        /// 当前视图展示位置移动到屏幕外指定位置进行关闭
        /// </summary>
        /// <param name="dir">从屏幕外的那个位置（以UI原始位置外参考，上、下、左、右四个方向）</param>
        /// <param name="duration">动画时间</param>
        /// <returns>TweenTask</returns>
        private static TweenTask ToOffScreenClose(Transform transform, Direction dir, float duration)
        {
            SetGameObjectActive(transform.gameObject, true);
            RectTransform rectTransform = transform.GetComponent<RectTransform>();
            Vector3 targetWorldPos = CalculateScreenEdgeWorldPos(rectTransform, dir);

            if (dir == Direction.Left || dir == Direction.Right)
            {
                float start = transform.position.x;
                return TweenBehavior.DoMoveX(transform, duration, targetWorldPos.x, TweenEase.OutExp).Call(() =>
                {
                    SetGameObjectActive(transform.gameObject, false);
                    transform.position = new Vector3(start, transform.position.y, transform.position.z);
                });
            }
            else
            {
                float start = transform.position.y;
                return TweenBehavior.DoMoveY(transform, duration, targetWorldPos.y, TweenEase.OutExp).Call(() =>
                {
                    SetGameObjectActive(transform.gameObject, false);
                    transform.position = new Vector3(transform.position.x, start, transform.position.z);
                });
            }
        }



        private static void SetGameObjectActive(GameObject obj,bool state)
        {
            if(state != obj.activeSelf)
            {
                obj.SetActive(state);
            }
        }


        /// <summary>
        /// 计算指定UI刚好处于屏幕外的世界坐标位置
        /// </summary>
        /// <param name="rectTransform"></param>
        /// <param name="dir">左、右、上、下四个位置</param>
        /// <returns>UI处于屏幕外的世界坐标</returns>
        public static Vector3 CalculateScreenEdgeWorldPos(RectTransform rectTransform,Direction dir)
        {
            Vector3 worldPos;

            //获取UI在屏幕空间中的坐标
            Vector2 screenPos = Camera.main.WorldToScreenPoint(rectTransform.position);

            //获取目标点的屏幕位置
            Vector2 targetScreenPos = Vector2.zero;
            switch (dir)
            {
                case Direction.Left:
                    targetScreenPos = new Vector2(-rectTransform.rect.width/2, screenPos.y);
                    break;
                case Direction.Right:
                    targetScreenPos = new Vector2(Screen.width+rectTransform.rect.width/2, screenPos.y);
                    break;
                case Direction.Up:
                    targetScreenPos = new Vector2(screenPos.x,Screen.height+rectTransform.rect.height/2);
                    break;
                case Direction.Down:
                    targetScreenPos = new Vector2(screenPos.x,-rectTransform.rect.height/2);
                    break;
            }

            //将目标点的屏幕位置转换为RectTransform下的世界坐标
            /*
             * 这儿的参数Camera参数是否填null是根据UI所在的Canvas的Render Mode确定的，
             * 只有当Render Mode 为 Screen Space - Overlay 模式时，参数应为 null。
             */
            bool isNull = ComponentFindUtil.LookUpFindComponent<Canvas>(rectTransform.gameObject)?.renderMode == RenderMode.ScreenSpaceOverlay;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTransform, targetScreenPos, isNull ? null : Camera.main, out worldPos);
            
            return worldPos;
        }
    }
}
