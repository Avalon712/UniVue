#region

using System;
using System.Collections.Generic;
using UnityEngine;

#endregion

namespace UniVue.Editor
{
    public static class UniVueEditorPrefs
    {
        private static Dictionary<string, string> _prefs;
        private static bool _dirty;

        private static void TryLoadPrefs()
        {
            if (_prefs == null)
            {
                string prefs = UniVueEditorSettings.instance.prefs;
                if (string.IsNullOrEmpty(prefs))
                {
                    _prefs = new Dictionary<string, string>();
                }
                else
                {
                    PrefsCollection collection = JsonUtility.FromJson<PrefsCollection>(prefs);
                    _prefs = collection.ToDictionary();
                }
            }
        }

        public static void Save()
        {
            if (_prefs == null || !_dirty) return;
            PrefsCollection prefs = PrefsCollection.FromDictionary(_prefs);
            UniVueEditorSettings.instance.prefs = JsonUtility.ToJson(prefs);
            UniVueEditorSettings.instance.SaveSettings();
            _dirty = false;
        }

        public static bool DeleteKey(string key)
        {
            _dirty = true;
            return _prefs.Remove(key);
        }

        public static bool ContainsKey(string key)
        {
            TryLoadPrefs();
            return _prefs.ContainsKey(key);
        }

        public static bool TryGetString(string key, out string value)
        {
            TryLoadPrefs();
            return _prefs.TryGetValue(key, out value);
        }

        public static string GetString(string key)
        {
            TryLoadPrefs();
            return TryGetString(key, out string value) ? value : null;
        }

        public static string GetString(string key, string defaultValue = null)
        {
            TryLoadPrefs();
            return TryGetString(key, out string value) ? value : defaultValue;
        }

        public static void SetString(string key, string value)
        {
            TryLoadPrefs();
            _dirty = true;
            _prefs[key] = value;
        }

        public static bool TryGetInt(string key, out int value)
        {
            TryLoadPrefs();
            value = 0;
            return TryGetString(key, out string valueStr) && int.TryParse(valueStr, out value);
        }

        public static int GetInt(string key)
        {
            TryLoadPrefs();
            TryGetInt(key, out int value);
            return value;
        }

        public static int GetInt(string key, int defaultValue)
        {
            TryLoadPrefs();
            return TryGetInt(key, out int value) ? value : defaultValue;
        }

        public static void SetInt(string key, int value)
        {
            TryLoadPrefs();
            _dirty = true;
            _prefs[key] = value.ToString();
        }

        public static bool TryGetFloat(string key, out float value)
        {
            TryLoadPrefs();
            value = 0;
            return TryGetString(key, out string valueStr) && float.TryParse(valueStr, out value);
        }

        public static float GetFloat(string key)
        {
            TryLoadPrefs();
            TryGetFloat(key, out float value);
            return value;
        }

        public static float GetFloat(string key, float defaultValue)
        {
            TryLoadPrefs();
            return TryGetFloat(key, out float value) ? value : defaultValue;
        }

        public static void SetFloat(string key, float value)
        {
            TryLoadPrefs();
            _dirty = true;
            _prefs[key] = value.ToString();
        }

        public static bool TryGetBool(string key, out bool value)
        {
            TryLoadPrefs();
            value = false;
            return TryGetString(key, out string vStr) && bool.TryParse(vStr, out value);
        }

        public static bool GetBool(string key)
        {
            TryLoadPrefs();
            TryGetBool(key, out bool value);
            return value;
        }

        public static bool GetBool(string key, bool defaultValue)
        {
            TryLoadPrefs();
            return TryGetBool(key, out bool value) ? value : defaultValue;
        }

        public static void SetBool(string key, bool value)
        {
            TryLoadPrefs();
            _dirty = true;
            _prefs[key] = value.ToString();
        }

        [Serializable]
        public sealed class PrefsItem
        {
            public string key;
            public string value;
        }

        [Serializable]
        public sealed class PrefsCollection
        {
            public List<PrefsItem> collection;

            public Dictionary<string, string> ToDictionary()
            {
                Dictionary<string, string> dictionary = new();
                foreach (PrefsItem item in collection) dictionary[item.key] = item.value;
                return dictionary;
            }

            public static PrefsCollection FromDictionary(Dictionary<string, string> dictionary)
            {
                PrefsCollection collection = new();
                collection.collection = new List<PrefsItem>(dictionary.Count);
                foreach (KeyValuePair<string, string> item in dictionary)
                    collection.collection.Add(new PrefsItem { key = item.Key, value = item.Value });
                return collection;
            }
        }
    }
}