using NonsensicalKit.Core.Service;
using UnityEngine;
using UnityEngine.UI;

namespace NonsensicalKit.Tools.LogicNodeTreeSystem
{
    [RequireComponent(typeof(Button))]
    public class SwitchNodeButton : MonoBehaviour
    {
        [SerializeField] private string m_nodeID;

        private LogicNodeManager _manager;

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(OnButtonClick);

            ServiceCore.SafeGet<LogicNodeManager>(OnGetService);
        }

        private void OnButtonClick()
        {
            if (_manager != null)
            {
                _manager.SwitchNode(m_nodeID);
            }
        }

        private void OnGetService(LogicNodeManager manager)
        {
            _manager = manager;
        }
    }
}
