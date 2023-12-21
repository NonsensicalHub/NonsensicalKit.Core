using UnityEditor;
using UnityEngine;

namespace NonsensicalKit.Editor.Core.Tools
{
    /// <summary>
    /// 批量添加组件
    /// </summary>
    public class ComponentMountModifier : EditorWindow
    {
        [MenuItem("NonsensicalKit/批量修改/组件挂载修改器")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(ComponentMountModifier));
        }

        private static class ComponentMountModifierPanel
        {
            public static int ComponentCount;

            public static int ApplyCount;

            public static bool[] IsMount;

            public static MonoScript[] Components;
        }

        private void OnGUI()
        {

            ComponentMountModifierPanel.ComponentCount = EditorGUILayout.IntField("组件数量", ComponentMountModifierPanel.ComponentCount, GUILayout.MinWidth(100f));

            if (GUILayout.Button("应用数量"))
            {
                ComponentMountModifierPanel.ApplyCount = ComponentMountModifierPanel.ComponentCount;
                ComponentMountModifierPanel.IsMount = new bool[ComponentMountModifierPanel.ApplyCount];
                ComponentMountModifierPanel.Components = new MonoScript[ComponentMountModifierPanel.ApplyCount];
            }

            for (int i = 0; i < ComponentMountModifierPanel.ApplyCount; i++)
            {
                ComponentMountModifierPanel.Components[i] = (MonoScript)EditorGUILayout.ObjectField("组件", ComponentMountModifierPanel.Components[i], typeof(MonoScript), false, GUILayout.MinWidth(100f));
                ComponentMountModifierPanel.IsMount[i] = EditorGUILayout.Toggle("是否挂载", ComponentMountModifierPanel.IsMount[i], GUILayout.MinWidth(100f));
            }

            if (GUILayout.Button("应用"))
            {
                for (int i = 0; i < ComponentMountModifierPanel.Components.Length; i++)
                {
                    Object[] tArray = Resources.FindObjectsOfTypeAll(typeof(Transform));

                    foreach (var item in tArray)
                    {
                        Transform temp = item as Transform;

                        Undo.RecordObject(temp, temp.gameObject.name);
                        if (ComponentMountModifierPanel.IsMount[i] == true)
                        {
                            if (temp.GetComponent(ComponentMountModifierPanel.Components[i].GetClass()) == null)
                            {
                                temp.gameObject.AddComponent(ComponentMountModifierPanel.Components[i].GetClass());
                            }
                        }
                        else
                        {
                            if (temp.GetComponent(ComponentMountModifierPanel.Components[i].GetClass()) != null)
                            {
                                DestroyImmediate(temp.gameObject.GetComponent(ComponentMountModifierPanel.Components[i].GetClass()));
                            }
                        }
                    }
                }
            }
        }
    }
}
