using System;

namespace UniVue.Internal
{
    /// <summary>
    /// 使用此特性，如果传递的可变参数数量不超过10个，则在编译时自动替换为此类下相同重载的方法
    /// <code>
    /// [ParamsOptimization]
    /// public void TestMethod(params string[] args){ ... }
    /// 
    /// //必须与重载方法具有相同的访问修饰符，唯一不同的是最后一个参数，其余方法参数必须保持一致
    /// public void TestMethod(in Params&lt;string&gt; args) {...}
    /// 
    /// //当调用TestMethod时会在编译时被智能替换为Params参数版的重载方法
    /// TestMethod("A","B","C"); //参数满足两个条件：1、数量不超过10；2、不能传递数组
    ///       |->编译时替换： TestMethod(Params&lt;string&gt;._("A","B","C")); // 0GC
    /// </code>
    /// </summary>
    internal sealed class InternalParamsGCOptimizationAttribute : Attribute { }
}