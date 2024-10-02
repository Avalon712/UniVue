using System;

namespace UniVue.Event
{
    public interface ICustomArgumentMapper
    {
        /// <summary>
        /// 获取自定义类型的参数的值
        /// </summary>
        /// <param name="arguments">参数UI</param>
        /// <returns>参数值</returns>
        public object GetValue(ReadOnlySpan<ArgumentUI> arguments);
    }
}
