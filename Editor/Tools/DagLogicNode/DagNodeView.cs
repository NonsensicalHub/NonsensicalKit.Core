using NonsensicalKit.Core.DagLogicNode;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace NonsensicalKit.Core.DagLogicNode.Editor
{
    public class DagNodeView : Node
    {
        public DagNode Node;

        public Port Input;
        public Port Output;
        public Port AutoJumpOutput;
        public Port AutoJumpInput;

        private readonly DagGraphView _graphView;

        public DagNodeView(DagNode node, DagGraphView graphView)
        {
            Node = node;
            _graphView = graphView;
        }

        public void Init()
        {
            if (string.IsNullOrWhiteSpace(Node.nodeId))
            {
                Node.nodeId = _graphView.ValidateAndApplyNodeId(Node, Node.describe);
            }

            viewDataKey = Node.nodeId;

            inputContainer.style.flexGrow = 1;
            inputContainer.style.justifyContent = Justify.FlexStart;
            outputContainer.style.flexGrow = 1;
            outputContainer.style.justifyContent = Justify.FlexStart;

            Input = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(float));
            Input.portName = "In";
            inputContainer.Add(Input);

            ApplyNodeVisual();

            Output = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(float));
            Output.portName = "Out";
            outputContainer.Add(Output);

            var idField = new TextField("ID") { value = Node.nodeId };
            idField.RegisterCallback<FocusOutEvent>(evt =>
            {
                var validatedId = _graphView.ValidateAndApplyNodeId(Node, idField.value);
                idField.SetValueWithoutNotify(validatedId);
                viewDataKey = validatedId;
                _graphView.SyncGraphEdgeVisuals();
                _graphView.RefreshAllNodeVisualsPublic();
            });
            extensionContainer.Add(idField);

            var describeField = new TextField("描述") { value = Node.describe };
            describeField.RegisterCallback<FocusOutEvent>(evt =>
            {
                if (_graphView.GetGraph() != null)
                    Undo.RecordObject(_graphView.GetGraph(), "修改描述");
                Node.describe = describeField.value;
                title = string.IsNullOrEmpty(describeField.value)
                    ? _graphView.GetNodeTitleFallback(Node)
                    : describeField.value;
                if (_graphView.GetGraph() != null)
                    EditorUtility.SetDirty(_graphView.GetGraph());
            });
            extensionContainer.Add(describeField);

            BuildAutoJumpPorts();

            RefreshPorts();
            RefreshExpandedState();
        }

        private void BuildAutoJumpPorts()
        {
            AutoJumpInput = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
            AutoJumpInput.portName = "入跳";
            inputContainer.Add(AutoJumpInput);

            AutoJumpOutput = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
            AutoJumpOutput.portName = "跳转";
            outputContainer.Add(AutoJumpOutput);
        }

        public void ApplyNodeVisual() => ApplyDefaultVisual();

        public void ApplyDefaultVisual()
        {
            var isDefault = _graphView.IsDefaultNode(Node);
            if (isDefault)
            {
                AddToClassList("dag-node-default");
                titleContainer.style.backgroundColor = new StyleColor(new Color(0.08f, 0.42f, 0.38f));
                mainContainer.style.borderLeftWidth = 4;
                mainContainer.style.borderLeftColor = new StyleColor(new Color(0.95f, 0.78f, 0.18f));
                mainContainer.style.borderTopWidth = 1;
                mainContainer.style.borderRightWidth = 1;
                mainContainer.style.borderBottomWidth = 1;
                mainContainer.style.borderTopColor = new StyleColor(new Color(0.95f, 0.78f, 0.18f, 0.55f));
                mainContainer.style.borderRightColor = mainContainer.style.borderTopColor;
                mainContainer.style.borderBottomColor = mainContainer.style.borderTopColor;
            }
            else
            {
                RemoveFromClassList("dag-node-default");
                titleContainer.style.backgroundColor = StyleKeyword.Null;
                mainContainer.style.borderLeftWidth = StyleKeyword.Null;
                mainContainer.style.borderLeftColor = StyleKeyword.Null;
                mainContainer.style.borderTopWidth = StyleKeyword.Null;
                mainContainer.style.borderRightWidth = StyleKeyword.Null;
                mainContainer.style.borderBottomWidth = StyleKeyword.Null;
                mainContainer.style.borderTopColor = StyleKeyword.Null;
                mainContainer.style.borderRightColor = StyleKeyword.Null;
                mainContainer.style.borderBottomColor = StyleKeyword.Null;
            }

            title = string.IsNullOrEmpty(Node.describe)
                ? _graphView.GetNodeTitleFallback(Node)
                : Node.describe;
        }

        public static bool IsAutoJumpPort(Port port) =>
            port != null &&
            port.node is DagNodeView nodeView &&
            (port == nodeView.AutoJumpOutput || port == nodeView.AutoJumpInput);
    }
}
