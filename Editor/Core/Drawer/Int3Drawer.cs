using NonsensicalKit.Core;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Int3))]
public class Int3Drawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // 使用 EditorGUI.LabelField 显示标签
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // 获取 Int3 的三个字段
        SerializedProperty i1Property = property.FindPropertyRelative("m_i1");
        SerializedProperty i2Property = property.FindPropertyRelative("m_i2");
        SerializedProperty i3Property = property.FindPropertyRelative("m_i3");

        // 创建一个 Vector3 用于显示
        Vector3Int vectorValue = new Vector3Int(i1Property.intValue, i2Property.intValue, i3Property.intValue);

        // 使用 Vector3Field 绘制
        vectorValue = EditorGUI.Vector3IntField(position, "", vectorValue);

        // 更新 Int3 的三个字段
        i1Property.intValue = (int)vectorValue.x;
        i2Property.intValue = (int)vectorValue.y;
        i3Property.intValue = (int)vectorValue.z;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight;
    }
}
