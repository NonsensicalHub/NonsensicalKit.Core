using NonsensicalKit.Core;
using NonsensicalKit.Core.Service;
using UnityEngine;

namespace NonsensicalKit.Tools.LogicNodeTreeSystem
{
    /// <summary>
    /// 自动添加监听
    /// </summary>
    [RequireComponent(typeof(LogicNodeMono))]
    public abstract class LogicNodeSwitchBase : NonsensicalMono
    {
        protected LogicNodeManager _manager;

        protected LogicNodeMono _nodeMono;

        protected virtual void Awake()
        {
            ServiceCore.SafeGet<LogicNodeManager>(OnGetService);
            if (TryGetComponent<LogicNodeMono>(out _nodeMono))
            {
                _nodeMono.OnNodeEnter.AddListener(OnEnter);
                _nodeMono.OnNodeExit.AddListener(OnExit);
            }
        }

        protected virtual void OnGetService(LogicNodeManager manager)
        {
            _manager = manager;
        }

        protected abstract void OnEnter();
        protected abstract void OnExit();
    }
}
