using UnityEngine;

namespace UniVue.Common
{
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>, new()
    {
        private static T _instance;

        protected virtual bool KeepLiveOnSceneChange { get; set; } = true;

        protected virtual bool HideInHierarchy { get; set; } = true;

        public static T Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = FindObjectOfType<T>(true);
                    if (!_instance)
                    {
                        _instance = new GameObject(typeof(T).Name).AddComponent<T>();
                        if (_instance)
                        {
                            _instance.OnInitialize();
                            if (_instance.HideInHierarchy)
                                _instance.gameObject.hideFlags = HideFlags.HideInHierarchy;
                            if (_instance.KeepLiveOnSceneChange)
                                DontDestroyOnLoad(_instance.gameObject);
                        }
                    }
                }

                return _instance;
            }
        }

        private void OnDestroy()
        {
            _instance = null;
        }

        protected virtual void OnInitialize() { }
    }
}