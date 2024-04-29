
namespace UniVue.Evt
{
    public abstract class EventRegister : IEventRegister
    {
        public EventRegister()
        {
            Signup();
        }

        public void Signout()
        {
            Vue.Event.Signout(this);
        }

        public void Signup()
        {
            Vue.Event.Signup(this);
        }

        public void Unregister(string eventName)
        {
            Vue.Event.UnregiserEventCall(this, eventName);
        }
    }
}
