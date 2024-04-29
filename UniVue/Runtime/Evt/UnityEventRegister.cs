using UnityEngine;

namespace UniVue.Evt
{
    public abstract class UnityEventRegister : MonoBehaviour, IEventRegister
    {
        protected virtual void Awake()
        {
            Signup();
        }

        public void Signup()
        {
            Vue.Event.Signup(this);
        }

        public void Signout()
        {
            Vue.Event.Signout(this);
        }

        protected virtual void OnDestory()
        {
            Signout();
        }

        public void Unregister(string eventName)
        {
            Vue.Event.UnregiserEventCall(this, eventName);
        }
    }
}
