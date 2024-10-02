using UnityEngine;

namespace UniVue.Common
{
    public sealed class UnityTempObject : MonoBehaviour
    {
        public static UnityTempObject Temp => new GameObject("UniVue_UnityTempObject").AddComponent<UnityTempObject>();

    }
}
