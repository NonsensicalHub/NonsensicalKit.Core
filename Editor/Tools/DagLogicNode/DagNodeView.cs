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
                Node.nodeId = _graphView.ValidateAndApplyNodeId(Node, Node.isRoot ? "root" : Node.describe);
            }

            viewDataKey = Node.nodeId;

            string defaultName = Node.isRoot ? "Root" : "Node";
            title = string.IsNullOrEmpty(Node.describe) ? defaultName : Node.describe;

            if (Node.isRoot)
            {
                // 根节点：红色标题栏，无输入口
                titleContainer.style.backgroundColor = new StyleColor(new Color(0.55f, 0.13f, 0.13f));
            }
            else
            {
                // 普通节点：输入口
                Input = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(float));
                Input.portName = "In";
                inputContainer.Add(Input);
            }

            Output = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(float));
            Output.portName = "Out";
            outputContainer.Add(Output);

            var idField = new TextField("ID") { value = Node.nodeId };
            idField.RegisterCallback<FocusOutEvent>(evt =>
            {
                var validatedId = _graphView.ValidateAndApplyNodeId(Node, idField.value);
                idField.SetValueWithoutNotify(validatedId);
                viewDataKey = validatedId;
                EditorUtility.SetDirty(_graphView.GetGraph());
            });
            extensionContainer.Add(idField);

            var describeField = new TextField("描述") { value = Node.describe };

            describeField.RegisterCallback<FocusOutEvent>(evt =>
            {
                Node.describe = describeField.value;
                title = string.IsNullOrEmpty(describeField.value) ? defaultName : describeField.value;
                EditorUtility.SetDirty(_graphView.GetGraph());
            });
            extensionContainer.Add(describeField);


            RefreshPorts();
            RefreshExpandedState();
        }
    }
}
