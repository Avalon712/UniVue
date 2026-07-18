#region

using System;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UniVue.UI.Widgets;
using Object = UnityEngine.Object;

#endregion

namespace UniVue.Editor
{
    /// <summary>
    /// 在 Hierarchy / GameObject → UI 下注册可创建的 UniVue UI 组件。
    /// 优先级低于官方 UI（约 2000），会显示在菜单最上方，并与自带项自动隔开。
    /// </summary>
    public static class UniVueUICreateRegistry
    {
        private const string MenuRoot = "GameObject/UI/";

        /// <summary>
        /// 官方 Image/TMP 从约 2000 起；priority 相差超过 10 会插入分隔线。
        /// 后续组件用 11、12… 紧挨排列。
        /// </summary>
        private const int MenuPriority = 10;

        [MenuItem(MenuRoot + "Localization Text (UniVue)", false, MenuPriority)]
        public static void CreateUI_LocalizationText(MenuCommand menuCommand)
        {
            GameObject go = ObjectFactory.CreateGameObject(
                "LocalizationText",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(TextMeshProUGUI),
                typeof(LocalizationText));

            TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = "Localization Text";
            tmp.raycastTarget = false;
            if (TMP_Settings.instance != null)
                tmp.fontSize = TMP_Settings.defaultFontSize > 0 ? TMP_Settings.defaultFontSize : 36f;
            else
                tmp.fontSize = 36f;

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(200f, 50f);

            // Inspector 中排到 Transform/RectTransform 正下方
            MoveComponentJustBelowTransform(go.GetComponent<LocalizationText>());

            PlaceUIElementRoot(go, menuCommand);
        }

        /// <summary>
        /// 将组件上移到仅次于 Transform / RectTransform 的位置。
        /// </summary>
        private static void MoveComponentJustBelowTransform(Component component)
        {
            if (component == null) return;

            Component[] components = component.gameObject.GetComponents<Component>();
            // index 0 一般为是 Transform/RectTransform，目标是把 component 移到 index 1
            int index = Array.IndexOf(components, component);
            while (index > 1)
            {
                ComponentUtility.MoveComponentUp(component);
                components = component.gameObject.GetComponents<Component>();
                index = Array.IndexOf(components, component);
            }
        }

        private static void PlaceUIElementRoot(GameObject element, MenuCommand menuCommand)
        {
            GameObject parent = menuCommand.context as GameObject;
            bool explicitParentChoice = parent != null;

            if (parent == null)
            {
                parent = GetOrCreateCanvasGameObject();

                PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
                if (prefabStage != null && !prefabStage.IsPartOfPrefabContents(parent))
                    parent = prefabStage.prefabContentsRoot;
            }

            if (parent.GetComponentsInParent<Canvas>(true).Length == 0)
            {
                GameObject canvas = CreateNewUI();
                Undo.SetTransformParent(canvas.transform, parent.transform, "Reparent Canvas");
                parent = canvas;
            }

            GameObjectUtility.EnsureUniqueNameForSibling(element);
            SetParentAndAlign(element, parent);

            if (!explicitParentChoice)
                SetPositionCentered(parent.GetComponent<RectTransform>(), element.GetComponent<RectTransform>());

            Undo.RegisterCreatedObjectUndo(element, "Create " + element.name);
            Selection.activeGameObject = element;
        }

        private static void SetParentAndAlign(GameObject child, GameObject parent)
        {
            if (parent == null) return;

            Undo.SetTransformParent(child.transform, parent.transform, "Parent " + child.name);

            RectTransform rect = child.transform as RectTransform;
            if (rect != null)
            {
                rect.anchoredPosition = Vector2.zero;
                rect.localRotation = Quaternion.identity;
                rect.localScale = Vector3.one;
            }
            else
            {
                child.transform.localPosition = Vector3.zero;
                child.transform.localRotation = Quaternion.identity;
                child.transform.localScale = Vector3.one;
            }

            child.layer = parent.layer;
        }

        private static void SetPositionCentered(RectTransform canvasRect, RectTransform itemTransform)
        {
            if (canvasRect == null || itemTransform == null) return;
            itemTransform.anchoredPosition = Vector2.zero;
            itemTransform.localRotation = Quaternion.identity;
            itemTransform.localScale = Vector3.one;
        }

        private static GameObject CreateNewUI()
        {
            GameObject canvas = ObjectFactory.CreateGameObject("Canvas", typeof(Canvas), typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            canvas.layer = LayerMask.NameToLayer("UI");

            Canvas c = canvas.GetComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;

            StageUtility.PlaceGameObjectInCurrentStage(canvas);
            Undo.RegisterCreatedObjectUndo(canvas, "Create " + canvas.name);

            CreateEventSystem();
            return canvas;
        }

        private static void CreateEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() != null) return;

            GameObject eventSystem =
                ObjectFactory.CreateGameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            StageUtility.PlaceGameObjectInCurrentStage(eventSystem);
            Undo.RegisterCreatedObjectUndo(eventSystem, "Create " + eventSystem.name);
        }

        private static GameObject GetOrCreateCanvasGameObject()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected != null)
            {
                Canvas canvas = selected.GetComponentInParent<Canvas>();
                if (canvas != null && canvas.gameObject.activeInHierarchy) return canvas.gameObject;
            }

            Canvas[] canvases = Object.FindObjectsOfType<Canvas>();
            for (int i = 0; i < canvases.Length; i++)
                if (canvases[i].gameObject.activeInHierarchy)
                    return canvases[i].gameObject;

            return CreateNewUI();
        }
    }
}