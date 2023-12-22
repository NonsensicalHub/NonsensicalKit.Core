using UnityEditor;
using UnityEngine;

namespace NonsensicalKit.Core.Editor.Tools
{
    public class PrefsModifier : EditorWindow
    {
        [MenuItem("NonsensicalKit/Prefs修改器")]
        private static void ShowWindow()
        {
            GetWindow(typeof(PrefsModifier));
        }

        private void OnGUI()
        {
            if (GUILayout.Button("清空所有PlayerPrefs"))
            {
                PlayerPrefs.DeleteAll();
            }
            if (GUILayout.Button("清空所有EditorPrefs"))
            {
                EditorPrefs.DeleteAll();
            }
        }
    }
}
