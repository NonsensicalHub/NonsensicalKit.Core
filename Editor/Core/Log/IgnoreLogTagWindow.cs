using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace NonsensicalKit.Core.Log.Editor
{
    public class IgnoreLogTagWindow : EditorWindow
    {
        [MenuItem("NonsensicalKit/日志过滤")]
        public static void ShowWindow()
        {
            GetWindow(typeof(IgnoreLogTagWindow));
        }


        private List<string> _ignoreTags;
        private ReorderableList _ignoreTagsList;

        private void OnEnable()
        {
            var ignoreStr = PlayerPrefs.GetString("NonsensicalKit_Editor_Ignore_Log_Tag_List", "");
            _ignoreTags = ignoreStr.Split("|", StringSplitOptions.RemoveEmptyEntries).ToList();
            _ignoreTagsList = new ReorderableList(_ignoreTags, typeof(List<string>), true, true, true, true)
            {
                drawHeaderCallback = DrawHeader,
                drawElementCallback = DrawListItems,
                onAddCallback = OnAddItem
            };
        }

        private void OnGUI()
        {
            _ignoreTagsList.DoLayoutList();
            if (GUILayout.Button("保存"))
            {
                var ignoreTags = new StringBuilder();

                foreach (var tag in _ignoreTags)
                {
                    ignoreTags.Append(tag);
                    ignoreTags.Append("|");
                }

                PlayerPrefs.SetString("NonsensicalKit_Editor_Ignore_Log_Tag_List", ignoreTags.ToString());
            }
        }

        private void OnAddItem(ReorderableList list)
        {
            list.list.Add("new ignore tag");
        }

        void DrawListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            _ignoreTags[index] = EditorGUI.TextField(rect, _ignoreTags[index]);
        }

        private void DrawHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "忽略日志标签");
        }
    }
}
