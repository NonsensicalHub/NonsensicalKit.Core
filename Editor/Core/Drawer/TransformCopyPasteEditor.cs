using UnityEditor;
using UnityEngine;
/// <summary>
/// 快速复制粘贴物体坐标信息
/// </summary>
[CustomEditor(typeof(Transform))]
public class TransformCopyPasteEditor : Editor
{
    private Vector3 _copiedPosition;
    private Quaternion _copiedRotation;
    private Vector3 _copiedScale;
    private bool _showCopiedCoordinates = false;

    private SerializedProperty _positionProp;
    private SerializedProperty _rotationProp;
    private SerializedProperty _scaleProp;

    private void OnEnable()
    {
        _positionProp = serializedObject.FindProperty("m_LocalPosition");
        _rotationProp = serializedObject.FindProperty("m_LocalRotation");
        _scaleProp = serializedObject.FindProperty("m_LocalScale");

        // 从 EditorPrefs 加载 showCopiedCoordinates 的值
        _showCopiedCoordinates = EditorPrefs.GetBool("TransformCopyPaste.ShowCopiedCoordinates", false);
    }

    private void OnDisable()
    {
        // 保存 showCopiedCoordinates 的值到 EditorPrefs
        EditorPrefs.SetBool("TransformCopyPaste.ShowCopiedCoordinates", _showCopiedCoordinates);
    }


    public override void OnInspectorGUI()
    {
        serializedObject.Update();
       // DrawDefaultInspector();
       EditorGUILayout.PropertyField(_positionProp);
       EditorGUILayout.PropertyField(_rotationProp);
       EditorGUILayout.PropertyField(_scaleProp);


        // Toggle 控制是否显示复制的世界坐标
        _showCopiedCoordinates = EditorGUILayout.Toggle("显示世界坐标", _showCopiedCoordinates);

        if (_showCopiedCoordinates)
        {
            // 显示复制的世界坐标
            EditorGUILayout.LabelField("世界坐标:", EditorStyles.boldLabel);
            Matrix4x4 transform = ((Transform)target).localToWorldMatrix;
            EditorGUILayout.LabelField($"位置: {transform.GetPosition()}");
            EditorGUILayout.LabelField($"旋转: {transform.rotation.eulerAngles}");
            EditorGUILayout.LabelField($"缩放: {transform.lossyScale}");
        }

        EditorGUILayout.BeginHorizontal();
        // 复制按钮
        if (GUILayout.Button("复制世界坐标"))
        {
            var transform = target as Transform;
            if (transform)
            {
                _copiedPosition = transform.position;
                _copiedRotation = transform.rotation;
                _copiedScale = transform.localScale;
            }

            // 使用 JsonUtility 序列化数据
            EditorPrefs.SetString("TransformCopyPaste.Position", JsonUtility.ToJson(_copiedPosition));
            EditorPrefs.SetString("TransformCopyPaste.Rotation", JsonUtility.ToJson(_copiedRotation));
            EditorPrefs.SetString("TransformCopyPaste.Scale", JsonUtility.ToJson(_copiedScale));

            Debug.Log("世界坐标已复制");
        }
        // 粘贴按钮
        if (GUILayout.Button("粘贴世界坐标"))
        {
            if (EditorPrefs.HasKey("TransformCopyPaste.Position") &&
                EditorPrefs.HasKey("TransformCopyPaste.Rotation") &&
                EditorPrefs.HasKey("TransformCopyPaste.Scale"))
            {
                var transform = target as Transform;
                // 使用 JsonUtility 反序列化数据
                if (transform)
                {
                    transform.position = JsonUtility.FromJson<Vector3>(EditorPrefs.GetString("TransformCopyPaste.Position"));
                    transform.rotation = JsonUtility.FromJson<Quaternion>(EditorPrefs.GetString("TransformCopyPaste.Rotation"));
                    transform.localScale = JsonUtility.FromJson<Vector3>(EditorPrefs.GetString("TransformCopyPaste.Scale"));
                }

                Debug.Log("世界坐标已粘贴");
            }
            else
            {
                Debug.LogWarning("没有复制的世界坐标");
            }
        }

        EditorGUILayout.EndHorizontal();
        serializedObject.ApplyModifiedProperties();
    }
}
