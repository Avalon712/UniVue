using System.Collections.Generic;
using UnityEngine;
using UniVue.Model;

namespace UniVue.View.Views
{
    public interface IView
    {
        /// <summary>
        /// 当前视图的级别
        /// </summary>
        ViewLevel level { get; }

        /// <summary>
        /// 当前视图的名称
        /// </summary>
        string name { get; }

        /// <summary>
        /// 当前视图的状态
        /// </summary>
        bool state { get; }

        /// <summary>
        /// 获取视图的GameObject对象
        /// </summary>
        GameObject viewObject { get; }

        /// <summary>
        /// 当前视图是否是属主视图
        /// </summary>
        bool isMaster { get; }

        /// <summary>
        /// 获取当前嵌套视图的根视图名称
        /// </summary>
        string root { get; }

        /// <summary>
        /// 获取当前视图的属主视图的名称
        /// </summary>
        string master { get; }

        /// <summary>
        /// 当前视图被打开后是否禁止其它视图再打开
        /// </summary>
        bool forbid { get; }

        /// <summary>
        /// 获取此时图嵌套的所有的视图
        /// </summary>
        /// <typeparam name="T">实现IView</typeparam>
        /// <returns>此视图包含的所有嵌套视图</returns>
        IEnumerable<IView> GetNestedViews();

        /// <summary>
        /// 视图加载时调用
        /// </summary>
        void OnLoad();

        /// <summary>
        /// 视图卸载时调用
        /// </summary>
        void OnUnload();

        /// <summary>
        /// 绑定模型
        /// </summary>
        /// <typeparam name="T">实现IBindableModel接口类型</typeparam>
        /// <param name="model">绑定的模型数据</param>
        /// <param name="allowUIUpdateModel">是否允许通过UI修改模型的值</param>
        /// <param name="modelName">模型名称，若为null则默认为该类型的TypeName</param>
        /// <param name="forceRebind">当已经绑定了数据时是否指示进行强制重新绑定</param>
        /// <returns>IView</returns>
        IView BindModel<T>(T model, bool allowUIUpdateModel = true, string modelName = null, bool forceRebind = false) where T : IBindableModel;

        /// <summary>
        /// 打开视图
        /// </summary>
        void Open();

        /// <summary>
        /// 关闭视图
        /// </summary>
        void Close();

        /// <summary>
        /// 重新绑定模型数据，注意新的模型类型应该与之前绑定过的模型类型一致
        /// 注：重新绑定指定类型的数据
        /// </summary>
        /// <param name="newModel">新模型</param>
        /// <param name="oldModel">旧模型</param>
        void RebindModel<T>(T newModel, T oldModel) where T : IBindableModel;

        /// <summary>
        /// 重新绑定模型数据
        /// 注：重绑定所有此类型的数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="newModel"></param>
        void RebindModel<T>(T newModel) where T : IBindableModel;
    }
}
