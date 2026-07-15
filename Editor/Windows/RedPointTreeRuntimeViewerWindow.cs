using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using UniVue.UI;

namespace UniVue.Editor
{
    public sealed class RedPointTreeRuntimeViewerWindow : EditorWindow
    {
        private const float LeftPanelWidth = 200f;
        private const float SplitWidth = 4f;
        private const float PmListRowHeight = 22f;
        private const float NodeRowSpacing = 0f;
        private readonly Dictionary<ulong, LeafAction> _leafActionSelections = new();
        private readonly Dictionary<ulong, NonLeafAction> _nonLeafActionSelections = new();
        private GUIStyle _boxStyle;

        private double _lastRepaintTime;
        private int _leftListHoverIndex = -1;
        private Vector2 _leftScroll;
        private GUIStyle _leftScrollStyle;
        private GUIStyle _pmListLabelStyle;
        private GUIStyle _richTextLabelStyle;
        private Vector2 _rightScroll;

        private RedPointMgr.RedPointNode _selectedRoot;
        private ulong _selectedRootKey;

        private void Update()
        {
            if (!EditorApplication.isPlaying) return;
            double now = EditorApplication.timeSinceStartup;
            if (now - _lastRepaintTime < 0.05) return; // 降低刷新频率，避免打断下拉框交互
            _lastRepaintTime = now;
            Repaint();
        }

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void OnGUI()
        {
            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("请在运行模式下使用此窗口", MessageType.Warning);
                return;
            }

            RedPointMgr mgr = UIMgr.RedPointMgr;
            if (mgr == null)
            {
                EditorGUILayout.HelpBox("RedPointMgr 未初始化", MessageType.Warning);
                return;
            }

            IReadOnlyDictionary<ulong, RedPointMgr.RedPointNode> trees = GetTreesViaReflection(mgr);
            if (trees == null || trees.Count == 0)
            {
                EditorGUILayout.HelpBox("暂无红点树数据", MessageType.Info);
                return;
            }

            if (_boxStyle == null)
                _boxStyle = new GUIStyle(GUI.skin.box) { padding = new RectOffset(8, 8, 8, 8) };
            if (_leftScrollStyle == null)
            {
                _leftScrollStyle = new GUIStyle(GUI.skin.box)
                    { padding = new RectOffset(0, 0, 0, 0), margin = new RectOffset(0, 0, 0, 0) };
            }

            if (_pmListLabelStyle == null)
            {
                _pmListLabelStyle = new GUIStyle(EditorStyles.label)
                {
                    padding = new RectOffset(12, 8, 2, 2),
                    alignment = TextAnchor.MiddleLeft,
                    clipping = TextClipping.Clip
                };
            }

            if (_richTextLabelStyle == null)
                _richTextLabelStyle = new GUIStyle(EditorStyles.label) { richText = true };

            EditorGUILayout.Space(4);
            EditorGUILayout.BeginHorizontal();

            DrawLeftPanel(trees);
            GUILayout.Space(SplitWidth);
            DrawRightPanel(mgr);

            EditorGUILayout.EndHorizontal();
        }

        private static IReadOnlyDictionary<ulong, RedPointMgr.RedPointNode> GetTreesViaReflection(RedPointMgr mgr)
        {
            if (mgr == null) return null;
            FieldInfo field = typeof(RedPointMgr).GetField("_trees", BindingFlags.NonPublic | BindingFlags.Instance);
            return field?.GetValue(mgr) as IReadOnlyDictionary<ulong, RedPointMgr.RedPointNode>;
        }

