using NonsensicalKit.Tools;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NonsensicalKit.Core.Editor
{
    [CustomPropertyDrawer(typeof(TypeQualifiedStringAttribute))]
    public class TypeQualifiedStringPropertyDrawer : PropertyDrawer
    {
        private List<string> _cachedTypes;
        private bool _needRefresh = true;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (_cachedTypes == null || _needRefresh)
            {
                var type = (attribute as TypeQualifiedStringAttribute).Type;
                _cachedTypes = ReflectionTool.GetConcreteTypesString(type);
                _needRefresh = false;
            }

            string crtValue = property.stringValue;

            int index = _cachedTypes.IndexOf(crtValue);
            EditorGUI.BeginChangeCheck();
            int newIndex = EditorGUI.Popup(position, index, _cachedTypes.ToArray());
            if (EditorGUI.EndChangeCheck() && index != newIndex)
            {
                property.stringValue = _cachedTypes[newIndex];
                _needRefresh = true;
            }
        }
    }
}