using UnityEngine;
using UniVue.UI;

public class LayerMgrExtensionTest : MonoBehaviour
{
    public Canvas canvasPrefab;

    private void Start()
    {
        UIMgr.Initialize(new MyUIPrefabLoader(), new MyCustomLayerMgr(canvasPrefab));

        UIMgr.Open<GMView>();
    }
}