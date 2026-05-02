using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UniVue.UI.Widgets;

namespace UniVue.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Loopable), true)]
    internal sealed class LoopableEditor : UnityEditor.Editor
    {
        private LoopItem Prefab
        {
            get
            {
                if (target is Loopable loopable && loopable.Scroller.content.childCount > 0)
                {
                    return loopable.Scroller.content.GetChild(0).GetComponent<LoopItem>();
                }

                return null;
            }
        }

        public override void OnInspectorGUI()
        {
            if (Application.isPlaying) return;

            LoopItem prefab = Prefab;

            if (!prefab)
            {
                Debug.LogWarning("请在ScrollRect的content中创建至少一个Item");
                return;
            }

            Loopable loopable = (Loopable)target;

            Rect rect = loopable.Scroller.viewport.rect;
            Vector2 viewport = new(rect.width, rect.height);
            Vector2 cellSize = (prefab?.transform as RectTransform)?.sizeDelta ?? Vector2.one;

            ScrollDirection scrollDir = ReflectionUtils.GetFieldValue<ScrollDirection>(target, "scrollDir");
            Vector2 gap = ReflectionUtils.GetFieldValue<Vector2>(target, "gap");
            Vector2Int gridSize = ReflectionUtils.GetFieldValue<Vector2Int>(target, "grid");

            scrollDir = (ScrollDirection)EditorGUILayout.EnumPopup("Scroll Direction", scrollDir);

            if (target is LoopList list)
            {
                if (scrollDir == ScrollDirection.Vertical)
                {
                    gap.x = 0;
                    gap.y = EditorGUILayout.FloatField("Gap", gap.y);
                    gridSize.x = 1;
                    gridSize.y = Mathf.CeilToInt((viewport.y + gap.y) / (cellSize.y + gap.y)) + 1;
                }
                else
                {
                    gap.y = 0;
                    gap.x = EditorGUILayout.FloatField("Gap", gap.x);
                    gridSize.y = 1;
                    gridSize.x = Mathf.CeilToInt((viewport.x + gap.x) / (cellSize.x + gap.x)) + 1;
                }
            }
            else if (target is LoopGrid grid)
            {
                gap = EditorGUILayout.Vector2Field("Gap", gap);

                if (scrollDir == ScrollDirection.Vertical)
                {
                    gridSize.x = Mathf.FloorToInt((viewport.x + gap.x) / (cellSize.x + gap.x));
                    gridSize.y = Mathf.CeilToInt((viewport.y + gap.y) / (cellSize.y + gap.y)) + 1;
                }
                else
                {
                    gridSize.x = Mathf.CeilToInt((viewport.x + gap.x) / (cellSize.x + gap.x)) + 1;
                    gridSize.y = Mathf.FloorToInt((viewport.y + gap.y) / (cellSize.y + gap.y));
                }
            }

            if (GUI.changed)
            {
                ReflectionUtils.SetFieldValue(target, "scrollDir", scrollDir);
                ReflectionUtils.SetFieldValue(target, "gap", gap);
                ReflectionUtils.SetFieldValue(target, "grid", gridSize);
                MarkLoopableSerializedDirty(loopable);
            }

            if (loopable.gameObject.scene.IsValid() && GUILayout.Button("Create Items") && prefab)
            {
                CreateItems(gridSize, loopable);
                LayoutItems(gridSize, scrollDir, gap, loopable);
                EditorSceneManager.MarkSceneDirty(loopable.gameObject.scene);
                MarkLoopableSerializedDirty(loopable);
                MarkContentChildrenDirty(loopable);
            }

            SyncScrollAxes(loopable, scrollDir);
        }

        private static void SyncScrollAxes(Loopable loopable, ScrollDirection scrollDir)
        {
            ScrollRect sr = loopable.Scroller;
            if (!sr) return;

            bool vertical = scrollDir == ScrollDirection.Vertical;
            if (sr.vertical == vertical && sr.horizontal != vertical) return;

            sr.vertical = vertical;
            sr.horizontal = !vertical;
            EditorUtility.SetDirty(sr);
            if (PrefabUtility.IsPartOfPrefabInstance(sr))
                PrefabUtility.RecordPrefabInstancePropertyModifications(sr);
        }

        private static void MarkLoopableSerializedDirty(Loopable loopable)
        {
            if (!loopable) return;

            EditorUtility.SetDirty(loopable);
            ScrollRect sr = loopable.Scroller;
            if (!sr) return;

            EditorUtility.SetDirty(sr);
            if (sr.content) EditorUtility.SetDirty(sr.content);
            if (sr.viewport) EditorUtility.SetDirty(sr.viewport);

            if (PrefabUtility.IsPartOfPrefabInstance(loopable))
                PrefabUtility.RecordPrefabInstancePropertyModifications(loopable);
            if (PrefabUtility.IsPartOfPrefabInstance(sr))
                PrefabUtility.RecordPrefabInstancePropertyModifications(sr);
            if (sr.content && PrefabUtility.IsPartOfPrefabInstance(sr.content))
                PrefabUtility.RecordPrefabInstancePropertyModifications(sr.content);
        }

        private static void MarkContentChildrenDirty(Loopable loopable)
        {
            Transform content = loopable.Scroller.content;
            if (!content) return;

            for (int i = 0; i < content.childCount; i++)
            {
                GameObject child = content.GetChild(i).gameObject;
                EditorUtility.SetDirty(child);
                if (PrefabUtility.IsPartOfPrefabInstance(child))
                    PrefabUtility.RecordPrefabInstancePropertyModifications(child);
            }
        }

        private void CreateItems(Vector2Int gridSize, Loopable loopable)
        {
            int count = gridSize.x * gridSize.y;
            RectTransform content = loopable.Scroller.content;
            int needCount = count - content.childCount;
            if (count <= 0) return;

            if (needCount > 0)
            {
                GameObject prefab = Prefab.gameObject;
                for (int i = 0; i < needCount; i++)
                {
                    CloneUtils.RectTransformClone(prefab, content);
                }
            }
            else if (needCount < 0) //删除多余的
            {
                for (int i = needCount; i < 0; i++)
                {
                    DestroyImmediate(content.GetChild(0).gameObject);
                }
            }
        }

        private void LayoutItems(Vector2Int gridSize, ScrollDirection direction, Vector2 gap, Loopable loopable)
        {
            RectTransform content = loopable.Scroller.content;
            Vector2 itemPos = Vector2.zero;
            Vector2 cellSize = (content.GetChild(0) as RectTransform)?.sizeDelta ?? Vector2.one;
            int cols = gridSize.x;
            int rows = gridSize.y;
            float xDeltaPos = cellSize.x + gap.x;
            float yDeltaPos = cellSize.y + gap.y;

            string name = target.GetType().Name;
            int index = 0;

            if (direction == ScrollDirection.Vertical)
            {
                content.anchorMin = new Vector2(0, 1);
                content.anchorMax = new Vector2(1, 1);
                content.pivot = new Vector2(0.5f, 1);
            }
            else
            {
                content.anchorMin = new Vector2(0, 0);
                content.anchorMax = new Vector2(0, 1);
                content.pivot = new Vector2(0, 0.5f);
            }

            content.anchoredPosition = Vector2.zero;

            foreach (RectTransform rectTransform in content)
            {
                rectTransform.anchorMin = new Vector2(0, 1);
                rectTransform.anchorMax = new Vector2(0, 1);
                rectTransform.pivot = new Vector2(0, 1);
                rectTransform.name = $"{name}_{index++}";
            }

            Vector2 distance = cellSize * (gap / Vector2.Max(gap, Vector2.one)) + gap;
            float temp = direction == ScrollDirection.Vertical ? rows : cols;
            content.sizeDelta = temp * distance * new Vector2(direction == ScrollDirection.Vertical ? 0 : 1,
                                                              direction == ScrollDirection.Vertical ? 1 : 0);

            //按下面的方法确保content的前面rows个为第一列或前cols个为第一行
            if (direction == ScrollDirection.Vertical) //垂直滚动时位置按行一行一行的设置
            {
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        (content.GetChild(i * cols + j) as RectTransform).anchoredPosition = itemPos;
                        itemPos.x += xDeltaPos;
                    }

                    itemPos.y -= yDeltaPos; //下一行
                    itemPos.x -= xDeltaPos * cols;
                }
            }
            else //水平滚动时位置按列一列一列的设置
            {
                for (int i = 0; i < cols; ++i)
                {
                    for (int j = 0; j < rows; ++j)
                    {
                        (content.GetChild(i * rows + j) as RectTransform).anchoredPosition = itemPos;
                        itemPos.y -= yDeltaPos;
                    }

                    itemPos.x += xDeltaPos; //下一列
                    itemPos.y += yDeltaPos * rows;
                }
            }
        }
    }
}