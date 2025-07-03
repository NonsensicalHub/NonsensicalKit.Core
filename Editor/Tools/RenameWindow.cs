using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NonsensicalKit.Core.Editor.Tools
{
    public class RenameWindow : EditorWindow
    {
        
        [MenuItem("NonsensicalKit/批量修改/修改子物体名称")]
        public static void ShowWindow()
        {
            GetWindow(typeof(RenameWindow));
        }

        private string _inputName;

        private void OnEnable()
        {
            _inputName = string.Empty;
        }

        private void OnGUI()
        {
            _inputName = EditorGUILayout.TextField("名称", _inputName);
            
            if (GUILayout.Button("重命名", GUILayout.Height(40)))
            {
                if (NonsensicalEditorManager.SelectTransform!=null)
                {
                    Undo.SetCurrentGroupName("Batch Rename");
                    int undoGroup = Undo.GetCurrentGroup();
                    int index = 1;
                    foreach (Transform child in   NonsensicalEditorManager.SelectTransform)
                    {
                        child.name=$"{_inputName}_{index++}";
                        Undo.RecordObject(child, "Rename Object");
                    }
                    
                    Undo.CollapseUndoOperations(undoGroup);
                }
            }
        }
    }
}