using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UniVue.Model;
using Object = UnityEngine.Object;

namespace UniVue.Test
{
    /// <summary>
    /// 验证 <c>NotifyPropertyChangedInjector</c> 对 UniVue.Tests 程序集的 IL 注入结果。
    /// </summary>
    internal sealed class PropertyNotifyTest
    {
        #region BaseModel

        [Test]
        public void BaseModel_AutoProperty_NotifiesOnChange()
        {
            var model = new BasicBaseModel();
            var log = new EventLog();
            model.OnPropertyChanged += log.Add;

            model.Counter = 1;
            Assert.That(log.Count, Is.EqualTo(1));
            Assert.That(log.LastName, Is.EqualTo(nameof(BasicBaseModel.Counter)));
            Assert.That(log.LastValue, Is.EqualTo(1));
            Assert.That(log.LastSender, Is.SameAs(model));
        }

        [Test]
        public void BaseModel_AutoProperty_DoesNotNotifyWhenValueUnchanged()
        {
            var model = new BasicBaseModel { Counter = 1 };
            var log = new EventLog();
            model.OnPropertyChanged += log.Add;

            model.Counter = 1;
            Assert.That(log.Count, Is.EqualTo(0));
        }

        [Test]
        public void BaseModel_StringProperty_NotifiesOnChange()
        {
            var model = new BasicBaseModel();
            var log = new EventLog();
            model.OnPropertyChanged += log.Add;

            model.Label = "a";
            Assert.That(log.LastName, Is.EqualTo(nameof(BasicBaseModel.Label)));
            Assert.That(log.LastValue, Is.EqualTo("a"));

            log.Clear();
            model.Label = "a";
            Assert.That(log.Count, Is.EqualTo(0));

            model.Label = "b";
            Assert.That(log.LastName, Is.EqualTo(nameof(BasicBaseModel.Label)));
        }

        [Test]
        public void BaseModel_InheritedProperties_BothNotify()
        {
            var model = new DerivedBaseModel();
            var log = new EventLog();
            model.OnPropertyChanged += log.Add;

            model.BaseValue = 10;
            Assert.That(log.LastName, Is.EqualTo(nameof(InheritedBaseModel.BaseValue)));

            log.Clear();
            model.DerivedValue = 20;
            Assert.That(log.LastName, Is.EqualTo(nameof(DerivedBaseModel.DerivedValue)));
        }

        [Test]
        public void BaseModel_VirtualOverride_NotifiesWithDerivedSetter()
        {
            var model = new VirtualDerivedModel();
            var log = new EventLog();
            model.OnPropertyChanged += log.Add;

            model.Score = 5;
            Assert.That(log.Count, Is.EqualTo(1));
            Assert.That(log.LastName, Is.EqualTo(nameof(VirtualBaseModel.Score)));
            Assert.That(log.LastValue, Is.EqualTo(5));
            Assert.That(model.Score, Is.EqualTo(5));
        }

        [Test]
        public void BaseModel_VirtualDispatchThroughBaseReference_StillNotifies()
        {
            VirtualBaseModel model = new VirtualDerivedModel();
            var log = new EventLog();
            model.OnPropertyChanged += log.Add;

            model.Score = 7;
            Assert.That(log.Count, Is.EqualTo(1));
            Assert.That(log.LastValue, Is.EqualTo(7));
        }

        #endregion

        #region UIModel

