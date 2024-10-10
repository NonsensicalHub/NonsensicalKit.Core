using NonsensicalKit.Tools.EditorTool;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace NonsensicalKit.Core.Editor
{
    [CustomPropertyDrawer(typeof(ShowIFAttribute))]
    public class ShowIFDrawer : PropertyDrawer
    {
        bool _show = true;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ShowIFAttribute att = (ShowIFAttribute)attribute;
            var targetValue = att.Value;
            var obj = GetParent(property);
            _show = ShowTime(obj, att, targetValue);
            if (_show)
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (_show)
            {
                return EditorGUI.GetPropertyHeight(property, label);
            }
            else
            {
                return -2;
            }
        }

        public bool ShowTime(object obj, ShowIFAttribute att, object targetValue)
        {
            var type = obj.GetType();
            while (type != null)
            {
                var memberField = type.GetField(att.MemberName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (memberField != null)
                {
                    var value = memberField.GetValue(obj);
                    return value.Equals(targetValue);
                }
                else
                {
                    var memberProperty = obj.GetType().GetProperty(att.MemberName);
                    if (memberProperty != null)
                    {
                        var value = memberProperty.GetGetMethod(nonPublic: true).Invoke(obj, null);
                        return value.Equals(targetValue);
                    }
                }
                type = type.BaseType;
            }

            return false;
        }

        /// <summary>
        /// 通过序列化字段获取到所属类对象
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        public object GetParent(SerializedProperty prop)
        {
            var path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            var elements = path.Split('.');
            foreach (var element in elements.Take(elements.Length - 1))
            {
                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    obj = GetValue(obj, elementName, index);
                }
                else
                {
                    obj = GetValue(obj, element);
                }
            }
            return obj;
        }

        public object GetValue(object source, string name, int index)
        {
            var enumerable = GetValue(source, name) as IEnumerable;
            var enm = enumerable.GetEnumerator();
            while (index-- >= 0)
                enm.MoveNext();
            return enm.Current;
        }

        public object GetValue(object source, string name)
        {
            if (source == null)
                return null;
            var type = source.GetType();
            var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (f == null)
            {
                var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p == null)
                    return null;
                return p.GetValue(source, null);
            }
            return f.GetValue(source);
        }
    }
}
