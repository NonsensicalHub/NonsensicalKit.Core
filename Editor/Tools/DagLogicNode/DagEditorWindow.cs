using NonsensicalKit.Core.DagLogicNode;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace NonsensicalKit.Core.DagLogicNode.Editor
{
    public class DagEditorWindow : EditorWindow
    {
        private DagGraphView graphView;
        [SerializeField] private DagGraphConfig graphConfig;

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceId, int line)
        {
            var asset = EditorUtility.InstanceIDToObject(instanceId) as DagGraphConfig;
            if (asset == null) return false;
            OpenWithGraph(asset);
            return true;
        }

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
            RebuildGraphView();

            if (graphConfig != null)
                graphView.PopulateView(graphConfig);

            Undo.undoRedoPerformed += OnUndoRedo;
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;

            if (graphView != null)
            {
                rootVisualElement.Remove(graphView);
                graphView = null;
            }
        }

        public void LoadGraph(DagGraphConfig g)
        {
            graphConfig = g;
            if (graphView == null)
                RebuildGraphView();
            graphView.PopulateView(graphConfig);
        }

        public DagGraph GetGraph() => graphConfig?.DagGraph;

        private void RebuildGraphView()
        {
            if (graphView != null)
            {
                rootVisualElement.Remove(graphView);
            }

            graphView = new DagGraphView(this);
            rootVisualElement.Add(graphView);
        }

        private void OnUndoRedo()
        {
            if (graphConfig != null && graphView != null)
            {
                graphView.PopulateView(graphConfig);
            }
        }
    }
}