        [Test]
        public void UIModel_AutoProperty_NotifiesOnChange()
        {
            var go = new GameObject(nameof(UIModel_AutoProperty_NotifiesOnChange));
            try
            {
                var model = go.AddComponent<BasicUIModel>();
                var log = new EventLog();
                model.OnPropertyChanged += log.Add;

                model.Hp = 100;
                Assert.That(log.Count, Is.EqualTo(1));
                Assert.That(log.LastName, Is.EqualTo(nameof(BasicUIModel.Hp)));
                Assert.That(log.LastValue, Is.EqualTo(100));
                Assert.That(log.LastSender, Is.SameAs(model));

                log.Clear();
                model.Hp = 100;
                Assert.That(log.Count, Is.EqualTo(0));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void UIModel_InheritedProperties_BothNotify()
        {
            var go = new GameObject(nameof(UIModel_InheritedProperties_BothNotify));
            try
            {
                var model = go.AddComponent<DerivedUIModel>();
                var log = new EventLog();
                model.OnPropertyChanged += log.Add;

                model.BaseHp = 1;
                Assert.That(log.LastName, Is.EqualTo(nameof(InheritedUIModel.BaseHp)));

                log.Clear();
                model.Extra = "x";
                Assert.That(log.LastName, Is.EqualTo(nameof(DerivedUIModel.Extra)));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        #endregion

        #region NoNotify

        [Test]
        public void NoNotify_OnProperty_DoesNotNotify()
        {
            var model = new PropertyNoNotifyModel();
            var log = new EventLog();
            model.OnPropertyChanged += log.Add;

            model.Silent = 1;
            Assert.That(log.Count, Is.EqualTo(0));
            Assert.That(model.Silent, Is.EqualTo(1));

            model.Normal = 2;
            Assert.That(log.Count, Is.EqualTo(1));
            Assert.That(log.LastName, Is.EqualTo(nameof(PropertyNoNotifyModel.Normal)));
        }

        [Test]
        public void NoNotify_OnVirtualProperty_OverrideAlsoDoesNotNotify()
        {
            var model = new VirtualNoNotifyDerivedModel();
            var log = new EventLog();
            model.OnPropertyChanged += log.Add;

            model.Muted = 3;
            Assert.That(log.Count, Is.EqualTo(0));
            Assert.That(model.Muted, Is.EqualTo(3));

            model.Audible = 4;
            Assert.That(log.Count, Is.EqualTo(1));
            Assert.That(log.LastName, Is.EqualTo(nameof(VirtualNoNotifyBaseModel.Audible)));
        }

        [Test]
        public void NoNotify_OnClass_SkipsAllOwnProperties()
        {
            var model = new ClassNoNotifyModel();
            var log = new EventLog();
            model.OnPropertyChanged += log.Add;

            model.A = 1;
            model.B = "x";
            Assert.That(log.Count, Is.EqualTo(0));
        }

        [Test]
        public void NoNotify_OnClass_SkipsSubclassProperties()
        {
            var model = new ClassNoNotifyDerivedModel();
            var log = new EventLog();
            model.OnPropertyChanged += log.Add;

            model.A = 1;
            model.ChildValue = 9;
            Assert.That(log.Count, Is.EqualTo(0));
        }

        #endregion

        private sealed class EventLog
        {
            private readonly List<(IModel sender, string name, object value)> _entries = new();

            public int Count => _entries.Count;
            public IModel LastSender => _entries[_entries.Count - 1].sender;
            public string LastName => _entries[_entries.Count - 1].name;
            public object LastValue => _entries[_entries.Count - 1].value;

            public void Add(IModel sender, string name, object value) => _entries.Add((sender, name, value));

            public void Clear() => _entries.Clear();
        }

        private sealed class BasicBaseModel : BaseModel
        {
            public int Counter { get; set; }
            public string Label { get; set; }
        }

        private class InheritedBaseModel : BaseModel
        {
            public int BaseValue { get; set; }
        }

        private sealed class DerivedBaseModel : InheritedBaseModel
        {
            public int DerivedValue { get; set; }
        }

        private class VirtualBaseModel : BaseModel
        {
            public virtual int Score { get; set; }
        }

        private sealed class VirtualDerivedModel : VirtualBaseModel
        {
            public override int Score { get; set; }
        }

        private sealed class PropertyNoNotifyModel : BaseModel
        {
            [NoNotify]
            public int Silent { get; set; }

            public int Normal { get; set; }
        }

        private class VirtualNoNotifyBaseModel : BaseModel
        {
            [NoNotify]
            public virtual int Muted { get; set; }

            public virtual int Audible { get; set; }
        }

        private sealed class VirtualNoNotifyDerivedModel : VirtualNoNotifyBaseModel
        {
            public override int Muted { get; set; }
            public override int Audible { get; set; }
        }

        [NoNotify]
        private class ClassNoNotifyModel : BaseModel
        {
            public int A { get; set; }
            public string B { get; set; }
        }

        private sealed class ClassNoNotifyDerivedModel : ClassNoNotifyModel
        {
            public int ChildValue { get; set; }
        }
    }

    // UIModel / MonoBehaviour 使用顶层类型，避免嵌套类型 AddComponent 失败。
    public sealed class BasicUIModel : UIModel
    {
        public int Hp { get; set; }
    }

    public class InheritedUIModel : UIModel
    {
        public int BaseHp { get; set; }
    }

    public sealed class DerivedUIModel : InheritedUIModel
    {
        public string Extra { get; set; }
    }
}
