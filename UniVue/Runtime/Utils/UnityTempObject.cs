using UnityEngine;

namespace UniVue.Utils
{
    public sealed class UnityTempObject : MonoBehaviour
    {
        public static UnityTempObject Instance => new GameObject("UniVue_UnityTempObject").AddComponent<UnityTempObject>();

    }
}
