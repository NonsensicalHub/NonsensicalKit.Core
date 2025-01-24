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
        protected LogicNodeManager Manager;

        protected LogicNodeMono NodeMono;

        protected virtual void Awake()
        {
            ServiceCore.SafeGet<LogicNodeManager>(OnGetService);
            if (TryGetComponent<LogicNodeMono>(out NodeMono))
            {
                NodeMono.OnNodeEnter.AddListener(OnEnter);
                NodeMono.OnNodeExit.AddListener(OnExit);
            }
        }

        protected virtual void OnGetService(LogicNodeManager manager)
        {
            Manager = manager;
        }

        protected abstract void OnEnter();
        protected abstract void OnExit();
    }
}
