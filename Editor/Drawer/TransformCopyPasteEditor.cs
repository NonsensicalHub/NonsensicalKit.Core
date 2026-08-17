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
            DrawWorldValueRow("位置", transform.position, true, false, false);
            DrawWorldValueRow("旋转", transform.rotation.eulerAngles, false, true, false);
            DrawWorldValueRow("缩放", transform.lossyScale, false, false, true);
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

    private void DrawWorldValueRow(string label, Vector3 value, bool position, bool rotation, bool scale)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"{label}: {value}");
        if (GUILayout.Button("复制", GUILayout.Width(40f)))
        {
            EditorGUIUtility.systemCopyBuffer = $"{value.x:F3}, {value.y:F3}, {value.z:F3}";
            CopyWorldTransform(position, rotation, scale);
        }

        if (GUILayout.Button("粘贴", GUILayout.Width(40f)))
        {
            PasteWorldTransform(position, rotation, scale);
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
            // 存世界缩放，粘贴到任意深度节点时保持一致
            EditorPrefs.SetString(PrefScale, JsonUtility.ToJson(transform.lossyScale));
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
            Debug.Log("世界缩放已复制");
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

        // 先写位置/旋转，再按当前层级换算世界缩放，避免顺序影响结果
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
            SetWorldScale(transform, JsonUtility.FromJson<Vector3>(EditorPrefs.GetString(PrefScale)));
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
            Debug.Log("世界缩放已粘贴");
        }
    }

    /// <summary>
    /// 按目标节点父级换算 localScale，使 lossyScale 与指定世界缩放一致。
    /// </summary>
    private static void SetWorldScale(Transform transform, Vector3 worldScale)
    {
        if (transform.parent == null)
        {
            transform.localScale = worldScale;
            return;
        }

        transform.localScale = Vector3.one;
        var parentContribution = transform.lossyScale;
        transform.localScale = new Vector3(
            SafeDivide(worldScale.x, parentContribution.x),
            SafeDivide(worldScale.y, parentContribution.y),
            SafeDivide(worldScale.z, parentContribution.z));
    }

    private static float SafeDivide(float numerator, float denominator)
    {
        return Mathf.Approximately(denominator, 0f) ? 0f : numerator / denominator;
    }
}
