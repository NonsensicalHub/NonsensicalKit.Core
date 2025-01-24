using System.Collections.Generic;
using UnityEngine;

namespace NonsensicalKit.Tools.EasyTool
{
    /// <summary>
    /// 保证同样的射线检测在同一帧只执行一次
    /// 在需要大量射线检测时节省性能
    /// </summary>
    public class RaycastTool : MonoBehaviour
    {
        public static RaycastTool Instance;

        [SerializeField] private Camera m_raycastCamera;
        [SerializeField] private float m_distance = 200;

        private Dictionary<string, RaycastHitsInfo> _hitsInfo;
        private Dictionary<string, RaycastHitInfo> _hitInfo;
        private RaycastHitsInfo _raycastHitsInfo = new();
        private RaycastHitInfo _raycastHitInfo = new();

        private void Awake()
        {
            Instance = this;
            if (m_raycastCamera == null)
            {
                m_raycastCamera = Camera.main;
            }

            _hitsInfo = new Dictionary<string, RaycastHitsInfo>();
            _hitInfo = new Dictionary<string, RaycastHitInfo>();
        }

        public RaycastHit[] GetHits(string mask = "NULL")
        {
            int crtCount = Time.frameCount;
            if (_hitsInfo.ContainsKey(mask) && _hitsInfo[mask].FrameCount == crtCount)
            {
                return _hitsInfo[mask].RaycastHits;
            }
            else
            {
                if (_hitsInfo.ContainsKey(mask) == false)
                {
                    _hitsInfo.Add(mask, null);
                }

                _hitsInfo[mask] = CheckAll(mask);
                return _hitsInfo[mask].RaycastHits;
            }
        }

        public RaycastHit GetHit(string mask = "NULL")
        {
            int crtCount = Time.frameCount;
            if (_hitInfo.ContainsKey(mask) && _hitInfo[mask].FrameCount == crtCount)
            {
                return _hitInfo[mask].RaycastHit;
            }
            else
            {
                if (_hitInfo.ContainsKey(mask) == false)
                {
                    _hitInfo.Add(mask, null);
                }

                _hitInfo[mask] = CheckFirst(mask);
                return _hitInfo[mask].RaycastHit;
            }
        }

        private RaycastHitsInfo CheckAll(string mask)
        {
            _raycastHitsInfo.FrameCount = Time.frameCount;
            Ray ray = m_raycastCamera.ScreenPointToRay(Input.mousePosition);

            if (mask == "NULL")
            {
                _raycastHitsInfo.RaycastHits = Physics.RaycastAll(ray, m_distance);
            }
            else
            {
                _raycastHitsInfo.RaycastHits = Physics.RaycastAll(ray, m_distance, LayerMask.GetMask(mask));
            }

            return _raycastHitsInfo;
        }

        private RaycastHitInfo CheckFirst(string mask)
        {
            _raycastHitInfo.FrameCount = Time.frameCount;

            Ray ray = m_raycastCamera.ScreenPointToRay(Input.mousePosition);

            if (mask == "NULL")
            {
                Physics.Raycast(ray, out _raycastHitInfo.RaycastHit, m_distance);
            }
            else
            {
                Physics.Raycast(ray, out _raycastHitInfo.RaycastHit, m_distance, LayerMask.GetMask(mask));
            }

            return _raycastHitInfo;
        }
    }

    public class RaycastHitsInfo
    {
        public int FrameCount;
        public RaycastHit[] RaycastHits;
    }

    public class RaycastHitInfo
    {
        public int FrameCount;
        public RaycastHit RaycastHit;
    }
}
