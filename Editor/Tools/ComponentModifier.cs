using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace NonsensicalKit.Core.Editor.Tools
{
    /// <summary>
    /// 批量修改组件
    /// </summary>
    public class ComponentModifier : EditorWindow
    {
        [MenuItem("NonsensicalKit/批量修改/组件内容修改器")]
        public static void ShowWindow()
        {
            GetWindow(typeof(ComponentModifier));
        }

        private static class ComponentModifierPanel
        {
            public static readonly string[] Components = { "Transform", "Button", "Text" };

            public static Vector3 Scale;
            public static Navigation.Mode NavMode;
            public static int ChoiceComponent;
            public static Font ToChange;
            public static FontStyle ToFontStyle;
        }

        private void OnGUI()
        {
            ComponentModifierPanel.ChoiceComponent = EditorGUILayout.Popup("选择组件", ComponentModifierPanel.ChoiceComponent,
                new[] { "Transform", "Button", "Text" });

            EditorGUILayout.Space();

            switch (ComponentModifierPanel.Components[ComponentModifierPanel.ChoiceComponent])
            {
                case "Transform":
                {
                    ComponentModifierPanel.Scale =
                        EditorGUILayout.Vector3Field("Scale", ComponentModifierPanel.Scale, GUILayout.MinWidth(100f));
                    if (GUILayout.Button("修改"))
                    {
                        TransformModify();
                    }
                }
                    break;
                case "Button":
                {
                    ComponentModifierPanel.NavMode =
                        (Navigation.Mode)EditorGUILayout.EnumPopup("Navigation", ComponentModifierPanel.NavMode, GUILayout.MinWidth(100f));
                    if (GUILayout.Button("修改"))
                    {
                        ButtonModify();
                    }
                }
                    break;
                case "Text":
                {
                    ComponentModifierPanel.ToChange = (Font)EditorGUILayout.ObjectField("Font", ComponentModifierPanel.ToChange, typeof(Font), true,
                        GUILayout.MinWidth(100f));
                    ComponentModifierPanel.ToFontStyle =
                        (FontStyle)EditorGUILayout.EnumPopup("FontStyle", ComponentModifierPanel.ToFontStyle, GUILayout.MinWidth(100f));
                    if (GUILayout.Button("修改"))
                    {
                        FontModify();
                    }
                }
                    break;
                default:
                    Debug.LogError($"未判断的组件{ComponentModifierPanel.Components[ComponentModifierPanel.ChoiceComponent]}");
                    break;
            }
        }

        private void ButtonModify()
        {
            var tArray = GetSelectComponent<Button>();
            foreach (var button in tArray)
            {
                Undo.RecordObject(button, button.gameObject.name);

                Navigation nav = new Navigation
                {
                    mode = ComponentModifierPanel.NavMode
                };
                button.navigation = nav;
            }

            Debug.Log($"{ComponentModifierPanel.Components[ComponentModifierPanel.ChoiceComponent]} 组件修改成功");
        }

        private void FontModify()
        {
            var tArray = GetSelectComponent<Text>();
            foreach (var t in tArray)
            {
                Undo.RecordObject(t, t.gameObject.name);

                t.font = ComponentModifierPanel.ToChange;
                t.fontStyle = ComponentModifierPanel.ToFontStyle;
            }

            Debug.Log($"{ComponentModifierPanel.Components[ComponentModifierPanel.ChoiceComponent]} 组件修改成功");
        }


        private void TransformModify()
        {
            var tArray = GetSelectComponent<Transform>();
            foreach (var temp in tArray)
            {
                Undo.RecordObject(temp, temp.gameObject.name);

                temp.localScale = ComponentModifierPanel.Scale;
            }

            Debug.Log($"{ComponentModifierPanel.Components[ComponentModifierPanel.ChoiceComponent]} 组件修改成功");
        }

        private List<T> GetSelectComponent<T>()
        {
            var v = NonsensicalEditorManager.SelectGameObjects;
            List<T> components = new List<T>();

            foreach (var item in v)
            {
                components.AddRange(item.GetComponentsInChildren<T>());
            }

            return components;
        }
    }
}
