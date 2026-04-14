using NonsensicalKit.Core.Service;
using UnityEngine;

namespace NonsensicalKit.Core.DagLogicNode
{
    public class DagGraphConfigurator : MonoBehaviour
    {
        [SerializeField] private DagGraphConfig m_graph;

        private void Awake()
        {
            ServiceCore.Get<DagLogicManager>().InitGraph(m_graph.DagGraph);
        }
    }
}
