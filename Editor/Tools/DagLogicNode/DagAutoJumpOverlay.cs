using System;
using System.Collections.Generic;
using System.Linq;
using NonsensicalKit.Core.DagLogicNode;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace NonsensicalKit.Core.DagLogicNode.Editor
{
    /// <summary>绘制自动跳转连线：青色虚线 + 箭头。</summary>
    internal sealed class DagAutoJumpOverlay : ImmediateModeElement
    {
        private readonly DagGraphView _graphView;

        public DagAutoJumpOverlay(DagGraphView graphView)
        {
            _graphView = graphView;
            pickingMode = PickingMode.Ignore;
            style.position = Position.Absolute;
            style.left = 0;
            style.top = 0;
            style.right = 0;
            style.bottom = 0;

            graphView.viewTransformChanged += _ => MarkDirtyRepaint();
            graphView.RegisterCallback<GeometryChangedEvent>(_ => MarkDirtyRepaint());
            graphView.contentViewContainer.RegisterCallback<GeometryChangedEvent>(_ => MarkDirtyRepaint());

            graphView.schedule.Execute(RepaintWhileDragging).Every(33);
        }

        private void RepaintWhileDragging()
        {
            if (!_graphView.edges.Any(e =>
                    e?.output != null && DagNodeView.IsAutoJumpPort(e.output) && e.input == null))
            {
                return;
            }

            _graphView.ApplyAutoJumpEdgeHiding();
            MarkDirtyRepaint();
        }

        protected override void ImmediateRepaint()
        {
            if (_graphView == null)
            {
                return;
            }

            _graphView.ApplyAutoJumpEdgeHiding();

            var mat = GetLineMaterial();
            mat.SetPass(0);

            foreach (var edge in _graphView.edges.ToList())
            {
                if (edge?.output == null || !DagNodeView.IsAutoJumpPort(edge.output))
                {
                    continue;
                }

                if (!TryGetEndpoints(edge, out var fromPanel, out var toPanel))
                {
                    continue;
                }

                DrawConnection(PanelToLocal(fromPanel), PanelToLocal(toPanel));
            }

            foreach (var nodeView in _graphView.nodes.ToList().OfType<DagNodeView>())
            {
                if (nodeView.AutoJumpOutput == null || !nodeView.Node.autoJumpEnabled)
                {
                    continue;
                }

                var targetId = nodeView.Node.autoJumpTargetNodeId;
                if (string.IsNullOrWhiteSpace(targetId))
                {
                    continue;
                }

                var toView = _graphView.nodes.ToList().OfType<DagNodeView>()
                    .FirstOrDefault(v => v.Node.nodeId == targetId.Trim());
                if (toView?.AutoJumpInput == null)
                {
                    continue;
                }

                if (_graphView.edges.Any(e =>
                        e.output?.node == nodeView && e.input?.node == toView
                        && DagNodeView.IsAutoJumpPort(e.output)))
                {
                    continue;
                }

                var fromPanel = GetPortPanelPoint(nodeView.AutoJumpOutput);
                var toPanel = GetPortPanelPoint(toView.AutoJumpInput);
                DrawConnection(PanelToLocal(fromPanel), PanelToLocal(toPanel));
            }
        }

        private Vector2 PanelToLocal(Vector2 panelPoint)
        {
            var local = worldTransform.inverse.MultiplyPoint3x4(
                new Vector3(panelPoint.x, panelPoint.y, 0f));
            return new Vector2(local.x, local.y);
        }

        private static bool TryGetEndpoints(Edge edge, out Vector2 fromPanel, out Vector2 toPanel)
        {
            fromPanel = default;
            toPanel = default;
            if (edge?.output == null)
            {
                return false;
            }

            fromPanel = GetPortPanelPoint(edge.output);

            if (edge.input != null)
            {
                toPanel = GetPortPanelPoint(edge.input);
                return true;
            }

            var control = edge.edgeControl;
            if (control == null)
            {
                return false;
            }

            var world = edge.worldTransform.MultiplyPoint3x4(new Vector3(control.to.x, control.to.y, 0f));
            toPanel = new Vector2(world.x, world.y);
            return true;
        }

        private static Vector2 GetPortPanelPoint(Port port)
        {
            var bounds = port.worldBound;
            return port.direction == Direction.Output
                ? new Vector2(bounds.xMax, bounds.center.y)
                : new Vector2(bounds.xMin, bounds.center.y);
        }

        private static void DrawConnection(Vector2 from, Vector2 to)
        {
            if ((from - to).sqrMagnitude < 1f)
            {
                return;
            }

            WalkDashed(from, to, DagAutoJumpLineStyle.DashLength, DagAutoJumpLineStyle.GapLength,
                (a, b) => DrawSegment(a, b, DagAutoJumpLineStyle.Cyan));
            DrawArrow(from, to);
        }

        private static void WalkDashed(
            Vector2 start,
            Vector2 end,
            float dashLength,
            float gapLength,
            Action<Vector2, Vector2> drawSegment)
        {
            var delta = end - start;
            var length = delta.magnitude;
            if (length < 1e-4f)
            {
                return;
            }

            var dir = delta / length;
            var drawing = true;
            var patternRemain = dashLength;
            var traveled = 0f;

            while (traveled < length - 1e-4f)
            {
                var step = Mathf.Min(patternRemain, length - traveled);
                var p0 = start + dir * traveled;
                var p1 = start + dir * (traveled + step);

                if (drawing)
                {
                    drawSegment(p0, p1);
                }

                traveled += step;
                patternRemain -= step;
                if (patternRemain <= 1e-4f)
                {
                    drawing = !drawing;
                    patternRemain = drawing ? dashLength : gapLength;
                }
            }
        }

        private static void DrawSegment(Vector2 start, Vector2 end, Color color)
        {
            GL.Begin(GL.LINES);
            GL.Color(color);
            GL.Vertex3(start.x, start.y, 0f);
            GL.Vertex3(end.x, end.y, 0f);
            GL.End();
        }

        private static void DrawArrow(Vector2 from, Vector2 to)
        {
            var dir = (to - from).normalized;
            if (dir.sqrMagnitude < 1e-6f)
            {
                return;
            }

            var ortho = new Vector2(-dir.y, dir.x);
            var len = DagAutoJumpLineStyle.ArrowLength;
            var half = DagAutoJumpLineStyle.ArrowHalfWidth;
            var baseCenter = to - dir * len;
            var left = baseCenter + ortho * half;
            var right = baseCenter - ortho * half;

            GL.Begin(GL.TRIANGLES);
            GL.Color(DagAutoJumpLineStyle.Cyan);
            GL.Vertex3(to.x, to.y, 0f);
            GL.Vertex3(left.x, left.y, 0f);
            GL.Vertex3(right.x, right.y, 0f);
            GL.End();
        }

        private static Material _lineMaterial;

        private static Material GetLineMaterial()
        {
            if (_lineMaterial != null)
            {
                return _lineMaterial;
            }

            var shader = Shader.Find("Hidden/Internal-Colored");
            _lineMaterial = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
            _lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            _lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            _lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            _lineMaterial.SetInt("_ZWrite", 0);
            return _lineMaterial;
        }
    }
}
