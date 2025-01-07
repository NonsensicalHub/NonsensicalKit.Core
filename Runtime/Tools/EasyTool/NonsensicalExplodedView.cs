using NonsensicalKit.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NonsensicalKit.Tools.EasyTool
{
    /// <summary>
    /// 模型爆炸图，挂载在想要爆炸的根节点
    /// </summary>
    public class NonsensicalExplodedView : NonsensicalMono
    {
        [SerializeField][Tooltip("爆炸速度")][Range(0, 1)] private float m_explosionSpeed = 0.1f;

        [SerializeField][Tooltip("爆炸范围")][Range(1, 10)] private float m_explosionRange = 2f;

        [SerializeField][Tooltip("自定义id")] private string m_customID;

        private List<ExplosionInfo> _targets;

        private bool _isExplosion = false;

        private float _percentage = 0;

        private void Awake()
        {
            Init();

            Subscribe<bool>("toggleExplodedView", m_customID, ToggleExplodedView);
            Subscribe("toggleExplodedView", m_customID, ToggleExplodedView);
        }

        private void Update()
        {
            if (_isExplosion)
            {
                _percentage = _percentage * (1 - m_explosionSpeed) + m_explosionSpeed;
            }
            else
            {
                _percentage = _percentage * (1 - m_explosionSpeed);
            }

            foreach (var item in _targets)
            {
                item.Target.localPosition = Vector3.Lerp(item.OriginalPosition, item.ExplodedPosition, _percentage);
            }

            if (0.5f - Mathf.Abs(_percentage - 0.5f) < 0.01f)
            {
                enabled = false;
            }
        }

        public void ToggleExplodedView(bool exploded)
        {
            if (exploded == _isExplosion)
            {
                return;
            }

            _isExplosion = exploded;
            enabled = true;
        }

        public void ToggleExplodedView()
        {
            _isExplosion = !_isExplosion;
            enabled = true;
        }

        private void Init()
        {
            _targets = new List<ExplosionInfo>();

            Queue<Transform> nodes = new Queue<Transform>();
            Queue<Vector3> offsets = new Queue<Vector3>();  //父节点位置与根节点的偏移量


            foreach (Transform child in this.transform)
            {
                if (child.gameObject.activeSelf == true)
                {
                    nodes.Enqueue(child);
                    offsets.Enqueue(Vector3.zero);
                }
            }

            while (nodes.Count > 0)
            {
                Transform crtNode = nodes.Dequeue();
                Vector3 offset = offsets.Dequeue();
                Vector3 newOffset = offset + crtNode.position - crtNode.parent.position;

                if (crtNode.TryGetComponent<MeshRenderer>(out var item))
                {
                    ExplosionInfo info = new ExplosionInfo();

                    info.Target = crtNode;

                    info.OriginalPosition = crtNode.localPosition;

                    //Vector3 centerOffset = item.transform.position - item.bounds.center;      //渲染中心到节点位置的偏移
                    //Vector3 rootOffsetPos = item.bounds.center - this.transform.position;       //渲染中心与根节点的偏移
                    //Vector3 explosionOffset = rootOffsetPos * explosionRange;           //爆炸
                    //newOffset = explosionOffset + centerOffset;             //爆炸后的节点位置与根节点的偏移
                    //Vector3 explosionParentOffset = newOffset - offset;             //爆炸后的节点位置与父节点的偏移
                    //info.explodedPosition = crtNode.parent.InverseTransformVector(explosionParentOffset);  //偏移量转换成本地坐标偏移
                    //以上为推导过程，化简后如下
                    newOffset = item.bounds.center * (m_explosionRange - 1) - this.transform.position * m_explosionRange + item.transform.position;
                    info.ExplodedPosition = crtNode.parent.InverseTransformVector(newOffset - offset);

                    _targets.Add(info);
                }

                foreach (Transform child in crtNode)
                {
                    if (child.gameObject.activeSelf == true)
                    {
                        nodes.Enqueue(child);
                        offsets.Enqueue(newOffset);
                    }
                }
            }
            enabled = false;
        }

        private class ExplosionInfo
        {
            public Transform Target;
            public Vector3 OriginalPosition;
            public Vector3 ExplodedPosition;
        }
    }
}
