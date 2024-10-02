using System;

namespace UniVue.Extensions
{
    public interface IObservableList
    {
        int Capacity { get; set; }

        int Count { get; }

        object this[int index] { get; }

        object Get(int index);

        void Set(object item, int index);

        void Add(object item);

        bool Remove(object item);

        bool Contains(object item);

        void RemoveAt(int index);

        int IndexOf(object item);

        /// <summary>
        /// 当集合元素内容发生更改时调用
        /// </summary>
        /// <remarks>arg0-改变形式 arg1-代表当前被修改元素的索引，如果为Sort或Clear模式则为-1</remarks>
        public event Action<ChangedMode, int> OnChanged;

        /// <summary>
        /// 为当前List集合设置操作权限，只有被允许的权限才允许执行
        /// </summary>
        /// <param name="secretKey">设置权限验证的密钥</param>
        /// <param name="permission">后面对此List可操作的权限</param>
        void SetPermission(int secretKey, OperablePermission permission);

        /// <summary>
        /// 主人丢弃上一次的设置权限，丢失权限后，将设为默认权限All，即所有操作都被允许
        /// </summary>
        /// <param name="secretKey">上次设置权限验证的密钥</param>
        void DiscardPermission(int secretKey);

        /// <summary>
        /// 验证当前是否有对集合的操作的指定权限
        /// </summary>
        /// <param name="permission">要验证的权限</param>
        /// <returns>true:有此操作权限</returns>
        bool HavePermission(OperablePermission permission);

        /// <summary>
        /// 当前对List的操作权限
        /// </summary>
        OperablePermission Permission { get; }
    }

    public enum ChangedMode
    {
        Sort,
        Remove,
        Add,
        Replace,
        Clear,
    }
}
