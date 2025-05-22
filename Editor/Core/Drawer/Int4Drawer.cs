using NonsensicalKit.Core;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Int4))]
public class Int4Drawer : PropertyDrawer
{
    public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
    {
        // 使用 EditorGUI.LabelField 显示标签
        rect = EditorGUI.PrefixLabel(rect, GUIUtility.GetControlID(FocusType.Passive), label);

        // 获取 Int3 的三个字段
        SerializedProperty i1Property = property.FindPropertyRelative("m_i1");
        SerializedProperty i2Property = property.FindPropertyRelative("m_i2");
        SerializedProperty i3Property = property.FindPropertyRelative("m_i3");
        SerializedProperty i4Property = property.FindPropertyRelative("m_i4");
        
        
        var singleTextWidth = 12;
        var singleTextLeft = 4;
        var singleInputWidth = ( rect.width-4 * singleTextWidth - 3 * singleTextLeft)*0.25f;
        var singleInputOffset = singleInputWidth+singleTextLeft;
        
        rect.width = singleInputWidth;
        EditorGUI.LabelField(rect, "X");
        rect.x += singleTextWidth;
        i1Property.intValue = EditorGUI.IntField(rect, "", i1Property.intValue);
        rect.x += singleInputOffset;
        EditorGUI.LabelField(rect, "Y");
        rect.x += singleTextWidth;
        i2Property.intValue = EditorGUI.IntField(rect, "", i2Property.intValue);
        rect.x += singleInputOffset;
        EditorGUI.LabelField(rect, "Z");
        rect.x += singleTextWidth;
        i3Property.intValue = EditorGUI.IntField(rect, "", i3Property.intValue);
        rect.x += singleInputOffset;
        EditorGUI.LabelField(rect, "W");
        rect.x += singleTextWidth+3;
        rect.width -= 3;
        i4Property.intValue = EditorGUI.IntField(rect, "", i4Property.intValue);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight;
    }
}
