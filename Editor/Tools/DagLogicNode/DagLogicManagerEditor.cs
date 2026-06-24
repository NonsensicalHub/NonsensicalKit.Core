using UnityEditor;
using UnityEngine;

namespace NonsensicalKit.Core.DagLogicNode.Editor
{
    [CustomEditor(typeof(DagLogicManager))]
    internal sealed class DagLogicManagerEditor : UnityEditor.Editor
    {
        private SerializedProperty _activationChainProp;
        private SerializedProperty _activationSummaryProp;

        private void OnEnable()
        {
            _activationChainProp = serializedObject.FindProperty("m_activationChainInspector");
            _activationSummaryProp = serializedObject.FindProperty("m_activationChainSummary");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawPropertiesExcluding(serializedObject,
                "m_activationChainInspector",
                "m_activationChainSummary");

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("激活链路", EditorStyles.boldLabel);

            EditorGUI.BeginDisabledGroup(true);
            if (_activationSummaryProp != null)
            {
                EditorGUILayout.PropertyField(_activationSummaryProp, new GUIContent("路径摘要"));
            }

            if (_activationChainProp != null)
            {
                EditorGUILayout.PropertyField(_activationChainProp, new GUIContent("节点序列"), true);
            }
            EditorGUI.EndDisabledGroup();

            var manager = (DagLogicManager)target;
            if (Application.isPlaying == false)
            {
                EditorGUILayout.HelpBox("运行时将随节点切换实时更新。", MessageType.None);
            }
            else if (manager.CrtSelectNode != null)
            {
                EditorGUILayout.LabelField("当前节点", manager.CrtSelectNode.NodeID);
            }

            serializedObject.ApplyModifiedProperties();

            if (Application.isPlaying)
            {
                Repaint();
            }
        }
    }
}
