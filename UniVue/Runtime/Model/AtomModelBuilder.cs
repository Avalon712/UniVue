using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniVue.Model
{
    public static class AtomModelBuilder
    {
        public static AtomModel<int> Build(string modelName, string propertyName, int value)
        {
            var atom = new AtomModel<int>(modelName);
            atom.SetAtomProperty(new IntProperty(atom, propertyName, value));
            return atom;
        }

        public static AtomModel<float> Build(string modelName, string propertyName, float value)
        {
            var atom = new AtomModel<float>(modelName);
            atom.SetAtomProperty(new FloatProperty(atom, propertyName, value));
            return atom;
        }

        public static AtomModel<string> Build(string modelName, string propertyName, string value)
        {
            var atom = new AtomModel<string>(modelName);
            atom.SetAtomProperty(new StringProperty(atom, propertyName, value));
            return atom;
        }

        public static AtomModel<bool> Build(string modelName, string propertyName, bool value)
        {
            var atom = new AtomModel<bool>(modelName);
            atom.SetAtomProperty(new BoolProperty(atom, propertyName, value));
            return atom;
        }

        public static AtomModel<Sprite> Build(string modelName, string propertyName, Sprite value)
        {
            var atom = new AtomModel<Sprite>(modelName);
            atom.SetAtomProperty(new SpriteProperty(atom, propertyName, value));
            return atom;
        }

        public static AtomModel<T> Build<T>(string modelName, string propertyName, T value) where T : Enum
        {
            var atom = new AtomModel<T>(modelName);
            atom.SetAtomProperty(new EnumProperty<T>(atom, propertyName, value));
            return atom;
        }

        public static AtomModel<List<int>> Build(string modelName, string propertyName, List<int> value)
        {
            var atom = new AtomModel<List<int>>(modelName);
            atom.SetAtomProperty(new ListIntProperty(atom, propertyName, value));
            return atom;
        }

        public static AtomModel<List<float>> Build(string modelName, string propertyName, List<float> value)
        {
            var atom = new AtomModel<List<float>>(modelName);
            atom.SetAtomProperty(new ListFloatProperty(atom, propertyName, value));
            return atom;
        }

        public static AtomModel<List<string>> Build(string modelName, string propertyName, List<string> value)
        {
            var atom = new AtomModel<List<string>>(modelName);
            atom.SetAtomProperty(new ListStringProperty(atom, propertyName, value));
            return atom;
        }

        public static AtomModel<List<bool>> Build(string modelName, string propertyName, List<bool> value)
        {
            var atom = new AtomModel<List<bool>>(modelName);
            atom.SetAtomProperty(new ListBoolProperty(atom, propertyName, value));
            return atom;
        }

        public static AtomModel<List<Sprite>> Build(string modelName, string propertyName, List<Sprite> value)
        {
            var atom = new AtomModel<List<Sprite>>(modelName);
            atom.SetAtomProperty(new ListSpriteProperty(atom, propertyName, value));
            return atom;
        }

        public static AtomModel<List<T>> Build<T>(string modelName, string propertyName, List<T> value) where T : Enum
        {
            var atom = new AtomModel<List<T>>(modelName);
            atom.SetAtomProperty(new ListEnumProperty<T>(atom, propertyName, value));
            return atom;
        }


    }
}
