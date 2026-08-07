using UnityEditor;
using UnityEngine;

/// <summary>
/// 快速复制粘贴物体坐标信息
/// </summary>
[CustomEditor(typeof(Transform))]
public class TransformCopyPasteEditor : Editor
{
    private const string PrefPosition = "TransformCopyPaste.Position";
    private const string PrefRotation = "TransformCopyPaste.Rotation";
    private const string PrefScale = "TransformCopyPaste.Scale";
    private const string PrefShowWorld = "TransformCopyPaste.ShowCopiedCoordinates";

    private bool _showCopiedCoordinates;

    private SerializedProperty _positionProp;
    private SerializedProperty _rotationProp;
    private SerializedProperty _scaleProp;

    private void OnEnable()
    {
        _positionProp = serializedObject.FindProperty("m_LocalPosition");
        _rotationProp = serializedObject.FindProperty("m_LocalRotation");
        _scaleProp = serializedObject.FindProperty("m_LocalScale");
        _showCopiedCoordinates = EditorPrefs.GetBool(PrefShowWorld, false);
    }

    private void OnDisable()
    {
        EditorPrefs.SetBool(PrefShowWorld, _showCopiedCoordinates);
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(_positionProp);
        EditorGUILayout.PropertyField(_rotationProp);
        EditorGUILayout.PropertyField(_scaleProp);

        _showCopiedCoordinates = EditorGUILayout.Toggle("显示世界坐标", _showCopiedCoordinates);

        if (_showCopiedCoordinates)
        {
            var transform = (Transform)target;
            EditorGUILayout.LabelField("世界坐标:", EditorStyles.boldLabel);
            DrawCopyableValue("位置", transform.position);
            DrawCopyableValue("旋转", transform.rotation.eulerAngles);
            DrawCopyableValue("缩放", transform.lossyScale);
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("复制世界坐标"))
        {
            CopyWorldTransform(true, true, true);
        }

        if (GUILayout.Button("粘贴世界坐标"))
        {
            PasteWorldTransform(true, true, true);
        }

        var moreRect = GUILayoutUtility.GetRect(28f, EditorGUIUtility.singleLineHeight, GUILayout.Width(28f));
        if (GUI.Button(moreRect, "[...]"))
        {
            ShowMoreOptionsMenu(moreRect);
        }

        EditorGUILayout.EndHorizontal();
        serializedObject.ApplyModifiedProperties();
    }

    private static void DrawCopyableValue(string label, Vector3 value)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"{label}: {value}");
        if (GUILayout.Button("复制", GUILayout.Width(40f)))
        {
            EditorGUIUtility.systemCopyBuffer = $"{value.x:F3}, {value.y:F3}, {value.z:F3}";
            Debug.Log($"{label}已复制到剪贴板: {value}");
        }

        EditorGUILayout.EndHorizontal();
    }

    private void ShowMoreOptionsMenu(Rect buttonRect)
    {
        var menu = new GenericMenu();
        menu.AddItem(new GUIContent("仅复制位置"), false, () => CopyWorldTransform(true, false, false));
        menu.AddItem(new GUIContent("仅复制旋转"), false, () => CopyWorldTransform(false, true, false));
        menu.AddItem(new GUIContent("仅复制缩放"), false, () => CopyWorldTransform(false, false, true));
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("仅粘贴位置"), false, () => PasteWorldTransform(true, false, false));
        menu.AddItem(new GUIContent("仅粘贴旋转"), false, () => PasteWorldTransform(false, true, false));
        menu.AddItem(new GUIContent("仅粘贴缩放"), false, () => PasteWorldTransform(false, false, true));
        menu.DropDown(buttonRect);
    }

    private void CopyWorldTransform(bool position, bool rotation, bool scale)
    {
        var transform = target as Transform;
        if (!transform)
        {
            return;
        }

        if (position)
        {
            EditorPrefs.SetString(PrefPosition, JsonUtility.ToJson(transform.position));
        }

        if (rotation)
        {
            EditorPrefs.SetString(PrefRotation, JsonUtility.ToJson(transform.rotation));
        }

        if (scale)
        {
            EditorPrefs.SetString(PrefScale, JsonUtility.ToJson(transform.localScale));
        }

        if (position && rotation && scale)
        {
            Debug.Log("世界坐标已复制");
        }
        else if (position)
        {
            Debug.Log("世界位置已复制");
        }
        else if (rotation)
        {
            Debug.Log("世界旋转已复制");
        }
        else if (scale)
        {
            Debug.Log("缩放已复制");
        }
    }

    private void PasteWorldTransform(bool position, bool rotation, bool scale)
    {
        var transform = target as Transform;
        if (!transform)
        {
            return;
        }

        var hasPosition = EditorPrefs.HasKey(PrefPosition);
        var hasRotation = EditorPrefs.HasKey(PrefRotation);
        var hasScale = EditorPrefs.HasKey(PrefScale);

        if ((position && !hasPosition) || (rotation && !hasRotation) || (scale && !hasScale))
        {
            Debug.LogWarning("没有可粘贴的对应数据");
            return;
        }

        Undo.RecordObject(transform, "Paste World Transform");

        if (position)
        {
            transform.position = JsonUtility.FromJson<Vector3>(EditorPrefs.GetString(PrefPosition));
        }

        if (rotation)
        {
            transform.rotation = JsonUtility.FromJson<Quaternion>(EditorPrefs.GetString(PrefRotation));
        }

        if (scale)
        {
            transform.localScale = JsonUtility.FromJson<Vector3>(EditorPrefs.GetString(PrefScale));
        }

        if (position && rotation && scale)
        {
            Debug.Log("世界坐标已粘贴");
        }
        else if (position)
        {
            Debug.Log("世界位置已粘贴");
        }
        else if (rotation)
        {
            Debug.Log("世界旋转已粘贴");
        }
        else if (scale)
        {
            Debug.Log("缩放已粘贴");
        }
    }
}
