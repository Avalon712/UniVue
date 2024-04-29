
namespace UniVue.Evt
{
    public interface IEventRegister
    {
        /// <summary>
        /// 登记本注册器到事件中心
        /// </summary>
        void Signup();

        /// <summary>
        /// 从事件中心注销该注册器
        /// </summary>
        void Signout();

        /// <summary>
        /// 注销事件
        /// </summary>
        void Unregister(string eventName);
    }
}
