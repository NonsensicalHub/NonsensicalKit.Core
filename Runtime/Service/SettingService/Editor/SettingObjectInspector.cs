using NonsensicalKit.Core.Service.Setting;
using UnityEditor;
using UnityEngine;

namespace NonsensicalKit.Setting.Editor
{
    [CustomEditor(typeof(SettingObject))]
    public class SettingObjectInspector : UnityEditor.Editor
    {
        bool _showRaw;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            if (GUILayout.Button("打开可视化编辑", GUILayout.Height(28f)))
            {
                SettingJsonEditorWindow.Open((SettingObject)target);
            }

            EditorGUILayout.HelpBox("页签、分组和设置项在编辑器里改。原始 JSON 折在下面，平时不用手写。", MessageType.None);

            SerializedProperty config = serializedObject.FindProperty("m_config");
            if (config == null)
            {
                DrawDefaultInspector();
                return;
            }

            SerializedProperty id = config.FindPropertyRelative("ConfigID");
            if (id != null)
            {
                EditorGUILayout.PropertyField(id, new GUIContent("配置 Id"));
            }

            SerializedProperty usePanel = config.FindPropertyRelative("m_UseSettingPanel");
            if (usePanel != null)
            {
                EditorGUILayout.PropertyField(usePanel, new GUIContent("创建运行时面板"));
            }

            SerializedProperty panel = config.FindPropertyRelative("m_Panel");
            if (panel != null)
            {
                EditorGUILayout.PropertyField(panel, new GUIContent("面板"), true);
            }

            _showRaw = EditorGUILayout.Foldout(_showRaw, "原始 JSON");
            if (_showRaw)
            {
                SerializedProperty json = config.FindPropertyRelative("m_SettingJson");
                if (json != null)
                {
                    EditorGUILayout.PropertyField(json, new GUIContent("Setting JSON"), true);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
