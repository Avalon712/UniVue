using UnityEngine;
using UnityEngine.UI;
using UniVue.UI;

public sealed class CloseBtnUI : BaseUI
{
    [SerializeField] private string _viewName;

    protected override void OnCreate()
    {
        GetComponent<Button>().onClick.AddListener(Close);
    }

    public void Close()
    {
        UIMgr.Close(_viewName);
    }
}