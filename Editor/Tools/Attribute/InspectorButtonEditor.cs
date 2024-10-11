using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;

[CanEditMultipleObjects]
[CustomEditor(typeof(MonoBehaviour), true)]
public class InspectorButtonEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        // 获取当前对象类型
        var type = target.GetType();
        // 获取所有方法
        var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

        foreach (var method in methods)
        {
            // 查找带有 InspectorButtonAttribute 的方法
            var attribute = (InspectorButtonAttribute)Attribute.GetCustomAttribute(method, typeof(InspectorButtonAttribute));
            if (attribute != null)
            {
                string buttonName = attribute.Description ?? method.Name;

                if (GUILayout.Button(buttonName))
                {
                    method.Invoke(target, null);
                }
            }
        }
    }
}
