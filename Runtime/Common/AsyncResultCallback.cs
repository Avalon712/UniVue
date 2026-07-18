namespace UniVue.Common
{
    /// <summary>
    /// 异步加载回调
    /// </summary>
    /// <param name="success">true-加载成功，结果有效  false-加载失败，结果无效</param>
    /// <typeparam name="TResult">异步加载的结果</typeparam>
    public delegate void AsyncResultCallback<in TResult>(bool success, TResult result);
}