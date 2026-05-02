namespace UniVue.Common
{
    public abstract class Singleton<T> where T : Singleton<T>, new()
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new T();
                    _instance.OnCreateInstance();
                }

                return _instance;
            }
        }

        protected virtual void OnCreateInstance() { }
    }
}