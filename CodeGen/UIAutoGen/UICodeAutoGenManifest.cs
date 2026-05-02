using UnityEditor;

namespace UniVue.CodeGen
{
    [FilePath("ProjectSettings/UinVueUICodeAutoGenManifest.asset", FilePathAttribute.Location.ProjectFolder)]
    internal sealed class UICodeAutoGenManifest : ScriptableSingleton<UICodeAutoGenManifest>
    {
        public string manifestJson;

        public void Save()
        {
            Save(true);
        }
    }
}