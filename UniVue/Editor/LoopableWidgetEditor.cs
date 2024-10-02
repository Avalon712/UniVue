using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UniVue.Common;
using UniVue.Widgets;

namespace UniVue.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Loopable), true)]
    internal sealed class LoopableWidgetEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Create Items"))
            {
                if (target is LoopList list)
                    CreateItems(list);
                else if (target is LoopGrid grid)
                    CreateItems(grid);
            }
            Settings(target as Loopable);
        }

        private void Settings(Loopable loop)
        {
            ScrollRect scrollRect = loop.scrollRect;
            if (scrollRect == null) return;

            ScrollDirection direction = ReflectionUtils.GetFieldValue<ScrollDirection>(loop, "_direction");
            RectTransform content = scrollRect.content;
            if (direction == ScrollDirection.Horizontal)
            {
                scrollRect.horizontal = true;
                scrollRect.vertical = false;
                content.anchorMin = new Vector2(0, 0);
                content.anchorMax = new Vector2(0, 1);
                content.pivot = new Vector2(0, 0.5f);
            }
            else
            {
                scrollRect.horizontal = false;
                scrollRect.vertical = true;
                content.anchorMin = new Vector2(0, 1);
                content.anchorMax = new Vector2(1, 1);
                content.pivot = new Vector2(0.5f, 1f);
            }

            if (loop is LoopList)
            {
                bool unlimitScroll = ReflectionUtils.GetFieldValue<bool>(loop, "_unlimitScroll");
                scrollRect.movementType = unlimitScroll ? ScrollRect.MovementType.Unrestricted : ScrollRect.MovementType.Elastic;
            }

            if (content.childCount == 1)
            {
                (content.GetChild(0) as RectTransform).anchoredPosition = Vector2.zero;
            }
        }

        private void CreateItems(LoopList list)
        {
            ScrollRect scrollRect = list.scrollRect;
            Transform content = scrollRect.content;

            if (content.childCount >= 1)
            {
                ScrollDirection direction = ReflectionUtils.GetFieldValue<ScrollDirection>(list, "_direction");
                int viewCount = ReflectionUtils.GetFieldValue<int>(list, "_viewCount");
                float distance = ReflectionUtils.GetFieldValue<float>(list, "_distance");
                Vector2 deltaPos = direction == ScrollDirection.Vertical ? new Vector3(0, distance, 0) : new Vector3(distance, 0, 0);
                int singal = direction == ScrollDirection.Vertical ? 1 : -1;
                GameObject firstItem = content.GetChild(0).gameObject;
                Vector2 firstPos = (firstItem.transform as RectTransform).anchoredPosition;

                (firstItem.transform as RectTransform).anchoredPosition = Vector2.zero;
                SetName("LoopList", firstItem);
                SetItemAnchorPos(firstItem.transform as RectTransform);

                scrollRect.content.sizeDelta = deltaPos * (viewCount + 1);
                //创建Item
                for (int i = 1; i <= viewCount; i++)
                {
                    GameObject itemViewObject = GameObjectUtil.RectTransformClone(firstItem, content);
                    SetName("LoopList", itemViewObject);
                    (content.GetChild(i) as RectTransform).anchoredPosition = firstPos - singal * i * deltaPos;
                }
            }
        }

        private void CreateItems(LoopGrid grid)
        {
            ScrollRect scrollRect = grid.scrollRect;
            Transform content = scrollRect.content;

            if (content.childCount >= 1)
            {
                ScrollDirection direction = ReflectionUtils.GetFieldValue<ScrollDirection>(grid, "_direction");
                int rows = ReflectionUtils.GetFieldValue<int>(grid, "_rows");
                int cols = ReflectionUtils.GetFieldValue<int>(grid, "_cols");
                float xDeltaPos = ReflectionUtils.GetFieldValue<float>(grid, "_xDeltaPos");
                float yDeltaPos = ReflectionUtils.GetFieldValue<float>(grid, "_yDeltaPos");

                GameObject firstItem = content.GetChild(0).gameObject;
                (firstItem.transform as RectTransform).anchoredPosition = Vector2.zero;
                SetName("LoopGrid", firstItem);
                SetItemAnchorPos(firstItem.transform as RectTransform);

                Vector3 deltaPos = direction == ScrollDirection.Vertical ? new Vector2(0, Mathf.Abs(yDeltaPos)) : new Vector2(Mathf.Abs(xDeltaPos), 0);
                float temp = direction == ScrollDirection.Vertical ? rows : cols;
                scrollRect.content.sizeDelta = (temp + 1) * deltaPos;

                int iMax = direction == ScrollDirection.Vertical ? rows : cols;
                int jMax = direction == ScrollDirection.Vertical ? cols : rows;

                for (int i = 0; i <= iMax; i++)//垂直滚动多一行
                {
                    for (int j = 0; j < jMax; j++)
                    {
                        GameObject itemViewObject = i + j == 0 ? firstItem : GameObjectUtil.RectTransformClone(firstItem, content);
                        SetName("LoopGrid", itemViewObject);
                    }
                }

                ReflectionUtils.InvokeMethod(grid, "ResetItemPos", Vector2.zero);
            }
        }

        private void SetItemAnchorPos(RectTransform rect)
        {
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
        }

        private void SetName(string prefix, GameObject item)
        {
            item.name = $"{prefix}-{((int)(Random.value * Random.Range(100_000, 111_111))).ToString("X")}-ItemView";
        }
    }
}
