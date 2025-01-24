using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace NonsensicalKit.Core.Editor
{
    [CustomPropertyDrawer(typeof(EnumQualifiedStringAttribute))]
    public class EnumQualifiedStringPropertyDrawer : PropertyDrawer
    {
        private List<string> _cachedTypes;
        private bool _needRefresh = true;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (_cachedTypes == null || _needRefresh)
            {
                var type = (attribute as EnumQualifiedStringAttribute).EnumType;
                if (type.IsEnum == false)
                {
                    Debug.Log($"{property.name} is not enum");
                    return;
                }

                _cachedTypes = Enum.GetNames(type).ToList();
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
