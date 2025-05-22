using NonsensicalKit.Core;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Float3))]
public class Float3Drawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // 使用 EditorGUI.LabelField 显示标签
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // 获取 Int3 的三个字段
        SerializedProperty i1Property = property.FindPropertyRelative("m_f1");
        SerializedProperty i2Property = property.FindPropertyRelative("m_f2");
        SerializedProperty i3Property = property.FindPropertyRelative("m_f3");

        // 创建一个 Vector3 用于显示
        Vector3 vectorValue = new Vector3(i1Property.floatValue, i2Property.floatValue, i3Property.floatValue);

        // 使用 Vector3Field 绘制
        vectorValue = EditorGUI.Vector3Field(position, "", vectorValue);

        // 更新 Int3 的三个字段
        i1Property.floatValue = vectorValue.x;
        i2Property.floatValue = vectorValue.y;
        i3Property.floatValue = vectorValue.z;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight;
    }
}
