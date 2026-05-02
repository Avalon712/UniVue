using System;
using System.Collections.Generic;
using UnityEngine;
using UniVue.Model;

/// <summary>
/// 手动验证 <see cref="NotifyPropertyChangedInjector" /> 注入后的行为：
/// 继承 <see cref="BaseModel" /> 的自动属性 setter 会调用
/// <see cref="BaseModel.CheckPropertyChanged{T}" />，从而在值变化时触发 <see cref="BaseModel.OnPropertyChanged" />。
/// </summary>
public sealed class PropertyNotifyChangedTest : MonoBehaviour
{
    private int _failed;
    private int _passed;

    private void Start()
    {
        RunBasicAutoProperties();
        RunInheritanceBaseAndDerivedProperties();
        RunVirtualOverrideProperties();
        RunVirtualDispatchThroughBaseReference();

        Debug.Log($"[PropertyNotifyChangedTest] 完成: {_passed} 通过, {_failed} 失败");
    }

    private void RunBasicAutoProperties()
    {
        NotifyTestModel model = new();
        EventLog log = new();

        model.OnPropertyChanged += log.Add;

        log.Clear();
        model.Counter = 1;
        Assert(log.ContainsName("Counter") && log.LastContainsValue("1"), "基础: 首次设置 int 应触发并带新值");

        log.Clear();
        model.Counter = 1;
        Assert(log.IsEmpty, "基础: 相同 int 不应触发");

        log.Clear();
        model.Counter = 2;
        Assert(log.ContainsName("Counter"), "基础: int 变更应触发");

        log.Clear();
        model.Label = "a";
        Assert(log.ContainsName("Label"), "基础: string 首次赋值应触发");

        log.Clear();
        model.Label = "a";
        Assert(log.IsEmpty, "基础: 相同 string 不应重复触发");

        log.Clear();
        model.Label = "b";
        Assert(log.ContainsName("Label"), "基础: string 变更应触发");

        log.Clear();
        model.Counter = 10;
        model.Label = "z";
        Assert(log.ContainsName("Counter") && log.ContainsName("Label"), "基础: 多属性应分别通知");
    }

    private void RunInheritanceBaseAndDerivedProperties()
    {
        NotifyDerivedModel m = new();
        EventLog log = new();
        m.OnPropertyChanged += log.Add;

        log.Clear();
        m.BaseField = 100;
        Assert(log.ContainsName("BaseField") && log.LastContainsValue("100"), "继承: 基类上声明的属性应注入并通知");

        log.Clear();
        m.BaseField = 100;
        Assert(log.IsEmpty, "继承: 基类属性同值不重复通知");

        log.Clear();
        m.DerivedField = "x";
        Assert(log.ContainsName("DerivedField"), "继承: 派生类自有属性应通知");

        log.Clear();
        m.BaseField = 200;
        m.DerivedField = "y";
        Assert(log.ContainsName("BaseField") && log.ContainsName("DerivedField"), "继承: 基类与派生属性交替变更");
    }

    private void RunVirtualOverrideProperties()
    {
        NotifyVirtualDerived d = new();
        EventLog log = new();
        d.OnPropertyChanged += log.Add;

        log.Clear();
        d.VirtualScore = 1;
        Assert(log.ContainsName("VirtualScore"), "虚属性: override setter 应通知（属性名仍为 VirtualScore）");

        log.Clear();
        d.VirtualScore = 1;
        Assert(log.IsEmpty, "虚属性: 同值不重复通知");

        log.Clear();
        d.OnlyOnDerived = 7;
        Assert(log.ContainsName("OnlyOnDerived"), "虚属性: 派生类独有属性应通知");

        log.Clear();
        d.VirtualScore = 99;
        d.OnlyOnDerived = 8;
        Assert(log.ContainsName("VirtualScore") && log.ContainsName("OnlyOnDerived"), "虚属性: 与派生独有属性可同时通知");
    }

    private void RunVirtualDispatchThroughBaseReference()
    {
        NotifyVirtualBase viaBase = new NotifyVirtualDerived();
        EventLog log = new();
        viaBase.OnPropertyChanged += log.Add;

        log.Clear();
        viaBase.VirtualScore = 42;
        Assert(log.ContainsName("VirtualScore") && log.LastContainsValue("42"),
               "多态: 经基类引用赋值 override 属性应走派生 setter 并通知");
    }

    private void Assert(bool condition, string message)
    {
        if (condition)
        {
            _passed++;
            Debug.Log($"  [PASS] {message}");
        }
        else
        {
            _failed++;
            Debug.LogError($"  [FAIL] {message}");
        }
    }

    private sealed class EventLog
    {
        private readonly List<string> _lines = new();

        public bool IsEmpty => _lines.Count == 0;

        public void Add(BaseModel m, string name, object newValue)
        {
            _lines.Add($"[{name}] => {newValue}");
        }

        public void Clear()
        {
            _lines.Clear();
        }

        public bool ContainsName(string name)
        {
            foreach (string line in _lines)
            {
                if (line.StartsWith($"[{name}]", StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        public bool LastContainsValue(string value)
        {
            if (_lines.Count == 0) return false;
            return _lines[^1].IndexOf(value, StringComparison.Ordinal) >= 0;
        }

        public override string ToString()
        {
            return string.Join("\n", _lines);
        }
    }
}

/// <summary>供 IL 注入：直接继承 BaseModel。</summary>
internal sealed class NotifyTestModel : BaseModel
{
    public int Counter { get; set; }
    public string Label { get; set; }
}

/// <summary>继承链：中间基类带属性。</summary>
internal class NotifyBaseModel : BaseModel
{
    public int BaseField { get; set; }
}

/// <summary>继承链：派生类增加自有属性。</summary>
internal sealed class NotifyDerivedModel : NotifyBaseModel
{
    public string DerivedField { get; set; }
}

/// <summary>虚属性：基类声明 virtual 自动属性。</summary>
internal class NotifyVirtualBase : BaseModel
{
    public virtual int VirtualScore { get; set; }
}

/// <summary>虚属性：派生类 override，并增加独有属性。</summary>
internal sealed class NotifyVirtualDerived : NotifyVirtualBase
{
    public override int VirtualScore { get; set; }

    public int OnlyOnDerived { get; set; }
}