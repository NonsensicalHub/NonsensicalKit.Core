using UnityEngine;

namespace NonsensicalKit.Tools.EazyTool
{
    public class LineRendererConfigurator : MonoBehaviour
    {
        [SerializeField] private Transform m_pointsParent;
        [SerializeField] private bool m_loop;

        private LineRenderer _line;

        private void Awake()
        {
            if (m_pointsParent != null)
            {
                _line = GetComponent<LineRenderer>();
                var points = new Vector3[m_pointsParent.childCount+(m_loop?1:0)];
                for (int i = 0; i < m_pointsParent.childCount; i++)
                {
                    points[i] = m_pointsParent.GetChild(i).position;
                }
                if (m_loop)
                {
                    points[m_pointsParent.childCount] = m_pointsParent.GetChild(0).position;
                }
                _line.positionCount = points.Length;
                _line.SetPositions(points);
            }
        }
    }
}
