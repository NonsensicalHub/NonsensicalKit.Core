using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace NonsensicalKit.Core.DagLogicNode.Editor
{
    public class DagEditorWindow : EditorWindow
    {
        private DagGraphView graphView;
        [SerializeField] private DagGraphConfig graphConfig;

        // 双击 DagGraph 资产时自动打开
        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceId, int line)
        {
            var asset = EditorUtility.InstanceIDToObject(instanceId) as DagGraphConfig;
            if (asset == null) return false;
            OpenWithGraph(asset);
            return true;
        }

        // 右键 DagGraph 资产 → "Open DAG Editor"（仅 DagGraph 时显示）
        [MenuItem("Assets/Open DAG Editor", true)]
        private static bool OpenDAGEditorValidate() => Selection.activeObject is DagGraphConfig;

        [MenuItem("Assets/Open DAG Editor")]
        private static void OpenDAGEditor()
        {
            var selected = Selection.activeObject as DagGraphConfig;
            if (selected != null)
                OpenWithGraph(selected);
        }

        private static void OpenWithGraph(DagGraphConfig target)
        {
            var window = GetWindow<DagEditorWindow>();
            var graphName = target.DagGraph != null ? target.DagGraph.GraphName : target.name;
            window.titleContent = new GUIContent($"DAG - {graphName}");
            window.LoadGraph(target);
        }

        private void OnEnable()
        {
            graphView = new DagGraphView(this);
            rootVisualElement.Add(graphView);

            // 窗口重新激活时恢复已有的 graph（序列化保留）
            if (graphConfig != null)
                graphView.PopulateView(graphConfig);
        }

        public void LoadGraph(DagGraphConfig g)
        {
            graphConfig = g;
            graphView.PopulateView(graphConfig);
        }

        public DagGraph GetGraph() => graphConfig?.DagGraph;
    }
}