        [MenuItem("UniVue/Windows/RedPointTreeDebugger (Runtime)")]
        private static void ShowWindow()
        {
            if (!EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("提示", "红点树运行时查看器仅在游戏运行时可用", "确定");
                return;
            }

            GetWindow<RedPointTreeRuntimeViewerWindow>("红点树运行时查看");
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                _selectedRoot = null;
                _selectedRootKey = 0;
                _leafActionSelections.Clear();
                _nonLeafActionSelections.Clear();
            }
        }

        private void DrawLeftPanel(IReadOnlyDictionary<ulong, RedPointMgr.RedPointNode> trees)
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(LeftPanelWidth));

            EditorGUILayout.LabelField("红点树列表", EditorStyles.boldLabel);

            // 使用完整重载指定滚动条与背景样式，避免与 (scroll, bool, bool, ...) 等重载产生歧义
            _leftScroll = EditorGUILayout.BeginScrollView(
                                                          _leftScroll,
                                                          false,
                                                          false,
                                                          GUI.skin.horizontalScrollbar,
                                                          GUI.skin.verticalScrollbar,
                                                          _leftScrollStyle,
                                                          GUILayout.ExpandHeight(true));

            List<KeyValuePair<ulong, RedPointMgr.RedPointNode>> roots =
                trees.Where(kv => kv.Value != null).OrderBy(kv => kv.Key).ToList();

            int newHover = -1;
            for (int i = 0; i < roots.Count; i++)
            {
                KeyValuePair<ulong, RedPointMgr.RedPointNode> kv = roots[i];
                RedPointMgr.RedPointNode root = kv.Value;
                string keyName = root.keyName ?? $"0x{kv.Key:X}";
                bool selected = _selectedRoot == root;

                Rect rowRect = GUILayoutUtility.GetRect(0f, PmListRowHeight, GUILayout.ExpandWidth(true));

                if (UnityEngine.Event.current.type == EventType.MouseMove &&
                    rowRect.Contains(UnityEngine.Event.current.mousePosition))
                    newHover = i;

                switch (UnityEngine.Event.current.type)
                {
                    case EventType.MouseDown when UnityEngine.Event.current.button == 0 &&
                                                  rowRect.Contains(UnityEngine.Event.current.mousePosition):
                        _selectedRoot = root;
                        _selectedRootKey = kv.Key;
                        GUI.changed = true;
                        UnityEngine.Event.current.Use();
                        break;
                    case EventType.Repaint:
                        DrawPackageManagerListRow(rowRect, selected, _leftListHoverIndex == i);
                        GUI.Label(rowRect, new GUIContent(keyName), _pmListLabelStyle);
                        break;
                }
            }

            if (UnityEngine.Event.current.type == EventType.MouseMove)
            {
                if (newHover != _leftListHoverIndex)
                {
                    _leftListHoverIndex = newHover;
                    Repaint();
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 与 Package Manager 侧栏接近的行背景：选中高亮、悬停、底部分隔线（无左侧图标）。
        /// </summary>
        private static void DrawPackageManagerListRow(Rect rowRect, bool selected, bool hover)
        {
            bool pro = EditorGUIUtility.isProSkin;
            if (selected)
            {
                Color bg = pro
                    ? new Color(44f / 255f, 93f / 255f, 135f / 255f, 1f)
                    : new Color(62f / 255f, 125f / 255f, 231f / 255f, 1f);
                EditorGUI.DrawRect(rowRect, bg);
            }
            else if (hover)
            {
                Color bg = pro ? new Color(0.22f, 0.22f, 0.22f, 1f) : new Color(0.78f, 0.78f, 0.78f, 1f);
                EditorGUI.DrawRect(rowRect, bg);
            }

            Color line = pro ? new Color(0.12f, 0.12f, 0.12f, 1f) : new Color(0.65f, 0.65f, 0.65f, 1f);
            EditorGUI.DrawRect(new Rect(rowRect.x, rowRect.yMax - 1f, rowRect.width, 1f), line);
        }

        private void DrawRightPanel(RedPointMgr mgr)
        {
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));

            if (_selectedRoot != null)
            {
                EditorGUILayout.LabelField($"红点树结构: {_selectedRoot.keyName ?? "未命名"}", EditorStyles.boldLabel);

                _rightScroll = EditorGUILayout.BeginScrollView(_rightScroll, _boxStyle, GUILayout.ExpandHeight(true));

                DrawNode(mgr, _selectedRoot, "", true, true);

                EditorGUILayout.EndScrollView();
            }
            else
            {
                EditorGUILayout.BeginVertical(_boxStyle, GUILayout.ExpandHeight(true));
                EditorGUILayout.HelpBox("请在左侧选择一棵红点树", MessageType.None);
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawNode(RedPointMgr mgr, RedPointMgr.RedPointNode node, string prefix, bool isLast, bool isRoot)
        {
            if (node == null) return;

            EditorGUILayout.Space(NodeRowSpacing);
            EditorGUILayout.BeginHorizontal();

            // 树形连接符
            string connector = isRoot ? "" : isLast ? "└─ " : "├─ ";
            string prefixAndConnector = prefix + connector;
            if (!string.IsNullOrEmpty(prefixAndConnector))
            {
                float w = EditorStyles.label.CalcSize(new GUIContent(prefixAndConnector)).x;
                EditorGUILayout.LabelField(prefixAndConnector, GUILayout.Width(w));
            }

            // 只读 Toggle 显示红点状态（含强制设置）
            bool displayStatus = mgr.GetStatus(node.key);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.Toggle(displayStatus, GUILayout.Width(18));
            EditorGUI.EndDisabledGroup();

            bool isLeaf = node.IsLeaf;

            // 节点名称、红点规则、Force 标签用单个富文本 Label 显示
            string keyName = node.keyName ?? $"<color=#D2D2D2>0x{node.key:X}</color>";
            StringBuilder sb = new();
            sb.Append(keyName);
            if (!isLeaf)
                sb.Append(" <color=#33A659>[").Append(node.rule.ToString()).Append("]</color>");

            if (mgr.IsForceStatus(node.key))
                sb.Append("       <color=#D4A012><b>[Force]</b></color>");

            EditorGUILayout.LabelField(sb.ToString(), _richTextLabelStyle, GUILayout.MinWidth(100));

            // 下拉框 + 动作按钮（持久化选中值，避免每帧重绘时被重置）
            if (isLeaf)
            {
                if (!_leafActionSelections.TryGetValue(node.key, out LeafAction current))
                    current = LeafAction.Active;
                LeafAction selected = (LeafAction)EditorGUILayout.EnumPopup(current, GUILayout.Width(60));
                _leafActionSelections[node.key] = selected;
                if (GUILayout.Button("执行", GUILayout.Width(40))) mgr.SetActive(node.key, selected == LeafAction.Active);
            }
            else
            {
                if (!_nonLeafActionSelections.TryGetValue(node.key, out NonLeafAction current))
                    current = NonLeafAction.ForceActive;
                NonLeafAction selected = (NonLeafAction)EditorGUILayout.EnumPopup(current, GUILayout.Width(100));
                _nonLeafActionSelections[node.key] = selected;
                if (GUILayout.Button("执行", GUILayout.Width(40)))
                {
                    if (selected == NonLeafAction.DeleteForceStatus)
                        mgr.DeleteForceStatus(node.key);
                    else
                        mgr.ForceSetActive(node.key, selected == NonLeafAction.ForceActive);
                }
            }

            // 添加孩子节点按钮 "+"
            if (GUILayout.Button("+", GUILayout.Width(24))) mgr.AddDependency(node.key);

            // 删除节点按钮 "-"，仅当 IsDynamicDependency 为 true 时显示
            if (mgr.IsDynamicDependency(node.key))
            {
                if (GUILayout.Button("-", GUILayout.Width(24)))
                    mgr.DeleteDependency(node.key);
            }

            EditorGUILayout.EndHorizontal();

            // 子节点
            string childPrefix = isRoot ? "" : prefix + (isLast ? "   " : "│  ");
            List<RedPointMgr.RedPointNode> children = node.children;
            if (children != null && children.Count > 0)
            {
                for (int i = 0; i < children.Count; i++)
                    DrawNode(mgr, children[i], childPrefix, i == children.Count - 1, false);
            }
        }

        // 叶子节点下拉选项
        private enum LeafAction
        {
            Active,
            Inactive
        }

        // 非叶子节点下拉选项
        private enum NonLeafAction
        {
            ForceActive,
            ForceInactive,
            DeleteForceStatus
        }
    }
}