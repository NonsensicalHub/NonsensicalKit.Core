using NonsensicalKit.Core;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Float4))]
public class Float4Drawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // 使用 EditorGUI.LabelField 显示标签
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // 获取 Int3 的三个字段
        SerializedProperty i1Property = property.FindPropertyRelative("m_f1");
        SerializedProperty i2Property = property.FindPropertyRelative("m_f2");
        SerializedProperty i3Property = property.FindPropertyRelative("m_f3");
        SerializedProperty i4Property = property.FindPropertyRelative("m_f4");

        // 创建一个 Vector3 用于显示
        Vector4 vectorValue = new Vector4(i1Property.floatValue, i2Property.floatValue, i3Property.floatValue, i4Property.floatValue);

        // 使用 Vector3Field 绘制
        vectorValue = EditorGUI.Vector4Field(position, "", vectorValue);

        // 更新 Int3 的三个字段
        i1Property.floatValue = vectorValue.x;
        i2Property.floatValue = vectorValue.y;
        i3Property.floatValue = vectorValue.z;
        i4Property.floatValue = vectorValue.w;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight;
    }
}
