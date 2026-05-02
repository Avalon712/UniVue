using UnityEngine;
using UniVue.UI;

namespace Game
{
    public class ReactiveTest : MonoBehaviour
    {
        // Start is called before the first frame update
        private void Start()
        {
            UIMgr.Initialize(new MyUIPrefabLoader(), DefaultLayerMgr.Default);

            UIMgr.Open<GMView>();
        }
    }
}