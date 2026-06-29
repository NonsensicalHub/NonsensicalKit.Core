using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace NonsensicalKit.Core.DagLogicNode.Editor
{
    public sealed class DagDefaultParentEdge : Edge
    {
        private static readonly Color DefaultParentInputColor = new Color(0.78f, 0.58f, 0.08f, 1f);
        private static readonly Color DefaultParentOutputColor = new Color(1f, 0.84f, 0.2f, 1f);
        private static readonly Color DefaultParentCapColor = new Color(0.95f, 0.72f, 0.12f, 1f);

        private static readonly Color NormalEdgeInputColor = new Color(0.45f, 0.45f, 0.45f, 1f);
        private static readonly Color NormalEdgeOutputColor = new Color(0.85f, 0.85f, 0.85f, 1f);
        private static readonly Color NormalCapColor = new Color(0.7f, 0.7f, 0.7f, 1f);

        private const int DefaultParentEdgeWidth = 6;
        private const int NormalEdgeWidth = 2;

        public bool IsDefaultParent { get; private set; }
        public bool IsAutoJump { get; set; }

        public override bool UpdateEdgeControl()
        {
            var updated = base.UpdateEdgeControl();
            if (updated)
            {
                ApplyEdgeVisual();
            }

            return updated;
        }

        public override void OnSelected()
        {
            base.OnSelected();
            ApplyEdgeVisual();
        }

        public override void OnUnselected()
        {
            base.OnUnselected();
            ApplyEdgeVisual();
        }

        public void SetVisualStyle(bool isDefaultParent, bool isAutoJump = false)
        {
            IsDefaultParent = isDefaultParent;
            IsAutoJump = isAutoJump;
            EnableInClassList("dag-edge-default-parent", isDefaultParent);
            UpdateEdgeControl();
        }

        private void ApplyEdgeVisual()
        {
            var control = edgeControl;
            if (control == null)
            {
                return;
            }

            if (IsAutoJump || IsAutoJumpFromPorts())
            {
                control.edgeWidth = 0;
                control.capRadius = 0f;
                control.style.opacity = 0f;
                style.opacity = 0f;
            }
            else if (IsDefaultParent)
            {
                control.inputColor = DefaultParentInputColor;
                control.outputColor = DefaultParentOutputColor;
                control.fromCapColor = DefaultParentCapColor;
                control.toCapColor = DefaultParentCapColor;
                control.edgeWidth = DefaultParentEdgeWidth;
                control.capRadius = 6f;
                control.style.opacity = 1f;
            }
            else
            {
                control.inputColor = NormalEdgeInputColor;
                control.outputColor = NormalEdgeOutputColor;
                control.fromCapColor = NormalCapColor;
                control.toCapColor = NormalCapColor;
                control.edgeWidth = NormalEdgeWidth;
                control.capRadius = 4f;
                control.style.opacity = 1f;
            }

            control.MarkDirtyRepaint();
        }

        private bool IsAutoJumpFromPorts()
        {
            if (output != null && DagNodeView.IsAutoJumpPort(output)) return true;
            if (input != null && DagNodeView.IsAutoJumpPort(input)) return true;
            return false;
        }
    }
}
