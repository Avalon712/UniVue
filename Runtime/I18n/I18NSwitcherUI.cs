#region

using System;
using UnityEngine;

#endregion

namespace UniVue.I18n
{
    /// <summary>
    /// 仅在编辑器模式下生效
    /// </summary>
    public sealed class I18NSwitcherUI : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField] private Rect _screenRect = new(-320f, 24f, 280f, 52f);
        [SerializeField] private bool _anchorTopRight = true;
        [SerializeField] private float _referenceHeight = 1080f;
        [SerializeField] private int _fontSize = 24;
        [SerializeField] private string[] _languages;
        private int _appliedFontSize = -1;
        private GUIStyle _boxStyle;
        private GUIStyle _buttonStyle;
        private string[] _cachedLanguages;

        private bool _expanded;
        private GUIStyle _itemStyle;
        private bool _switching;
        private int _switchVersion;

        private void OnEnable()
        {
            RebuildLanguageCache();
        }

        private void OnGUI()
        {
            EnsureStyles();

            if (_cachedLanguages == null || _cachedLanguages.Length == 0) RebuildLanguageCache();

            if (_cachedLanguages == null || _cachedLanguages.Length == 0) return;

            // 按参考分辨率缩放，避免 1080p/2K 下 IMGUI 过小
            float scale = Mathf.Max(0.75f, Screen.height / Mathf.Max(1f, _referenceHeight));
            Matrix4x4 previousMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1f));

            float logicalWidth = Screen.width / scale;
            string current = string.IsNullOrEmpty(I18N.Language) ? I18N.DefaultLanguage : I18N.Language;
            if (string.IsNullOrEmpty(current)) current = _cachedLanguages[0];

            float itemHeight = Mathf.Max(40f, _fontSize + 18f);
            float headerHeight = Mathf.Max(52f, _screenRect.height);
            float totalHeight = _expanded
                ? headerHeight + 8f + _cachedLanguages.Length * itemHeight
                : headerHeight;

            Rect area = GetDrawRect(logicalWidth, totalHeight);
            GUILayout.BeginArea(area, _boxStyle);

            string header = _switching ? $"{current} ..." : $"▾  {current}";
            if (GUILayout.Button(header, _buttonStyle, GUILayout.Height(headerHeight - 8f))) _expanded = !_expanded;

            if (_expanded)
            {
                GUILayout.Space(4f);
                for (int i = 0; i < _cachedLanguages.Length; i++)
                {
                    string lang = _cachedLanguages[i];
                    bool selected = lang == current;
                    string label = selected ? $"●  {lang}" : $"○  {lang}";

                    using (new GUIEnabledScope(!selected))
                    {
                        if (GUILayout.Button(label, _itemStyle, GUILayout.Height(itemHeight - 4f))) SwitchTo(lang);
                    }
                }
            }

            GUILayout.EndArea();
            GUI.matrix = previousMatrix;
        }

        private void RebuildLanguageCache()
        {
            if (_languages != null && _languages.Length > 0)
            {
                _cachedLanguages = _languages;
                return;
            }

            _cachedLanguages = CollectLanguagesFromConfig();
        }

        private static string[] CollectLanguagesFromConfig()
        {
            string[] result = new string[I18N.SupportedLanguages.Count];
            for (int i = 0; i < result.Length; i++) result[i] = I18N.SupportedLanguages[i];
            Array.Sort(result, StringComparer.Ordinal);
            return result;
        }

        private void EnsureStyles()
        {
            if (_buttonStyle == null)
            {
                _buttonStyle = new GUIStyle(GUI.skin.button)
                {
                    alignment = TextAnchor.MiddleLeft,
                    padding = new RectOffset(14, 14, 8, 8),
                    fontStyle = FontStyle.Bold
                };
                _itemStyle = new GUIStyle(GUI.skin.button)
                {
                    alignment = TextAnchor.MiddleLeft,
                    padding = new RectOffset(16, 14, 8, 8)
                };
                _boxStyle = new GUIStyle(GUI.skin.box)
                {
                    padding = new RectOffset(8, 8, 8, 8)
                };
            }

            if (_appliedFontSize != _fontSize)
            {
                _buttonStyle.fontSize = _fontSize;
                _itemStyle.fontSize = _fontSize;
                _appliedFontSize = _fontSize;
            }
        }

        private Rect GetDrawRect(float logicalScreenWidth, float height)
        {
            float width = _screenRect.width;
            float x = _screenRect.x;
            float y = _screenRect.y;

            if (_anchorTopRight)
            {
                x = logicalScreenWidth + _screenRect.x;
                if (x + width > logicalScreenWidth - 12f) x = logicalScreenWidth - width - 12f;
            }

            if (x < 12f) x = 12f;
            if (y < 12f) y = 12f;
            return new Rect(x, y, width, height);
        }

        private async void SwitchTo(string language)
        {
            if (string.IsNullOrEmpty(language)) return;
            if (language == I18N.Language)
            {
                _expanded = false;
                return;
            }

            int version = ++_switchVersion;
            _switching = true;
            bool success = await I18N.SwitchLanguageAsync(language);
            if (version != _switchVersion) return;

            _switching = false;
            if (success) _expanded = false;
        }

        private readonly struct GUIEnabledScope : IDisposable
        {
            private readonly bool _prev;

            public GUIEnabledScope(bool enabled)
            {
                _prev = GUI.enabled;
                GUI.enabled = enabled;
            }

            public void Dispose()
            {
                GUI.enabled = _prev;
            }
        }
    }
#endif
}