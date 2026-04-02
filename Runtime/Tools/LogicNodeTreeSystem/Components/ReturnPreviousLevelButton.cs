using NonsensicalKit.Core;
using NonsensicalKit.Core.Service;
using NonsensicalKit.Tools.LogicNodeTreeSystem;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ReturnPreviousLevelButton : NonsensicalMono
{
    private LogicNodeManager _manager;



    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OnButtonClick);
        ServiceCore.SafeGet<LogicNodeManager>(OnGetService);
    }

    private void OnGetService(LogicNodeManager service)
    {
        _manager = service;
    }

    private void OnButtonClick()
    {
        if (_manager == null) return;

        _manager.ReturnPreviousLevel();
    }
}
