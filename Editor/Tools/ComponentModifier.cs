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
            EditorWindow.GetWindow(typeof(ComponentModifier));
        }

        private static class CompontentModifierPanel
        {
            public static readonly string[] components = new string[] { "Transform", "Button", "Text" };

            public static Vector3 scale;
            public static Navigation.Mode navMode;
            public static int choiceComponent;
            public static Font toChange;
            public static FontStyle toFontStyle;
        }

        private void OnGUI()
        {

            CompontentModifierPanel.choiceComponent = EditorGUILayout.Popup("选择组件", CompontentModifierPanel.choiceComponent, new string[] { "Transform", "Button", "Text" });

            EditorGUILayout.Space();

            switch (CompontentModifierPanel.components[CompontentModifierPanel.choiceComponent])
            {
                case "Transform":
                    {
                        CompontentModifierPanel.scale = (Vector3)EditorGUILayout.Vector3Field("Scale", CompontentModifierPanel.scale, GUILayout.MinWidth(100f));
                        if (GUILayout.Button("修改"))
                        {
                            TransformModify();
                        }
                    }
                    break;
                case "Button":
                    {
                        CompontentModifierPanel.navMode = (Navigation.Mode)EditorGUILayout.EnumPopup("Navigation", CompontentModifierPanel.navMode, GUILayout.MinWidth(100f));
                        if (GUILayout.Button("修改"))
                        {
                            ButtonModify();
                        }
                    }
                    break;
                case "Text":
                    {
                        CompontentModifierPanel.toChange = (Font)EditorGUILayout.ObjectField("Font", CompontentModifierPanel.toChange, typeof(Font), true, GUILayout.MinWidth(100f));
                        CompontentModifierPanel.toFontStyle = (FontStyle)EditorGUILayout.EnumPopup("FontStyle", CompontentModifierPanel.toFontStyle, GUILayout.MinWidth(100f));
                        if (GUILayout.Button("修改"))
                        {
                            FontModify();
                        }
                    }
                    break;
                default:
                    Debug.LogError($"未判断的组件{CompontentModifierPanel.components[CompontentModifierPanel.choiceComponent]}");
                    break;
            }
        }

        private void ButtonModify()
        {
            var tArray = GetSelectComponent<Button>();
            for (int i = 0; i < tArray.Count; i++)
            {
                Button button = tArray[i];

                Undo.RecordObject(button, button.gameObject.name);

                Navigation nav = new Navigation();
                nav.mode = CompontentModifierPanel.navMode;
                button.navigation = nav;
            }
            Debug.Log($"{CompontentModifierPanel.components[CompontentModifierPanel.choiceComponent]} 组件修改成功");
        }

        private void FontModify()
        {
            var tArray = GetSelectComponent<Text>();
            for (int i = 0; i < tArray.Count; i++)
            {
                Text t = tArray[i];

                Undo.RecordObject(t, t.gameObject.name);

                t.font = CompontentModifierPanel.toChange;
                t.fontStyle = CompontentModifierPanel.toFontStyle;
            }
            Debug.Log($"{CompontentModifierPanel.components[CompontentModifierPanel.choiceComponent]} 组件修改成功");
        }


        private void TransformModify()
        {
            var tArray = GetSelectComponent<Transform>();
            for (int i = 0; i < tArray.Count; i++)
            {
                Transform temp = tArray[i];

                Undo.RecordObject(temp, temp.gameObject.name);

                temp.localScale = CompontentModifierPanel.scale;
            }
            Debug.Log($"{CompontentModifierPanel.components[CompontentModifierPanel.choiceComponent]} 组件修改成功");
        }

        private List<T> GetSelectComponent<T>()
        {
            var v = NonsensicalEditorManager.SelectGameobjects;
            List<T> components = new List<T>();

            foreach (var item in v)
            {
                components.AddRange(item.GetComponentsInChildren<T>());
            }
            return components;
        }
    }
}
